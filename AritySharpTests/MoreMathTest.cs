using AritySharp;

namespace AritySharpTests;
[TestClass]

public class MoreMathTest
{

    [TestMethod]

    public void Test()
    {
        Assert.AreEqual(MoreMath.IntLog10(-0.03),  (0));
        Assert.AreEqual(MoreMath.IntLog10(0.03),  (-2));
        Assert.AreEqual(MoreMath.IntExp10(3),  (1000d));
        Assert.AreEqual(MoreMath.IntExp10(-1),  (0.1d));
    }

}
