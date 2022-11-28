namespace DecFloat.Tests;

[TestClass]
public class DecFloatTests
{
    // precision math - https://keisan.casio.com/calculator

    [TestMethod]
    public void ParseSmallNumber()
    {
        var actual = new DecFloat("123").ToString();
        Assert.AreEqual("123", actual);
    }

    [TestMethod]
    public void ParseBigNumber()
    {
        var actual = new DecFloat("9876543210").ToString();
        Assert.AreEqual("9876543210", actual);
    }

    [TestMethod]
    public void ParseDecimalNumber()
    {
        var actual = new DecFloat("3.1415926535897932384626433832795028841971693993751058209749445923078164").ToString();
        Assert.AreEqual("3.1415926535897932384626433832795028841971693993751058209749445923078164", actual);
    }

    [TestMethod]
    public void ParseSmallDecimal()
    {
        var actual = new DecFloat("0.0000893473").ToString();
        Assert.AreEqual(".0000893473", actual);
    }

    [TestMethod]
    public void ParseNegativeNumber()
    {
        var actual = new DecFloat("-420").ToString();
        Assert.AreEqual("-420", actual);
    }

    [TestMethod]
    public void Add()
    {
        var actual = new DecFloat("9876543210").Add(new DecFloat("123456790")).ToString();
        Assert.AreEqual("10000000000", actual);
    }

    [TestMethod]
    public void AddDecimal()
    {
        var actual = new DecFloat(".05").Add(new DecFloat(".05")).ToString();
        Assert.AreEqual(".10", actual);
    }

    [TestMethod]
    public void Mul()
    {
        var actual = new DecFloat("257").Mul(new DecFloat("257")).ToString();
        Assert.AreEqual("66049", actual);
    }

    [TestMethod]
    public void AddDecimal2()
    {
        var actual = new DecFloat(".05").Add(new DecFloat(".1")).ToString();
        Assert.AreEqual(".15", actual);
    }

    [TestMethod]
    public void MulDecimal()
    {
        var actual = new DecFloat("3.14").Mul(new DecFloat("2.718")).ToString();
        Assert.AreEqual("8.53452", actual);
    }

    [TestMethod]
    public void MulNegative()
    {
        var actual = new DecFloat("3.14").Mul(new DecFloat("-2.718")).ToString();
        Assert.AreEqual("-8.53452", actual);
    }

    [TestMethod]
    public void AddNegatives()
    {
        var actual = new DecFloat("-1234").Add(new DecFloat("-1234")).ToString();
        Assert.AreEqual("-2468", actual);
    }

    [TestMethod]
    public void SubSmall()
    {
        var actual = new DecFloat("420").Sub(new DecFloat("69")).ToString();
        Assert.AreEqual("351", actual);
    }

    [TestMethod]
    public void SubSmallNeg()
    {
        var actual = new DecFloat("69").Sub(new DecFloat("420")).ToString();
        Assert.AreEqual("-351", actual);
    }

    [TestMethod]
    public void Sub()
    {
        var actual = new DecFloat("10000000000").Sub(new DecFloat("123456790")).ToString();
        Assert.AreEqual("9876543210", actual);
    }

    [TestMethod]
    public void AddRandomInt()
    {
        var r = new Random();
        for (int i = 0; i < 1000; i++)
        {
            long a = r.Next(), b = r.Next();
            if (r.Next(2) == 1) a = -a;
            if (r.Next(2) == 1) b = -b;
            long c = a + b;
            var actual = new DecFloat("" + a).Add(new DecFloat("" + b)).ToString();
            Assert.AreEqual(c.ToString(), actual, "" + a + " + " + b);
        }
    }

