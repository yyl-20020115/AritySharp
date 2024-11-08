/*
 * Copyright (C) 2008 Mihai Preda.
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

/**
 * Represents the collection of names (<em>symbols</em>) used for parsing an expression
 * (the context in which the parsing takes place).
 * <p>
 * Each symbol maps to either a {@link Function} or a constant.
 * <p>
 * A symbol is identified by the pair (name, arity).
 * So a constant and a function with the same name,
 * or two function with the same name but with different arity
 * are distinct symbols.
 * <p>
 * Symbols functions as a stack of <em>frames</em>:
 * when you pop the topmost frame, all the symbols added in that frame dissapear
 * (i.e. all the modifications done between the push and the pop are undone).
 * <p>
 * Example:
 * 
 * <pre>
 <code>
 Symbols symbols = new Symbols();
 symbols.eval("1+1"); //doesn't update symbols
 symbols.define(symbols.compileWithName("f(x)=x^2")); //updates symbols
 </code>
 * </pre>
 * <p>
 * <h3>Thread safety</h3>
 * The Symbols class is thread-safe
 * (the same Symbols instance can be used by multiple threads without locking).
 * <p>
 * The compile() methods are synchronized, so parallel compile() calls on the same Symbols
 * instance are serialized.
 */

public class Symbols
{
    private readonly static Symbol[] builtin;
    private static readonly Symbol shell = new ("", 0, false);
    private readonly Compiler compiler = new ();
    private readonly Dictionary<Symbol, Symbol> symbols = []; // Hashtable<Symbol, Symbol>
    private HashSet<Symbol>? delta = null;
    private readonly Stack<HashSet<Symbol>> frames = new ();

    static Symbols()
    {
        List<Symbol> vect = [];
        foreach (byte i in VM.Builtins)
        {
            vect.Add(Symbol.MakeVmOp(VM.OpcodeName[i], i));
        }

        string[] IMPLICIT_ARGS = ["x", "y", "z"];
        for (byte i = 0; i < IMPLICIT_ARGS.Length; ++i)
        {
            vect.Add(Symbol.MakeArg(IMPLICIT_ARGS[i], i));
        }

        vect.Add(new Symbol("pi", Math.PI, true));
        vect.Add(new Symbol("\u03c0", Math.PI, true));
        vect.Add(new Symbol("e", Math.E, true));

        double infinity = double.PositiveInfinity;
        vect.Add(new Symbol("Infinity", infinity, true));
        vect.Add(new Symbol("infinity", infinity, true));
        vect.Add(new Symbol("Inf", infinity, true));
        vect.Add(new Symbol("inf", infinity, true));
        vect.Add(new Symbol("\u221e", infinity, true));
        vect.Add(new Symbol("NaN", double.NaN, true));
        vect.Add(new Symbol("nan", double.NaN, true));

        vect.Add(new Symbol("i", 0, 1, true));
        vect.Add(new Symbol("j", 0, 1, false));

        int size = vect.Count;
        builtin = new Symbol[size];
        vect.CopyTo(builtin);
    }

    /**
     * Constructs a Symbols containing the built-in symbols (such as sin, log).
     */
    public Symbols()
    {
        for (int i = 0; i < builtin.Length; ++i)
        {
            Add(builtin[i]);
            /*
             * Symbol s = builtin[i];
             * symbols.put(s, s);
             */
        }
        try
        {
            for (int i = 0; i < Defines.Length; ++i)
            {
                Define(CompileWithName(Defines[i]));
            }
        }
        catch (SyntaxException e)
        {
            throw new Exception("", e); // never
        }
    }

    /**
     * @param source
     *          the expression
     * @return true if the expression is a definition (i.e. contains a '=').
     *         <p>
     *         These are definitions: "a=1+1"; "f(k)=2^k"
     *         <p>
     *         These are not definitions: "1+1"; "x+1"
     */
    public static bool IsDefinition(string source) => source.Contains('=');

    /**
     * Evaluates a simple expression (such as "1+1") and returns its value.
     * 
     * @throws SyntaxException
     *           in these cases:
     *           <ul>
     *           <li>the expression is not well-formed
     *           <li>the expression is a definition (such as "a=1+1")
     *           <li>the expression is an implicit function (such as "x+1")
     *           </ul>
     */
    public double Eval(string expression)
    {
        lock (this)
            return compiler.CompileSimple(this, expression).Eval();
    }

    public Complex EvalComplex(string expression)
    {
        lock (this)
            return compiler.CompileSimple(this, expression).EvalComplex();
    }

    /**
     * Compiles an expression in the context of this Symbols.
     * Does not modify the symbols.
     * <p>
     * An expression is one of these cases (@see Symbols.isDefinition()):
     * <ul>
     * <li>constant value: 1+1
     * <li>implicit function: x+1
     * <li>constant definition: a=1+1
     * <li>function definition with explicit arguments: f(a)=a+1
     * <li>function definition with implicit arguments: f=x+1
     * </ul>
     * <p>
     * 
     * @param source
     *          the expression; may contain '=' to denote a definition (with a name).
     * @return the function together with its eventual name.
     *         <p>
     *         If this is not a definition (e.g. "1+1", "x^2"), the name is null.
     *         <p>
     *         If the expression is a constant (e.g. "1+1", "a=2"),
     *         the returned Function is an instance of {@link Constant}.
     * @throws SyntaxException
     *           if there are errors compiling the expression.
     */
    public FunctionAndName CompileWithName(string source)
    {
        lock (this)
            return compiler.CompileWithName(this, source);
    }

