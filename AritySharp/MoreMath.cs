/*
 * Copyright (C) 2006-2009 Mihai Preda.
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
 *
 */

namespace AritySharp;

public class MoreMath
{
    private static readonly double LOG2E = 1.4426950408889634074;

    public static double Asinh(double x) => (x < 0) ? -Asinh(-x) : Math.Log(x + x + 1 / (Math.Sqrt(x * x + 1) + x));

    public static double Acosh(double x) => Math.Log(x + x - 1 / (Math.Sqrt(x * x - 1) + x));

    public static double Atanh(double x) => (x < 0) ? -Atanh(-x) : 0.5 * Math.Log(1.0 + (x + x) / (1 - x));

    public static double Trunc(double x) => x >= 0 ? Math.Floor(x) : Math.Ceiling(x);

    public static double Gcd(double x, double y)
    {
        if (double.IsNaN(x) || double.IsNaN(y) ||
            double.IsInfinity(x) || double.IsInfinity(y))
        {
            return double.NaN;
        }
        x = Math.Abs(x);
        y = Math.Abs(y);
        while (x < y * 1e15)
        {
            double save = y;
            y = x % y;
            x = save;
        }
        return x;
    }

    public static readonly double[] GAMMA = [
        57.156235665862923517,
        -59.597960355475491248,
        14.136097974741747174,
        -0.49191381609762019978,
        .33994649984811888699e-4,
        .46523628927048575665e-4,
        -.98374475304879564677e-4,
        .15808870322491248884e-3,
        -.21026444172410488319e-3,
        .21743961811521264320e-3,
        -.16431810653676389022e-3,
        .84418223983852743293e-4,
        -.26190838401581408670e-4,
        .36899182659531622704e-5
    ];

    public static double Lgamma(double x)
    {
        double tmp = x + 5.2421875; //== 607/128. + .5;
        double sum = 0.99999999999999709182;
        for (int i = 0; i < GAMMA.Length; ++i)
        {
            sum += GAMMA[i] / ++x;
        }

        return 0.9189385332046727418 //LN_SQRT2PI, ln(sqrt(2*pi))
            + Math.Log(sum)
            + (tmp - 4.7421875) * Math.Log(tmp) - tmp
            ;
    }

    static readonly double[] FACT = {
        1.0,
        40320.0,
        2.0922789888E13,
        6.204484017332394E23,
        2.631308369336935E35,
        8.159152832478977E47,
        1.2413915592536073E61,
        7.109985878048635E74,
        1.2688693218588417E89,
        6.1234458376886085E103,
        7.156945704626381E118,
        1.8548264225739844E134,
        9.916779348709496E149,
        1.0299016745145628E166,
        1.974506857221074E182,
        6.689502913449127E198,
        3.856204823625804E215,
        3.659042881952549E232,
        5.5502938327393044E249,
        1.3113358856834524E267,
        4.7147236359920616E284,
        2.5260757449731984E302,
    };

    public static double Factorial(double x)
    {
        if (x < 0)
        { // x <= -1 ?
            return double.NaN;
        }
        if (x <= 170)
        {
            if (Math.Floor(x) == x)
            {
                int n = (int)x;
                double extra = x;
                switch (n & 7)
                {
                    case 7:
                        extra *= --x;
                        goto case 6;
                    case 6:
                        extra *= --x;
                        goto case 5;
                    case 5:
                        extra *= --x;
                        goto case 4;
                    case 4:
                        extra *= --x;
                        goto case 3;
                    case 3:
                        extra *= --x;
                        goto case 2;
                    case 2:
                        extra *= --x;
                        goto case 1;
                    case 1:
                        return FACT[n >> 3] * extra;
                    case 0:
                        return FACT[n >> 3];
                }
            }
        }
        return Math.Exp(Lgamma(x));
    }

    public static double Combinations(double n, double k)
    {
        if (n < 0 || k < 0) { return double.NaN; }
        if (n < k) { return 0; }
        if (Math.Floor(n) == n && Math.Floor(k) == k)
        {
            k = Math.Min(k, n - k);
            if (n <= 170 && 12 < k && k <= 170)
            {
                return Factorial(n) / Factorial(k) / Factorial(n - k);
            }
            else
            {
                double r = 1, diff = n - k;
                for (double i = k; i > .5 && r < double.PositiveInfinity; --i)
                {
                    r *= (diff + i) / i;
                }
                return r;
            }
        }
        else
        {
            return Math.Exp(Lgamma(n) - Lgamma(k) - Lgamma(n - k));
        }
    }

    public static double Permutations(double n, double k)
    {
        if (n < 0 || k < 0) { return double.NaN; }
        if (n < k) { return 0; }
        if (Math.Floor(n) == n && Math.Floor(k) == k)
        {
            if (n <= 170 && 10 < k && k <= 170)
            {
                return Factorial(n) / Factorial(n - k);
            }
            else
            {
                double r = 1, limit = n - k + .5;
                for (double i = n; i > limit && r < double.PositiveInfinity; --i)
                {
                    r *= i;
                }
                return r;
            }
        }
        else
        {
            return Math.Exp(Lgamma(n) - Lgamma(n - k));
        }
    }

    public static double Log2(double x) => Math.Log(x) * LOG2E;

    private static bool IsPiMultiple(double x)
    {
        // x % y == 0
        double d = x / Math.PI;
        return d == Math.Floor(d);
    }

    public static double Sin(double x) => IsPiMultiple(x) ? 0 : (double)Math.Sin(x);

    public static double Cos(double x) => IsPiMultiple(x - Math.PI / 2) ? 0 : Math.Cos(x);

    public static double Tan(double x) => IsPiMultiple(x) ? 0 : Math.Tan(x);

    public static int IntLog10(double x)
    {
        double d = Math.Log10(x);
        //an alternative implem is using a for loop.
        double f = Math.Floor(d);//return (int)log10(x);
        return double.IsNaN(f)?0: (int)f;
    }
    public static double IntExp10(int exp) => double.TryParse("1E" + exp, out var v) ? v : double.NaN;
}