    [TestMethod]
    public void SubRandomInt()
    {
        var r = new Random();
        for (int i = 0; i < 1000; i++)
        {
            long a = r.Next(), b = r.Next();
            if (r.Next(2) == 1) a = -a;
            if (r.Next(2) == 1) b = -b;
            long c = a - b;
            var actual = new DecFloat("" + a).Sub(new DecFloat("" + b)).ToString();
            Assert.AreEqual(c.ToString(), actual, "" + a + " - " + b);
        }
    }

    [TestMethod]
    public void MulRandomInt()
    {
        var r = new Random();
        for (int i = 0; i < 1000; i++)
        {
            long a = r.Next(), b = r.Next();
            if (r.Next(2) == 1) a = -a;
            if (r.Next(2) == 1) b = -b;
            long c = a * b;
            var actual = new DecFloat("" + a).Mul(new DecFloat("" + b)).ToString();
            Assert.AreEqual(c.ToString(), actual, "" + a + " * " + b);
        }
    }

    [TestMethod]
    public void DivSmall()
    {
        var actual = new DecFloat("420").Div(new DecFloat("69"), 15).ToString();
        Assert.AreEqual("6.08695652173913", actual);
    }

    [TestMethod]
    public void DivAdjust()
    {
        var actual = new DecFloat("42").Div(new DecFloat("69"), 15).ToString();
        Assert.AreEqual(".608695652173913", actual);
    }

    [TestMethod]
    public void DivAdjust2()
    {
        var actual = new DecFloat("4200").Div(new DecFloat("69"), 15).ToString();
        Assert.AreEqual("60.8695652173913", actual);
    }

    [TestMethod]
    public void DivNegative()
    {
        var actual = new DecFloat("255").Div(new DecFloat("-51"), 10).ToString();
        Assert.AreEqual("-5", actual);
    }

    [TestMethod]
    public void DivSmallByLarge()
    {
        var actual = new DecFloat("1").Div(new DecFloat("98723894723984"), 20).ToString();
        Assert.AreEqual(".000000000000010129260021556461423", actual);
    }

    [TestMethod]
    public void DivLargeBySmall()
    {
        var actual = new DecFloat("98723894723984").Div(new DecFloat("7"), 29).ToString();
        Assert.AreEqual("14103413531997.714285714285714", actual);
    }

    [TestMethod]
    public void DivLargeEven()
    {
        var actual = new DecFloat("255000000000").Div(new DecFloat("51000000"), 30).ToString();
        Assert.AreEqual("5000", actual);
    }

    [TestMethod]
    public void DivLargeOne()
    {
        var actual = new DecFloat("255000000000").Div(new DecFloat("25500000"), 30).ToString();
        Assert.AreEqual("10000", actual);
    }

    [TestMethod]
    public void DivSmallOne()
    {
        var actual = new DecFloat("25500000").Div(new DecFloat("255000000000"), 30).ToString();
        Assert.AreEqual(".0001", actual);
    }

    [TestMethod]
    public void DivBug()
    {
        var actual = new DecFloat("3.5").Div(new DecFloat("2"), 30).ToString();
        Assert.AreEqual("1.75", actual);
    }

    [TestMethod]
    public void AddBug()
    {
        var actual = new DecFloat("2").Add(new DecFloat(".5")).ToString();
        Assert.AreEqual("2.5", actual);
    }

    [TestMethod]
    public void AddBug2()
    {
        var actual = new DecFloat("-333").Add(new DecFloat(".5")).ToString();
        Assert.AreEqual("-332.5", actual);
    }

    [TestMethod]
    public void DivRandom()
    {
        var r = new Random();
        for (int i = 0; i < 1000; i++)
        {
            double a = r.Next(), b = r.Next();
            if (r.Next(2) == 1) a = -a;
            if (r.Next(2) == 1) b = -b;
            double c = a / b;
            var actual = new DecFloat("" + a).Div(new DecFloat("" + b), 30).ToString();
            Assert.AreEqual(c, double.Parse(actual), "" + a + " / " + b);
        }
    }

