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

/**
   Base class for functions.<p>
   A function has an arity (the number of arguments), and a way for evaluation
   given the values of the arguments.<p>
   To create user-defined functions, 
   derive from this class and override one of the eval() methods.<p>

   If the user subclasses Function, he is responsible for the thread-safety of 
   his user-defined Functions.
 */
public abstract class Function
{
    public class EmptyFunction : Function
    {
        public override int Arity => 0;
    }
    public static readonly Function Default = new EmptyFunction();

    private Function? cachedDerivate;
    public string? comment;

    /**
       Gives the arity of this function. 
       @return the arity (the number of arguments). Arity >= 0.
    */
    public abstract int Arity { get; }
    public Function Derivative
    {
        get => cachedDerivate ??= new Derivative(this);
        set => cachedDerivate = value;
    }

    /**
       Evaluates an arity-0 function (a function with no arguments).
       @return the value of the function
    */
    public virtual double Eval() => throw new ArityException(0);

    /**
       Evaluates a function with a single argument (arity == 1).
     */
    public virtual double Eval(double x) => throw new ArityException(1);

    /**
       Evaluates a function with two arguments (arity == 2).
    */
    public virtual double Eval(double x, double y) => throw new ArityException(2);

    /**
       Evaluates the function given the argument values.
       @param args array containing the arguments.
       @return the value of the function
       @throws ArityException if args.Length != arity.
    */
    public virtual double Eval(double[] args) => args.Length switch
    {
        0 => Eval(),
        1 => Eval(args[0]),
        2 => Eval(args[0], args[1]),
        _ => throw new ArityException(args.Length),
    };


    /** By default complex forwards to real eval is the arguments are real,        
     *  otherwise returns NaN.
     *  This allow calling any real functions as a (restricted) complex one.
     */
    public virtual Complex EvalComplex()
    {
        CheckArity(0);
        return new Complex(Eval(), 0);
    }

    /**
       Complex evaluates a function with a single argument.
    */
    public virtual Complex Eval(Complex x)
    {
        CheckArity(1);
        return new Complex(x.im == 0 ? Eval(x.re) : double.NaN, 0);
    }

    /**
       Complex evaluates a function with two arguments.
     */
    public virtual Complex Eval(Complex x, Complex y)
    {
        CheckArity(2);
        return new Complex(x.im == 0 && y.im == 0 ? Eval(x.re, y.re) : double.NaN, 0);
    }

    /**
       Complex evaluates a function with an arbitrary number of arguments.
    */
    public virtual Complex Eval(Complex[] args)
    {
        switch (args.Length)
        {
            case 0:
                return EvalComplex();
            case 1:
                return Eval(args[0]);
            case 2:
                return Eval(args[0], args[1]);
            default:
                int len = args.Length;
                CheckArity(len);
                double[] reArgs = new double[len];
                for (int i = args.Length - 1; i >= 0; --i)
                {
                    if (args[i].im != 0)
                    {
                        return new Complex(double.NaN, 0);
                    }
                    reArgs[i] = args[i].re;
                }
                return new Complex(Eval(reArgs), 0);
        }
    }

    public virtual void CheckArity(int nArgs)
    {
        if (Arity != nArgs)
            throw new ArityException($"Expected {Arity} arguments, got {nArgs}");
    }
}
