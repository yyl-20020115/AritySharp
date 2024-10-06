using AritySharp;

namespace AritySharpTests;

[TestClass]

public class EvalTest
{

    [TestMethod]
    public void EvalCase1()
    {
        Assert.AreEqual(new Symbols().Eval("."), (0d));
        Assert.AreEqual(new Symbols().Eval("1+."), (1d));
        Assert.AreEqual(new Symbols().Eval("1"), (1d));
        Assert.AreEqual(new Symbols().Eval("\u03c0"), (Math.PI));
        Assert.AreEqual(new Symbols().Eval("2\u00d73"), (6d)); // 2*3
        Assert.AreEqual(new Symbols().Eval("1+\u221a9*2"), (7d)); // 1+sqrt(9)*2
        Assert.AreEqual(new Symbols().Eval("3\u221a 4"), (6d)); // 3*sqrt(4)
        Assert.AreEqual(new Symbols().Eval("\u221a16sin(2\u03c0/4)"), (4d)); // sqrt(16)*sin(2pi/4)
        Assert.AreEqual(new Symbols().Eval("1+1"), (2d));
        Assert.AreEqual(new Symbols().Eval("1+-1"), (0d));
        Assert.AreEqual(new Symbols().Eval("-0.5"), (-.5d));
        Assert.AreEqual(new Symbols().Eval("+1e2"), (100d));
        Assert.AreEqual(new Symbols().Eval("1e-1"), (.1d));
        Assert.AreEqual(new Symbols().Eval("1e\u22122"), (.01d)); // unicode minus
        Assert.AreEqual(new Symbols().Eval("-2^3!"), (-64d));
        Assert.AreEqual(new Symbols().Eval("(-2)^3!"), (64d));
        Assert.AreEqual(new Symbols().Eval("-2^1^2"), (-2d));
        Assert.AreEqual(new Symbols().Eval("--1"), (1d));
        Assert.AreEqual(new Symbols().Eval("-3^--2"), (-9d));
        Assert.AreEqual(new Symbols().Eval("1+2)(2+3"), (15d));
        Assert.AreEqual(new Symbols().Eval("1+2)!^-2"), (1.0 / 36d));
        Assert.AreEqual(new Symbols().Eval("sin(0)"), (0d));
        Assert.AreEqual(new Symbols().Eval("cos(0)"), (1d));
        Assert.AreEqual(new Symbols().Eval("sin(-1--1)"), (0d));
        Assert.AreEqual(new Symbols().Eval("-(2+1)*-(4/2)"), (6d));
        Assert.AreEqual(new Symbols().Eval("-.5E-1"), (-.05d));
        Assert.AreEqual(new Symbols().Eval("2 3 4"), (24d));
        Assert.AreEqual(new Symbols().Eval("pi"), (Math.PI));
        Assert.AreEqual(new Symbols().Eval("e"), (Math.E));
        Assert.AreEqual(new Symbols().Eval("sin(pi/2)"), (1d));
        Assert.AreEqual(new Symbols().Eval("NaN"), (Double.NaN));
        Assert.AreEqual(new Symbols().Eval("Inf"), (Double.PositiveInfinity));
        Assert.AreEqual(new Symbols().Eval("Infinity"), (Double.PositiveInfinity));
        Assert.AreEqual(new Symbols().Eval("-Inf"), (Double.NegativeInfinity));
        Assert.AreEqual(new Symbols().Eval("0/0"), (Double.NaN));
        try
        {
            new Symbols().Eval("1+");

        }
        catch (SyntaxException e)
        {
            Assert.AreEqual(e.ToString(), ("SyntaxException: unexpected ) or END in '1+' at position 2"));
        }
        try
        {
            new Symbols().Eval("1E1.5");
        }
        catch (SyntaxException e)
        {
            Assert.AreEqual(e.ToString(), ("SyntaxException: invalid number '1E1.5' in '1E1.5' at position 0"));
        }
        try
        {
            new Symbols().Eval("=");
        }
        catch (SyntaxException e)
        {
            Assert.AreEqual(e.ToString(), ("SyntaxException: invalid character '=' in '=' at position 0"));
        }
    }

    [TestMethod]
    public void EvalCase2()
    {
        Assert.AreEqual(new Symbols().Eval("comb(11, 9)"), (55d));
        Assert.AreEqual(new Symbols().Eval("perm(11, 2)"), (110d));
        Assert.AreEqual(new Symbols().Eval("comb(1000, 999)"), (1000d));
        Assert.AreEqual(new Symbols().Eval("perm(1000, 1)"), (1000d));
    }

    [TestMethod]
    public void EvalCase3()
    {
        Assert.AreEqual(new Symbols().Eval("abs(3-4i)"), (5d));
        Assert.AreEqual(new Symbols().Eval("exp(pi*i)"), (-1d));
    }

    [TestMethod]
    public void EvalCase4()
    {
        Assert.AreEqual(new Symbols().Eval("5%"), (0.05d));
        Assert.AreEqual(new Symbols().Eval("200+5%"), (210d));
        Assert.AreEqual(new Symbols().Eval("200-5%"), (190d));
        Assert.AreEqual(new Symbols().Eval("100/200%"), (50d));
        Assert.AreEqual(new Symbols().Eval("100+200%+5%"), (315d));
    }

    [TestMethod]
    public void EvalCase5()
    {
        Assert.AreEqual(new Symbols().Eval("mod(5,3)"), (2d));
        Assert.AreEqual(new Symbols().Eval("5.2 # 3.2"), (2d));
    }

