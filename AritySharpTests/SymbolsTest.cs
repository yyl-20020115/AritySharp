using AritySharp;

namespace AritySharpTests;
[TestClass]

public class SymbolsTest
{

    [TestMethod]
    public void Test10p_add_20p()
    {
        Symbols symbols = new Symbols();
        Assert.AreEqual(Util.SizeTruncate(symbols.Eval("10%+20%").ToString(), 4), "0.12");
    }

    [TestMethod]
    public void Case1()
    {
        Symbols symbols = new Symbols();
        symbols.Define("a", 1);
        Assert.AreEqual(symbols.Eval("a"), (1d));

        symbols.PushFrame();
        Assert.AreEqual(symbols.Eval("a"), (1d));
        symbols.Define("a", 2);
        Assert.AreEqual(symbols.Eval("a"), (2d));
        symbols.Define("a", 3);
        Assert.AreEqual(symbols.Eval("a"), (3d));

        symbols.PopFrame();
        Assert.AreEqual(symbols.Eval("a"), (1d));
    }

    [TestMethod]
    public void Case2()
    {
        Symbols symbols = new Symbols();
        symbols.PushFrame();
        symbols.Add(Symbol.MakeArg("base", 0));
        symbols.Add(Symbol.MakeArg("x", 1));
        Assert.AreEqual(symbols.LookupConst("x").op, (VM.LOAD1));
        symbols.PushFrame();
        Assert.AreEqual(symbols.LookupConst("base").op, (VM.LOAD0));
        Assert.AreEqual(symbols.LookupConst("x").op, (VM.LOAD1));
        symbols.PopFrame();
        Assert.AreEqual(symbols.LookupConst("base").op, (VM.LOAD0));
        Assert.AreEqual(symbols.LookupConst("x").op, (VM.LOAD1));
        symbols.PopFrame();
        Assert.AreEqual(symbols.LookupConst("x").op, (VM.LOAD0));
    }

    [TestMethod]

    public void TestRecursiveEval()
    {
        Symbols symbols = new Symbols();
        symbols.Define("myfun", new MyFun());
        Function f = symbols.Compile("1+myfun(x)");
        Assert.AreEqual(f.Eval(0), (2d));
        Assert.AreEqual(f.Eval(1), (1d));
        Assert.AreEqual(f.Eval(2), (0d));
        Assert.AreEqual(f.Eval(3), (-1d));
    }

    public class MyFun : Function
    {
        Symbols symbols = new();
        Function f = Default;

        public MyFun()
        {
            try
            {
                f = symbols.Compile("1-x");
            }
            catch (SyntaxException e)
            {
                Console.WriteLine("" + e);
            }
        }


        public override double Eval(double x)
        {
            return f.Eval(x);
        }
        public override int Arity => 1;
    }
}
