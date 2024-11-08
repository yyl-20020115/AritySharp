/*
 * Copyright (C) 2008 Mihai Preda.
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

public class Symbol
{
    public static readonly int CONST_ARITY = -3;

    private string name;
    private int arity = 0;

    public byte op;
    public Function function = Function.Empty;
    public double valueRe, valueIm;
    public bool isConst = false;

    public Symbol(string name, int arity, byte op, bool isConst, int dummy)
    {
        SetKey(this.name = name, arity);
        this.op = op;
        this.isConst = isConst;
    }

    public Symbol(string name, Function function)
    {
        SetKey(this.name = name, function.Arity);
        this.function = function;
        // this.comment = fun.comment;
    }

    public Symbol(string name, double re, bool isConst)
        : this(name, re, 0, isConst)
    {
    }

    public Symbol(string name, double re, double im, bool isConst)
    {
        SetKey(this.name = name, CONST_ARITY);
        valueRe = re;
        valueIm = im;
        this.isConst = isConst;
    }

    public static Symbol MakeArg(string name, int order) => new Symbol(name, CONST_ARITY, (byte)(VM.LOAD0 + order), false, 0);

    public static Symbol MakeVmOp(string name, int op) => new Symbol(name, VM.Arity[op], (byte)op, true, 0);

    public override string ToString() => $"Symbol '{name}' arity {arity} val {valueRe} op {op}";

    public string Name => name ?? "";

    /*
    public string GetComment() {
	return comment;
    }
    */

    public int GetArity() => arity == CONST_ARITY ? 0 : arity;

    public static Symbol NewEmpty(Symbol s) => new(s.name ?? "", s.arity, (byte)0, false, 0);

    public bool IsEmpty() => op == 0 
        && function == Function.Empty 
        && valueRe == 0 
        && valueIm == 0;

    public Symbol SetKey(string name, int arity)
    {
        this.name = name;
        this.arity = arity;
        return this;
    }

    public override bool Equals(object? other) => other is Symbol symbol && name == (symbol.name) && arity == symbol.arity;

    public override int GetHashCode() => name.GetHashCode() + arity;
}
