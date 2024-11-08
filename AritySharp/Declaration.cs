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

public class Declaration
{
    private static readonly string[] NO_ARGS = [];
    public string? name;
    public string[]? args;
    public int arity = 0;
    public string? expression;

    public void Parse(string source, Lexer lexer, DeclarationParser declParser)
    {
        int equalPos = source.IndexOf('=');
        if (equalPos == -1)
        {
            expression = source;
            name = null;
            args = NO_ARGS;
            arity = DeclarationParser.UNKNOWN_ARITY;
        }
        else
        {
            var decl = source[..equalPos];
            expression = source[(equalPos + 1)..];
            lexer.Scan(decl, declParser);
            name = declParser.name;
            args = declParser.ArgNames();
            arity = declParser.arity;
        }
        /*
        if (arity == DeclarationParser.UNKNOWN_ARITY) {
            args = IMPLICIT_ARGS;
        }
        */
    }
}
