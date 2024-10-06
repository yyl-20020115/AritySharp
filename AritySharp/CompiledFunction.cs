/*
 * Copyright (C) 2007-2009 Mihai Preda.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Text;

namespace AritySharp;

/**
   CompiledFunction is a function that was parsed from a string expression.
   It is represented as a sequence of bytecodes which are executed in order to
   evaluate the function.

   <h3>Thread safety</h3>
   CompiledFunction instances are thread-safe (don't require external locking),
   By default the evaluation is globally serialized 
   (it doesn't take advantage of multiple threads).<p>

   You can achive parallel evaluation by creating an instance of EvalContext
   for each thread, and passing the EvalContext as the last parameter of the
   eval() methods.
 */

public class CompiledFunction : ContextFunction
{
    private static readonly IsComplexException IS_COMPLEX = new ();
    private static readonly Random random = new ();
    private static readonly double[] EMPTY_DOUBLE = [];
    private static readonly Function[] EMPTY_FUN = [];
    private static readonly Complex ONE_THIRD = new (1 / 3.0, 0);

    private readonly double[] constsRe, constsIm;

    private readonly Function[] funcs;
    private readonly byte[] code;
    private readonly int _arity; // >= 0

    public CompiledFunction(int arity, byte[] code, double[] constsRe, double[] constsIm, Function[] funcs)
    {
        this._arity = arity;
        this.code = code;
        this.constsRe = constsRe;
        this.constsIm = constsIm;
        this.funcs = funcs;
    }
    private class FC : Function
    {
        public override int GetArity()
        {
            return 1;
        }

        public override double Eval(double x)
        {
            return x > 0 ? 1 : x < 0 ? -1 : 0;
        }
    }
    public static Function makeOpFunction(int op)
    {
        if (VM.arity[op] != 1)
        {
            throw new Exception("makeOpFunction expects arity 1, found " + VM.arity[op]);
        }
        CompiledFunction fun = new CompiledFunction(VM.arity[op], new byte[] { VM.LOAD0, (byte)op }, EMPTY_DOUBLE, EMPTY_DOUBLE, EMPTY_FUN);
        if (op == VM.ABS)
        {
            fun.            Derivative = new FC();
        }
        return fun;
    }

    //@Override
    public override int GetArity()
    {
        return _arity;
    }

    public string toString()
    {
        StringBuilder buf = new StringBuilder();
        int cpos = 0, fpos = 0;
        if (_arity != 0)
        {
            buf.Append("arity ").Append(_arity).Append("; ");
        }
        for (int i = 0; i < code.Length; ++i)
        {
            byte op = code[i];
            buf.Append(VM.opcodeName[op]);
            if (op == VM.CONST)
            {
                buf.Append(' ');
                if (constsIm == null)
                {
                    buf.Append(constsRe[cpos]);
                }
                else
                {
                    buf.Append('(').Append(constsRe[cpos]).Append(", ").Append(constsIm[cpos]).Append(')');
                }
                ++cpos;
            }
            else if (op == VM.CALL)
            {
                ++fpos;
                //buf.append(" {").append(funcs[fpos++].toString()).append('}');
            }
            buf.Append("; ");
        }
        if (cpos != constsRe.Length)
        {
            buf.Append("\nuses only ").Append(cpos).Append(" consts out of ").Append(constsRe.Length);
        }
        if (fpos != funcs.Length)
        {
            buf.Append("\nuses only ").Append(fpos).Append(" funcs out of ").Append(funcs.Length);
        }
        return buf.ToString();
    }

    public override double Eval(double[] args, EvalContext context)
    {
        if (constsIm != null)
        {
            return EvalComplexToReal(args, context);
        }
        CheckArity(args.Length);
        Array.Copy(args, 0, context.stackRe, context.stackBase, args.Length);
        try
        {
            execReal(context, context.stackBase + args.Length - 1);
            return context.stackRe[context.stackBase];
        }
        catch (IsComplexException e)
        {
            return EvalComplexToReal(args, context);
        }
    }

    private double EvalComplexToReal(double[] args, EvalContext context)
    {
        Complex[] argsC = ToComplex(args, context);
        Complex c = Eval(argsC, context);
        return c.AsReal;
    }

