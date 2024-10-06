/*
 * Copyright (C) 2007-2008 Mihai Preda.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Text;

namespace AritySharp;

/**
   Contains static helper methods for formatting double values.
 */
public static class Util
{
    public const int SIGNIFICAND_WIDTH = 53;
    public const long EXP_BIT_MASK = 0x7FF0000000000000L;
    public const int EXP_BIAS = 1023;
    public const int MAX_EXPONENT = 1023;
    public const double MIN_VALUE = 0.0000000000001E-1022; // 4.9e-324
    public static double PowerOfTwoD(int n) =>
        //assert(n >= Double.MIN_EXPONENT && n <= Double.MAX_EXPONENT);
        BitConverter.Int64BitsToDouble((((long)n + (long)EXP_BIAS) <<
                                        (SIGNIFICAND_WIDTH - 1))
                                       & EXP_BIT_MASK);
    /**
     * Minimum exponent a normalized {@code double} variable may
     * have.  It is equal to the value returned by
     * {@code Math.getExponent(Double.MIN_NORMAL)}.
     *
     * @since 1.6
     */
    public const int MIN_EXPONENT = -1022;
    public static int GetExponent(double d) =>
        /*
         * Bitwise convert d to long, mask out exponent bits, shift
         * to the right and then subtract out double's bias adjust to
         * get true exponent value.
         */
        (int)(((BitConverter.DoubleToInt64Bits(d) & EXP_BIT_MASK) >>
                      (SIGNIFICAND_WIDTH - 1)) - EXP_BIAS);

    public static double MathUlp(double d)
    {
        int exp = GetExponent(d);
        switch (exp)
        {
            case MAX_EXPONENT + 1:       // NaN or infinity
                return Math.Abs(d);

            case MIN_EXPONENT - 1:       // zero or subnormal
                return MIN_VALUE;

            default:
                //assert exp <= Double.MAX_EXPONENT && exp >= Double.MIN_EXPONENT;

                // ulp(x) is usually 2^(SIGNIFICAND_WIDTH-1)*(2^ilogb(x))
                exp = exp - (SIGNIFICAND_WIDTH - 1);
                if (exp >= MIN_EXPONENT)
                {
                    return PowerOfTwoD(exp);
                }
                else
                {
                    // return a subnormal result; left shift integer
                    // representation of Double.MIN_VALUE appropriate
                    // number of positions
                    return BitConverter.Int64BitsToDouble(1L <<
                    (exp - (MIN_EXPONENT - (SIGNIFICAND_WIDTH - 1))));
                }
        }
    }


    public static readonly int LEN_UNLIMITED = 100;
    public static readonly int FLOAT_PRECISION = -1;

    /** Returns a number which is an approximation of v (within maxError)
       and which has fewer digits in base-10).
       @param value the value to be approximated
       @param maxError the maximum deviation from value
       @return an approximation with a more compact base-10 representation.
    */
    public static double ShortApprox(double value, double maxError)
    {
        double v = Math.Abs(value);
        double tail = MoreMath.IntExp10(MoreMath.IntLog10(Math.Abs(maxError)));
        double ret = Math.Floor(v / tail + .5) * tail;
        return (value < 0) ? -ret : ret;
    }

    /**
      Returns an approximation with no more than maxLen chars.

      This method is not public, it is called through doubleToString, 
      that's why we can make some assumptions about the format of the string,
      such as assuming that the exponent 'E' is upper-case.

      @param str the value to truncate (e.g. "-2.898983455E20")
      @param maxLen the maximum number of characters in the returned string
      @return a truncation no longer then maxLen (e.g. "-2.8E20" for maxLen=7).
     */
    public static string SizeTruncate(string str, int maxLen)
    {
        if (maxLen == LEN_UNLIMITED)
        {
            return str;
        }
        int ePos = str.LastIndexOf('E');
        string tail = (ePos != -1) ? str[ePos..] : "";
        int tailLen = tail.Length;
        int headLen = str.Length - tailLen;
        int maxHeadLen = maxLen - tailLen;
        int keepLen = Math.Min(headLen, maxHeadLen);
        if (keepLen < 1 || (keepLen < 2 && str.Length > 0 && str[(0)] == '-'))
        {
            return str; // impossible to truncate
        }
        int dotPos = str.IndexOf('.');
        if (dotPos == -1)
        {
            dotPos = headLen;
        }
        if (dotPos > keepLen)
        {
            int exponent = (ePos != -1) ? int.TryParse(str[(ePos + 1)..],out var t)?t:0 : 0;
            int start = str[(0)] == '-' ? 1 : 0;
            exponent += dotPos - start - 1;
            string newStr = $"{str[..(start + 1)]}.{str[(start + 1)..headLen]}E{exponent}";
            return SizeTruncate(newStr, maxLen);

        }
        return str[..keepLen] + tail;
    }

