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
    private readonly OptCodeGenerator codeGenerator;
    private readonly SimpleCodeGenerator simpleCodeGenerator;
    private readonly Declaration declaration;
    public Compiler()
    {
        this.exception = new();
        this.lexer = new(exception);
        this.rpn = new(exception);
        this.declParser = new(exception);
        this.codeGenerator = new(exception);
        this.simpleCodeGenerator = new(exception);
        this.declaration = new();

    }
    public Function CompileSimple(Symbols symbols, string expression)
    {
        this.rpn.SetConsumer(simpleCodeGenerator.SetSymbols(symbols));
        this.lexer.Scan(expression, rpn);
        return this.simpleCodeGenerator.GetFunction();
    }

    public Function Compile(Symbols symbols, string source)
    {
        Function? function = null;
        declaration.Parse(source, lexer, declParser);
        if (declaration.arity == DeclarationParser.UNKNOWN_ARITY)
        {
            try
            {
                function = new Constant(CompileSimple(symbols, declaration.expression).EvalComplex());
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

        if (function == null && declaration != null)
        {
            // either decl.arity was set, or an HAS_ARGUMENTS exception ocurred above
            symbols.PushFrame();
            symbols.AddArguments(declaration.args ?? []);
            try
            {
                rpn.SetConsumer(codeGenerator.SetSymbols(symbols));
                lexer.Scan(declaration.expression, rpn);
            }
            finally
            {
                symbols.PopFrame();
            }
            int arity = declaration.arity;
            if (arity == DeclarationParser.UNKNOWN_ARITY)
            {
                arity = codeGenerator.intrinsicArity;
            }
            function = codeGenerator.GetFunction(arity);
        }
        if(function!=null)
            function.Comment = source;
        return function ?? Function.Empty;
    }

    public FunctionAndName CompileWithName(Symbols symbols, string source)
        => new(Compile(symbols, source), declaration.name ?? "");
}
