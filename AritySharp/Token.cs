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

public class Token(int id, int priority, int assoc, int vmop)
{
    // kind
    public const int PREFIX = 1, LEFT = 2, RIGHT = 3, SUFIX = 4;

    public readonly int priority = priority;
    public readonly int assoc = assoc;
    public readonly int id = id;
    public readonly byte vmop = (byte)vmop;

    public double value; // for NUMBER only
    public string? name = null; // for CONST & CALL
    public int arity = id == Lexer.CALL ? 1 : Symbol.CONST_ARITY;
    public int position; // pos inside expression

    public Token SetPos(int pos)
    {
        this.position = pos;
        return this;
    }

    public Token SetValue(double value)
    {
        this.value = value;
        return this;
    }

    public Token SetAlpha(string alpha)
    {
        name = alpha;
        return this;
    }

    public bool IsDerivative() => name != null && (name.Length) > 0 && name[(name.Length - 1)] == '\'';

    public override string ToString() => id switch
    {
        Lexer.NUMBER => "" + value,
        Lexer.CALL => name + '(' + arity + ')',
        Lexer.CONST => name,
        _ => "" + id,
    }??"";
}
