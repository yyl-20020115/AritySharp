namespace AritySharp;

// f'(x)=Im(f(x+i*h)/h) 
public class Derivative : Function
{
    private readonly Function function;
    private readonly Complex c = new ();
    private const double H = 1e-12;
    private const double INVH = 1.0 / H;

    public Derivative(Function function)
    {
        this.function = function;
        function.CheckArity(1);
    }

    public override double Eval(double x) => function.Eval(c.Set(x, H)).Imaginary * INVH;

    public override int Arity => 1;
}
