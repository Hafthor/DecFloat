namespace DecFloat;

public class DecFloat
{
    private readonly bool neg;    // true if negative
    private readonly uint[] num;  // number stored in LSB -> MSB order
    private readonly int dp;      // number of decimal places

    public static readonly DecFloat Zero = new DecFloat(false, Array.Empty<uint>(), 0);
    public static readonly DecFloat One = new DecFloat(false, new uint[] { 1 }, 0);
    public static readonly DecFloat Two = new DecFloat(false, new uint[] { 2 }, 0);
    public static readonly DecFloat Half = new DecFloat(false, new uint[] { 5 }, 1);

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

    public DecFloat(bool neg, uint[] num, int dp)
    {
        this.neg = neg;
        this.num = num;
        this.dp = dp;
    }

    public static uint[] Dec2Bin(string s)
    {
        var b = Array.Empty<uint>();
        foreach (var c in s)
            AddDigit(c - '0', ref b);
        return b;
    }

    public static void AddDigit(int d, ref uint[] b)
    {
        uint c = (uint)d;
        for (int i = 0; i < b.Length; i++)
        {
            var r = b[i] * 10l + c;
            c = (uint)(r / 4294967296);
            b[i] = (uint)(r % 4294967296);
        }
        if (c > 0)
        {
            Array.Resize(ref b, b.Length + 1);
            b[b.Length - 1] = (uint)c;
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

    private static uint[] Add(uint[] a, uint[] b)
    {
        var r = new uint[Math.Max(a.Length, b.Length)];
        uint c = 0;
        for (int i = 0; i < r.Length; i++)
        {
            var ai = i < a.Length ? a[i] : 0;
            var bi = i < b.Length ? b[i] : 0;
            var ri = (long)ai + bi + c;
            c = (uint)(ri / 4294967296);
            r[i] = (uint)(ri % 4294967296);
        }
        if (c > 0)
        {
            Array.Resize(ref r, r.Length + 1);
            r[r.Length - 1] = c;
        }
        return r;
    }

    private static uint[] Sub(uint[] a, uint[] b)
    {
        var r = new uint[Math.Max(a.Length, b.Length)];
        uint c = 0;
        for (int i = 0; i < r.Length; i++)
        {
            var ai = i < a.Length ? a[i] : 0;
            var bi = i < b.Length ? b[i] : 0;
            var ri = (long)ai - bi - c;
            c = 0; while (ri < 0) { ri += 4294967296; c++; }
            r[i] = (uint)ri;
        }
        if (c != 0) throw new Exception("unexpected borrow");
        var rl = r.Length;
        while (rl > 0 && r[rl - 1] == 0) rl--;
        if (rl != r.Length) Array.Resize(ref r, rl);
        return r;
    }

    public DecFloat Mul(DecFloat b)
    {
        var r = Array.Empty<uint>();
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

    private uint[] Mul(uint b) => Mul(num, b);

    private static uint[] Mul(uint[] num, uint b)
    {
        var r = new uint[num.Length];
        uint c = 0;
        for (int i = 0; i < r.Length; i++)
        {
            var ai = num[i];
            var ri = ((ulong)ai * b) + c;
            c = (uint)(ri / 4294967296);
            r[i] = (uint)(ri % 4294967296);
        }
        if (c > 0)
        {
            Array.Resize(ref r, r.Length + 1);
            r[r.Length - 1] = c;
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

        uint[] anum = new uint[this.num.Length], bnum = b.num;
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

        uint[][] divs = new uint[11][];
        divs[0] = Array.Empty<uint>();
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
        var limit = new DecFloat(false, new uint[] { 1 }, precisionDigits + 4);
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

    public DecFloat Sqrt(int precisionDigits)
    {
        if (this.Compare(Zero) < 0)
            throw new ArgumentOutOfRangeException("Cannot take square root of negative number");
        if (this.Compare(One) == 0) return this;
        var h = this;
        var a = Zero;
        if (h.Compare(One) < 0) h = One;
        
        var limit = this.Mul(new DecFloat(false, new uint[] { 1 }, precisionDigits));
        for (; ; )
        {
            var ah = a.Add(h);
            var sqr = ah.Mul(ah);
            var cmp = sqr.Sub(this);
            if (cmp.num.Length == 0) return ah;
            if (cmp.neg) a = ah;
            h = h.Mul(Half);
            if (!cmp.neg && cmp.Compare(limit) < 0) break;
        }

        return a.Round(precisionDigits);
    }

    public DecFloat Exp2(int precisionDigits)
    {
        var cmp = this.Compare(Zero);
        if (cmp == 0) return One;
        if (cmp > 0) // positive exp
        {
            var intexp = Two;
            var intpow = One;
            while (intpow.Compare(this) < 0)
            {
                intexp = intexp.Mul(intexp);
                intpow = intpow.Add(intpow);
            }
            var remainingExp = this;
            var answer = One;
            for (int iter = 0; iter < precisionDigits * 4; iter++)
            {
                var cmp2 = intpow.Compare(remainingExp);
                if (cmp2 <= 0)
                {
                    answer = answer.Mul(intexp);
                    remainingExp = remainingExp.Sub(intpow);
                }
                if (cmp == 0) return answer;
                intexp = intexp.Sqrt(precisionDigits * 2);
                intpow = intpow.Mul(Half);
            }
            return answer.Round(precisionDigits);
        }
        else // negative exp
            return One.Div(new DecFloat(!neg, num, dp).Exp2(precisionDigits), precisionDigits);
    }

    public static DecFloat Pi(int precisionDigits)
    {
        // Nilakantha
        var pi = new DecFloat("3");
        var four = new DecFloat("4");
        var limit = new DecFloat(false, new uint[] { 1 }, precisionDigits + 5);
        var a = Two;
        var b = a.Add(One);
        var c = b.Add(One);
        var d = c.Add(One);
        var e = d.Add(One);
        for (; ; )
        {
            var add = four.Div(a.Mul(b).Mul(c), precisionDigits * 2);
            pi = pi.Add(add);
            var sub = four.Div(c.Mul(d).Mul(e), precisionDigits * 2);
            pi = pi.Sub(sub);
            if (add.Compare(limit) < 0) break;
            a = e;
            b = a.Add(One);
            c = b.Add(One);
            d = c.Add(One);
            e = d.Add(One);
        }
        return pi.Round(precisionDigits);
    }

    public override string ToString()
    {
        string s = "";
        var n = new uint[num.Length];
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

    private static uint LeastSignificantDigit(ref uint[] n)
    {
        uint mod = 0;
        for (int i = n.Length - 1; i >= 0; i--)
        {
            var num = n[i] + mod * 4294967296;
            mod = (uint)(num % 10);
            n[i] = (uint)(num / 10);
        }
        while (n.Length > 0 && n[n.Length - 1] == 0)
            Array.Resize(ref n, n.Length - 1);
        return mod;
    }

    private static int Compare(uint[] a, uint[] b)
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
        return Mul(new DecFloat(false, new uint[] { 1 }, -decimalPoints)).Round().Mul(new DecFloat(false, new uint[] { 1 }, decimalPoints));
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
        var num = new uint[this.num.Length];
        Array.Copy(this.num, num, this.num.Length);
        return new DecFloat(neg, num, dp);
    }
}