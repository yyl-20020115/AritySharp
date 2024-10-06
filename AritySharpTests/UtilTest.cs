using AritySharp;

namespace AritySharpTests;
[TestClass]

public class UtilTest
{

    [TestMethod]
    public void Test()
    {
        Assert.AreEqual(Util.ShortApprox(1.235, 0.02), (1.24));
        Assert.AreEqual(Util.ShortApprox(1.235, 0.4), (1.2000000000000002));
        Assert.AreEqual(Util.ShortApprox(-1.235, 0.02), (-1.24));
        Assert.AreEqual(Util.ShortApprox(-1.235, 0.4), (-1.2000000000000002));
    }

}
