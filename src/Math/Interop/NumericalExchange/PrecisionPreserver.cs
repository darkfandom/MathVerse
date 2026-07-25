namespace MathVerse.Math.Interop.NumericalExchange;

using System;
using System.Globalization;

/// <summary>
/// Preserves numerical precision during format conversions and serialization.
/// Uses round-trip formatting to ensure lossless double-precision round-trips.
/// </summary>
public sealed class PrecisionPreserver
{
    /// <summary>
    /// Performs a round-trip of a double-precision value through string representation
    /// at the specified number of significant digits.
    /// </summary>
    /// <param name="value">The value to round-trip.</param>
    /// <param name="significantDigits">The desired number of significant digits.</param>
    /// <returns>The value after rounding to the specified significant digits.</returns>
    public double RoundTrip(double value, int significantDigits)
    {
        if (significantDigits < 1 || significantDigits > 17)
        {
            throw new ArgumentOutOfRangeException(nameof(significantDigits),
                "Significant digits must be between 1 and 17 for double-precision values.");
        }

        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return value;
        }

        var format = CreateSignificantDigitsFormat(significantDigits);
        var text = value.ToString(format, CultureInfo.InvariantCulture);
        if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }
        return value;
    }

    /// <summary>
    /// Converts a double-precision value to an exact string representation
    /// using the round-trip "R" format specifier.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The exact round-trip string representation.</returns>
    public string ToExactString(double value)
    {
        return value.ToString("R", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Parses a double-precision value from its exact round-trip string representation.
    /// </summary>
    /// <param name="value">The round-trip string representation to parse.</param>
    /// <returns>The parsed double-precision value.</returns>
    public double FromExactString(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);

        if (double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands,
            CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        throw new FormatException($"Unable to parse '{value}' as a double-precision value.");
    }

    private static string CreateSignificantDigitsFormat(int digits)
    {
        return "0." + new string('#', digits - 1) + "E+0";
    }
}
