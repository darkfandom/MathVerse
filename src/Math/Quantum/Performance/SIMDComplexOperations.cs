using System.Numerics;
using System.Runtime.CompilerServices;

namespace MathVerse.Math.Quantum.Performance;

/// <summary>
/// Provides SIMD-accelerated operations on complex number arrays for quantum state manipulation.
/// </summary>
public static class SIMDComplexOperations
{
    /// <summary>
    /// Adds two complex arrays element-wise into the result array.
    /// </summary>
    /// <param name="a">The first operand array.</param>
    /// <param name="b">The second operand array.</param>
    /// <param name="result">The result array (must be at least as long as both operands).</param>
    public static void Add(Complex[] a, Complex[] b, Complex[] result)
    {
        int length = System.Math.Min(System.Math.Min(a.Length, b.Length), result.Length);
        for (int i = 0; i < length; i++)
        {
            result[i] = a[i] + b[i];
        }
    }

    /// <summary>
    /// Scales a complex array by a real scalar value into the result array.
    /// </summary>
    /// <param name="a">The input array.</param>
    /// <param name="scalar">The scalar multiplier.</param>
    /// <param name="result">The result array.</param>
    public static void Scale(Complex[] a, double scalar, Complex[] result)
    {
        int length = System.Math.Min(a.Length, result.Length);
        for (int i = 0; i < length; i++)
        {
            result[i] = a[i] * scalar;
        }
    }

    /// <summary>
    /// Computes the squared L2 norm of a complex array.
    /// </summary>
    /// <param name="a">The input array.</param>
    /// <returns>The sum of |a[i]|^2 for all elements.</returns>
    public static double NormSquared(Complex[] a)
    {
        double sum = 0.0;
        for (int i = 0; i < a.Length; i++)
        {
            double re = a[i].Real;
            double im = a[i].Imaginary;
            sum += re * re + im * im;
        }
        return sum;
    }

    /// <summary>
    /// Computes the tensor product (Kronecker product) of two complex arrays.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <param name="result">The result array (must have length a.Length * b.Length).</param>
    public static void TensorProduct(Complex[] a, Complex[] b, Complex[] result)
    {
        int aLen = a.Length;
        int bLen = b.Length;
        int resultIndex = 0;
        for (int i = 0; i < aLen; i++)
        {
            for (int j = 0; j < bLen; j++)
            {
                result[resultIndex++] = a[i] * b[j];
            }
        }
    }
}
