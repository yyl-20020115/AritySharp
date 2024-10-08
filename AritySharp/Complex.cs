/*
 * Copyright (C) 2008-2009 Mihai Preda.
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace AritySharp;

/**
 * A complex value, composed of the real part (re) and the imaginary part (im).
 * All the methods that return a Complex (such as add(), mul(), etc)
 * modify the object on which they are called and return it (this), in order
 * to avoid new object creation.
 */
public class Complex
{
    /** The real component. */
    public double re;

    /** The imaginary component. */
    public double im;

    /** Constructor taking the real and imaginary components. */
    public Complex(double re = 0.0, double im = 0.0) => Set(re, im);

    /** Copy constructor. */
    public Complex(Complex o) => Set(o);

    /** Sets the real and imaginary components. */
    public Complex Set(double re, double im)
    {
        this.re = re;
        this.im = im;
        return this;
    }

    /** Sets from other object (copy). */
    public Complex Set(Complex o)
    {
        re = o.re;
        im = o.im;
        return this;
    }

    /** Formats the real and imaginary part into a string. */
    public override string ToString() => im == 0 ? $"{re}" : $"({re}, {im})";

    /**
     * Returns the real part if the imaginary part is zero, otherwise returns NaN.
     */
    public double AsReal => im == 0 ? re : double.NaN;// return Math.abs(im) < 1E-30 ? re : double.NaN;

    /**
     * Complex conjugate (negates imaginary).
     */
    public Complex Conjugate() => Set(re, -im);

    /**
     * Negate, i.e. multiply with -1.
     */
    public Complex Negate() => Set(-re, -im);

    /**
     * True if this is an infinite (and not a NaN).
     */
    public bool IsInfinite => double.IsInfinity(re) || double.IsInfinity(im) && !IsNaN;

    /**
     * True if both the real and the imaginary parts
     * are finite (not infinite and not NaN).
     */
    public bool IsFinite => !IsInfinite && !IsNaN;

    /**
     * True if either real or imaginary is NaN.
     */
    public bool IsNaN => double.IsNaN(re) || double.IsNaN(im);

    public bool Equals(Complex o) => ((re == o.re) || (re != re && o.re != o.re)) && ((im == o.im) || (im != im && o.im != o.im));

    /**
     * The argument (angle) in polar coordinates.
     */
    public double Arg => Math.Atan2(im, re);

    /**
     * The absolute value (length in polar coordinates).
     */
    public double Abs()
    {
        double a = Math.Abs(re);
        double b = Math.Abs(im);
        if (a == 0 || b == 0)
        {
            return a + b;
        }
        bool aGreater = a > b;
        double q = aGreater ? b / a : a / b;
        return (aGreater ? a : b) * Math.Sqrt(1 + q * q);
    }

    /**
     * The absolute value squared.
     * re^2 + im^2
     */
    public double Abs2() => re * re + im * im;

    /**
     * Addition.
     * Modifies and returns this.
     */
    public Complex Add(Complex o)
    {
        double ulp = Util.MathUlp(re);
        re += o.re;
        im += o.im;
        // hack for "1.1-1-.1"
        if (Math.Abs(re) < ulp * 1024)
        {
            re = 0;
        }
        return this;
    }

    /**
     * Substraction.
     */
    public Complex Sub(Complex o)
    {
        double ulp = Util.MathUlp(re);
        re -= o.re;
        im -= o.im;
        // hack for "1.1-1-.1"
        if (Math.Abs(re) < ulp * 1024)
        {
            re = 0;
        }
        return this;
    }

    public Complex Mul(double o)
    {
        re *= o;
        im *= o;
        return this;
    }

