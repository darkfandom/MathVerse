namespace MathVerse.Math.DataScience.UnitsIntegration;

using System;

/// <summary>
/// Represents a physical dimension using base dimension exponents [M, L, T, I, Th, N, J]
/// corresponding to Mass, Length, Time, Current, Temperature, Amount, LuminousIntensity.
/// </summary>
public sealed class Dimension
{
    /// <summary>
    /// Gets the mass exponent.
    /// </summary>
    public double Mass { get; }

    /// <summary>
    /// Gets the length exponent.
    /// </summary>
    public double Length { get; }

    /// <summary>
    /// Gets the time exponent.
    /// </summary>
    public double Time { get; }

    /// <summary>
    /// Gets the electric current exponent.
    /// </summary>
    public double Current { get; }

    /// <summary>
    /// Gets the thermodynamic temperature exponent.
    /// </summary>
    public double Temperature { get; }

    /// <summary>
    /// Gets the amount of substance exponent.
    /// </summary>
    public double Amount { get; }

    /// <summary>
    /// Gets the luminous intensity exponent.
    /// </summary>
    public double LuminousIntensity { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Dimension"/> class with the specified base exponents.
    /// </summary>
    /// <param name="mass">Mass exponent.</param>
    /// <param name="length">Length exponent.</param>
    /// <param name="time">Time exponent.</param>
    /// <param name="current">Current exponent.</param>
    /// <param name="temperature">Temperature exponent.</param>
    /// <param name="amount">Amount exponent.</param>
    /// <param name="luminousIntensity">Luminous intensity exponent.</param>
    public Dimension(
        double mass = 0.0,
        double length = 0.0,
        double time = 0.0,
        double current = 0.0,
        double temperature = 0.0,
        double amount = 0.0,
        double luminousIntensity = 0.0)
    {
        Mass = mass;
        Length = length;
        Time = time;
        Current = current;
        Temperature = temperature;
        Amount = amount;
        LuminousIntensity = luminousIntensity;
    }

    /// <summary>
    /// Gets the dimensionless identity (all exponents zero).
    /// </summary>
    public static Dimension Dimensionless => new();

    /// <summary>
    /// Gets the mass dimension [M].
    /// </summary>
    public static Dimension MassDimension => new(mass: 1.0);

    /// <summary>
    /// Gets the length dimension [L].
    /// </summary>
    public static Dimension LengthDimension => new(length: 1.0);

    /// <summary>
    /// Gets the time dimension [T].
    /// </summary>
    public static Dimension TimeDimension => new(time: 1.0);

    /// <summary>
    /// Gets the current dimension [I].
    /// </summary>
    public static Dimension CurrentDimension => new(current: 1.0);

    /// <summary>
    /// Gets the temperature dimension [Th].
    /// </summary>
    public static Dimension TemperatureDimension => new(temperature: 1.0);

    /// <summary>
    /// Gets the amount dimension [N].
    /// </summary>
    public static Dimension AmountDimension => new(amount: 1.0);

    /// <summary>
    /// Gets the luminous intensity dimension [J].
    /// </summary>
    public static Dimension LuminousIntensityDimension => new(luminousIntensity: 1.0);

    /// <summary>
    /// Multiplies two dimensions by adding their exponents.
    /// </summary>
    /// <param name="a">The first dimension.</param>
    /// <param name="b">The second dimension.</param>
    /// <returns>The product dimension.</returns>
    public static Dimension operator *(Dimension a, Dimension b)
    {
        return new Dimension(
            a.Mass + b.Mass,
            a.Length + b.Length,
            a.Time + b.Time,
            a.Current + b.Current,
            a.Temperature + b.Temperature,
            a.Amount + b.Amount,
            a.LuminousIntensity + b.LuminousIntensity);
    }

    /// <summary>
    /// Divides two dimensions by subtracting exponents.
    /// </summary>
    /// <param name="a">The numerator dimension.</param>
    /// <param name="b">The denominator dimension.</param>
    /// <returns>The quotient dimension.</returns>
    public static Dimension operator /(Dimension a, Dimension b)
    {
        return new Dimension(
            a.Mass - b.Mass,
            a.Length - b.Length,
            a.Time - b.Time,
            a.Current - b.Current,
            a.Temperature - b.Temperature,
            a.Amount - b.Amount,
            a.LuminousIntensity - b.LuminousIntensity);
    }

    /// <summary>
    /// Raises a dimension to a power by scaling all exponents.
    /// </summary>
    /// <param name="dim">The base dimension.</param>
    /// <param name="exp">The exponent.</param>
    /// <returns>The raised dimension.</returns>
    public static Dimension operator ^(Dimension dim, double exp)
    {
        return new Dimension(
            dim.Mass * exp,
            dim.Length * exp,
            dim.Time * exp,
            dim.Current * exp,
            dim.Temperature * exp,
            dim.Amount * exp,
            dim.LuminousIntensity * exp);
    }

    /// <summary>
    /// Determines whether two dimensions are equal within a tolerance.
    /// </summary>
    /// <param name="other">The other dimension to compare.</param>
    /// <param name="tolerance">The comparison tolerance.</param>
    /// <returns>True if dimensions are equivalent; otherwise, false.</returns>
    public bool IsEquivalentTo(Dimension other, double tolerance = 1e-10)
    {
        return System.Math.Abs(Mass - other.Mass) < tolerance
            && System.Math.Abs(Length - other.Length) < tolerance
            && System.Math.Abs(Time - other.Time) < tolerance
            && System.Math.Abs(Current - other.Current) < tolerance
            && System.Math.Abs(Temperature - other.Temperature) < tolerance
            && System.Math.Abs(Amount - other.Amount) < tolerance
            && System.Math.Abs(LuminousIntensity - other.LuminousIntensity) < tolerance;
    }

    /// <summary>
    /// Determines whether the dimension is dimensionless (all exponents zero).
    /// </summary>
    /// <returns>True if dimensionless; otherwise, false.</returns>
    public bool IsDimensionless()
    {
        return IsEquivalentTo(Dimensionless);
    }

    /// <summary>
    /// Returns the string representation of the dimension using SI bracket notation.
    /// </summary>
    /// <returns>A string like [M^1 L^2 T^-2].</returns>
    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append('[');

        bool first = true;
        first = AppendExponent(sb, "M", Mass, first);
        first = AppendExponent(sb, "L", Length, first);
        first = AppendExponent(sb, "T", Time, first);
        first = AppendExponent(sb, "I", Current, first);
        first = AppendExponent(sb, "Th", Temperature, first);
        first = AppendExponent(sb, "N", Amount, first);
        first = AppendExponent(sb, "J", LuminousIntensity, first);

        sb.Append(']');
        return sb.ToString();
    }

    /// <summary>
    /// Determines whether this dimension equals another object.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns>True if equal; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is Dimension other && IsEquivalentTo(other);
    }

    /// <summary>
    /// Returns the hash code for this dimension.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + Mass.GetHashCode();
            hash = hash * 31 + Length.GetHashCode();
            hash = hash * 31 + Time.GetHashCode();
            hash = hash * 31 + Current.GetHashCode();
            hash = hash * 31 + Temperature.GetHashCode();
            hash = hash * 31 + Amount.GetHashCode();
            hash = hash * 31 + LuminousIntensity.GetHashCode();
            return hash;
        }
    }

    private static bool AppendExponent(System.Text.StringBuilder sb, string symbol, double exponent, bool first)
    {
        if (System.Math.Abs(exponent) < 1e-10)
            return first;

        if (!first)
            sb.Append(' ');

        sb.Append(symbol);
        if (System.Math.Abs(exponent - 1.0) > 1e-10)
        {
            sb.Append('^');
            sb.Append(exponent.ToString("G"));
        }

        return false;
    }
}
