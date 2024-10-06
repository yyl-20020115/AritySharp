/*
 * Copyright (C) 2007-2008 Mihai Preda.
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

/* Non-optimizing Code Generator
   Reads tokens in RPN (Reverse Polish Notation) order,
   and generates VM opcodes,
   without any optimization.
 */

public class SimpleCodeGen(SyntaxException exception) :  TokenConsumer {
    public static readonly SyntaxException HAS_ARGUMENTS = new ();

    public ByteStack code      = new ();
    public DoubleStack consts  = new ();
    public FunctionStack funcs = new ();

    //string argNames[];
    Symbols? symbols;

    SyntaxException exception = exception;

    public SimpleCodeGen SetSymbols(Symbols symbols) {
        this.symbols = symbols;
        return this;
    }

    //@Override
    public override void Start() {
        code.Clear();
        consts.Clear();
        funcs.Clear();
    }

    public Symbol GetSymbol(Token token)  {
        string name = token.name;
        bool isDerivative = token.IsDerivative();
        if (isDerivative) {
            if (token.arity == 1) {
                name = name.Substring(0, name.Length-1);
            } else {
                throw exception.Set("Derivative expects arity 1 but found " + token.arity, token.position);
            }
        }
        Symbol symbol = symbols.Lookup(name, token.arity) ?? throw exception.Set("undefined '" + name + "' with arity " + token.arity, token.position);
        if (isDerivative && symbol.op > 0 && symbol.fun == null) {
            symbol.fun = CompiledFunction.makeOpFunction(symbol.op);
        }
        if (isDerivative && symbol.fun == null) {
            throw exception.Set("Invalid derivative " + name, token.position);
        }
        return symbol;
    }

    public override void Push(Token token)  {
        byte op;
        switch (token.id) {
        case Lexer.NUMBER:
            op = VM.CONST;
            consts.Push(token.value, 0);
            break;
            
        case Lexer.CONST:
        case Lexer.CALL:
            Symbol symbol = GetSymbol(token);
            if (token.IsDerivative()) {
                op = VM.CALL;
                funcs.Push(symbol.fun.Derivative);
            } else if (symbol.op > 0) { // built-in
                op = symbol.op;
                if (op >= VM.LOAD0 && op <= VM.LOAD4) {
                    throw HAS_ARGUMENTS.Set("eval() on implicit function", exception.position);
                }
            } else if (symbol.fun != null) { // function call
                op = VM.CALL;
                funcs.Push(symbol.fun);
            } else { // variable reference
                op = VM.CONST;
                consts.Push(symbol.valueRe, symbol.valueIm);
            }
            break;
                        
        default:
            op = token.vmop;
            if (op <= 0) {
                throw new Exception("wrong vmop: " + op + ", id " + token.id + " in \"" + exception.expression +'"' );
            }
                break; 
        }
        code.Push(op);
    }

    public CompiledFunction GetFun() {
        return new CompiledFunction(0, code.ToArray(), consts.GetRe(), consts.GetIm(), funcs.ToArray());
    }

    /*
    double getValue() {
        return new CompiledFunction(0, code.toArray(), consts.getRe(), consts.getIm(), funcs.toArray()).eval();
    }
    */
}