    public override Complex Eval(Complex[] args, EvalContext context)
    {
        CheckArity(args.Length);
        Complex[] stack = context.stackComplex;
        int _base = context.stackBase;
        for (int i = 0; i < args.Length; ++i)
        {
            stack[i + _base].Set(args[i]);
        }
        ExecComplex(context, _base + args.Length - 1);
        return stack[_base];
    }

    private int execReal(EvalContext context, int p)
    {
        int expected = p + 1;
        p = ExecWithoutCheck(context, p);
        if (p != expected)
        {
            throw new Exception("Stack pointer after exec: expected " +
                            expected + ", got " + p);
        }
        context.stackRe[p - _arity] = context.stackRe[p];
        return p - _arity;
    }

    private int ExecComplex(EvalContext context, int p)
    {
        int expected = p + 1;
        p = ExecWithoutCheckComplex(context, p, -2);
        if (p != expected)
        {
            throw new Exception("Stack pointer after exec: expected " +
                            expected + ", got " + p);
        }
        context.stackComplex[p - _arity].Set(context.stackComplex[p]);
        return p - _arity;
    }

    public int ExecWithoutCheck(EvalContext context, int p)
    {
        if (constsIm != null)
        {
            throw IS_COMPLEX;
        }

        double[] s = context.stackRe;

        int stackBase = p - _arity;
        int constp = 0;
        int funp = 0;

        int codeLen = code.Length;
        int percentPC = -2;
        for (int pc = 0; pc < codeLen; ++pc)
        {
            int opcode = code[pc];
            switch (opcode)
            {
                case VM.CONST:
                    s[++p] = constsRe[constp++];
                    break;

                case VM.CALL:
                    {
                        Function f = funcs[funp++];
                        if (f is CompiledFunction)
                        {
                            p = ((CompiledFunction)f).execReal(context, p);
                        }
                        else
                        {
                            int arity = f.GetArity();
                            p -= arity;
                            double result;
                            int prevBase = context.stackBase;
                            try
                            {
                                context.stackBase = p + 1;
                                switch (arity)
                                {
                                    case 0:
                                        result = f.Eval();
                                        break;
                                    case 1:
                                        result = f.Eval(s[p + 1]);
                                        break;
                                    case 2:
                                        result = f.Eval(s[p + 1], s[p + 2]);
                                        break;
                                    default:
                                        double[] args = new double[arity];
                                        Array.Copy(s, p + 1, args, 0, arity);
                                        result = f.Eval(args);
                                        break;
                                }
                            }
                            finally
                            {
                                context.stackBase = prevBase;
                            }
                            s[++p] = result;
                            //System.out.println(": " + p + " " + s[0] + " " + s[1] + " " + s[2]);
                        }
                        break;
                    }

                case VM.RND: s[++p] = random.NextDouble(); break;

                case VM.ADD:
                    {
                        double a = s[--p];
                        double res = a + (percentPC == pc - 1 ? s[p] * s[p + 1] : s[p + 1]);
                        if (Math.Abs(res) < Util.MathUlp(a) * 1024)
                        {
                            // hack for "1.1-1-.1"
                            res = 0;
                        }
                        s[p] = res;
                        break;
                    }

                case VM.SUB:
                    {
                        double a = s[--p];
                        double res = a - (percentPC == pc - 1 ? s[p] * s[p + 1] : s[p + 1]);
                        if (Math.Abs(res) < Util.MathUlp(a) * 1024)
                        {
                            // hack for "1.1-1-.1"
                            res = 0;
                        }
                        s[p] = res;
                        break;
                    }

                case VM.MUL: s[--p] *= s[p + 1]; break;
                case VM.DIV: s[--p] /= s[p + 1]; break;
                case VM.MOD: s[--p] %= s[p + 1]; break;

                case VM.POWER:
                    {
                        s[--p] = Math.Pow(s[p], s[p + 1]);
                        break;
                    }

                case VM.UMIN: s[p] = -s[p]; break;
                case VM.FACT: s[p] = MoreMath.Factorial(s[p]); break;
                case VM.PERCENT: s[p] = s[p] * .01; percentPC = pc; break;

                case VM.SIN: s[p] = MoreMath.Sin(s[p]); break;
                case VM.COS: s[p] = MoreMath.Cos(s[p]); break;
                case VM.TAN: s[p] = MoreMath.Tan(s[p]); break;

                case VM.ASIN:
                    {
                        double v = s[p];
                        if (v < -1 || v > 1)
                        {
                            throw IS_COMPLEX;
                        }
                        s[p] = Math.Asin(v);
                        break;
                    }

                case VM.ACOS:
                    {
                        double v = s[p];
                        if (v < -1 || v > 1)
                        {
                            throw IS_COMPLEX;
                        }
                        s[p] = Math.Acos(v);
                        break;
                    }

                case VM.ATAN: s[p] = Math.Atan(s[p]); break;

                case VM.EXP: s[p] = Math.Exp(s[p]); break;
                case VM.LN: s[p] = Math.Log(s[p]); break;

                case VM.SQRT:
                    {
                        double v = s[p];
                        if (v < 0) { throw IS_COMPLEX; }
                        s[p] = Math.Sqrt(v);
                        break;
                    }

                case VM.CBRT: s[p] = Math.Cbrt(s[p]); break;

                case VM.SINH: s[p] = Math.Sinh(s[p]); break;
                case VM.COSH: s[p] = Math.Cosh(s[p]); break;
                case VM.TANH: s[p] = Math.Tanh(s[p]); break;
                case VM.ASINH: s[p] = MoreMath.Asinh(s[p]); break;
                case VM.ACOSH: s[p] = MoreMath.Acosh(s[p]); break;
                case VM.ATANH: s[p] = MoreMath.Atanh(s[p]); break;

                case VM.ABS: s[p] = Math.Abs(s[p]); break;
                case VM.FLOOR: s[p] = Math.Floor(s[p]); break;
                case VM.CEIL: s[p] = Math.Ceiling(s[p]); break;
                case VM.SIGN:
                    {
                        double v = s[p];
                        s[p] = v > 0 ? 1 : v < 0 ? -1 : v == 0 ? 0 : Double.NaN;
                        break;
                    }

                case VM.MIN: s[--p] = Math.Min(s[p], s[p + 1]); break;
                case VM.MAX: s[--p] = Math.Max(s[p], s[p + 1]); break;
                case VM.GCD: s[--p] = MoreMath.Gcd(s[p], s[p + 1]); break;
                case VM.COMB: s[--p] = MoreMath.Combinations(s[p], s[p + 1]); break;
                case VM.PERM: s[--p] = MoreMath.Permutations(s[p], s[p + 1]); break;

                case VM.LOAD0:
                case VM.LOAD1:
                case VM.LOAD2:
                case VM.LOAD3:
                case VM.LOAD4:
                    //System.out.println("base " + stackBase + "; p " + p + "; arity " + arity);                
                    s[++p] = s[stackBase + opcode - (VM.LOAD0 - 1)];
                    break;

                case VM.REAL:
                    break; // NOP

                case VM.IMAG:
                    if (!Double.IsNaN(s[p]))
                    {
                        s[p] = 0;
                    }
                    break;

                default:
                    throw new Exception("Unknown opcode " + opcode);
            }
        }
        return p;
    }

