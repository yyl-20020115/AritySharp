/*
 * Copyright (C) 2007-2008 Mihai Preda.
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace AritySharp;

public class DeclarationParser(SyntaxException e) : TokenConsumer
{
    public static readonly string[] NO_ARGS = [];
    public const int UNKNOWN_ARITY = -2;
    public const int MAX_ARITY = 5;

    public string? name;
    public int arity = 0;
    public List<string> args = [];

    private readonly SyntaxException exception = e;

    public override void Start()
    {
        name = null;
        arity = UNKNOWN_ARITY;
        args.Clear();//.setSize(0);
    }

    public override void Push(Token token)
    {
        switch (token.id)
        {
            case Lexer.CALL:
                if (name == null)
                {
                    name = token.name;
                    arity = 0;
                }
                else
                {
                    throw exception.Set("repeated CALL in declaration", token.position);
                }
                break;

            case Lexer.CONST:
                if (name == null)
                {
                    name = token.name;
                    arity = UNKNOWN_ARITY;
                }
                else if (arity >= 0)
                {
                    args.Add(token.name);
                    ++arity;
                    if (arity > MAX_ARITY)
                    {
                        throw exception.Set("Arity too large " + arity, token.position);
                    }
                }
                else
                {
                    throw exception.Set("Invalid declaration", token.position);
                }
                break;

            case Lexer.RPAREN:
            case Lexer.COMMA:
            case Lexer.END:
                break;

            default:
                throw exception.Set("invalid token in declaration", token.position);
        }
    }

    public string[] ArgNames() => arity > 0 ? [.. this.args] : NO_ARGS;
}
