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

namespace AritySharp;

/* Optimizing Code Generator
   Reads tokens in RPN (Reverse Polish Notation) order,
   and generates VM opcodes,
   doing constant-folding optimization.
 */

public class OptCodeGenerator  :  SimpleCodeGenerator {
    readonly EvalContext context = new ();
    readonly Complex[] stack;
    int sp;

    readonly double[] traceConstsRe = new double[1];
    readonly double[] traceConstsIm = new double[1];
    readonly Function[] traceFuncs = new Function[1];
    readonly byte[] traceCode = new byte[1];
    readonly CompiledFunction tracer;

    public int intrinsicArity;
    private bool isPercent;
    
    public OptCodeGenerator(SyntaxException e):base(e) {
        this.stack = context.StackComplex;
        this.tracer
        = new CompiledFunction(0, traceCode, traceConstsRe, traceConstsIm, traceFuncs);
    }

    //@Override
    public override void Start() {
        base.Start();
        this.sp = -1;
        this.intrinsicArity = 0;
        this.isPercent = false;
    }

    //@Override
    public override void Push(Token token) {
        // System.err.println("state " + getFun(0) + "; token " + token);
        var prevWasPercent = isPercent;
        this.isPercent = false;
        byte op;
        switch (token.id) {
        case Lexer.NUMBER:
            op = VM.CONST;
            traceConstsRe[0] = token.value;
            traceConstsIm[0] = 0;
            break;
        case Lexer.CONST:
        case Lexer.CALL:
            Symbol symbol = GetSymbol(token);
            if (token.IsDerivative()) {
                op = VM.CALL;
                traceFuncs[0] = symbol.function.Derivative;
            } else if (symbol.op > 0) { // built-in
                op = symbol.op;
                if (op >= VM.LOAD0 && op <= VM.LOAD4) {
                    int arg = op - VM.LOAD0;
                    if (arg + 1 > intrinsicArity) {
                        this.intrinsicArity = arg + 1;
                    }
                    stack[++sp].Real = double.NaN;
                    stack[sp].Imaginary = 0;
                    code.Push(op);
                    //System.out.println("op " + VM.opcodeName[op] + "; sp " + sp + "; top " + stack[sp]);
                    return;
                }
            } else if (symbol.function != Function.Empty) { // function call
                op = VM.CALL;
                traceFuncs[0] = symbol.function;
            } else { // variable reference
                op = VM.CONST;
                traceConstsRe[0] = symbol.valueRe;
                traceConstsIm[0] = symbol.valueIm;
            }
            break;
                        
        default:
            op = token.vmop;
            if (op <= 0) {
                throw new Exception($"wrong vmop: {op}");
            }
            if (op == VM.PERCENT) {
                this.isPercent = true;
            }
                break;
        }
        //int oldSP = sp;
        traceCode[0] = op;
        if (op != VM.RND) {
            this.sp = tracer.ExecWithoutCheckComplex(context, sp, prevWasPercent ? -1 : -2);
        } else {
            stack[++sp].Real = double.NaN;
            stack[sp].Imaginary = 0;
        }
        //constant folding
        if (!stack[sp].IsNaN || op == VM.CONST) {
            int nPopCode = op==VM.CALL ? traceFuncs[0].Arity : VM.Arity[op];
            while (nPopCode > 0) {
                 byte pop = code.Pop();
                if (pop == VM.CONST) {
                    consts.Pop();
                } else if (pop == VM.CALL) {
                    Function f = functions.Pop();
                    nPopCode += f.Arity - 1;
                } else {
                    nPopCode += VM.Arity[pop];
                }
                --nPopCode;
            }
            consts.Push(stack[sp].Real, stack[sp].Imaginary);
            op = VM.CONST;
        } else if (op == VM.CALL) {
            functions.Push(traceFuncs[0]);
        }
        code.Push(op);
    }

    public CompiledFunction GetFunction(int arity) 
        => new(arity, code.ToArray(), consts.Reals, consts.Imaginaries, functions.ToArray());
}
