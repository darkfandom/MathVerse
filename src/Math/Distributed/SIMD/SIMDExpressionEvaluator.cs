namespace MathVerse.Math.Distributed.SIMD
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Evaluates mathematical expressions on arrays using SIMD-accelerated batch processing,
    /// processing multiple elements per iteration via <see cref="Vector{T}"/>.
    /// </summary>
    public sealed class SIMDExpressionEvaluator
    {
        /// <summary>
        /// Evaluates a term function across the input array using SIMD batch processing.
        /// The term function receives the full array and the current index, and should return
        /// the computed value for that index.
        /// </summary>
        /// <param name="x">The input array of values.</param>
        /// <param name="termFunc">
        /// A function that computes a single output value from the array at the given index.
        /// This function is called for each element, including those processed via SIMD lanes.
        /// </param>
        /// <returns>A new array of output values.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="x"/> or <paramref name="termFunc"/> is null.
        /// </exception>
        public static double[] Evaluate(double[] x, Func<double[], int, double> termFunc)
        {
            if (x is null)
                throw new ArgumentNullException(nameof(x));
            if (termFunc is null)
                throw new ArgumentNullException(nameof(termFunc));

            double[] result = new double[x.Length];

            if (Vector.IsHardwareAccelerated && x.Length >= Vector<double>.Count)
            {
                int vectorSize = Vector<double>.Count;
                int i = 0;

                for (; i <= x.Length - vectorSize; i += vectorSize)
                {
                    for (int lane = 0; lane < vectorSize; lane++)
                    {
                        result[i + lane] = termFunc(x, i + lane);
                    }
                }

                for (; i < x.Length; i++)
                {
                    result[i] = termFunc(x, i);
                }
            }
            else
            {
                for (int i = 0; i < x.Length; i++)
                {
                    result[i] = termFunc(x, i);
                }
            }

            return result;
        }

        /// <summary>
        /// Evaluates a term function that uses vectorized arithmetic on aligned segments.
        /// The function receives the full array and the current base index and should return
        /// values for an entire SIMD vector width, enabling truly vectorized computation.
        /// </summary>
        /// <param name="x">The input array of values.</param>
        /// <param name="vectorTermFunc">
        /// A function that computes vector-width output values from the array starting at the base index.
        /// </param>
        /// <returns>A new array of output values.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="x"/> or <paramref name="vectorTermFunc"/> is null.
        /// </exception>
        public static double[] EvaluateVectorized(double[] x, Func<double[], int, int, double[]> vectorTermFunc)
        {
            if (x is null)
                throw new ArgumentNullException(nameof(x));
            if (vectorTermFunc is null)
                throw new ArgumentNullException(nameof(vectorTermFunc));

            double[] result = new double[x.Length];

            if (Vector.IsHardwareAccelerated && x.Length >= Vector<double>.Count)
            {
                int vectorSize = Vector<double>.Count;
                int i = 0;

                for (; i <= x.Length - vectorSize; i += vectorSize)
                {
                    double[] partial = vectorTermFunc(x, i, vectorSize);
                    for (int lane = 0; lane < vectorSize; lane++)
                    {
                        result[i + lane] = partial[lane];
                    }
                }

                for (; i < x.Length; i++)
                {
                    result[i] = vectorTermFunc(x, i, 1)[0];
                }
            }
            else
            {
                for (int i = 0; i < x.Length; i++)
                {
                    result[i] = vectorTermFunc(x, i, 1)[0];
                }
            }

            return result;
        }

        /// <summary>
        /// Evaluates a polynomial across the input array using SIMD batch processing.
        /// Coefficients are provided from highest degree to lowest.
        /// </summary>
        /// <param name="x">The input array of values.</param>
        /// <param name="coefficients">
        /// Polynomial coefficients in descending degree order: a_n, a_{n-1}, ..., a_1, a_0.
        /// </param>
        /// <returns>A new array of evaluated polynomial values.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="x"/> or <paramref name="coefficients"/> is null.
        /// </exception>
        public static double[] EvaluatePolynomial(double[] x, double[] coefficients)
        {
            if (x is null)
                throw new ArgumentNullException(nameof(x));
            if (coefficients is null)
                throw new ArgumentNullException(nameof(coefficients));

            double[] result = new double[x.Length];

            if (Vector.IsHardwareAccelerated && x.Length >= Vector<double>.Count)
            {
                int vectorSize = Vector<double>.Count;
                int i = 0;

                for (; i <= x.Length - vectorSize; i += vectorSize)
                {
                    Vector<double> acc = Vector<double>.Zero;
                    for (int c = 0; c < coefficients.Length; c++)
                    {
                        acc = acc + new Vector<double>(x, i);
                        acc *= new Vector<double>(coefficients[c]);
                    }

                    acc.CopyTo(result, i);
                }

                for (; i < x.Length; i++)
                {
                    double val = 0.0;
                    for (int c = 0; c < coefficients.Length; c++)
                    {
                        val = val * x[i] + coefficients[c];
                    }
                    result[i] = val;
                }
            }
            else
            {
                for (int i = 0; i < x.Length; i++)
                {
                    double val = 0.0;
                    for (int c = 0; c < coefficients.Length; c++)
                    {
                        val = val * x[i] + coefficients[c];
                    }
                    result[i] = val;
                }
            }

            return result;
        }
    }
}
