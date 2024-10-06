namespace AritySharp;

// f'(x)=Im(f(x+i*h)/h) 
public class Derivative : Function
{
    private readonly Function f;
    private readonly Complex c = new ();
    private const double H = 1e-12;
    private const double INVH = 1.0 / H;

    public Derivative(Function f)
    {
        this.f = f;
        f.CheckArity(1);
    }

    public override double Eval(double x) => f.Eval(c.Set(x, H)).im * INVH;

    public override int Arity => 1;
}
