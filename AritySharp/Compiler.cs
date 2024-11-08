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

/**
   Compiles a textual arithmetic expression to a {@link Function}.<p>
*/
public class Compiler
{
    private readonly SyntaxException exception;
    private readonly Lexer lexer;
    private readonly RPN rpn;
    private readonly DeclarationParser declParser;
    private readonly OptCodeGen codeGen;
    private readonly SimpleCodeGenerator simpleCodeGen;
    private readonly Declaration decl;
    public Compiler()
    {
        this.exception = new();
        this.lexer = new(exception);
        this.rpn = new(exception);
        this.declParser = new(exception);
        this.codeGen = new(exception);
        this.simpleCodeGen = new(exception);
        this.decl = new();

    }
    public Function CompileSimple(Symbols symbols, string expression)
    {
        this.rpn.SetConsumer(simpleCodeGen.SetSymbols(symbols));
        this.lexer.Scan(expression, rpn);
        return this.simpleCodeGen.GetFunction();
    }

    public Function Compile(Symbols symbols, string source)
    {
        Function? fun = null;
        decl.Parse(source, lexer, declParser);
        if (decl.arity == DeclarationParser.UNKNOWN_ARITY)
        {
            try
            {
                fun = new Constant(CompileSimple(symbols, decl.expression).EvalComplex());
            }
            catch (SyntaxException e)
            {
                if (e != null && e != SimpleCodeGenerator.HAS_ARGUMENTS)
                {
                    throw;
                }
                // fall-through (see below)
            }
        }

        if (fun == null && decl != null)
        {
            // either decl.arity was set, or an HAS_ARGUMENTS exception ocurred above
            symbols.PushFrame();
            symbols.AddArguments(decl.args ?? []);
            try
            {
                rpn.SetConsumer(codeGen.SetSymbols(symbols));
                lexer.Scan(decl.expression, rpn);
            }
            finally
            {
                symbols.PopFrame();
            }
            int arity = decl.arity;
            if (arity == DeclarationParser.UNKNOWN_ARITY)
            {
                arity = codeGen.intrinsicArity;
            }
            fun = codeGen.GetFun(arity);
        }
        if(fun!=null)
            fun.comment = source;
        return fun ?? Function.Default;
    }

    public FunctionAndName CompileWithName(Symbols symbols, string source)
        => new(Compile(symbols, source), decl?.name ?? "");
}
