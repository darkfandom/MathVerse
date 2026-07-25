namespace MathVerse.Math.DataScience.Performance;

using System;
using System.Numerics;
using System.Threading.Tasks;

/// <summary>
/// Provides SIMD-accelerated aggregation operations for arrays of doubles.
/// </summary>
public sealed class SIMDAggregator
{
    /// <summary>
    /// Computes the sum of all values in the array using SIMD when available.
    /// </summary>
    /// <param name="values">The input values.</param>
    /// <returns>The sum of all values.</returns>
    public static double Sum(double[] values)
    {
        if (values is null) throw new ArgumentNullException(nameof(values));
        if (values.Length == 0) return 0.0;

        int vectorSize = Vector<double>.Count;
        int remainder = values.Length % vectorSize;
        int alignedLength = values.Length - remainder;

        double sum = 0.0;

        if (alignedLength > 0 && vectorSize > 1)
        {
            Vector<double> accumulator = Vector<double>.Zero;
            int i = 0;
            for (; i <= alignedLength - vectorSize; i += vectorSize)
            {
                Vector<double> v = new(values, i);
                accumulator += v;
            }
            for (int j = 0; j < vectorSize; j++)
            {
                sum += accumulator[j];
            }
        }

        for (int i = alignedLength; i < values.Length; i++)
        {
            sum += values[i];
        }

        return sum;
    }

    /// <summary>
    /// Computes the arithmetic mean of all values in the array using SIMD.
    /// </summary>
    /// <param name="values">The input values.</param>
    /// <returns>The arithmetic mean.</returns>
    public static double Mean(double[] values)
    {
        if (values is null) throw new ArgumentNullException(nameof(values));
        if (values.Length == 0) throw new ArgumentException("Array cannot be empty.", nameof(values));

        return Sum(values) / values.Length;
    }

    /// <summary>
    /// Computes the population variance of all values using SIMD.
    /// </summary>
    /// <param name="values">The input values.</param>
    /// <returns>The population variance.</returns>
    public static double Variance(double[] values)
    {
        if (values is null) throw new ArgumentNullException(nameof(values));
        if (values.Length < 2) throw new ArgumentException("Array must contain at least 2 values.", nameof(values));

        double mean = Mean(values);
        int vectorSize = Vector<double>.Count;
        int remainder = values.Length % vectorSize;
        int alignedLength = values.Length - remainder;

        double m2 = 0.0;

        if (alignedLength > 0 && vectorSize > 1)
        {
            Vector<double> meanVec = new(mean);
            Vector<double> accumulator = Vector<double>.Zero;
            int i = 0;
            for (; i <= alignedLength - vectorSize; i += vectorSize)
            {
                Vector<double> v = new(values, i);
                Vector<double> diff = v - meanVec;
                accumulator += diff * diff;
            }
            for (int j = 0; j < vectorSize; j++)
            {
                m2 += accumulator[j];
            }
        }

        for (int i = alignedLength; i < values.Length; i++)
        {
            double diff = values[i] - mean;
            m2 += diff * diff;
        }

        return m2 / values.Length;
    }

    /// <summary>
    /// Computes the dot product of two arrays using SIMD when available.
    /// </summary>
    /// <param name="a">The first array.</param>
    /// <param name="b">The second array.</param>
    /// <returns>The dot product (sum of element-wise products).</returns>
    public static double DotProduct(double[] a, double[] b)
    {
        if (a is null) throw new ArgumentNullException(nameof(a));
        if (b is null) throw new ArgumentNullException(nameof(b));
        if (a.Length != b.Length)
            throw new ArgumentException("Arrays must have the same length.");

        int length = a.Length;
        int vectorSize = Vector<double>.Count;
        int remainder = length % vectorSize;
        int alignedLength = length - remainder;

        double result = 0.0;

        if (alignedLength > 0 && vectorSize > 1)
        {
            Vector<double> accumulator = Vector<double>.Zero;
            int i = 0;
            for (; i <= alignedLength - vectorSize; i += vectorSize)
            {
                Vector<double> va = new(a, i);
                Vector<double> vb = new(b, i);
                accumulator += va * vb;
            }
            for (int j = 0; j < vectorSize; j++)
            {
                result += accumulator[j];
            }
        }

        for (int i = alignedLength; i < length; i++)
        {
            result += a[i] * b[i];
        }

        return result;
    }

    /// <summary>
    /// Computes the L2 norm (Euclidean norm) of an array using SIMD.
    /// </summary>
    /// <param name="values">The input values.</param>
    /// <returns>The L2 norm.</returns>
    public static double L2Norm(double[] values)
    {
        if (values is null) throw new ArgumentNullException(nameof(values));
        if (values.Length == 0) return 0.0;

        double dotProduct = DotProduct(values, values);
        return System.Math.Sqrt(dotProduct);
    }
}
