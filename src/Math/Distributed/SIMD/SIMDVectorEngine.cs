namespace MathVerse.Math.Distributed.SIMD
{
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.Intrinsics;

    /// <summary>
    /// Provides SIMD-accelerated vector operations for double-precision arrays.
    /// Falls back to scalar operations when hardware acceleration is not available.
    /// </summary>
    public sealed class SIMDVectorEngine
    {
        /// <summary>
        /// Adds two double arrays element-wise using SIMD when available.
        /// </summary>
        /// <param name="a">The first operand array.</param>
        /// <param name="b">The second operand array.</param>
        /// <returns>A new array containing the element-wise sum.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="a"/> or <paramref name="b"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the arrays have different lengths.</exception>
        public static double[] Add(double[] a, double[] b)
        {
            ValidateArrays(a, b);
            double[] result = new double[a.Length];

            if (Vector.IsHardwareAccelerated && a.Length >= Vector<double>.Count)
            {
                int vectorSize = Vector<double>.Count;
                int i = 0;

                for (; i <= a.Length - vectorSize; i += vectorSize)
                {
                    Vector<double> va = new Vector<double>(a, i);
                    Vector<double> vb = new Vector<double>(b, i);
                    (va + vb).CopyTo(result, i);
                }

                for (; i < a.Length; i++)
                {
                    result[i] = a[i] + b[i];
                }
            }
            else
            {
                for (int i = 0; i < a.Length; i++)
                {
                    result[i] = a[i] + b[i];
                }
            }

            return result;
        }

        /// <summary>
        /// Subtracts the second double array from the first element-wise using SIMD when available.
        /// </summary>
        /// <param name="a">The first operand array.</param>
        /// <param name="b">The second operand array.</param>
        /// <returns>A new array containing the element-wise difference.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="a"/> or <paramref name="b"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the arrays have different lengths.</exception>
        public static double[] Subtract(double[] a, double[] b)
        {
            ValidateArrays(a, b);
            double[] result = new double[a.Length];

            if (Vector.IsHardwareAccelerated && a.Length >= Vector<double>.Count)
            {
                int vectorSize = Vector<double>.Count;
                int i = 0;

                for (; i <= a.Length - vectorSize; i += vectorSize)
                {
                    Vector<double> va = new Vector<double>(a, i);
                    Vector<double> vb = new Vector<double>(b, i);
                    (va - vb).CopyTo(result, i);
                }

                for (; i < a.Length; i++)
                {
                    result[i] = a[i] - b[i];
                }
            }
            else
            {
                for (int i = 0; i < a.Length; i++)
                {
                    result[i] = a[i] - b[i];
                }
            }

            return result;
        }

        /// <summary>
        /// Multiplies two double arrays element-wise using SIMD when available.
        /// </summary>
        /// <param name="a">The first operand array.</param>
        /// <param name="b">The second operand array.</param>
        /// <returns>A new array containing the element-wise product.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="a"/> or <paramref name="b"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the arrays have different lengths.</exception>
        public static double[] Multiply(double[] a, double[] b)
        {
            ValidateArrays(a, b);
            double[] result = new double[a.Length];

            if (Vector.IsHardwareAccelerated && a.Length >= Vector<double>.Count)
            {
                int vectorSize = Vector<double>.Count;
                int i = 0;

                for (; i <= a.Length - vectorSize; i += vectorSize)
                {
                    Vector<double> va = new Vector<double>(a, i);
                    Vector<double> vb = new Vector<double>(b, i);
                    (va * vb).CopyTo(result, i);
                }

                for (; i < a.Length; i++)
                {
                    result[i] = a[i] * b[i];
                }
            }
            else
            {
                for (int i = 0; i < a.Length; i++)
                {
                    result[i] = a[i] * b[i];
                }
            }

            return result;
        }

        /// <summary>
        /// Divides the first double array by the second element-wise using SIMD when available.
        /// </summary>
        /// <param name="a">The dividend array.</param>
        /// <param name="b">The divisor array.</param>
        /// <returns>A new array containing the element-wise quotient.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="a"/> or <paramref name="b"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the arrays have different lengths.</exception>
        public static double[] Divide(double[] a, double[] b)
        {
            ValidateArrays(a, b);
            double[] result = new double[a.Length];

            if (Vector.IsHardwareAccelerated && a.Length >= Vector<double>.Count)
            {
                int vectorSize = Vector<double>.Count;
                int i = 0;

                for (; i <= a.Length - vectorSize; i += vectorSize)
                {
                    Vector<double> va = new Vector<double>(a, i);
                    Vector<double> vb = new Vector<double>(b, i);
                    (va / vb).CopyTo(result, i);
                }

                for (; i < a.Length; i++)
                {
                    result[i] = a[i] / b[i];
                }
            }
            else
            {
                for (int i = 0; i < a.Length; i++)
                {
                    result[i] = a[i] / b[i];
                }
            }

            return result;
        }

        /// <summary>
        /// Computes the dot product of two double arrays using SIMD when available.
        /// </summary>
        /// <param name="a">The first vector array.</param>
        /// <param name="b">The second vector array.</param>
        /// <returns>The scalar dot product of the two vectors.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="a"/> or <paramref name="b"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the arrays have different lengths.</exception>
        public static double DotProduct(double[] a, double[] b)
        {
            ValidateArrays(a, b);

            if (Vector.IsHardwareAccelerated && a.Length >= Vector<double>.Count)
            {
                int vectorSize = Vector<double>.Count;
                Vector<double> accumulator = Vector<double>.Zero;
                int i = 0;

                for (; i <= a.Length - vectorSize; i += vectorSize)
                {
                    Vector<double> va = new Vector<double>(a, i);
                    Vector<double> vb = new Vector<double>(b, i);
                    accumulator += va * vb;
                }

                double sum = 0.0;
                for (int j = 0; j < vectorSize; j++)
                {
                    sum += accumulator[j];
                }

                for (; i < a.Length; i++)
                {
                    sum += a[i] * b[i];
                }

                return sum;
            }
            else
            {
                double sum = 0.0;
                for (int i = 0; i < a.Length; i++)
                {
                    sum += a[i] * b[i];
                }
                return sum;
            }
        }

        /// <summary>
        /// Multiplies every element of an array by a scalar value using SIMD when available.
        /// </summary>
        /// <param name="a">The input array.</param>
        /// <param name="scalar">The scalar multiplier.</param>
        /// <returns>A new array with each element scaled by the scalar.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="a"/> is null.</exception>
        public static double[] Scale(double[] a, double scalar)
        {
            if (a is null)
                throw new ArgumentNullException(nameof(a));

            double[] result = new double[a.Length];

            if (Vector.IsHardwareAccelerated && a.Length >= Vector<double>.Count)
            {
                int vectorSize = Vector<double>.Count;
                Vector<double> vScalar = new Vector<double>(scalar);
                int i = 0;

                for (; i <= a.Length - vectorSize; i += vectorSize)
                {
                    Vector<double> va = new Vector<double>(a, i);
                    (va * vScalar).CopyTo(result, i);
                }

                for (; i < a.Length; i++)
                {
                    result[i] = a[i] * scalar;
                }
            }
            else
            {
                for (int i = 0; i < a.Length; i++)
                {
                    result[i] = a[i] * scalar;
                }
            }

            return result;
        }

        /// <summary>
        /// Validates that two arrays are non-null and have the same length.
        /// </summary>
        /// <param name="a">The first array.</param>
        /// <param name="b">The second array.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="a"/> or <paramref name="b"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the arrays have different lengths.</exception>
        private static void ValidateArrays(double[] a, double[] b)
        {
            if (a is null)
                throw new ArgumentNullException(nameof(a));
            if (b is null)
                throw new ArgumentNullException(nameof(b));
            if (a.Length != b.Length)
                throw new ArgumentException("Arrays must have the same length.");
        }
    }
}