    [TestMethod]
    public void EvalCase6()
    {
        Assert.AreEqual(new Symbols().Eval("100.1-100-.1"), (0d));
        Assert.AreEqual(new Symbols().Eval("1.1-1+(-.1)"), (0d));
    }

    [TestMethod]

    public void EvalCase7()
    {
        Assert.AreEqual(new Symbols().Eval("log(2,8)"), (3d));
        Assert.AreEqual(new Symbols().Eval("log(9,81)"), (2d));
        Assert.AreEqual(new Symbols().Eval("log(4,2)"), (.5d));
    }

    [TestMethod]
    public void EvalCase8()
    {
        Assert.AreEqual(new Symbols().Eval("sin'(0)"), (1d));
        Assert.AreEqual(new Symbols().Eval("cos'(0)"), (-0d));
        Assert.AreEqual(new Symbols().Eval("cos'(pi/2)"), (-1d));
        Assert.AreEqual(new Symbols().Eval("abs'(2)"), (1d));
        Assert.AreEqual(new Symbols().Eval("abs'(-3)"), (-1d));
    }

    [TestMethod]
    public void EvalCase9()
    {
        Assert.AreEqual(new Symbols().Eval("0x0"), (0d));
        Assert.AreEqual(new Symbols().Eval("0x100"), (256d));
        Assert.AreEqual(new Symbols().Eval("0X10"), (16d));
        Assert.AreEqual(new Symbols().Eval("0b10"), (2d));
        Assert.AreEqual(new Symbols().Eval("0o10"), (8d));
        Assert.AreEqual(new Symbols().Eval("sin(0x1*pi/2)"), (1d));
    }

    [TestMethod]
    public void EvalCase10()
    {
        Assert.AreEqual(new Symbols().Eval("ln(e)"), (1d));
        Assert.AreEqual(new Symbols().Eval("log(10)"), (1d));
        Assert.AreEqual(new Symbols().Eval("log10(100)"), (2d));
        // Assert.AreEqual(new Symbols().Eval("lg(.1)"), (-1d)); defunct: equals on floating point value
        Assert.AreEqual(new Symbols().Eval("log2(2)"), (1d));
        Assert.AreEqual(new Symbols().Eval("lb(256)"), (8d));
    }

    [TestMethod]
    public void EvalCase11()
    {
        Assert.AreEqual(new Symbols().Eval("rnd()*0"), (0d));
        Assert.AreEqual(new Symbols().Eval("rnd(5)*0"), (0d));
    }

    [TestMethod]
    public void EvalCase12()
    {
        Assert.AreEqual(new Symbols().Eval("max(2,3)"), (3d));
        Assert.AreEqual(new Symbols().Eval("min(2,3)"), (2d));
        Assert.AreEqual(new Symbols().Eval("cbrt(8)"), (2d));
        Assert.AreEqual(new Symbols().Eval("cbrt(-8)"), (-2d));
    }

    [TestMethod]
    public void EvalCase13()
    {
        Assert.AreEqual(new Symbols().Eval("real(8.123)"), (8.123d));
        Assert.AreEqual(new Symbols().Eval("imag(8.123)"), (0d));
        Assert.AreEqual(new Symbols().Eval("im(sqrt(-1))"), (1d));
        Assert.AreEqual(new Symbols().Eval("im(nan)"), (Double.NaN));
    }

    [TestMethod]
    public void EvalComplexCase1()
    {
        Assert.AreEqual(new Symbols().EvalComplex("sqrt(-1)^2"), (new Complex(-1, 0)));
        Assert.AreEqual(new Symbols().EvalComplex("i"), (new Complex(0, 1)));
        Assert.AreEqual(new Symbols().EvalComplex("sqrt(-1)"), (new Complex(0, 1)));
    }

    [TestMethod]
    public void EvalComplexCase2()
    {
        Assert.AreEqual(new Symbols().EvalComplex("ln(-1)"), (new Complex(0, -Math.PI)));
        Assert.AreEqual(new Symbols().EvalComplex("i^i"), (new Complex(0.20787957635076193, 0)));
        Assert.AreEqual(new Symbols().EvalComplex("gcd(135-14i, 155+34i)"), (new Complex(12, -5)));

    }

    [TestMethod]
    public void EvalComplexCase3()
    {
        Assert.AreEqual(new Symbols().EvalComplex("sign(2i)"), (new Complex(0, 1)));
        //NOTICE:NaN!=NaN
        Assert.AreEqual(new Symbols().EvalComplex("sign(nan)").ToString(), (new Complex(double.NaN, 0)).ToString());
        Assert.AreEqual(new Symbols().EvalComplex("sign(nan i)").ToString(), (new Complex(double.NaN, 0)).ToString());
        Assert.AreEqual(new Symbols().EvalComplex("sign(0)"), (new Complex(0, 0)));
    }

    [TestMethod]
    public void EvalComplexCase4()
    {
        Assert.AreEqual(new Symbols().EvalComplex("real(8.123)"), (new Complex(8.123, 0)));
        Assert.AreEqual(new Symbols().EvalComplex("imag(8.123)"), (new Complex(0, 0)));
        Assert.AreEqual(new Symbols().EvalComplex("real(1+3i)"), (new Complex(1, 0)));
        Assert.AreEqual(new Symbols().EvalComplex("imag(1+3i)"), (new Complex(3, 0)));
        Assert.AreEqual(new Symbols().EvalComplex("re(1+3i)"), (new Complex(1, 0)));
        Assert.AreEqual(new Symbols().EvalComplex("im(1+3i)"), (new Complex(3, 0)));
    }

}