    public Function Compile(string source)
    {
        lock (this)
            return compiler.Compile(this, source);
    }

    /**
     * Adds a new function symbol to the top-most frame of this Symbols.
     * 
     * @param name
     *          the name of the function (e.g. "sin")
     * @param function
     *          the function to which the name maps
     */
    public void Define(string name, Function function)
    {
        lock (this)
            if (function is Constant)
            {
                Define(name, function.Eval());
            }
            else
            {
                Add(new Symbol(name, function));
            }
    }

    /**
     * Adds a new function symbol to the top-most frame of this Symbols.
     * 
     * @param funAndName
     *          structure containing the function and its name
     */
    public void Define(FunctionAndName funAndName)
    {
        lock (this)
            if (funAndName.name != null)
                Define(funAndName.name, funAndName.function);
    }

    /**
     * Adds a new constant symbol to the top-most frame of this Symbols.
     * 
     * @param name
     *          the name of the constant (e.g. "pi")
     * @param value
     *          the value of the constant
     */
    public void Define(string name, double value)
    {
        lock (this)
            Add(new Symbol(name, value, 0, false));
    }

    public void Define(string name, Complex value)
    {
        lock (this)
            Add(new Symbol(name, value.re, value.im, false));
    }

    /**
     * Pushes a new top frame.
     * <p>
     * All modifications (defining new symbols) happen in the top-most frame.
     * When the frame is pop-ed the modifications that happened in it are reverted.
     */
    public void PushFrame()
    {
        lock (this)
        {
            frames.Push(delta ?? []);
            delta = null;
        }
    }

    /**
     * Pops the top frame.
     * <p>
     * All the modifications done since this frame was pushed are reverted.
     * 
     * @throws EmptyStackException
     *           if there were fewer <code>pushFrame</code> than <code>popFrame</code>.
     */
    public void PopFrame()
    {
        lock (this)
        {
            if (delta != null)
            {
                foreach (Symbol previous in delta)
                {
                    if (previous.IsEmpty())
                        symbols.Remove(previous);
                    else
                        symbols[previous]= previous;
                }
            }
            delta = frames.Pop();
        }
    }

    /**
     * Returns all the symbols that were added in the top frame.
     * (i.e. since the most recent pushFrame()).
     */
    public Symbol[] GetTopFrame() => delta == null ? [] : [.. delta];

    /**
     * Return all the defined symbols.
     */
    public Symbol[] GetAllSymbols()
    {
        int size = symbols.Count;
        var ret = new Symbol[size];
        symbols.Keys.CopyTo(ret, 0);
        return ret;
    }

    /**
     * Return all the strings that are defined in this symbols.
     */
    public string[] GetDictionary()
    {
        var syms = GetAllSymbols();
        int size = syms.Length;
        var strings = new string[size];
        for (int i = 0; i < size; ++i)
            strings[i] = syms[i].Name;
        return strings;
    }

    // --- non-public below

    private static readonly string[] Defines = [
        "log(x)=ln(x)*0.43429448190325182765", // log10(e)
        "log10(x)=log(x)", "lg(x)=log(x)",
        "log2(x)=ln(x)*1.4426950408889634074", // log2(e)
        "lb(x)=log2(x)",
        "log(base,x)=ln(x)/ln(base)", "gamma(x)=(x-1)!",
        "deg=0.017453292519943295", // PI/180
        "indeg=57.29577951308232", // 180/PI
        "sind(x)=sin(x deg)", "cosd(x)=cos(x deg)", "tand(x)=tan(x deg)",
        "asind(x)=asin(x) indeg", "acosd(x)=acos(x) indeg", "atand(x)=atan(x) indeg",
        "tg(x)=tan(x)", "tgd(x)=tand(x)", "rnd(max)=rnd()*max",
        "re(x)=real(x)", "im(x)=imag(x)", ];

    public void AddArguments(string[] args)
    {
        for (int i = 0; i < args.Length; ++i)
        {
            Add(Symbol.MakeArg(args[i], i));
        }
    }

    public void Add(Symbol s)
    {
        symbols.TryGetValue(s, out var previous);

        symbols[s] = s;
        if (previous != null && previous.isConst)
        {
            symbols.Add(previous, previous);
            return;
        }
        delta ??= [];
        if (!delta.Contains(s))
        {
            delta.Add(previous ?? Symbol.NewEmpty(s));
        }
    }

    public Symbol? Lookup(string name, int arity)
    {
        lock (this)
            return symbols.TryGetValue(shell.SetKey(name, arity), out var r) ? r : null;
    }

    public Symbol? LookupConst(string name) => Lookup(name, Symbol.CONST_ARITY);
}