    public int ExecWithoutCheckComplex(EvalContext context, int p, int percentPC)
    {
        Complex[] s = context.stackComplex;

        int stackBase = p - _arity;
        int constp = 0;
        int funp = 0;

        int codeLen = code.Length;
        // System.out.println("exec " + this);
        for (int pc = 0; pc < codeLen; ++pc)
        {
            int opcode = code[pc];
            // System.out.println("+ " + pc + ' ' + opcode + ' ' + p);
            switch (opcode)
            {
                case VM.CONST:
                    ++p;
                    s[p].Set(constsRe[constp], constsIm == null ? 0 : constsIm[constp]);
                    ++constp;
                    break;

                case VM.CALL:
                    {
                        Function f = funcs[funp++];
                        if (f is CompiledFunction)
                        {
                            p = ((CompiledFunction)f).ExecComplex(context, p);
                        }
                        else
                        {
                            int arity = f.GetArity();
                            p -= arity;
                            Complex result;
                            int prevBase = context.stackBase;
                            try
                            {
                                context.stackBase = p + 1;
                                switch (arity)
                                {
                                    case 0:
                                        result = new Complex(f.Eval(), 0);
                                        break;
                                    case 1:
                                        result = f.Eval(s[p + 1]);
                                        break;
                                    case 2:
                                        result = f.Eval(s[p + 1], s[p + 2]);
                                        break;
                                    default:
                                        Complex[] args = new Complex[arity];
                                        Array.Copy(s, p + 1, args, 0, arity);
                                        result = f.Eval(args);
                                        break;
                                }
                            }
                            finally
                            {
                                context.stackBase = prevBase;
                            }
                            s[++p].Set(result);
                        }
                        break;
                    }

                case VM.RND: s[++p].Set(random.NextDouble(), 0); break;

                case VM.ADD: s[--p].Add(percentPC == pc - 1 ? s[p + 1].Mul(s[p]) : s[p + 1]); break;
                case VM.SUB: s[--p].Sub(percentPC == pc - 1 ? s[p + 1].Mul(s[p]) : s[p + 1]); break;
                case VM.MUL: s[--p].Mul(s[p + 1]); break;
                case VM.DIV: s[--p].Div(s[p + 1]); break;
                case VM.MOD: s[--p].Mod(s[p + 1]); break;
                case VM.POWER: s[--p].Pow(s[p + 1]); break;

                case VM.UMIN: s[p].Negate(); break;
                case VM.FACT: s[p].Factorial(); break;
                case VM.PERCENT: s[p].Mul(new Complex(0.01, 0)); percentPC = pc; break;

                case VM.SIN: s[p].Sin(); break;
                case VM.COS: s[p].Cos(); break;
                case VM.TAN: s[p].Tan(); break;
                case VM.SINH: s[p].Sinh(); break;
                case VM.COSH: s[p].Cosh(); break;
                case VM.TANH: s[p].Tanh(); break;

                case VM.ASIN: s[p].Asin(); break;
                case VM.ACOS: s[p].Acos(); break;
                case VM.ATAN: s[p].Atan(); break;
                case VM.ASINH: s[p].Asinh(); break;
                case VM.ACOSH: s[p].Acosh(); break;
                case VM.ATANH: s[p].Atanh(); break;

                case VM.EXP: s[p].Exp(); break;
                case VM.LN: s[p].Log(); break;

                case VM.SQRT: s[p].Sqrt(); break;
                case VM.CBRT:
                    if (s[p].im == 0)
                    {
                        s[p].re = Math.Cbrt(s[p].re);
                    }
                    else
                    {
                        s[p].Pow(ONE_THIRD);
                    }
                    break;

                case VM.ABS: s[p].Set(s[p].Abs(), 0); break;
                case VM.FLOOR: s[p].Set(Math.Floor(s[p].re), 0); break;
                case VM.CEIL: s[p].Set(Math.Ceiling(s[p].re), 0); break;
                case VM.SIGN:
                    {
                        double a = s[p].re;
                        double b = s[p].im;
                        if (b == 0)
                        {
                            s[p].Set(a > 0 ? 1 : a < 0 ? -1 : a == 0 ? 0 : Double.NaN, 0);
                        }
                        else if (!s[p].IsNaN)
                        {
                            double abs = s[p].Abs();
                            s[p].Set(a / abs, b / abs);
                        }
                        else
                        {
                            s[p].Set(Double.NaN, 0);
                        }
                        break;
                    }

                case VM.MIN:
                    --p;
                    if (s[p + 1].re < s[p].re || s[p + 1].IsNaN)
                    {
                        s[p].Set(s[p + 1]);
                    }
                    break;

                case VM.MAX:
                    --p;
                    if (s[p].re < s[p + 1].re || s[p + 1].IsNaN)
                    {
                        s[p].Set(s[p + 1]);
                    }
                    break;

                case VM.GCD:
                    //s[--p] = MoreMath.gcd(s[p], s[p+1]); 
                    s[--p].Gcd(s[p + 1]);
                    break;

                case VM.COMB:
                    s[--p].Combinations(s[p + 1]);
                    break;

                case VM.PERM:
                    s[--p].Permutations(s[p + 1]);
                    break;

                case VM.LOAD0:
                case VM.LOAD1:
                case VM.LOAD2:
                case VM.LOAD3:
                case VM.LOAD4:
                    s[++p].Set(s[stackBase + opcode - (VM.LOAD0 - 1)]);
                    break;

                case VM.REAL:
                    s[p].Set(s[p].IsNaN ? Double.NaN : s[p].re, 0);
                    break;

                case VM.IMAG:
                    s[p].Set(s[p].IsNaN ? Double.NaN : s[p].im, 0);
                    break;

                default:
                    throw new Exception("Unknown opcode " + opcode);
            }
        }
        return p;
    }
}
