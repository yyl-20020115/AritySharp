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

public class CompiledFunction(int arity, byte[] code, double[] constsRe, double[] constsIm, Function[] functions) : ContextFunction
{
    private static readonly IsComplexException IS_COMPLEX = new();
    private static readonly double[] EMPTY_DOUBLES = [];
    private static readonly Function[] EMPTY_FUNCTIONS = [];
    private static readonly Complex ONE_THIRD = new(1 / 3.0, 0);
    private static readonly Random random = new();

    private readonly double[] constsRe = constsRe;
    private readonly double[] constsIm = constsIm;
    private readonly Function[] functions = functions;
    private readonly byte[] code = code;
    private readonly int arity = arity; // >= 0

    private class CompareFunction : Function
    {
        public override int Arity => 1;
        public override double Eval(double x) => x > 0 ? 1 : x < 0 ? -1 : 0;
    }
    public static Function MakeOpFunction(int op)
    {
        if (VM.Arity[op] != 1)
            throw new Exception($"makeOpFunction expects arity 1, found {VM.Arity[op]}");
        var function = new CompiledFunction(VM.Arity[op], [VM.LOAD0, (byte)op], EMPTY_DOUBLES, EMPTY_DOUBLES, EMPTY_FUNCTIONS);
        if (op == VM.ABS)
        {
            function.Derivative = new CompareFunction();
        }
        return function;
    }

    public override int Arity => arity;

    public override string ToString()
    {
        var builder = new StringBuilder();
        int cpos = 0, fpos = 0;
        if (arity != 0)
        {
            builder.Append($"arity {arity};");
        }
        for (int i = 0; i < code.Length; ++i)
        {
            byte op = code[i];
            builder.Append(VM.OpcodeName[op]);
            if (op == VM.CONST)
            {
                builder.Append(constsIm.Length == 0 ? $" {constsRe[cpos]}" : " ({constsRe[cpos]}, {constsIm[cpos]})");
                ++cpos;
            }
            else if (op == VM.CALL)
            {
                ++fpos;
                //buf.append(" {").append(funcs[fpos++].toString()).append('}');
            }
            builder.Append("; ");
        }
        if (cpos != constsRe.Length)
        {
            builder.Append($"\nuses only {cpos} consts out of {constsRe.Length}");
        }
        if (fpos != functions.Length)
        {
            builder.Append($"\nuses only {fpos} funcs out of {functions.Length}");
        }
        return builder.ToString();
    }

    public override double Eval(double[] args, EvalContext context)
    {
        if (constsIm.Length > 0) return EvalComplexToReal(args, context);
        CheckArity(args.Length);
        Array.Copy(args, 0, context.StackRe, context.StackBase, args.Length);
        try
        {
            ExecReal(context, context.StackBase + args.Length - 1);
            return context.StackRe[context.StackBase];
        }
        catch (IsComplexException)
        {
            return EvalComplexToReal(args, context);
        }
    }

    private double EvalComplexToReal(double[] args, EvalContext context)
    {
        var argsC = ToComplex(args, context);
        var c = Eval(argsC, context);
        return c.AsReal;
    }

    public override Complex Eval(Complex[] args, EvalContext context)
    {
        CheckArity(args.Length);
        var stack = context.StackComplex;
        var _base = context.StackBase;
        for (int i = 0; i < args.Length; ++i)
        {
            stack[i + _base].Set(args[i]);
        }
        ExecComplex(context, _base + args.Length - 1);
        return stack[_base];
    }

    private int ExecReal(EvalContext context, int p)
    {
        int expected = p + 1;
        p = ExecWithoutCheck(context, p);
        if (p != expected)
            throw new Exception($"Stack pointer after exec: expected {expected}, got {p}");
        context.StackRe[p - arity] = context.StackRe[p];
        return p - arity;
    }

    private int ExecComplex(EvalContext context, int p)
    {
        int expected = p + 1;
        p = ExecWithoutCheckComplex(context, p, -2);
        if (p != expected)
            throw new Exception($"Stack pointer after exec: expected {expected}, got {p}");
        context.StackComplex[p - arity].Set(context.StackComplex[p]);
        return p - arity;
    }

    public int ExecWithoutCheck(EvalContext context, int p)
    {
        if (constsIm.Length > 0) throw IS_COMPLEX;
        var s = context.StackRe;
        int stackBase = p - arity;
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
                        Function function = functions[funp++];
                        if (function is CompiledFunction compiled_function)
                        {
                            p = compiled_function.ExecReal(context, p);
                        }
                        else
                        {
                            var arity = function.Arity;
                            p -= arity;
                            var result = 0.0;
                            var prevBase = context.StackBase;
                            try
                            {
                                context.StackBase = p + 1;
                                switch (arity)
                                {
                                    case 0:
                                        result = function.Eval();
                                        break;
                                    case 1:
                                        result = function.Eval(s[p + 1]);
                                        break;
                                    case 2:
                                        result = function.Eval(s[p + 1], s[p + 2]);
                                        break;
                                    default:
                                        var args = new double[arity];
                                        Array.Copy(s, p + 1, args, 0, arity);
                                        result = function.Eval(args);
                                        break;
                                }
                            }
                            finally
                            {
                                context.StackBase = prevBase;
                            }
                            s[++p] = result;
                        }
                        break;
                    }