    /**
       Rounds by dropping roundingDigits of double precision 
       (similar to 'hidden precision digits' on calculators),
       and formats to string.
       @param v the value to be converted to string
       @param roundingDigits the number of 'hidden precision' digits (e.g. 2).
       @return a string representation of v
     */
    public static string DoubleToString(double v, int roundingDigits)
    {
        double absv = Math.Abs(v);
        string str = roundingDigits == FLOAT_PRECISION ? ((float)absv).ToString() : (absv.ToString());
        var buf = new StringBuilder(str);
        int roundingStart = (roundingDigits <= 0 || roundingDigits > 13) ? 17 : (16 - roundingDigits);

        int ePos = str.LastIndexOf('E');
        int exp = (ePos != -1) ? (int.TryParse(str.Substring(ePos + 1),out var t)?t:0) : 0;
        if (ePos != -1)
        {
            buf.Length = (ePos);
        }
        int len = buf.Length;

        //remove dot
        int dotPos;
        for (dotPos = 0; dotPos < len && buf[(dotPos)] != '.';)
        {
            ++dotPos;
        }
        exp += dotPos;
        if (dotPos < len)
        {
            buf.Remove(dotPos,1);
            --len;
        }

        //round
        for (int p = 0; p < len && buf[(p)] == '0'; ++p)
        {
            ++roundingStart;
        }

        if (roundingStart < len)
        {
            if (buf[(roundingStart)] >= '5')
            {
                int p;
                for (p = roundingStart - 1; p >= 0 && buf[(p)] == '9'; --p)
                {
                    buf[p]= '0';
                }
                if (p >= 0)
                {
                    buf[p]= (char)(buf[(p)] + 1);
                }
                else
                {
                    buf.Insert(0, '1');
                    ++roundingStart;
                    ++exp;
                }
            }
            buf.Length=(roundingStart);
        }

        //re-insert dot
        if ((exp < -5) || (exp > 10))
        {
            buf.Insert(1, '.');
            --exp;
        }
        else
        {
            for (int i = len; i < exp; ++i)
            {
                buf.Append('0');
            }
            for (int i = exp; i <= 0; ++i)
            {
                buf.Insert(0, '0');
            }
            buf.Insert((exp <= 0) ? 1 : exp, '.');
            exp = 0;
        }
        len = buf.Length;

        //remove trailing dot and 0s.
        int tail;
        for (tail = len - 1; tail >= 0 && buf[(tail)] == '0'; --tail)
        {
            buf.Remove(tail,1);
        }
        if (tail >= 0 && buf[(tail)] == '.')
        {
            buf.Remove(tail,1);
        }

        if (exp != 0)
        {
            buf.Append('E').Append(exp);
        }
        if (v < 0)
        {
            buf.Insert(0, '-');
        }
        return buf.ToString();
    }

    /**
       Renders a real number to a string (for user display).
       @param maxLen the maximum total length of the resulting string
       @param rounding the number of readonly digits to round
     */
    public static string DoubleToString(double x, int maxLen, int rounding) => SizeTruncate(DoubleToString(x, rounding), maxLen);

    /**
       Renders a complex number to a string (for user display).
       @param maxLen the maximum total length of the resulting string
       @param rounding the number of readonly digits to round
     */
    public static string ComplexToString(Complex x, int maxLen, int rounding)
    {
        //System.out.println("" + x.re + ' ' + x.im);
        if (x.im == 0)
        {
            return DoubleToString(x.re, maxLen, rounding);
        }

        if (x.IsNaN)
        {
            return "NaN";
        }

        double xre = x.re, xim = x.im;
        if (x.IsInfinite)
        {
            if (!Double.IsInfinity(xre))
            {
                xre = 0;
            }
            else if (!Double.IsInfinity(xim))
            {
                xim = 0;
            }
        }
        if (xim == 0)
        {
            return DoubleToString(xre, maxLen, rounding);
        }

        // insert plus between re & im
        bool addPlus = xre != 0 && !(xim < 0);
        string sre = xre == 0 ? "" : DoubleToString(xre, rounding);
        string sim = DoubleToString(xim, rounding);
        string finalMultiply = Double.IsInfinity(xim) ? "*" : "";

        if (sim == ("1"))
        {
            sim = "";
        }
        if (sim == ("-1"))
        {
            sim = "-";
        }

        if (maxLen != LEN_UNLIMITED)
        {
            --maxLen; // for readonly "i"
            if (addPlus)
            {
                --maxLen;
            }
            maxLen -= finalMultiply.Length;

            int sreLen = sre.Length;
            int simLen = sim.Length;
            int reduce = sreLen + simLen - maxLen;
            if (reduce > 0)
            {
                int diff = Math.Abs(sreLen - simLen);
                int rShort = reduce > diff ? (reduce - diff) / 2 : 0;
                int rLong = rShort + Math.Min(reduce, diff);
                int sreTarget = sreLen;
                int simTarget = simLen;
                if (sreLen > simLen)
                {
                    sreTarget -= rLong;
                    simTarget -= rShort;
                }
                else
                {
                    sreTarget -= rShort;
                    simTarget -= rLong;
                }
                if (sreTarget + simTarget > maxLen)
                {
                    --simTarget;
                }
                sre = SizeTruncate(sre, sreTarget);
                sim = SizeTruncate(sim, simTarget);
            }
        }
        return sre + (addPlus ? "+" : "") + sim + finalMultiply + 'i';
    }
}
