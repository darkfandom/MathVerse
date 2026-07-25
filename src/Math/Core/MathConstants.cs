namespace MathVerse.Math.Core;

/// <summary>
/// Provides well-known mathematical constants as <see cref="double"/> values.
/// </summary>
public static class MathConstants
{
    /// <summary>The ratio of a circle's circumference to its diameter (π ≈ 3.14159).</summary>
    public const double Pi = System.Math.PI;

    /// <summary>Euler's number, the base of natural logarithms (e ≈ 2.71828).</summary>
    public const double E = System.Math.E;

    /// <summary>The square root of 2 (≈ 1.41421).</summary>
    public const double Sqrt2 = 1.4142135623730951;

    /// <summary>The square root of 3 (≈ 1.73205).</summary>
    public const double Sqrt3 = 1.7320508075688772;

    /// <summary>The natural logarithm of 2 (≈ 0.69315).</summary>
    public const double Ln2 = 0.6931471805599453;

    /// <summary>The natural logarithm of 10 (≈ 2.30259).</summary>
    public const double Ln10 = 2.3025850929940457;

    /// <summary>The logarithm of e to base 2 (≈ 1.44270).</summary>
    public const double Log2E = 1.4426950408889634;

    /// <summary>The logarithm of e to base 10 (≈ 0.43429).</summary>
    public const double Log10E = 0.43429448190325176;

    /// <summary>The golden ratio φ = (1 + √5) / 2 (≈ 1.61803).</summary>
    public const double GoldenRatio = 1.6180339887498949;

    /// <summary>Positive infinity.</summary>
    public const double Infinity = double.PositiveInfinity;

    /// <summary>The smallest positive <see cref="double"/> value such that 1.0 + Epsilon ≠ 1.0.</summary>
    public const double Epsilon = double.Epsilon;
}
