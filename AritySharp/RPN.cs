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

/*
 * Reverse Polish Notation
 * reads tokens in normal infix order (e.g.: 1 + 2)
 * and outputs them in Reverse Polish order (e.g.: 1 2 +).
 * See Dijkstra's Shunting Yard algorithm:
 * http://en.wikipedia.org/wiki/Shunting_yard_algorithm
 */
public class RPN(SyntaxException exception) : TokenConsumer
{
    private readonly Stack<Token> stack = new();
    private int prevTokenId = 0;
    private TokenConsumer consumer = TokenConsumer.Default;
    private readonly SyntaxException exception = exception;

    public void SetConsumer(TokenConsumer consumer) => this.consumer = consumer;

    // @Override

    public override void Start()
    {
        stack.Clear();
        prevTokenId = 0;
        consumer.Start();
    }

    private Token Top() => stack.Count == 0 ? Token.Default : stack.Peek();

    private void PopHigher(int priority)
    {
        Token t = Top();
        while (t != Token.Default && t.priority >= priority)
        {
            consumer.Push(t);
            // code.push(t);
            stack.Pop();
            t = Top();
        }
    }

    static bool IsOperand(int id)
        => id is Lexer.FACT or Lexer.RPAREN or Lexer.NUMBER or Lexer.CONST or Lexer.PERCENT;

    public override void Push(Token token)
    {
        int priority = token.priority;
        int id = token.id;
        switch (id)
        {
            case Lexer.NUMBER:
            case Lexer.CONST:
                if (IsOperand(prevTokenId))
                {
                    Push(Lexer.TOK_MUL);
                }
                consumer.Push(token);
                break;

            case Lexer.RPAREN:
                {
                    if (prevTokenId == Lexer.CALL)
                    {
                        Top().arity--;
                    }
                    else if (!IsOperand(prevTokenId))
                    {
                        throw exception.Set("unexpected ) or END", token.position);
                    }

                    PopHigher(priority);
                    Token t = Top();
                    if (t != Token.Default)
                    {
                        if (t.id == Lexer.CALL)
                        {
                            consumer.Push(t);
                        }
                        else if (t != Lexer.TOK_LPAREN)
                        {
                            throw exception.Set("expected LPAREN or CALL", token.position);
                        }
                        stack.Pop();
                    }
                    break;
                }

            case Lexer.COMMA:
                {
                    if (!IsOperand(prevTokenId))
                    {
                        throw exception.Set("misplaced COMMA", token.position);
                    }
                    PopHigher(priority);
                    Token t = Top();
                    if (t == Token.Default || t.id != Lexer.CALL)
                    {
                        throw exception.Set("COMMA not inside CALL", token.position);
                    }
                    t.arity++;
                    // code.push(stack.pop());
                    break;
                }

            case Lexer.END:
                {
                    Token t = Lexer.TOK_RPAREN;
                    t.position = token.position;
                    do
                    {
                        Push(t);
                    } while (Top() != Token.Default);
                    break;
                }

            default: // operators, CALL, LPAREN
                if (token.assoc == Token.PREFIX)
                {
                    if (IsOperand(prevTokenId))
                    {
                        Push(Lexer.TOK_MUL);
                    }
                    stack.Push(token);
                    break;
                }
                if (!IsOperand(prevTokenId))
                {
                    if (id == Lexer.SUB)
                    {
                        // change SUB to unary minus
                        token = Lexer.TOK_UMIN;
                        stack.Push(token);
                        break;
                    }
                    else if (id == Lexer.ADD)
                    {
                        // ignore, keep prevTokenId unchanged
                        return;
                    }
                    throw exception.Set("operator without operand", token.position);
                }
                PopHigher(priority + (token.assoc == Token.RIGHT ? 1 : 0));
                stack.Push(token);
                break;
        }
        prevTokenId = token.id;
    }
}
