namespace DecFloat;

public class DecFloat
{
    private readonly bool neg;    // true if negative
    private readonly byte[] num;  // number stored in LSB -> MSB order
    private readonly int dp;      // number of decimal places

    public static readonly DecFloat Zero = new DecFloat(false, Array.Empty<byte>(), 0);
    public static readonly DecFloat One = new DecFloat(false, new byte[] { 1 }, 0);
    public static readonly DecFloat Two = new DecFloat(false, new byte[] { 2 }, 0);
    public static readonly DecFloat Half = new DecFloat(false, new byte[] { 5 }, 1);

    public DecFloat(string s)
    {
        neg = s.StartsWith('-');
        if (neg) s = s.Substring(1);
        var ss = s.Split('.');
        var bdp = ss[0].TrimStart('0');
        var adp = ss.Length > 1 ? ss[1] : "";
        this.dp = adp.Length;
        num = Dec2Bin(bdp + adp);
    }

    public DecFloat(bool neg, byte[] num, int dp)
    {
        this.neg = neg;
        this.num = num;
        this.dp = dp;
    }

    public static byte[] Dec2Bin(string s)
    {
        var b = Array.Empty<byte>();
        foreach (var c in s)
            AddDigit(c - '0', ref b);
        return b;
    }

    public static void AddDigit(int d, ref byte[] b)
    {
        int c = d;
        for (int i = 0; i < b.Length; i++)
        {
            var r = b[i] * 10 + c;
            c = r / 256;
            b[i] = (byte)(r % 256);
        }
        if (c > 0)
        {
            Array.Resize(ref b, b.Length + 1);
            b[b.Length - 1] = (byte)c;
        }
    }

    public DecFloat Sub(DecFloat b) => Add(new DecFloat(!b.neg, b.num, b.dp));

    public DecFloat Add(DecFloat b)
    {
        var a = this;
        if (b.ToString() == "0") return this;
        if (a.ToString() == "0") return b;
        if (dp > b.dp)
            b = b.Mul(new DecFloat("1" + new string('0', dp - b.dp)));
        else if (dp < b.dp)
            a = a.Mul(new DecFloat("1" + new string('0', b.dp - dp)));

        if (a.neg == b.neg)
            return new DecFloat(a.neg, Add(a.num, b.num), Math.Max(b.dp, dp));

        var cmp = Compare(a.num, b.num);
        if (cmp == 0) return Zero;
        var subb = cmp > 0;
        var rneg = a.neg != cmp < 0;
        var r = subb ? Sub(a.num, b.num) : Sub(b.num, a.num);
        return new DecFloat(rneg, r, Math.Max(b.dp, dp));
    }

    private static byte[] Add(byte[] a, byte[] b)
    {
        var r = new byte[Math.Max(a.Length, b.Length)];
        int c = 0;
        for (int i = 0; i < r.Length; i++)
        {
            var ai = i < a.Length ? a[i] : 0;
            var bi = i < b.Length ? b[i] : 0;
            var ri = ai + bi + c;
            c = ri / 256;
            r[i] = (byte)(ri % 256);
        }
        if (c > 0)
        {
            Array.Resize(ref r, r.Length + 1);
            r[r.Length - 1] = (byte)c;
        }
        return r;
    }

    private static byte[] Sub(byte[] a, byte[] b)
    {
        var r = new byte[Math.Max(a.Length, b.Length)];
        int c = 0;
        for (int i = 0; i < r.Length; i++)
        {
            var ai = i < a.Length ? a[i] : 0;
            var bi = i < b.Length ? b[i] : 0;
            var ri = ai - bi - c;
            c = 0; while (ri < 0) { ri += 256; c++; }
            r[i] = (byte)ri;
        }
        if (c != 0) throw new Exception("unexpected borrow");
        var rl = r.Length;
        while (rl > 0 && r[rl - 1] == 0) rl--;
        if (rl != r.Length) Array.Resize(ref r, rl);
        return r;
    }

    public DecFloat Mul(DecFloat b)
    {
        var r = Array.Empty<byte>();
        for (int i = 0; i < b.num.Length; i++)
        {
            var ri = Mul(b.num[i]);
            var rl = ri.Length;
            if (i > 0)
            {
                Array.Resize(ref ri, rl + i);
                Array.Copy(ri, 0, ri, i, rl);
                Array.Clear(ri, 0, i);
            }
            r = Add(r, ri);
        }
        return new DecFloat(this.neg != b.neg, r, dp + b.dp);
    }

    private byte[] Mul(byte b) => Mul(num, b);

    private static byte[] Mul(byte[] num, byte b)
    {
        var r = new byte[num.Length];
        var c = 0;
        for (int i = 0; i < r.Length; i++)
        {
            var ai = num[i];
            var ri = (ai * b) + c;
            c = ri / 256;
            r[i] = (byte)(ri % 256);
        }
        if (c > 0)
        {
            Array.Resize(ref r, r.Length + 1);
            r[r.Length - 1] = (byte)c;
        }
        return r;
    }