    /**
     * Multiplication.
     */
    public Complex Mul(Complex o)
    {
        double a = re, b = im, c = o.re, d = o.im;
        if (b == 0 && d == 0)
        {
            return Set(a * c, 0);
        }

        double mre = a * c - b * d;
        double mim = a * d + b * c;

        if (!Set(mre, mim).IsNaN)
        {
            return this;
        }

        if (Set(a, b).IsInfinite)
        {
            NormalizeInfinity();
            a = re;
            b = im;
        }

        if (o.IsInfinite)
        {
            Set(c, d).NormalizeInfinity();
            c = re;
            d = im;
        }

        if (b == 0)
        {
            if (d == 0)
            {
                return Set(a * c, 0);
            }
            if (c == 0)
            {
                return Set(0, a * d);
            }
            return Set(a * c, a * d);
        }

        if (a == 0)
        {
            if (c == 0)
            {
                return Set(-b * d, 0);
            }
            if (d == 0)
            {
                return Set(0, b * c);
            }
            return Set(-b * d, b * c);
        }

        if (d == 0)
        {
            return Set(a * c, b * c);
        }
        if (c == 0)
        {
            return Set(-b * d, a * d);
        }
        return Set(mre, mim);
    }

    /**
     * Division.
     */
    public Complex Div(Complex o)
    {
        double c = o.re;
        double d = o.im;
        if (im == 0 && d == 0)
        {
            return Set(re / c, 0);
        }
        if (o.IsInfinite && IsFinite)
        {
            return Set(0, 0);
        }
        if (d == 0)
        {
            if (re == 0)
            {
                return Set(0, im / c);
            }
            return Set(re / c, im / c);
        }
        if (c == 0)
        {
            return Set(im / d, -re / d);
        }
        if (Math.Abs(c) > Math.Abs(d))
        {
            double q = d / c;
            double down = c + d * q;
            return Set((re + im * q) / down, (im - re * q) / down);
        }
        else
        {
            double q = c / d;
            double down = c * q + d;
            return Set((re * q + im) / down, (im * q - re) / down);
        }
    }

    /**
     * Complex square root.
     */
    public Complex Sqrt()
    {
        if (im == 0)
        {
            if (!(re < 0))
            {
                Set(Math.Sqrt(re), 0);
            }
            else
            {
                Set(0, Math.Sqrt(-re));
            }
        }
        else
        {
            double t = Math.Sqrt((Math.Abs(re) + Abs()) / 2);
            if (re >= 0)
            {
                Set(t, im / (t + t));
            }
            else
            {
                Set(Math.Abs(im) / (t + t), im >= 0 ? t : -t);
            }
        }
        return this;
    }

    /**
     * Complex modulo (integer division remainder).
     */
    public Complex Mod(Complex o)
    {
        double a = re;
        double b = im;
        if (b == 0 && o.im == 0)
        {
            return Set(a % o.re, 0);
        }
        return Div(o).Set((int)Math.Round(re), (int)Math.Round(im)).Mul(o).Set(a - re, b - im);
    }

    /**
     * Complex GCD, Greatest Common Denominator.
     */
    public Complex Gcd(Complex o)
    {
        if (im == 0 && o.im == 0)
        {
            return Set(MoreMath.Gcd(re, o.re), 0);
        }
        Complex y = new Complex(o);
        double xabs2 = Abs2();
        double yabs2 = y.Abs2();
        while (xabs2 < yabs2 * 1e30)
        {
            double yRe = y.re;
            double yIm = y.im;
            y.Set(Mod(y));
            Set(yRe, yIm);
            xabs2 = yabs2;
            yabs2 = y.Abs2();
        }
        // normalize to positive & larger real
        if (Math.Abs(re) < Math.Abs(im))
        {
            Set(-im, re);
        }
        if (re < 0)
        {
            Negate();
        }
        return this;
    }

    /**
     * Complex natural logarithm.
     */
    public Complex Log()
    {
        if (im == 0 && !(re < 0))
        {
            return Set(Math.Log(re), 0);
        }
        double newIm = Math.Atan2(im, re);
        return Set(Math.Log(Abs()), newIm);
    }

    /**
     * Complex exponential.
     */
    public Complex Exp()
    {
        double expRe = Math.Exp(re);
        if (im == 0)
        {
            return Set(expRe, 0);
        }
        else
        {
            return Set(expRe * MoreMath.Cos(im), expRe * MoreMath.Sin(im));
        }
    }

    /**
     * Complex square (x^2).
     */
    public Complex Square() => Set(re * re - im * im, 2 * re * im);

