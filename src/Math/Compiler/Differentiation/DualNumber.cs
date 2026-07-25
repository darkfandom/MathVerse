namespace MathVerse.Math.Compiler.Differentiation;

using System;
using System.Numerics;

/// <summary>Dual number for forward-mode automatic differentiation. Carries a real part and a dual (derivative) part.</summary>
public readonly record struct DualNumber(double Real, double Dual)
{
    /// <summary>Creates a dual number from a real value (derivative = 0).</summary>
    public static DualNumber FromValue(double x) => new(x, 0.0);

    /// <summary>Creates a dual number representing an infinitesimal (real = 0, dual = 1).</summary>
    public static DualNumber Derivative() => new(0.0, 1.0);

    /// <summary>Creates a dual number with a specific real and dual part.</summary>
    public static DualNumber Create(double real, double dual) => new(real, dual);

    /// <summary>Addition of two dual numbers.</summary>
    public static DualNumber operator +(DualNumber a, DualNumber b) =>
        new(a.Real + b.Real, a.Dual + b.Dual);

    /// <summary>Subtraction of two dual numbers.</summary>
    public static DualNumber operator -(DualNumber a, DualNumber b) =>
        new(a.Real - b.Real, a.Dual - b.Dual);

    /// <summary>Multiplication of two dual numbers: (a + a'ε)(b + b'ε) = ab + (ab' + a'b)ε.</summary>
    public static DualNumber operator *(DualNumber a, DualNumber b) =>
        new(a.Real * b.Real, a.Real * b.Dual + a.Dual * b.Real);

    /// <summary>Division of two dual numbers: (a + a'ε) / (b + b'ε) ≈ (a/b) + (a'b - ab')/b² · ε.</summary>
    public static DualNumber operator /(DualNumber a, DualNumber b)
    {
        double bSq = b.Real * b.Real;
        if (bSq == 0) throw new DivideByZeroException("Cannot divide dual numbers by zero.");
        return new(a.Real / b.Real, (a.Dual * b.Real - a.Real * b.Dual) / bSq);
    }

    /// <summary>Negation.</summary>
    public static DualNumber operator -(DualNumber a) => new(-a.Real, -a.Dual);

    /// <summary>Positive (identity).</summary>
    public static DualNumber operator +(DualNumber a) => a;

    /// <summary>Raising a dual number to a constant power: d/dx(x^n) = n·x^(n-1).</summary>
    public static DualNumber Pow(DualNumber a, double exponent) =>
        new(Math.Pow(a.Real, exponent), exponent * Math.Pow(a.Real, exponent - 1.0) * a.Dual);

    /// <summary>Sine: sin(a + a'ε) = sin(a) + a'·cos(a)·ε.</summary>
    public static DualNumber Sin(DualNumber a) =>
        new(Math.Sin(a.Real), a.Dual * Math.Cos(a.Real));

    /// <summary>Cosine: cos(a + a'ε) = cos(a) - a'·sin(a)·ε.</summary>
    public static DualNumber Cos(DualNumber a) =>
        new(Math.Cos(a.Real), -a.Dual * Math.Sin(a.Real));

    /// <summary>Tangent: tan(a + a'ε) = tan(a) + a'·sec²(a)·ε.</summary>
    public static DualNumber Tan(DualNumber a)
    {
        double cosA = Math.Cos(a.Real);
        return new(Math.Tan(a.Real), a.Dual / (cosA * cosA));
    }

    /// <summary>Arc sine: asin(a + a'ε) = asin(a) + a'/√(1-a²)·ε.</summary>
    public static DualNumber Asin(DualNumber a)
    {
        double denom = Math.Sqrt(1.0 - a.Real * a.Real);
        return new(Math.Asin(a.Real), a.Dual / denom);
    }

    /// <summary>Arc cosine: acos(a + a'ε) = acos(a) - a'/√(1-a²)·ε.</summary>
    public static DualNumber Acos(DualNumber a)
    {
        double denom = Math.Sqrt(1.0 - a.Real * a.Real);
        return new(Math.Acos(a.Real), -a.Dual / denom);
    }

    /// <summary>Arc tangent: atan(a + a'ε) = atan(a) + a'/(1+a²)·ε.</summary>
    public static DualNumber Atan(DualNumber a) =>
        new(Math.Atan(a.Real), a.Dual / (1.0 + a.Real * a.Real));

    /// <summary>Natural logarithm: ln(a + a'ε) = ln(a) + a'/a·ε.</summary>
    public static DualNumber Ln(DualNumber a) =>
        new(Math.Log(a.Real), a.Dual / a.Real);

    /// <summary>Logarithm base 10: log(a + a'ε) = log(a) + a'/(a·ln(10))·ε.</summary>
    public static DualNumber Log10(DualNumber a) =>
        new(Math.Log10(a.Real), a.Dual / (a.Real * Math.Log(10.0)));

    /// <summary>Exponential: exp(a + a'ε) = exp(a) + a'·exp(a)·ε.</summary>
    public static DualNumber Exp(DualNumber a)
    {
        double expVal = Math.Exp(a.Real);
        return new(expVal, a.Dual * expVal);
    }

    /// <summary>Square root: sqrt(a + a'ε) = sqrt(a) + a'/(2·sqrt(a))·ε.</summary>
    public static DualNumber Sqrt(DualNumber a)
    {
        double sqrtVal = Math.Sqrt(a.Real);
        return new(sqrtVal, a.Dual / (2.0 * sqrtVal));
    }

    /// <summary>Absolute value: abs(a + a'ε) = |a| + a'·sign(a)·ε.</summary>
    public static DualNumber Abs(DualNumber a) =>
        new(Math.Abs(a.Real), a.Dual * Math.Sign(a.Real));

    /// <summary>Ceiling: ceil(a + a'ε) = ceil(a) + 0·ε (not differentiable at integers).</summary>
    public static DualNumber Ceil(DualNumber a) => new(Math.Ceiling(a.Real), 0.0);

    /// <summary>Floor: floor(a + a'ε) = floor(a) + 0·ε (not differentiable at integers).</summary>
    public static DualNumber Floor(DualNumber a) => new(Math.Floor(a.Real), 0.0);

    /// <summary>Implicit conversion from double to dual number (derivative = 0).</summary>
    public static implicit operator DualNumber(double value) => new(value, 0.0);

    /// <summary>Gets the derivative component.</summary>
    public double DerivativeValue => Dual;

    /// <summary>Gets the real value.</summary>
    public double RealValue => Real;

    /// <summary>Multiplies by a scalar.</summary>
    public DualNumber Scale(double scalar) => new(Real * scalar, Dual * scalar);

    /// <inheritdoc />
    public override string ToString() => $"{Real} + {Dual}ε";
}
