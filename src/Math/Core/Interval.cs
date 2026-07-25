using System.Numerics;

namespace MathVerse.Math.Core;

/// <summary>
/// Represents a closed real-valued interval [Lower, Upper] and supports interval arithmetic.
/// This is a value type for efficient allocation-free interval computations.
/// </summary>
/// <remarks>
/// All arithmetic operations assume standard interval arithmetic rules.
/// Empty intervals represent impossible results (e.g., division by an interval containing zero).
/// </remarks>
public readonly record struct Interval : IFormattable
{
    /// <summary>Initializes an interval with the specified bounds.</summary>
    /// <param name="lower">The lower bound.</param>
    /// <param name="upper">The upper bound.</param>
    public Interval(double lower, double upper)
    {
        Lower = lower;
        Upper = lower > upper ? lower : upper;
        IsEmpty = lower > upper;
    }

    /// <summary>Gets the lower bound of the interval.</summary>
    public double Lower { get; }

    /// <summary>Gets the upper bound of the interval.</summary>
    public double Upper { get; }

    /// <summary>Gets whether this interval represents an empty set (lower &gt; upper).</summary>
    public bool IsEmpty { get; }

    /// <summary>Gets whether this interval represents a single point (Lower equals Upper).</summary>
    public bool IsPoint => !IsEmpty && System.Math.Abs(Lower - Upper) < double.Epsilon;

    /// <summary>Gets the length (measure) of the interval.</summary>
    public double Length => IsEmpty ? 0.0 : Upper - Lower;

    /// <summary>Gets the midpoint of the interval.</summary>
    public double Mid => IsEmpty ? double.NaN : (Lower + Upper) / 2.0;

    /// <summary>Gets whether the interval is the entire real line.</summary>
    public bool IsRealLine => Lower == double.NegativeInfinity && Upper == double.PositiveInfinity;

    /// <summary>Determines whether the interval contains the specified value.</summary>
    /// <param name="value">The value to test.</param>
    /// <returns><c>true</c> if <paramref name="value"/> is within [Lower, Upper]; otherwise <c>false</c>.</returns>
    public bool Contains(double value) =>
        !IsEmpty && value >= Lower && value <= Upper;

    /// <summary>Determines whether the interval fully contains another interval.</summary>
    /// <param name="other">The interval to test.</param>
    /// <returns><c>true</c> if <paramref name="other"/> is entirely within this interval; otherwise <c>false</c>.</returns>
    public bool Contains(Interval other) =>
        !IsEmpty && !other.IsEmpty && Lower <= other.Lower && other.Upper <= Upper;

    /// <summary>Determines whether this interval intersects with another interval.</summary>
    /// <param name="other">The other interval.</param>
    /// <returns><c>true</c> if the intervals share at least one point; otherwise <c>false</c>.</returns>
    public bool Intersects(Interval other) =>
        !IsEmpty && !other.IsEmpty && Lower <= other.Upper && other.Lower <= Upper;

    /// <summary>Computes the smallest interval containing both this and the other interval.</summary>
    /// <param name="other">The other interval.</param>
    /// <returns>The union (convex hull) of the two intervals.</returns>
    public Interval Union(Interval other)
    {
        if (IsEmpty) return other;
        if (other.IsEmpty) return this;

        return new Interval(System.Math.Min(Lower, other.Lower), System.Math.Max(Upper, other.Upper));
    }

    /// <summary>Computes the intersection of this and the other interval.</summary>
    /// <param name="other">The other interval.</param>
    /// <returns>The intersection, or <see cref="Empty"/> if they do not overlap.</returns>
    public Interval Intersection(Interval other)
    {
        if (IsEmpty || other.IsEmpty) return Empty;

        var lo = System.Math.Max(Lower, other.Lower);
        var hi = System.Math.Min(Upper, other.Upper);

        return lo > hi ? Empty : new Interval(lo, hi);
    }

    /// <summary>Adds two intervals using interval arithmetic.</summary>
    /// <param name="other">The interval to add.</param>
    /// <returns>The resulting interval [Lower+other.Lower, Upper+other.Upper].</returns>
    public Interval Add(Interval other)
    {
        if (IsEmpty || other.IsEmpty) return Empty;
        return new Interval(Lower + other.Lower, Upper + other.Upper);
    }

    /// <summary>Subtracts an interval using interval arithmetic.</summary>
    /// <param name="other">The interval to subtract.</param>
    /// <returns>The resulting interval [Lower-other.Upper, Upper-other.Lower].</returns>
    public Interval Subtract(Interval other)
    {
        if (IsEmpty || other.IsEmpty) return Empty;
        return new Interval(Lower - other.Upper, Upper - other.Lower);
    }

    /// <summary>Multiplies two intervals using interval arithmetic.</summary>
    /// <param name="other">The interval to multiply.</param>
    /// <returns>The resulting interval covering all possible products.</returns>
    public Interval Multiply(Interval other)
    {
        if (IsEmpty || other.IsEmpty) return Empty;

        var products = new[]
        {
            Lower * other.Lower,
            Lower * other.Upper,
            Upper * other.Lower,
            Upper * other.Upper
        };

        return new Interval(
            System.Math.Min(System.Math.Min(products[0], products[1]),
                            System.Math.Min(products[2], products[3])),
            System.Math.Max(System.Math.Max(products[0], products[1]),
                            System.Math.Max(products[2], products[3])));
    }

    /// <summary>Divides two intervals using interval arithmetic.</summary>
    /// <param name="other">The divisor interval.</param>
    /// <returns>The resulting interval. Returns <see cref="Empty"/> if the divisor contains zero.</returns>
    public Interval Divide(Interval other)
    {
        if (IsEmpty || other.IsEmpty) return Empty;
        if (other.Contains(0.0)) return Empty;

        var reciprocal = new Interval(1.0 / other.Upper, 1.0 / other.Lower);
        return Multiply(reciprocal);
    }

    /// <summary>Computes the negation of the interval.</summary>
    /// <returns>The interval [-Upper, -Lower].</returns>
    public Interval Negate()
    {
        if (IsEmpty) return Empty;
        return new Interval(-Upper, -Lower);
    }

    /// <summary>Gets an empty interval.</summary>
    public static Interval Empty => new(double.PositiveInfinity, double.NegativeInfinity);

    /// <summary>Gets the entire real line (-∞, +∞).</summary>
    public static Interval RealLine => new(double.NegativeInfinity, double.PositiveInfinity);

    /// <summary>Creates a degenerate interval representing a single point.</summary>
    /// <param name="value">The point value.</param>
    /// <returns>An interval where Lower equals Upper.</returns>
    public static Interval FromPoint(double value) => new(value, value);

    /// <summary>Creates an interval from explicit lower and upper bounds.</summary>
    /// <param name="lower">The lower bound.</param>
    /// <param name="upper">The upper bound.</param>
    /// <returns>A new interval. If lower &gt; upper, the interval is empty.</returns>
    public static Interval FromBounds(double lower, double upper) => new(lower, upper);

    /// <summary>Implements the addition operator.</summary>
    public static Interval operator +(Interval left, Interval right) => left.Add(right);

    /// <summary>Implements the subtraction operator.</summary>
    public static Interval operator -(Interval left, Interval right) => left.Subtract(right);

    /// <summary>Implements the multiplication operator.</summary>
    public static Interval operator *(Interval left, Interval right) => left.Multiply(right);

    /// <summary>Implements the division operator.</summary>
    public static Interval operator /(Interval left, Interval right) => left.Divide(right);

    /// <summary>Implements the unary negation operator.</summary>
    public static Interval operator -(Interval interval) => interval.Negate();

    /// <inheritdoc/>
    public override string ToString() => IsEmpty
        ? "∅"
        : $"[{Lower}, {Upper}]";

    /// <summary>Formats the interval using the specified format string and provider.</summary>
    /// <param name="format">The format string for the bounds (e.g., "F4").</param>
    /// <param name="formatProvider">The format provider.</param>
    /// <returns>A formatted string representation of the interval.</returns>
    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        if (IsEmpty) return "∅";

        var lo = Lower.ToString(format, formatProvider);
        var hi = Upper.ToString(format, formatProvider);
        return $"[{lo}, {hi}]";
    }
}