    /**
     * Complex power (x^y == exp(y*log(x))).
     */
    public Complex Pow(Complex y)
    {
        if (y.im == 0)
        {
            if (y.re == 0)
            {
                // anything^0==1, including NaN^0 (!)
                return Set(1, 0);
            }
            if (im == 0)
            {
                double res = Math.Pow(re, y.re);
                if (res == res)
                { // !NaN
                    return Set(res, 0);
                }
            }
            if (y.re == 2)
            {
                return Square();
            }
            if (y.re == .5)
            {
                return Sqrt();
            }
            double p = Math.Pow(Abs2(), y.re / 2);
            double a = Arg * y.re;
            return Set(p * MoreMath.Cos(a), p * MoreMath.Sin(a));
        }
        if (im == 0 && re > 0)
        {
            double a = Math.Pow(re, y.re);
            return Set(0, y.im * Math.Log(re)).Exp().Set(a * re, a * im);
        }
        return Log().Set(y.re * re - y.im * im, y.re * im + y.im * re).Exp();
    }

    /**
     * Complex lgamma (log Gamma).
     */
    public Complex Lgamma()
    {
        double sumRe = 0.99999999999999709182, sumIm = 0;
        double down = re * re + im * im;
        double xplusk = re;
        double[] GAMMA = MoreMath.GAMMA;
        for (int k = 0; k < GAMMA.Length; ++k)
        {
            ++xplusk;
            down += xplusk + xplusk - 1;
            double cc = GAMMA[k];
            sumRe += cc * xplusk / down;
            sumIm -= cc * im / down;
        }

        double a = re + .5;
        double tmpRe = re + 5.2421875;
        double saveIm = im;

        re = tmpRe;

        Log();
        double termRe = a * re - saveIm * im + 0.9189385332046727418 - tmpRe;
        double termIm = a * im + saveIm * re - saveIm;

        Set(sumRe, sumIm).Log();
        re += termRe;
        im += termIm;
        return this;
    }

    /**
     * Complex factorial, based on lgamma().
     */
    public Complex Factorial() => im == 0 ? Set(MoreMath.Factorial(re), 0) : Lgamma().Exp();

    /** sin(a+ib) = sin(a)*cosh(b) + i*cos(a)*sinh(b). */
    public Complex Sin() => im == 0 ? Set(MoreMath.Sin(re), 0) : Set(MoreMath.Sin(re) * Math.Cosh(im), MoreMath.Cos(re) * Math.Sinh(im));

    /** sinh(a+ib) = sinh(a)*cos(b) + i*cosh(a)*sin(b). */
    public Complex Sinh() => im == 0 ? Set(Math.Sinh(re), 0) : Swap().Sin().Swap();

    /** cos(a+ib) = cos(a)cosh(b) - i*sin(a)sinh(b). */
    public Complex Cos() => im == 0 ? Set(MoreMath.Cos(re), 0)
            : Set(MoreMath.Cos(re) * Math.Cosh(im), -MoreMath.Sin(re) * Math.Sinh(im));

    /** cosh(a+ib) = cosh(a)cos(b) + i*sinh(a)sin(b). */
    public Complex Cosh() => im == 0 ? Set(Math.Cosh(re), 0) : Swap().Cos().Conjugate();

    /** tan(a+ib) = sin(2a)/(cos(2a)+cosh(2b) + i*sinh(2b)/(cos(2a)+cosh(2b)). */
    public Complex Tan()
    {
        if (im == 0)
        {
            return Set(MoreMath.Tan(re), 0);
        }
        double aa = re + re;
        double bb = im + im;
        double down = MoreMath.Cos(aa) + Math.Cosh(bb);
        return Set(MoreMath.Sin(aa) / down, Math.Sinh(bb) / down);
    }

    /** tanh(a+ib) = sinh(2a)/(cosh(2a) + cos(2b)) + i*sin(2b)/(cosh(2a)+cos(2b)). */
    public Complex Tanh() => im == 0 ? Set(Math.Tanh(re), 0) : Swap().Tan().Swap();

