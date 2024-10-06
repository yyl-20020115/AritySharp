using AritySharp;

namespace AritySharpTests;

[TestClass]
public class ComplexTest
{

    [TestMethod]
    public void Case1()
    {
        Assert.AreEqual(new Complex(-1, 0).Pow(new Complex(0, 1)), (new Complex(0.04321391826377226, 0)));
        Assert.AreEqual(new Complex(-1, 0).Pow(new Complex(1, 1)), (new Complex(-0.04321391826377226, 0)));
    }

    [TestMethod]
    public void Case2()
    {
        Assert.AreEqual(new Complex(-1, 0).Abs(), (1d));
        Assert.AreEqual(new Complex(Math.E * Math.E, 0).Log(), (new Complex(2, 0)));
        Assert.AreEqual(new Complex(-1, 0).Log(), (new Complex(0, Math.PI)));
    }

    [TestMethod]
    public void Case3()
    {
        // Assert.AreEqual(new Complex(2, 0).exp(), (new Complex(Math.E * Math.E, 0)));
        Assert.AreEqual(new Complex(0, Math.PI).Exp(), (new Complex(-1, 0)));
    }

    [TestMethod]
    public void Case4()
    {
        Assert.AreEqual(MoreMath.Lgamma(1), (0d));
        Assert.AreEqual(new Complex(1, 0).Lgamma(), (new Complex(0, 0)));
    }

    [TestMethod]
    public void Case5()
    {
        Assert.AreEqual(new Complex(0, 0).Factorial(), (new Complex(1, 0)));
        Assert.AreEqual(new Complex(1, 0).Factorial(), (new Complex(1, 0)));
        // Assert.AreEqual(new Complex(0, 1).factorial(), (new Complex(0.49801566811835596,
        // -0.1549498283018106)));
        Assert.AreEqual(new Complex(-2, 1).Factorial(), (new Complex(-0.17153291990834815, 0.32648274821006623)));
        Assert.AreEqual(new Complex(4, 0).Factorial(), (new Complex(24, 0)));
        // Assert.AreEqual(new Complex(4, 3).factorial(), (new Complex(0.016041882741649555,
        // -9.433293289755953)));
    }

}
