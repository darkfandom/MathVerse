namespace MathVerse.Math.Distributed.SIMD
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Provides SIMD-accelerated polynomial evaluation using Horner's method
    /// for batch evaluation across multiple input points.
    /// </summary>
    public sealed class SIMDPolynomialEvaluation
    {
        /// <summary>
        /// Evaluates a polynomial at multiple points simultaneously using SIMD-accelerated Horner's method.
        /// Coefficients are provided from highest degree to lowest.
        /// </summary>
        /// <param name="coefficients">
        /// Polynomial coefficients in descending degree order: a_n, a_{n-1}, ..., a_1, a_0.
        /// Must contain at least one element.
        /// </param>
        /// <param name="xValues">The array of input points at which to evaluate the polynomial.</param>
        /// <returns>An array of polynomial values, one per input point.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="coefficients"/> or <paramref name="xValues"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="coefficients"/> is empty.</exception>
        public static double[] Evaluate(double[] coefficients, double[] xValues)
        {
            if (coefficients is null)
                throw new ArgumentNullException(nameof(coefficients));
            if (xValues is null)
                throw new ArgumentNullException(nameof(xValues));
            if (coefficients.Length == 0)
                throw new ArgumentException("Coefficients must not be empty.", nameof(coefficients));

            double[] result = new double[xValues.Length];

            if (coefficients.Length == 1)
            {
                double constant = coefficients[0];
                for (int i = 0; i < xValues.Length; i++)
                {
                    result[i] = constant;
                }
                return result;
            }

            if (Vector.IsHardwareAccelerated && xValues.Length >= Vector<double>.Count)
            {
                int vectorSize = Vector<double>.Count;
                int i = 0;

                for (; i <= xValues.Length - vectorSize; i += vectorSize)
                {
                    Vector<double> vx = new Vector<double>(xValues, i);
                    Vector<double> vResult = new Vector<double>(coefficients[0]);

                    for (int c = 1; c < coefficients.Length; c++)
                    {
                        vResult = vResult * vx + new Vector<double>(coefficients[c]);
                    }

                    vResult.CopyTo(result, i);
                }

                for (; i < xValues.Length; i++)
                {
                    double val = coefficients[0];
                    for (int c = 1; c < coefficients.Length; c++)
                    {
                        val = val * xValues[i] + coefficients[c];
                    }
                    result[i] = val;
                }
            }
            else
            {
                for (int i = 0; i < xValues.Length; i++)
                {
                    double val = coefficients[0];
                    for (int c = 1; c < coefficients.Length; c++)
                    {
                        val = val * xValues[i] + coefficients[c];
                    }
                    result[i] = val;
                }
            }

            return result;
        }

        /// <summary>
        /// Evaluates a polynomial at a single point using Horner's method.
        /// </summary>
        /// <param name="coefficients">
        /// Polynomial coefficients in descending degree order: a_n, a_{n-1}, ..., a_1, a_0.
        /// </param>
        /// <param name="x">The input point.</param>
        /// <returns>The polynomial value at the given point.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="coefficients"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="coefficients"/> is empty.</exception>
        public static double EvaluateSingle(double[] coefficients, double x)
        {
            if (coefficients is null)
                throw new ArgumentNullException(nameof(coefficients));
            if (coefficients.Length == 0)
                throw new ArgumentException("Coefficients must not be empty.", nameof(coefficients));

            double result = coefficients[0];
            for (int i = 1; i < coefficients.Length; i++)
            {
                result = result * x + coefficients[i];
            }
            return result;
        }

        /// <summary>
        /// Evaluates multiple polynomials at multiple points using SIMD where possible.
        /// </summary>
        /// <param name="polynomialCoefficients">
        /// A 2D array where each row contains coefficients for one polynomial in descending degree order.
        /// </param>
        /// <param name="xValues">The array of input points.</param>
        /// <returns>A 2D result array where result[p, i] is polynomial p evaluated at point i.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="polynomialCoefficients"/> or <paramref name="xValues"/> is null.
        /// </exception>
        public static double[,] EvaluateMultiple(double[,] polynomialCoefficients, double[] xValues)
        {
            if (polynomialCoefficients is null)
                throw new ArgumentNullException(nameof(polynomialCoefficients));
            if (xValues is null)
                throw new ArgumentNullException(nameof(xValues));

            int polyCount = polynomialCoefficients.GetLength(0);
            int coeffCount = polynomialCoefficients.GetLength(1);
            double[,] result = new double[polyCount, xValues.Length];

            for (int p = 0; p < polyCount; p++)
            {
                double[] coeffs = new double[coeffCount];
                for (int c = 0; c < coeffCount; c++)
                {
                    coeffs[c] = polynomialCoefficients[p, c];
                }

                double[] polyResult = Evaluate(coeffs, xValues);
                for (int i = 0; i < xValues.Length; i++)
                {
                    result[p, i] = polyResult[i];
                }
            }

            return result;
        }
    }
}