    /** asin(x) = -i*log(sqrt(1-x^2)+ix). */
    public Complex Asin()
    {
        if (im == 0 && Math.Abs(re) <= 1)
        {
            return Set(Math.Asin(re), 0);
        }
        double saveA = re;
        double saveB = im;
        return Sqrt1z().Set(re - saveB, im + saveA).Log().Set(im, -re);
    }

    /** acos(x) = -i*log(x + i*sqrt(1-x^2)). */
    public Complex Acos()
    {
        if (im == 0 && Math.Abs(re) <= 1)
        {
            return Set(Math.Acos(re), 0);
        }
        double saveA = re;
        double saveB = im;
        return Sqrt1z().Set(saveA - im, saveB + re).Log().Set(im, -re);
    }

    /** atan(x) = i/2 * log((i+x)/(i-x)). */
    public Complex Atan()
    {
        if (im == 0)
        {
            return Set(Math.Atan(re), 0);
        }
        double a2 = re * re;
        double b2 = im * im;
        double down = a2 + b2 - im - im + 1;
        return Set(-(a2 + b2 - 1) / down, -(re + re) / down).Log().Set(-im / 2, re / 2);
    }

    /** asinh(x) = log(x+sqrt(x^2+1)). */
    public Complex Asinh()
    {
        if (im == 0)
        {
            return Set(MoreMath.Asinh(re), 0);
        }
        double a = re;
        double b = im;
        return Set(re * re - im * im + 1, 2 * re * im).Sqrt().Set(re + a, im + b).Log();
    }

    /** acosh(x) = log(x+sqrt(x^2-1)). */
    public Complex Acosh()
    {
        if (im == 0 && re >= 1)
        {
            return Set(MoreMath.Acosh(re), 0);
        }
        double a = re;
        double b = im;
        return Set(re * re - im * im - 1, 2 * re * im).Sqrt().Set(re + a, im + b).Log();
    }

    /** atanh(x) = log((1+x)/(1-x))/2. */
    public Complex Atanh()
    {
        if (im == 0)
        {
            return Set(MoreMath.Atanh(re), 0);
        }
        double a2 = re * re;
        double b2 = im * im;
        double down = a2 + 1 - re - re;
        return Set((1 - a2 - b2) / down, (im + im) / down).Log().Set(re / 2, im / 2);
    }

    /** Combinations: C(n, k) == exp(lgamma(n) - lgamma(k) - lgamma(n-k)). */
    public Complex Combinations(Complex o)
    {
        if (im == 0 && o.im == 0)
        {
            return Set(MoreMath.Combinations(re, o.re), 0);
        }

        double a = re;
        double b = im;

        Lgamma();
        double c = re;
        double d = im;

        Set(o).Lgamma();
        double e = re;
        double f = im;

        Set(a - o.re, b - o.im).Lgamma();
        return Set(c - e - re, d - f - im).Exp();
    }

    /** Permutations: P(n, k) == exp(lgamma(n) - lgamma(n-k)). */
    public Complex Permutations(Complex o)
    {
        if (im == 0 && o.im == 0)
        {
            return Set(MoreMath.Permutations(re, o.re), 0);
        }

        double a = re;
        double b = im;

        Lgamma();
        double c = re;
        double d = im;

        Set(a - o.re, b - o.im).Lgamma();
        return Set(c - re, d - im).Exp();
    }

    /**
     * Swaps real and imaginary.
     */
    private Complex Swap() => Set(im, re);

    /**
     * Normalizes the finite components of an infinity to zero.
     * Used internally by mul().
     * Assumes this is infinite.
     */
    private Complex NormalizeInfinity()
    {
        // assumes this.isInfinite()
        if (!double.IsInfinity(im))
        {
            im = 0;
        }
        else if (!double.IsInfinity(re))
        {
            re = 0;
        } 
        return this;
    }

    /** sqrt(1-x^2) */
    private Complex Sqrt1z() => Set(1 - re * re + im * im, -2 * re * im).Sqrt();


    public override bool Equals(object? obj) => obj is Complex c && re == c.re && this.im == c.im;
    public override int GetHashCode() => this.re.GetHashCode() ^ this.im.GetHashCode();
}
