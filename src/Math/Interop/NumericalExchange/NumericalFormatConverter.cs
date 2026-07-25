namespace MathVerse.Math.Interop.NumericalExchange;

using System;
using System.Buffers.Binary;
using System.Globalization;

/// <summary>
/// Converts between different numerical precision formats.
/// </summary>
public sealed class NumericalFormatConverter
{
    /// <summary>
    /// Converts an array of double-precision values to single-precision (float).
    /// </summary>
    /// <param name="doubles">The array of double values.</param>
    /// <returns>An array of float values.</returns>
    public float[] ToSingle(double[] doubles)
    {
        ArgumentNullException.ThrowIfNull(doubles);

        var result = new float[doubles.Length];
        for (var i = 0; i < doubles.Length; i++)
        {
            result[i] = (float)doubles[i];
        }
        return result;
    }

    /// <summary>
    /// Converts an array of single-precision (float) values to double-precision.
    /// </summary>
    /// <param name="singles">The array of float values.</param>
    /// <returns>An array of double values.</returns>
    public double[] ToDouble(float[] singles)
    {
        ArgumentNullException.ThrowIfNull(singles);

        var result = new double[singles.Length];
        for (var i = 0; i < singles.Length; i++)
        {
            result[i] = singles[i];
        }
        return result;
    }

    /// <summary>
    /// Converts an array of double-precision values to half-precision.
    /// Returns the data as a ushort array since System.Half may not be available
    /// on all target platforms.
    /// </summary>
    /// <param name="doubles">The array of double values.</param>
    /// <returns>An array of ushort values representing half-precision floats.</returns>
    public ushort[] ToHalf(double[] doubles)
    {
        ArgumentNullException.ThrowIfNull(doubles);

        var result = new ushort[doubles.Length];
        for (var i = 0; i < doubles.Length; i++)
        {
            result[i] = DoubleToHalf(doubles[i]);
        }
        return result;
    }

    /// <summary>
    /// Converts a double-precision value to a byte array in the specified format.
    /// </summary>
    /// <param name="value">The double value to convert.</param>
    /// <param name="format">
    /// The target format: "f32" for single-precision, "f64" for double-precision.
    /// </param>
    /// <returns>A byte array representing the value in the target format.</returns>
    public byte[] ToBytes(double value, string format)
    {
        ArgumentException.ThrowIfNullOrEmpty(format);

        return format.ToLowerInvariant() switch
        {
            "f32" => BitConverter.GetBytes((float)value),
            "f64" => BitConverter.GetBytes(value),
            _ => throw new ArgumentException($"Unsupported format '{format}'. Use 'f32' or 'f64'.", nameof(format))
        };
    }

    private static ushort DoubleToHalf(double value)
    {
        if (double.IsNaN(value)) return 0x7E00;
        if (double.IsPositiveInfinity(value)) return 0x7C00;
        if (double.IsNegativeInfinity(value)) return 0xFC00;
        if (value == 0.0) return (ushort)(double.IsNegative(value) ? 0x8000 : 0x0000);

        var bits = BitConverter.DoubleToInt64Bits(value);
        var sign = (ushort)((bits >> 48) & 0x8000);
        var exponent = (int)((bits >> 52) & 0x7FF);
        var mantissa = bits & 0xFFFFFFFFFFFFF;

        exponent -= 1023;
        exponent += 15;

        if (exponent > 30) return (ushort)(sign | 0x7C00);
        if (exponent < -10) return sign;

        if (exponent <= 0)
        {
            mantissa |= 0x10000000000000;
            var shift = 1 - exponent;
            mantissa >>= shift;
            exponent = 0;
        }

        var halfExponent = (ushort)(sign | ((ushort)(exponent & 0x1F) << 10));
        var halfMantissa = (ushort)((mantissa >> 42) & 0x3FF);
        return (ushort)(halfExponent | halfMantissa);
    }
}
