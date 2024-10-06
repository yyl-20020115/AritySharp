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

public class OptCodeGen  :  SimpleCodeGen {
    readonly EvalContext context = new EvalContext();
    int sp;
    readonly Complex[] stack;

    readonly double[] traceConstsRe = new double[1];
    readonly double[] traceConstsIm = new double[1];
    readonly Function[] traceFuncs = new Function[1];
    readonly byte[] traceCode = new byte[1];
    readonly CompiledFunction tracer;

    public int intrinsicArity;
    private bool isPercent;
    
    public OptCodeGen(SyntaxException e):base(e) {
        stack = context.stackComplex;
        tracer
        = new CompiledFunction(0, traceCode, traceConstsRe, traceConstsIm, traceFuncs);
    }

    //@Override
    public override void Start() {
        base.Start();
        sp = -1;
        intrinsicArity = 0;
        isPercent = false;
    }

    //@Override
    public override void Push(Token token) {
        // System.err.println("state " + getFun(0) + "; token " + token);
        bool prevWasPercent = isPercent;
        isPercent = false;
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
                traceFuncs[0] = symbol.fun.Derivative;
            } else if (symbol.op > 0) { // built-in
                op = symbol.op;
                if (op >= VM.LOAD0 && op <= VM.LOAD4) {
                    int arg = op - VM.LOAD0;
                    if (arg + 1 > intrinsicArity) {
                        intrinsicArity = arg + 1;
                    }
                    stack[++sp].re = Double.NaN;
                    stack[sp].im = 0;
                    code.Push(op);
                    //System.out.println("op " + VM.opcodeName[op] + "; sp " + sp + "; top " + stack[sp]);
                    return;
                }
            } else if (symbol.fun != null) { // function call
                op = VM.CALL;
                traceFuncs[0] = symbol.fun;
            } else { // variable reference
                op = VM.CONST;
                traceConstsRe[0] = symbol.valueRe;
                traceConstsIm[0] = symbol.valueIm;
            }
            break;
                        
        default:
            op = token.vmop;
            if (op <= 0) {
                throw new Exception("wrong vmop: " + op);
            }
            if (op == VM.PERCENT) {
                isPercent = true;
            }
                break;
        }
        int oldSP = sp;
        traceCode[0] = op;
        if (op != VM.RND) {
            sp = tracer.ExecWithoutCheckComplex(context, sp, prevWasPercent ? -1 : -2);
        } else {
            stack[++sp].re = Double.NaN;
            stack[sp].im = 0;
        }

        //System.out.println("op " + VM.opcodeName[op] + "; old " + oldSP + "; sp " + sp + "; top " + stack[sp] + " " + stack[0]);
            
        //constant folding
        if (!stack[sp].IsNaN || op == VM.CONST) {
            int nPopCode = op==VM.CALL ? traceFuncs[0].GetArity() : VM.arity[op];
            while (nPopCode > 0) {
                 byte pop = code.Pop();
                if (pop == VM.CONST) {
                    consts.Pop();
                } else if (pop == VM.CALL) {
                    Function f = funcs.Pop();
                    nPopCode += f.GetArity() - 1;
                } else {
                    nPopCode += VM.arity[pop];
                }
                --nPopCode;
            }
            consts.Push(stack[sp].re, stack[sp].im);
            op = VM.CONST;
        } else if (op == VM.CALL) {
            funcs.Push(traceFuncs[0]);
        }
        code.Push(op);
    }

    public CompiledFunction GetFun(int arity) {
        return new CompiledFunction(arity, code.ToArray(), consts.GetRe(), consts.GetIm(), funcs.ToArray());
    }
}
