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

public class Lexer(SyntaxException exception)
{
    public const int
        ADD = 1,
        SUB = 2,
        MUL = 3,
        DIV = 4,
        MOD = 5,
        UMIN = 6,
        POWER = 7,
        FACT = 8,
        NUMBER = 9,
        CONST = 10,
        CALL = 11,
        COMMA = 12,
        LPAREN = 13,
        RPAREN = 14,
        END = 15,
        SQRT = 16,
        PERCENT = 17;

    public static readonly Token
        TOK_ADD = new (ADD, 4, Token.LEFT, VM.ADD),
        TOK_SUB = new (SUB, 4, Token.LEFT, VM.SUB),

        TOK_MUL = new (MUL, 5, Token.LEFT, VM.MUL),
        TOK_DIV = new (DIV, 5, Token.LEFT, VM.DIV),
        TOK_MOD = new (MOD, 5, Token.LEFT, VM.MOD),

        TOK_UMIN = new (UMIN, 6, Token.PREFIX, VM.UMIN),

        TOK_POWER = new (POWER, 7, Token.RIGHT, VM.POWER),

        TOK_FACT = new (FACT, 8, Token.SUFIX, VM.FACT),
        TOK_PERCENT = new (PERCENT, 9, Token.SUFIX, VM.PERCENT),

        TOK_SQRT = new (SQRT, 10, Token.PREFIX, VM.SQRT),

        TOK_LPAREN = new (LPAREN, 1, Token.PREFIX, 0),
        TOK_RPAREN = new (RPAREN, 3, 0, 0),
        TOK_COMMA = new (COMMA, 2, 0, 0),
        TOK_END = new (END, 0, 0, 0),

        TOK_NUMBER = new (NUMBER, 20, 0, 0),
        TOK_CONST = new (CONST, 20, 0, 0);

    private const char
        UNICODE_MINUS = '\u2212',
        UNICODE_MUL = '\u00d7',
        UNICODE_DIV = '\u00f7',
        UNICODE_SQRT = '\u221a';

    private const string WHITESPACE = " \n\r\t";
    private const char END_MARKER = '$';
    private char[] input = new char[32];
    private int pos;
    private readonly SyntaxException exception = exception;

    public void Scan(string str, TokenConsumer consumer)
    {
        exception.expression = str;
        if (str.Contains(END_MARKER))
        {
            throw exception.Set($"Invalid character '{END_MARKER}\'", str.IndexOf(END_MARKER));
        }
        Init(str);
        consumer.Start();
        Token token;
        do
        {
            int savePos = pos;
            token = NextToken();
            token.position = savePos;
            consumer.Push(token);
        } while (token != TOK_END);
    }

    private void Init(string str)
    {
        int len = str.Length;
        if (input.Length < len + 1)
        {
            input = new char[len + 1];
        }
        str.ToCharArray().CopyTo(input, 0);
        //str.getChars(0, len, input, 0);
        input[len] = END_MARKER;
        pos = 0;
    }

    public Token NextToken()
    {
        while (WHITESPACE.Contains(input[pos])) ++pos;

        char c = input[pos];
        int begin = pos++;

        switch (c)
        {
            case '!': return TOK_FACT;
            case END_MARKER: return TOK_END;
            case '%': return TOK_PERCENT;
            case '#': return TOK_MOD;
            case '(': return TOK_LPAREN;
            case ')': return TOK_RPAREN;
            case '*': return TOK_MUL;
            case '+': return TOK_ADD;
            case ',': return TOK_COMMA;
            case '-': return TOK_SUB;
            case '/': return TOK_DIV;
        }

        int p = pos;
        if (('0' <= c && c <= '9') || c == '.')
        {
            if (c == '0')
            {
                var cc = char.ToLower(input[p]);
                int _base = (cc == 'x') ? 16 : (cc == 'b') ? 2 : (cc == 'o') ? 8 : 0;
                if (_base > 0)
                {
                    p++;
                    do
                    {
                        c = input[p++];
                    } while (('a' <= c && c <= 'z') ||
                             ('A' <= c && c <= 'Z') ||
                             ('0' <= c && c <= '9'));
                    string coded = new(input, begin + 2, p - 3 - begin);
                    pos = p - 1;
                    try
                    {
                        //Integer.parseInt(coded, _base);
                        var i = Convert.ToInt16(coded, _base);
                        return TOK_NUMBER.SetValue(i);
                    }
                    catch (Exception e)
                    {
                        throw exception.Set($"invalid number '{new string(input, begin, p - 1 - begin)}'", begin);
                    }
                }
            }

            while (('0' <= c && c <= '9') || c == '.' || c == 'E' || c == 'e')
            {
                //accept '-' only after E
                if ((c == 'E' || c == 'e') && (input[p] == '-' || input[p] == UNICODE_MINUS))
                {
                    input[p] = '-'; //replace unicode with plain minus, for double.parseDouble()
                    ++p;
                }
                c = input[p++];
            }
            pos = p - 1;
            var nbStr = new string(input, begin, p - 1 - begin);
            try
            {
                // parse single dot as 0
                if (nbStr == ("."))
                {
                    return TOK_NUMBER.SetValue(0);
                }
                else
                {
                    //double numberValue = double.parseDouble(nbStr);
                    double.TryParse(nbStr, out var numberValue);
                    return TOK_NUMBER.SetValue(numberValue);
                }
            }
            catch (Exception e)
            {
                throw exception.Set("invalid number '" + nbStr + "'", begin);
            }
        }
        else if (('a' <= c && c <= 'z') ||
                   ('A' <= c && c <= 'Z'))
        {
            do
            {
                c = input[p++];
            } while (('a' <= c && c <= 'z') ||
                     ('A' <= c && c <= 'Z') ||
                     ('0' <= c && c <= '9'));
            if (c == '\'')
            {
                c = input[p++];
            }
            var nameValue = new string(input, begin, p - 1 - begin);
            while (WHITESPACE.Contains(c))
            {
                c = input[p++];
            }
            if (c == '(')
            {
                pos = p;
                return (new Token(CALL, 0, Token.PREFIX, 0)).SetAlpha(nameValue);
            }
            else
            {
                pos = p - 1;
                return TOK_CONST.SetAlpha(nameValue);
            }
        }
        else if ((c >= '\u0391' && c <= '\u03a9') || (c >= '\u03b1' && c <= '\u03c9')
                   || c == '\u221e')
        {
            return TOK_CONST.SetAlpha("" + c);
        }
        else
        {
            return c switch
            {
                '^' => TOK_POWER,
                UNICODE_MUL => TOK_MUL,
                UNICODE_DIV => TOK_DIV,
                UNICODE_MINUS => TOK_SUB,
                UNICODE_SQRT => TOK_SQRT,
                _ => throw exception.Set("invalid character '" + c + "'", begin),
            };
        }
    }
}
