using AritySharp;

namespace AritySharpTests;
[TestClass]

public class FormatTest
{

    [TestMethod]
    public void FormatCase1()
    {
        Assert.AreEqual(Util.DoubleToString(0.1, 0),  ("0.1"));
        Assert.AreEqual(Util.DoubleToString(0.12, 0),  ("0.12"));
        Assert.AreEqual(Util.DoubleToString(0.001, 0),  ("0.001"));
        Assert.AreEqual(Util.DoubleToString(0.0012, 0),  ("0.0012"));
        Assert.AreEqual(Util.DoubleToString(0.0000001, 0),  ("1E-7"));
        Assert.AreEqual(Util.DoubleToString(0.00000012, 0),  ("1.2E-7"));
        Assert.AreEqual(Util.DoubleToString(0.123456789012345, 0),  ("0.123456789012345"));
    }

    [TestMethod]
    public void FormatCase2()
    {
        Assert.AreEqual(Util.DoubleToString(0, 0),  ("0"));
        Assert.AreEqual(Util.DoubleToString(1, 0),  ("1"));
        Assert.AreEqual(Util.DoubleToString(12, 0),  ("12"));
        Assert.AreEqual(Util.DoubleToString(1234567890.0, 0),  ("1234567890"));
        Assert.AreEqual(Util.DoubleToString(1000000000.0, 0),  ("1000000000"));
    }

    [TestMethod]
    public void FormatCase3()
    {
        Assert.AreEqual(Util.DoubleToString(1.23456789012345, 0),  ("1.23456789012345"));
        Assert.AreEqual(Util.DoubleToString(12345.6789012345, 0),  ("12345.6789012345"));
        Assert.AreEqual(Util.DoubleToString(1234567890.12345, 0),  ("1234567890.12345"));
        Assert.AreEqual(Util.DoubleToString(123456789012345.0, 0),  ("1.23456789012345E14"));
        Assert.AreEqual(Util.DoubleToString(100000000000000.0, 0),  ("1E14"));
        Assert.AreEqual(Util.DoubleToString(120000000000000.0, 0),  ("1.2E14"));
        Assert.AreEqual(Util.DoubleToString(100000000000001.0, 0),  ("1.00000000000001E14"));
    }

    [TestMethod]
    public void FormatCase4()
    {
        Assert.AreEqual(Util.DoubleToString(0.1, 2),  ("0.1"));
        Assert.AreEqual(Util.DoubleToString(0.00000012, 2),  ("1.2E-7"));
        Assert.AreEqual(Util.DoubleToString(0.123456789012345, 2),  ("0.12345678901235"));
    }

    [TestMethod]
    public void FormatCase5()
    {
        Assert.AreEqual(Util.DoubleToString(0, 2),  ("0"));
    }

    [TestMethod]
    public void FormatCase6()
    {
        Assert.AreEqual(Util.DoubleToString(1.23456789012345, 2),  ("1.2345678901235"));
        Assert.AreEqual(Util.DoubleToString(1.23456789012345, 3),  ("1.234567890123"));
    }

    [TestMethod]
    public void FormatCase7()
    {
        Assert.AreEqual(Util.DoubleToString(12345.6789012345, 0),  ("12345.6789012345"));
        Assert.AreEqual(Util.DoubleToString(1234567890.12345, 2),  ("1234567890.1235"));
        Assert.AreEqual(Util.DoubleToString(123456789012345.0, 3),  ("1.234567890123E14"));
        Assert.AreEqual(Util.DoubleToString(100000000000001.0, 2),  ("1E14"));
    }

    [TestMethod]
    public void FormatCase8()
    {
        Assert.AreEqual(Util.DoubleToString(12345678901234567.0, 0),  ("1.2345678901234568E16"));
        Assert.AreEqual(Util.DoubleToString(12345678901234567.0, 2),  ("1.2345678901235E16"));
    }

    [TestMethod]
    public void FormatCase9()
    {
        Assert.AreEqual(Util.DoubleToString(99999999999999999.0, 0),  ("1E17"));
        Assert.AreEqual(Util.DoubleToString(9999999999999999.0, 0),  ("1E16"));
        Assert.AreEqual(Util.DoubleToString(999999999999999.0, 0),  ("9.99999999999999E14"));
        Assert.AreEqual(Util.DoubleToString(999999999999999.0, 2),  ("1E15"));
        Assert.AreEqual(Util.DoubleToString(999999999999994.0, 2),  ("9.9999999999999E14"));
    }

    [TestMethod]
    public void FormatCase10()
    {
        Assert.AreEqual(Util.DoubleToString(MoreMath.Log2(1 + .00002), 2),  ("0.000028853612282487"));
        Assert.AreEqual(Util.DoubleToString(4E-4, 0),  ("0.0004"));
        Assert.AreEqual(Util.DoubleToString(1e30, 0),  ("1E30"));
    }

    [TestMethod]
    public void FormatComplex()
    {
        Assert.AreEqual(Util.ComplexToString(new Complex(0, -1), 10, 1),  ("-i"));
        Assert.AreEqual(Util.ComplexToString(new Complex(2.123, 0), 3, 0),  ("2.1"));
        Assert.AreEqual(Util.ComplexToString(new Complex(0, 1.0000000000001), 20, 3),  ("i"));
        Assert.AreEqual(Util.ComplexToString(new Complex(1, -1), 10, 1),  ("1-i"));
        Assert.AreEqual(Util.ComplexToString(new Complex(1, 1), 10, 1),  ("1+i"));
        Assert.AreEqual(Util.ComplexToString(new Complex(1.12, 1.12), 9, 0),  ("1.12+1.1i"));
        Assert.AreEqual(Util.ComplexToString(new Complex(1.12345, -1), 7, 0),  ("1.123-i"));
    }

    [TestMethod]
    public void SizeCase1()
    {
        Assert.AreEqual(Util.SizeTruncate("1111111110", 9),  ("1.11111E9"));
        Assert.AreEqual(Util.SizeTruncate("1111111110", 10),  ("1111111110"));
        Assert.AreEqual(Util.SizeTruncate("11111111110", 10),  ("1.11111E10"));
        Assert.AreEqual(Util.SizeTruncate("12.11111E9", 10),  ("12.11111E9"));
        Assert.AreEqual(Util.SizeTruncate("12.34567E9", 9),  ("12.3456E9"));
        Assert.AreEqual(Util.SizeTruncate("12345678E3", 9),  ("1.2345E10"));
        Assert.AreEqual(Util.SizeTruncate("-12345678E3", 9),  ("-1.234E10"));
    }

    [TestMethod]
    public void SizeCase2()
    {
        Assert.AreEqual(Util.SizeTruncate("-0.00000007", 9),  ("-0.000000"));
    }

    [TestMethod]
    public void SizeCase3()
    {
        Assert.AreEqual(Util.SizeTruncate("-1.23E123", 5),  ("-1.23E123"));
        Assert.AreEqual(Util.SizeTruncate("-1.2E123", 5),  ("-1.2E123"));
        Assert.AreEqual(Util.SizeTruncate("-1E123", 5),  ("-1E123"));
        Assert.AreEqual(Util.SizeTruncate("-1", 2),  ("-1"));
        Assert.AreEqual(Util.SizeTruncate("-1", 1),  ("-1"));
        Assert.AreEqual(Util.SizeTruncate("-0.02", 1),  ("-0.02"));
        Assert.AreEqual(Util.SizeTruncate("0.02", 1),  ("0"));
    }

}
