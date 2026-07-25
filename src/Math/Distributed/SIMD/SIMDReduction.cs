namespace MathVerse.Math.Distributed.SIMD
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Provides SIMD-accelerated reduction operations for computing the sum, minimum,
    /// and maximum of double-precision arrays.
    /// </summary>
    public sealed class SIMDReduction
    {
        /// <summary>
        /// Computes the sum of all elements in a double array using SIMD when available.
        /// Partial SIMD vector sums are reduced to a single scalar at the end.
        /// </summary>
        /// <param name="values">The array of values to sum.</param>
        /// <returns>The sum of all elements.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
        public static double Sum(double[] values)
        {
            if (values is null)
                throw new ArgumentNullException(nameof(values));

            if (values.Length == 0)
                return 0.0;

            if (Vector.IsHardwareAccelerated && values.Length >= Vector<double>.Count)
            {
                int vectorSize = Vector<double>.Count;
                Vector<double> accumulator = Vector<double>.Zero;
                int i = 0;

                for (; i <= values.Length - vectorSize; i += vectorSize)
                {
                    Vector<double> v = new Vector<double>(values, i);
                    accumulator += v;
                }

                double sum = 0.0;
                for (int j = 0; j < vectorSize; j++)
                {
                    sum += accumulator[j];
                }

                for (; i < values.Length; i++)
                {
                    sum += values[i];
                }

                return sum;
            }
            else
            {
                double sum = 0.0;
                for (int i = 0; i < values.Length; i++)
                {
                    sum += values[i];
                }
                return sum;
            }
        }

        /// <summary>
        /// Finds the minimum value in a double array using SIMD when available.
        /// Uses per-lane minimums within SIMD vectors, then reduces lanes to a scalar.
        /// </summary>
        /// <param name="values">The array of values.</param>
        /// <returns>The minimum value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty.</exception>
        public static double Min(double[] values)
        {
            if (values is null)
                throw new ArgumentNullException(nameof(values));
            if (values.Length == 0)
                throw new ArgumentException("Array must not be empty.", nameof(values));

            if (Vector.IsHardwareAccelerated && values.Length >= Vector<double>.Count)
            {
                int vectorSize = Vector<double>.Count;
                Vector<double> vMin = new Vector<double>(values, 0);
                int i = vectorSize;

                for (; i <= values.Length - vectorSize; i += vectorSize)
                {
                    Vector<double> v = new Vector<double>(values, i);
                    vMin = Vector.Min(vMin, v);
                }

                double min = vMin[0];
                for (int j = 1; j < vectorSize; j++)
                {
                    if (vMin[j] < min)
                        min = vMin[j];
                }

                for (; i < values.Length; i++)
                {
                    if (values[i] < min)
                        min = values[i];
                }

                return min;
            }
            else
            {
                double min = values[0];
                for (int i = 1; i < values.Length; i++)
                {
                    if (values[i] < min)
                        min = values[i];
                }
                return min;
            }
        }

        /// <summary>
        /// Finds the maximum value in a double array using SIMD when available.
        /// Uses per-lane maximums within SIMD vectors, then reduces lanes to a scalar.
        /// </summary>
        /// <param name="values">The array of values.</param>
        /// <returns>The maximum value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty.</exception>
        public static double Max(double[] values)
        {
            if (values is null)
                throw new ArgumentNullException(nameof(values));
            if (values.Length == 0)
                throw new ArgumentException("Array must not be empty.", nameof(values));

            if (Vector.IsHardwareAccelerated && values.Length >= Vector<double>.Count)
            {
                int vectorSize = Vector<double>.Count;
                Vector<double> vMax = new Vector<double>(values, 0);
                int i = vectorSize;

                for (; i <= values.Length - vectorSize; i += vectorSize)
                {
                    Vector<double> v = new Vector<double>(values, i);
                    vMax = Vector.Max(vMax, v);
                }

                double max = vMax[0];
                for (int j = 1; j < vectorSize; j++)
                {
                    if (vMax[j] > max)
                        max = vMax[j];
                }

                for (; i < values.Length; i++)
                {
                    if (values[i] > max)
                        max = values[i];
                }

                return max;
            }
            else
            {
                double max = values[0];
                for (int i = 1; i < values.Length; i++)
                {
                    if (values[i] > max)
                        max = values[i];
                }
                return max;
            }
        }
    }
}
