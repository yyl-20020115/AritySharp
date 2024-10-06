namespace AritySharp;

public abstract class ContextFunction : Function
{
    private static EvalContext context = new();
    private static readonly double[] NO_ARGS = [];
    private static readonly Complex[] NO_ARGS_COMPLEX = [];

    public abstract double Eval(double[] args, EvalContext context);

    public abstract Complex Eval(Complex[] args, EvalContext context);

    public static Complex[] ToComplex(double[] args, EvalContext context)
    {
        Complex[] argsC;
        switch (args.Length)
        {
            case 0:
                argsC = NO_ARGS_COMPLEX;
                break;
            case 1:
                argsC = context.args1c;
                argsC[0].Set(args[0], 0);
                break;
            case 2:
                argsC = context.args2c;
                argsC[0].Set(args[0], 0);
                argsC[1].Set(args[1], 0);
                break;
            default:
                argsC = new Complex[args.Length];
                for (int i = 0; i < args.Length; ++i)
                {
                    argsC[i] = new Complex(args[i], 0);
                }
                break;
        }
        return argsC;
    }

    public override double Eval() => Eval(NO_ARGS);

    public override double Eval(double x)
    {
        lock (context)
        {
            return Eval(x, context);
        }
    }


    public override double Eval(double x, double y)
    {
        lock (context)
        {
            return Eval(x, y, context);
        }
    }

    public override double Eval(double[] args)
    {
        lock (context)
        {
            return Eval(args, context);
        }
    }

    public double Eval(double x, EvalContext context)
    {
        var args = context.args1;
        args[0] = x;
        return Eval(args, context);
    }

    public double Eval(double x, double y, EvalContext context)
    {
        var args = context.args2;
        args[0] = x;
        args[1] = y;
        return Eval(args, context);
    }

    public override Complex EvalComplex() => Eval(NO_ARGS_COMPLEX);

    public override Complex Eval(Complex x)
    {
        lock (context)
        {
            return Eval(x, context);
        }
    }

    public override Complex Eval(Complex x, Complex y)
    {
        lock (context)
        {
            return Eval(x, y, context);
        }
    }

    public override Complex Eval(Complex[] args)
    {
        lock (context)
        {
            return Eval(args, context);
        }
    }

    public Complex Eval(Complex x, EvalContext context)
    {
        var args = context.args1c;
        args[0] = x;
        return Eval(args, context);
    }

    public Complex Eval(Complex x, Complex y, EvalContext context)
    {
        var args = context.args2c;
        args[0] = x;
        args[1] = y;
        return Eval(args, context);
    }
}