    [TestMethod]
    public void Log2()
    {
        var actual = new DecFloat("7").Log2(20).ToString();
        //               2.807354922057604
        Assert.AreEqual("2.80735492205760410744", actual);
        //               2.807354922057604107442
    }

    [TestMethod]
    public void Log2BigPow2()
    {
        var actual = new DecFloat("16777216").Log2(20).ToString();
        Assert.AreEqual("24", actual);
    }

    [TestMethod]
    public void Log2SmallPow2()
    {
        var actual = new DecFloat("0.0625").Log2(20).ToString();
        Assert.AreEqual("-4", actual);
    }

    [TestMethod]
    public void Log2Big()
    {
        var actual = new DecFloat(false, new uint[] { 1 }, -100).Log2(20).ToString();
        Assert.AreEqual("332.19280948873623478703", actual);
        //               332.192809488736234787
    }

    [TestMethod]
    public void Round()
    {
        var actual = new DecFloat("3.14159265359").Round().ToString();
        Assert.AreEqual("3", actual);
    }

    [TestMethod]
    public void RoundUp()
    {
        var actual = new DecFloat("2.7182818").Round().ToString();
        Assert.AreEqual("3", actual);
    }

    [TestMethod]
    public void RoundDP()
    {
        var actual = new DecFloat("3.14159265359").Round(4).ToString();
        Assert.AreEqual("3.1416", actual);
    }

    [TestMethod]
    public void Int()
    {
        var actual = new DecFloat("-2.7182818").Int().ToString();
        Assert.AreEqual("-2", actual);
    }

    [TestMethod]
    public void Trunc()
    {
        var actual = new DecFloat("-2.7182818").Trunc().ToString();
        Assert.AreEqual("-.7182818", actual);
    }

    [TestMethod]
    public void SrqtSquare()
    {
        var actual = new DecFloat("1024").Sqrt(20).ToString();
        Assert.AreEqual("32.00000", actual);
    }

    [TestMethod]
    public void Sqrt()
    {
        var actual = new DecFloat("420").Sqrt(20).ToString();
        Assert.AreEqual("20.49390153191919676638", actual);
        //               20.493901531919196
    }

    [TestMethod]
    public void SqrtSmall()
    {
        var actual = new DecFloat(".034234").Sqrt(20).ToString();
        Assert.AreEqual(".18502432272541899510", actual);
        //               .185024322725419
    }

    [TestMethod]
    public void Exp2()
    {
        var actual = new DecFloat("24").Exp2(20).ToString();
        Assert.AreEqual("16777216.00000000000000000000", actual);
    }

    [TestMethod]
    public void Exp2Pi()
    {
        var actual = new DecFloat("3.14159265359").Exp2(20).ToString();
        //               8.824977827077554
        Assert.AreEqual("8.82497782707755238594", actual);
    }

    [TestMethod]
    public void Exp2Negative()
    {
        var actual = new DecFloat("-4.5").Exp2(20).ToString();
        //               .044194173824159216
        Assert.AreEqual(".044194173824159220275", actual);
    }

    [TestMethod]
    public void Pi()
    {
        var actual = DecFloat.Pi(10).ToString();
        //               3.1415926535897932384626433832795028841971693993751058209749445923078164
        Assert.AreEqual("3.1415926536", actual);
    }

    [TestMethod]
    public void E()
    {
        var actual = DecFloat.E(30).ToString();
        //               2.718281828459045235360287471352662497757247093699959574966967627724076630353
        Assert.AreEqual("2.718281828459045235360287471353", actual);
    }

    [TestMethod]
    public void Mod()
    {
        var actual = new DecFloat("10.5").Mod(new DecFloat("3"), 10).ToString();
        Assert.AreEqual("1.5", actual);
    }

    [TestMethod]
    public void Pow()
    {
        var actual = new DecFloat("3.14").Pow(new DecFloat("2.718"), 20).ToString();
        //               22.420989921777696
        Assert.AreEqual("22.42098992177769557497", actual);
    }
}