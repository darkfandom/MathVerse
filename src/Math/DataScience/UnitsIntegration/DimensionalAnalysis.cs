namespace MathVerse.Math.DataScience.UnitsIntegration;

using System;
using System.Collections.Generic;

/// <summary>
/// Provides dimensional analysis operations for checking, multiplying, dividing, and powering dimensions.
/// Uses base dimension exponents [M, L, T, I, Th, N, J].
/// </summary>
public static class DimensionalAnalysis
{
    /// <summary>
    /// Determines whether two dimension strings represent compatible physical dimensions.
    /// </summary>
    /// <param name="dim1">The first dimension string in bracket notation (e.g., "[M L T^-2]").</param>
    /// <param name="dim2">The second dimension string.</param>
    /// <returns>True if both dimensions are equivalent; otherwise, false.</returns>
    /// <exception cref="FormatException">Thrown when a dimension string cannot be parsed.</exception>
    public static bool AreCompatible(string dim1, string dim2)
    {
        Dimension d1 = ParseDimension(dim1);
        Dimension d2 = ParseDimension(dim2);
        return d1.IsEquivalentTo(d2);
    }

    /// <summary>
    /// Multiplies two dimension strings and returns the resulting dimension string.
    /// </summary>
    /// <param name="dim1">The first dimension string.</param>
    /// <param name="dim2">The second dimension string.</param>
    /// <returns>The product dimension string in bracket notation.</returns>
    /// <exception cref="FormatException">Thrown when a dimension string cannot be parsed.</exception>
    public static string Multiply(string dim1, string dim2)
    {
        Dimension d1 = ParseDimension(dim1);
        Dimension d2 = ParseDimension(dim2);
        Dimension result = d1 * d2;
        return result.ToString();
    }

    /// <summary>
    /// Divides two dimension strings and returns the resulting dimension string.
    /// </summary>
    /// <param name="dim1">The numerator dimension string.</param>
    /// <param name="dim2">The denominator dimension string.</param>
    /// <returns>The quotient dimension string in bracket notation.</returns>
    /// <exception cref="FormatException">Thrown when a dimension string cannot be parsed.</exception>
    public static string Divide(string dim1, string dim2)
    {
        Dimension d1 = ParseDimension(dim1);
        Dimension d2 = ParseDimension(dim2);
        Dimension result = d1 / d2;
        return result.ToString();
    }

    /// <summary>
    /// Raises a dimension string to a power and returns the resulting dimension string.
    /// </summary>
    /// <param name="dim">The dimension string.</param>
    /// <param name="exp">The exponent.</param>
    /// <returns>The powered dimension string in bracket notation.</returns>
    /// <exception cref="FormatException">Thrown when a dimension string cannot be parsed.</exception>
    public static string Power(string dim, double exp)
    {
        Dimension d = ParseDimension(dim);
        Dimension result = d ^ exp;
        return result.ToString();
    }

    /// <summary>
    /// Parses a dimension string in bracket notation (e.g., "[M L T^-2]") into a <see cref="Dimension"/> instance.
    /// Supports symbols M, L, T, I, Th, N, J with optional exponents.
    /// </summary>
    /// <param name="dimStr">The dimension string to parse.</param>
    /// <returns>The parsed <see cref="Dimension"/>.</returns>
    /// <exception cref="FormatException">Thrown when the string cannot be parsed.</exception>
    public static Dimension ParseDimension(string dimStr)
    {
        if (string.IsNullOrWhiteSpace(dimStr))
            return Dimension.Dimensionless;

        string s = dimStr.Trim().Trim('[', ']');

        if (string.IsNullOrWhiteSpace(s) || s == "1" || s.Equals("dimensionless", StringComparison.OrdinalIgnoreCase))
            return Dimension.Dimensionless;

        var exponents = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            ["M"] = 0.0,
            ["L"] = 0.0,
            ["T"] = 0.0,
            ["I"] = 0.0,
            ["Th"] = 0.0,
            ["N"] = 0.0,
            ["J"] = 0.0,
        };

        string[] tokens = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (string token in tokens)
        {
            string[] parts = token.Split('^');
            string symbol = parts[0].Trim();

            double exp = 1.0;
            if (parts.Length > 1)
            {
                string expStr = parts[1].Trim();
                if (expStr.StartsWith('-'))
                    expStr = expStr[1..];

                if (!double.TryParse(expStr, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out exp))
                    throw new FormatException($"Cannot parse exponent '{parts[1]}' in dimension token '{token}'.");

                if (token.Contains('^') && parts[1].Trim().StartsWith('-'))
                    exp = -exp;
            }

            if (exponents.ContainsKey(symbol))
            {
                exponents[symbol] += exp;
            }
            else
            {
                throw new FormatException($"Unknown dimension symbol '{symbol}' in dimension string '{dimStr}'.");
            }
        }

        return new Dimension(
            exponents["M"],
            exponents["L"],
            exponents["T"],
            exponents["I"],
            exponents["Th"],
            exponents["N"],
            exponents["J"]);
    }

    /// <summary>
    /// Returns the common SI base dimensions as a list of (symbol, exponent) pairs.
    /// </summary>
    /// <param name="dim">The dimension to decompose.</param>
    /// <returns>A list of (symbol, exponent) pairs for non-zero exponents.</returns>
    public static List<(string Symbol, double Exponent)> Decompose(Dimension dim)
    {
        var result = new List<(string, double)>();

        if (System.Math.Abs(dim.Mass) > 1e-10) result.Add(("M", dim.Mass));
        if (System.Math.Abs(dim.Length) > 1e-10) result.Add(("L", dim.Length));
        if (System.Math.Abs(dim.Time) > 1e-10) result.Add(("T", dim.Time));
        if (System.Math.Abs(dim.Current) > 1e-10) result.Add(("I", dim.Current));
        if (System.Math.Abs(dim.Temperature) > 1e-10) result.Add(("Th", dim.Temperature));
        if (System.Math.Abs(dim.Amount) > 1e-10) result.Add(("N", dim.Amount));
        if (System.Math.Abs(dim.LuminousIntensity) > 1e-10) result.Add(("J", dim.LuminousIntensity));

        return result;
    }
}
