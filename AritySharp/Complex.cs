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
    public double Real;

    /** The imaginary component. */
    public double Imaginary;

    /** Constructor taking the real and imaginary components. */
    public Complex(double real = 0.0, double imaginary = 0.0)
        => Set(real, imaginary);

    /** Copy constructor. */
    public Complex(Complex o)
        => Set(o);

    /** Sets the real and imaginary components. */
    public Complex Set(double real, double imaginary)
    {
        this.Real = real;
        this.Imaginary = imaginary;
        return this;
    }

    /** Sets from other object (copy). */
    public Complex Set(Complex complex)
    {
        this.Real = complex.Real;
        this.Imaginary = complex.Imaginary;
        return this;
    }

    /** Formats the real and imaginary part into a string. */
    public override string ToString() => Imaginary == 0 ? $"{Real}" : $"({Real}, {Imaginary})";

    /**
     * Returns the real part if the imaginary part is zero, otherwise returns NaN.
     */
    public double AsReal => Imaginary == 0 ? Real : double.NaN;// return Math.abs(im) < 1E-30 ? re : double.NaN;

    /**
     * Complex conjugate (negates imaginary).
     */
    public Complex Conjugate() => Set(Real, -Imaginary);

    /**
     * Negate, i.e. multiply with -1.
     */
    public Complex Negate() => Set(-Real, -Imaginary);

    /**
     * True if this is an infinite (and not a NaN).
     */
    public bool IsInfinite => double.IsInfinity(Real) || double.IsInfinity(Imaginary) && !IsNaN;

    /**
     * True if both the real and the imaginary parts
     * are finite (not infinite and not NaN).
     */
    public bool IsFinite => !IsInfinite && !IsNaN;

    /**
     * True if either real or imaginary is NaN.
     */
    public bool IsNaN => double.IsNaN(Real) || double.IsNaN(Imaginary);

    //public bool Equals(Complex o) 
    //    => ((this.re == o.re) 
    //    || (double.IsNaN(this.re) && double.IsNaN(o.re))) && ((this.im == o.im) 
    //    || (double.IsNaN(this.im) && double.IsNaN(o.im)));

    /**
     * The argument (angle) in polar coordinates.
     */
    public double Arg => Math.Atan2(Imaginary, Real);

    /**
     * The absolute value (length in polar coordinates).
     */
    public double Abs()
    {
        var a = Math.Abs(Real);
        var b = Math.Abs(Imaginary);
        if (a == 0 || b == 0) return a + b;
        var aGreater = a > b;
        var q = aGreater ? b / a : a / b;
        return (aGreater ? a : b) * Math.Sqrt(1 + q * q);
    }

    /**
     * The absolute value squared.
     * re^2 + im^2
     */
    public double Abs2() => Real * Real + Imaginary * Imaginary;

    /**
     * Addition.
     * Modifies and returns this.
     */
    public Complex Add(Complex complex)
    {
        var ulp = Util.MathUlp(Real);
        this.Real += complex.Real;
        this.Imaginary += complex.Imaginary;
        // hack for "1.1-1-.1"
        if (Math.Abs(Real) < ulp * 1024)
            this.Real = 0;
        return this;
    }

    /**
     * Substraction.
     */
    public Complex Sub(Complex complex)
    {
        var ulp = Util.MathUlp(Real);
        this.Real -= complex.Real;
        this.Imaginary -= complex.Imaginary;
        // hack for "1.1-1-.1"
        if (Math.Abs(Real) < ulp * 1024)
            this.Real = 0;
        return this;
    }

    public Complex Mul(double real)
    {
        this.Real *= real;
        this.Imaginary *= real;
        return this;
    }

    /**
     * Multiplication.
     */
    public Complex Mul(Complex complex)
    {
        double a = this.Real, b = this.Imaginary, c = complex.Real, d = complex.Imaginary;
        if (b == 0 && d == 0)
            return Set(a * c, 0);

        var mre = a * c - b * d;
        var mim = a * d + b * c;

        if (!Set(mre, mim).IsNaN) return this;
        if (Set(a, b).IsInfinite)
        {
            NormalizeInfinity();
            a = Real;
            b = Imaginary;
        }

        if (complex.IsInfinite)
        {
            Set(c, d).NormalizeInfinity();
            c = Real;
            d = Imaginary;
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
    public Complex Div(Complex complex)
    {
        double c = complex.Real;
        double d = complex.Imaginary;
        if (Imaginary == 0 && d == 0)
        {
            return Set(Real / c, 0);
        }
        if (complex.IsInfinite && IsFinite)
        {
            return Set(0, 0);
        }
        if (d == 0)
        {
            if (Real == 0)
            {
                return Set(0, Imaginary / c);
            }
            return Set(Real / c, Imaginary / c);
        }
        if (c == 0)
        {
            return Set(Imaginary / d, -Real / d);
        }
        if (Math.Abs(c) > Math.Abs(d))
        {
            double q = d / c;
            double down = c + d * q;
            return Set((Real + Imaginary * q) / down, (Imaginary - Real * q) / down);
        }
        else
        {
            double q = c / d;
            double down = c * q + d;
            return Set((Real * q + Imaginary) / down, (Imaginary * q - Real) / down);
        }
    }

    /**
     * Complex square root.
     */
    public Complex Sqrt()
    {
        if (Imaginary == 0)
        {
            if (!(Real < 0))
            {
                Set(Math.Sqrt(Real), 0);
            }
            else
            {
                Set(0, Math.Sqrt(-Real));
            }
        }
        else
        {
            double t = Math.Sqrt((Math.Abs(Real) + Abs()) / 2);
            if (Real >= 0)
            {
                Set(t, Imaginary / (t + t));
            }
            else
            {
                Set(Math.Abs(Imaginary) / (t + t), Imaginary >= 0 ? t : -t);
            }
        }
        return this;
    }

    /**
     * Complex modulo (integer division remainder).
     */
    public Complex Mod(Complex complex)
    {
        double a = Real;
        double b = Imaginary;
        if (b == 0 && complex.Imaginary == 0)
        {
            return Set(a % complex.Real, 0);
        }
        return Div(complex).Set((int)Math.Round(Real), (int)Math.Round(Imaginary)).Mul(complex).Set(a - Real, b - Imaginary);
    }

    /**
     * Complex GCD, Greatest Common Denominator.
     */
    public Complex Gcd(Complex complex)
    {
        if (Imaginary == 0 && complex.Imaginary == 0)
        {
            return Set(MoreMath.Gcd(Real, complex.Real), 0);
        }
        Complex y = new(complex);
        double xabs2 = Abs2();
        double yabs2 = y.Abs2();
        while (xabs2 < yabs2 * 1e30)
        {
            double yRe = y.Real;
            double yIm = y.Imaginary;
            y.Set(Mod(y));
            Set(yRe, yIm);
            xabs2 = yabs2;
            yabs2 = y.Abs2();
        }
        // normalize to positive & larger real
        if (Math.Abs(Real) < Math.Abs(Imaginary))
        {
            Set(-Imaginary, Real);
        }
        if (Real < 0)
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
        if (Imaginary == 0 && !(Real < 0))
        {
            return Set(Math.Log(Real), 0);
        }
        double newIm = Math.Atan2(Imaginary, Real);
        return Set(Math.Log(Abs()), newIm);
    }

    /**
     * Complex exponential.
     */
    public Complex Exp()
    {
        var expRe = Math.Exp(Real);
        return Imaginary == 0 ? Set(expRe, 0) : Set(expRe * MoreMath.Cos(Imaginary), expRe * MoreMath.Sin(Imaginary));
    }

    /**
     * Complex square (x^2).
     */
    public Complex Square() => Set(Real * Real - Imaginary * Imaginary, 2 * Real * Imaginary);

    /**
     * Complex power (x^y == exp(y*log(x))).
     */
    public Complex Pow(Complex y)
    {
        if (y.Imaginary == 0)
        {
            if (y.Real == 0)
            {
                // anything^0==1, including NaN^0 (!)
                return Set(1, 0);
            }
            if (Imaginary == 0)
            {
                var res = Math.Pow(Real, y.Real);
                //if (res == res)
                if(!double.IsNaN(res))
                { // !NaN
                    return Set(res, 0);
                }
            }
            if (y.Real == 2)
            {
                return Square();
            }
            if (y.Real ==0.5)
            {
                return Sqrt();
            }
            double p = Math.Pow(Abs2(), y.Real / 2.0);
            double a = Arg * y.Real;
            return Set(p * MoreMath.Cos(a), p * MoreMath.Sin(a));
        }
        if (Imaginary == 0 && Real > 0)
        {
            double a = Math.Pow(Real, y.Real);
            return Set(0, y.Imaginary * Math.Log(Real)).Exp().Set(a * Real, a * Imaginary);
        }
        return Log().Set(y.Real * Real - y.Imaginary * Imaginary, y.Real * Imaginary + y.Imaginary * Real).Exp();
    }

    /**
     * Complex lgamma (log Gamma).
     */
    public Complex Lgamma()
    {
        var sumRe = 0.99999999999999709182;
        var sumIm = 0.0;
        var down = Real * Real + Imaginary * Imaginary;
        var xplusk = Real;
        var GAMMA = MoreMath.GAMMA;
        for (int k = 0; k < GAMMA.Length; ++k)
        {
            ++xplusk;
            down += xplusk + xplusk - 1;
            var cc = GAMMA[k];
            sumRe += cc * xplusk / down;
            sumIm -= cc * Imaginary / down;
        }

        var a = Real + 0.5;
        var tmpRe = Real + 5.2421875;
        var saveIm = Imaginary;

        this.Real = tmpRe;

        Log();
        double termRe = a * Real - saveIm * Imaginary + 0.9189385332046727418 - tmpRe;
        double termIm = a * Imaginary + saveIm * Real - saveIm;

        Set(sumRe, sumIm).Log();
        this.Real += termRe;
        this.Imaginary += termIm;
        return this;
    }

    /**
     * Complex factorial, based on lgamma().
     */
    public Complex Factorial() => this.Imaginary == 0 ? Set(MoreMath.Factorial(this.Real), 0) : Lgamma().Exp();

    /** sin(a+ib) = sin(a)*cosh(b) + i*cos(a)*sinh(b). */
    public Complex Sin() => this.Imaginary == 0 ? Set(MoreMath.Sin(this.Real), 0) : Set(MoreMath.Sin(this.Real) * Math.Cosh(this.Imaginary), MoreMath.Cos(Real) * Math.Sinh(this.Imaginary));

    /** sinh(a+ib) = sinh(a)*cos(b) + i*cosh(a)*sin(b). */
    public Complex Sinh() => this.Imaginary == 0 ? Set(Math.Sinh(this.Real), 0) : Swap().Sin().Swap();

    /** cos(a+ib) = cos(a)cosh(b) - i*sin(a)sinh(b). */
    public Complex Cos() => this.Imaginary == 0 ? Set(MoreMath.Cos(this.Real), 0)
            : Set(MoreMath.Cos(this.Real) * Math.Cosh(Imaginary), -MoreMath.Sin(this.Real) * Math.Sinh(this.Imaginary));

    /** cosh(a+ib) = cosh(a)cos(b) + i*sinh(a)sin(b). */
    public Complex Cosh() => this.Imaginary == 0 ? Set(Math.Cosh(this.Real), 0) : Swap().Cos().Conjugate();

    /** tan(a+ib) = sin(2a)/(cos(2a)+cosh(2b) + i*sinh(2b)/(cos(2a)+cosh(2b)). */
    public Complex Tan()
    {
        if (this.Imaginary == 0)
            return Set(MoreMath.Tan(this.Real), 0);
        var aa = this.Real + this.Real;
        var bb = this.Imaginary + this.Imaginary;
        var down = MoreMath.Cos(aa) + Math.Cosh(bb);
        return Set(MoreMath.Sin(aa) / down, Math.Sinh(bb) / down);
    }

    /** tanh(a+ib) = sinh(2a)/(cosh(2a) + cos(2b)) + i*sin(2b)/(cosh(2a)+cos(2b)). */
    public Complex Tanh() 
        => this.Imaginary == 0 ? Set(Math.Tanh(this.Real), 0) : Swap().Tan().Swap();

    /** asin(x) = -i*log(sqrt(1-x^2)+ix). */
    public Complex Asin()
    {
        if (Imaginary == 0 && Math.Abs(this.Real) <= 1)
            return Set(Math.Asin(this.Real), 0);
        var saveA = this.Real;
        var saveB = this.Imaginary;
        return Sqrt1z().Set(this.Real - saveB, this.Imaginary + saveA).Log().Set(this.Imaginary, -this.Real);
    }

    /** acos(x) = -i*log(x + i*sqrt(1-x^2)). */
    public Complex Acos()
    {
        if (Imaginary == 0 && Math.Abs(this.Real) <= 1)
            return Set(Math.Acos(this.Real), 0);
        var saveA = this.Real;
        var saveB = this.Imaginary;
        return Sqrt1z().Set(saveA - this.Imaginary, saveB + this.Real).Log().Set(this.Imaginary, -this.Real);
    }

    /** atan(x) = i/2 * log((i+x)/(i-x)). */
    public Complex Atan()
    {
        if (this.Imaginary == 0)
            return Set(Math.Atan(this.Real), 0);
        var a2 = this.Real * this.Real;
        var b2 = this.Imaginary * this.Imaginary;
        var down = a2 + b2 - this.Imaginary - this.Imaginary + 1;
        return Set(-(a2 + b2 - 1) / down, -(this.Real + this.Real) / down).Log().Set(-this.Imaginary / 2, this.Real / 2);
    }

    /** asinh(x) = log(x+sqrt(x^2+1)). */
    public Complex Asinh()
    {
        if (this.Imaginary == 0)
            return Set(MoreMath.Asinh(this.Real), 0);
        var a = this.Real;
        var b = this.Imaginary;
        return Set(this.Real * this.Real - this.Imaginary * this.Imaginary + 1.0, 2.0 * this.Real * this.Imaginary).Sqrt().Set(this.Real + a, this.Imaginary + b).Log();
    }

    /** acosh(x) = log(x+sqrt(x^2-1)). */
    public Complex Acosh()
    {
        if (this.Imaginary == 0 && this.Real >= 1)
            return Set(MoreMath.Acosh(this.Real), 0);
        var a = this.Real;
        var b = this.Imaginary;
        return Set(this.Real * this.Real - this.Imaginary * this.Imaginary - 1.0, 2.0 * this.Real * this.Imaginary).Sqrt().Set(this.Real + a, this.Imaginary + b).Log();
    }

    /** atanh(x) = log((1+x)/(1-x))/2. */
    public Complex Atanh()
    {
        if (this.Imaginary == 0)
            return Set(MoreMath.Atanh(this.Real), 0);
        var a2 = this.Real * this.Real;
        var b2 = this.Imaginary * this.Imaginary;
        var down = a2 + 1.0 - this.Real - this.Real;
        return Set((1 - a2 - b2) / down, (this.Imaginary + this.Imaginary) / down).Log().Set(this.Real / 2.0, this.Imaginary / 2.0);
    }

    /** Combinations: C(n, k) == exp(lgamma(n) - lgamma(k) - lgamma(n-k)). */
    public Complex Combinations(Complex o)
    {
        if (this.Imaginary == 0 && o.Imaginary == 0)
            return Set(MoreMath.Combinations(Real, o.Real), 0);

        var a = this.Real;
        var b = this.Imaginary;

        Lgamma();
        var c = this.Real;
        var d = this.Imaginary;

        Set(o).Lgamma();
        var e = this.Real;
        var f = this.Imaginary;

        Set(a - o.Real, b - o.Imaginary).Lgamma();
        return Set(c - e - Real, d - f - Imaginary).Exp();
    }

    /** Permutations: P(n, k) == exp(lgamma(n) - lgamma(n-k)). */
    public Complex Permutations(Complex o)
    {
        if (this.Imaginary == 0 && o.Imaginary == 0)
            return Set(MoreMath.Permutations(this.Real, o.Real), 0);

        var a = this.Real;
        var b = this.Imaginary;

        Lgamma();
        var c = this.Real;
        var d = this.Imaginary;

        Set(a - o.Real, b - o.Imaginary).Lgamma();
        return Set(c - Real, d - Imaginary).Exp();
    }

    /**
     * Swaps real and imaginary.
     */
    private Complex Swap() => this.Set(Imaginary, Real);

    /**
     * Normalizes the finite components of an infinity to zero.
     * Used internally by mul().
     * Assumes this is infinite.
     */
    private Complex NormalizeInfinity()
    {
        // assumes this.isInfinite()
        if (!double.IsInfinity(this.Imaginary))
        {
            this.Imaginary = 0;
        }
        else if (!double.IsInfinity(this.Real))
        {
            this.Real = 0;
        }
        return this;
    }

    /** sqrt(1-x^2) */
    private Complex Sqrt1z()
        => Set(1.0 - this.Real * this.Real + this.Imaginary * this.Imaginary, -2.0 * this.Real * this.Imaginary).Sqrt();

    public override bool Equals(object? obj) => obj is Complex c && Real == c.Real && this.Imaginary == c.Imaginary;
    public override int GetHashCode() => this.Real.GetHashCode() ^ this.Imaginary.GetHashCode();
}