    public DecFloat Div(DecFloat b, int maxDigitsOfPrecision)
    {
        // divide works like this:
        // normalize divisor such that it is at least >1/10x <=1x the dividend.
        // normalizing happens by 10x'ing or 1/10x'ing the divisor and remembering how many times we did that.
        // we precompute the values of 1x - 9x of the divisor for easy reference.
        // then we write out the digit to the result, subtract, and repeat with the remainder.
        // we keep going with this until we reach the desired level of precision.

        // example: 420 / 69 which is really 420(0) / 69(0)
        // 69x1=69
        // 69x2=138
        // 69x3=207
        // 69x4=276
        // 69x5=345
        // 69x6=414
        // 69x7=483
        // 69x8=552
        // 69x9=621
        // 69x10=690
        // 420 - 69x6 or 414 = 6,  x10 = 60
        // --------------------------------- decimal point goes here
        //  60 - 69x0 or 0   = 60, x10 = 600
        // 600 - 69x8 or 552 = 48, x10 = 480
        // 480 - 69x6 or 414 = 66, x10 = 660
        // 660 - 69x9 or 621 = 39, x10 = 390
        // 390 - 69x5 or 345 = 45, x10 = 450
        // 450 - 69x6 or 414 = 36, x10 = 360
        // 360 - 69x5 or 345 = 15, x10 = 150
        // 150 - 69x2 or 138 = 12, x10 = 120
        // 120 - 69x1 or 69  = 51, x10 = 510

        byte[] anum = new byte[this.num.Length], bnum = b.num;
        Array.Copy(this.num, anum, this.num.Length);
        int adp = this.dp, bdp = b.dp;
        int rdp = -adp + bdp + 1;
        // make bnum at least as big as anum
        while (Compare(bnum, anum) < 0)
        {
            bnum = Mul(bnum, 10);
            rdp++;
        }
        // make anum at least as big as bnum
        while (Compare(bnum, anum) > 0)
        {
            anum = Mul(anum, 10);
            rdp--;
        }

        byte[][] divs = new byte[11][];
        divs[0] = Array.Empty<byte>();
        divs[1] = bnum;
        for (int i = 2; i <= 10; i++)
            divs[i] = Add(bnum, divs[i - 1]);

        var result = "";
        while (result.Length < maxDigitsOfPrecision)
        {
            int cmp = 0;
            for (int i = 10; i >= 0; i--)
            {
                cmp = Compare(anum, divs[i]);
                if (cmp >= 0)
                {
                    result += i;
                    anum = Sub(anum, divs[i]);
                    anum = Mul(anum, 10);
                    break;
                }
            }
            if (cmp == 0) break;
        }
        if (rdp < 0) { result = new string('0', -rdp) + result; rdp = 0; }
        if (rdp > result.Length) result += new string('0', rdp - result.Length);
        result = result.Insert(rdp, ".");
        if (this.neg != b.neg) result = "-" + result;
        return new DecFloat(result);
    }

    public DecFloat Log2(int precisionDigits)
    {
        if (this.Compare(Zero) <= 0)
            throw new ArgumentOutOfRangeException("Cannot take log of 0 or negative number");
        var exp = Zero;
        var num = this.Clone();
        while (num.Compare(Two) >= 0)
        {
            num = num.Mul(Half);
            exp = exp.Add(One);
        }
        while (num.Compare(One) < 0)
        {
            num = num.Mul(Two);
            exp = exp.Sub(One);
        }
        var limit = new DecFloat(false, new byte[] { 1 }, precisionDigits + 4);
        var fract = One;
        if (num.Compare(One) == 0) return exp;
        while (fract.Compare(limit) > 0)
        {
            fract = fract.Mul(Half);
            num = num.Mul(num);
            if (num.Compare(Two) >= 0)
            {
                exp = exp.Add(fract);
                num = num.Div(Two, precisionDigits * 2);
            }
        }
        return exp.Round(precisionDigits);
    }

    public override string ToString()
    {
        string s = "";
        var n = new byte[num.Length];
        Array.Copy(num, n, num.Length);
        while (n.Length > 0)
            s = LeastSignificantDigit(ref n) + s;
        if (dp != 0)
            if (dp < 0)
                s += new string('0', -dp);
            else
            {
                if (dp > s.Length) s = new string('0', dp - s.Length) + s;
                s = s.Insert(s.Length - dp, ".");
            }

        s = s.TrimStart('0');
        if (s == "") s = "0"; else if (neg) s = "-" + s;
        return s;
    }

    private static int LeastSignificantDigit(ref byte[] n)
    {
        var mod = 0;
        for (int i = n.Length - 1; i >= 0; i--)
        {
            var num = n[i] + mod * 256;
            mod = num % 10;
            n[i] = (byte)(num / 10);
        }
        while (n.Length > 0 && n[n.Length - 1] == 0)
            Array.Resize(ref n, n.Length - 1);
        return mod;
    }

    private static int Compare(byte[] a, byte[] b)
    {
        if (a.Length > b.Length) return 1;
        if (a.Length < b.Length) return -1;
        for (int i = a.Length - 1; i >= 0; i--)
        {
            if (a[i] > b[i]) return 1;
            if (a[i] < b[i]) return -1;
        }
        return 0;
    }

    private int Compare(DecFloat b)
    {
        var x = this.Sub(b);
        if (x.num.Length == 0) return 0;
        return x.neg ? -1 : 1;
    }

    public DecFloat Round(int decimalPoints)
    {
        if (decimalPoints == 0)
            return Round();
        return Mul(new DecFloat(false, new byte[] { 1 }, -decimalPoints)).Round().Mul(new DecFloat(false, new byte[] { 1 }, decimalPoints));
    }

    public DecFloat Round()
    {
        string s = this.ToString();
        int i = s.IndexOf('.');
        if (i < 0) return this;
        var roundUp = s[i + 1] >= '5';
        var num = new DecFloat(s.Substring(0, i));
        if (roundUp) num = num.Add(One);
        return num;
    }

    public DecFloat Clone()
    {
        var num = new byte[this.num.Length];
        Array.Copy(this.num, num, this.num.Length);
        return new DecFloat(neg, num, dp);
    }
}