                case VM.RND: 
                    s[++p] = random.NextDouble(); 
                    break;

                case VM.ADD:
                    {
                        var a = s[--p];
                        var res = a + (percentPC == pc - 1 ? s[p] * s[p + 1] : s[p + 1]);
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
                        var a = s[--p];
                        var res = a - (percentPC == pc - 1 ? s[p] * s[p + 1] : s[p + 1]);
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
                case VM.POWER: s[--p] = Math.Pow(s[p], s[p + 1]); break;
                case VM.UMIN: s[p] = -s[p]; break;
                case VM.FACT: s[p] = MoreMath.Factorial(s[p]); break;
                case VM.PERCENT: s[p] = s[p] * .01; percentPC = pc; break;
                case VM.SIN: s[p] = MoreMath.Sin(s[p]); break;
                case VM.COS: s[p] = MoreMath.Cos(s[p]); break;
                case VM.TAN: s[p] = MoreMath.Tan(s[p]); break;

                case VM.ASIN:
                    {
                        var v = s[p];
                        if (v < -1 || v > 1)
                        {
                            throw IS_COMPLEX;
                        }
                        s[p] = Math.Asin(v);
                        break;
                    }

                case VM.ACOS:
                    {
                        var v = s[p];
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
                        var v = s[p];
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
                        var v = s[p];
                        s[p] = v > 0 ? 1 : v < 0 ? -1 : v == 0 ? 0 : double.NaN;
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
                    if (!double.IsNaN(s[p]))
                    {
                        s[p] = 0;
                    }
                    break;

                default:
                    throw new Exception($"Unknown opcode {opcode}");
            }
        }
        return p;
    }

    public int ExecWithoutCheckComplex(EvalContext context, int p, int percentPC)
    {
        var s = context.StackComplex;
        int stackBase = p - arity;
        int constp = 0;
        int funp = 0;
        int codeLen = code.Length;
        for (int pc = 0; pc < codeLen; ++pc)
        {
            int opcode = code[pc];
            switch (opcode)
            {
                case VM.CONST:
                    ++p;
                    s[p].Set(constsRe[constp], constsIm.Length == 0 ? 0 : constsIm[constp]);
                    ++constp;
                    break;

                case VM.CALL:
                    {
                        var function = functions[funp++];
                        if (function is CompiledFunction compiled_function)
                        {
                            p = compiled_function.ExecComplex(context, p);
                        }
                        else
                        {
                            var arity = function.Arity;
                            p -= arity;
                            Complex result;
                            var prevBase = context.StackBase;
                            try
                            {
                                context.StackBase = p + 1;
                                switch (arity)
                                {
                                    case 0:
                                        result = new Complex(function.Eval(), 0);
                                        break;
                                    case 1:
                                        result = function.Eval(s[p + 1]);
                                        break;
                                    case 2:
                                        result = function.Eval(s[p + 1], s[p + 2]);
                                        break;
                                    default:
                                        var args = new Complex[arity];
                                        Array.Copy(s, p + 1, args, 0, arity);
                                        result = function.Eval(args);
                                        break;
                                }
                            }
                            finally
                            {
                                context.StackBase = prevBase;
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
                    if (s[p].Imaginary == 0)
                    {
                        s[p].Real = Math.Cbrt(s[p].Real);
                    }
                    else
                    {
                        s[p].Pow(ONE_THIRD);
                    }
                    break;

                case VM.ABS: s[p].Set(s[p].Abs(), 0); break;
                case VM.FLOOR: s[p].Set(Math.Floor(s[p].Real), 0); break;
                case VM.CEIL: s[p].Set(Math.Ceiling(s[p].Real), 0); break;
                case VM.SIGN:
                    {
                        var a = s[p].Real;
                        var b = s[p].Imaginary;
                        if (b == 0)
                        {
                            s[p].Set(a > 0 ? 1 : a < 0 ? -1 : a == 0 ? 0 : double.NaN, 0);
                        }
                        else if (!s[p].IsNaN)
                        {
                            var abs = s[p].Abs();
                            s[p].Set(a / abs, b / abs);
                        }
                        else
                        {
                            s[p].Set(double.NaN, 0);
                        }
                        break;
                    }

                case VM.MIN:
                    --p;
                    if (s[p + 1].Real < s[p].Real || s[p + 1].IsNaN)
                    {
                        s[p].Set(s[p + 1]);
                    }
                    break;

                case VM.MAX:
                    --p;
                    if (s[p].Real < s[p + 1].Real || s[p + 1].IsNaN)
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
                    s[p].Set(s[p].IsNaN ? double.NaN : s[p].Real, 0);
                    break;

                case VM.IMAG:
                    s[p].Set(s[p].IsNaN ? double.NaN : s[p].Imaginary, 0);
                    break;

                default:
                    throw new Exception($"Unknown opcode {opcode}");
            }
        }
        return p;
    }
}
