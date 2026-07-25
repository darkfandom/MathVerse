namespace MathVerse.Math.DataScience.ScientificComputing
{
    using System;

    /// <summary>
    /// Provides uncertainty propagation for arithmetic operations and general functions
    /// using the standard error propagation (delta) method.
    /// </summary>
    public sealed class MeasurementUncertainty
    {
        /// <summary>
        /// Propagates uncertainty through addition: result = sum(values).
        /// For independent uncertainties, they combine in quadrature.
        /// </summary>
        /// <param name="values">The measured values.</param>
        /// <param name="uncertainties">The standard uncertainties for each value.</param>
        /// <returns>The propagated uncertainty of the sum.</returns>
        public static double PropagateAddition(double[] values, double[] uncertainties)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (uncertainties == null) throw new ArgumentNullException(nameof(uncertainties));
            if (values.Length != uncertainties.Length)
                throw new ArgumentException("Values and uncertainties must have the same length.");
            if (values.Length == 0)
                throw new ArgumentException("Arrays must not be empty.");

            double sumUncertSq = 0.0;
            for (int i = 0; i < values.Length; i++)
            {
                sumUncertSq += uncertainties[i] * uncertainties[i];
            }
            return System.Math.Sqrt(sumUncertSq);
        }

        /// <summary>
        /// Propagates uncertainty through multiplication: result = product(values).
        /// Uses relative uncertainties combined in quadrature.
        /// </summary>
        /// <param name="values">The measured values.</param>
        /// <param name="uncertainties">The standard uncertainties for each value.</param>
        /// <returns>The propagated uncertainty of the product.</returns>
        public static double PropagateMultiplication(double[] values, double[] uncertainties)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (uncertainties == null) throw new ArgumentNullException(nameof(uncertainties));
            if (values.Length != uncertainties.Length)
                throw new ArgumentException("Values and uncertainties must have the same length.");
            if (values.Length == 0)
                throw new ArgumentException("Arrays must not be empty.");

            double product = 1.0;
            for (int i = 0; i < values.Length; i++)
            {
                product *= values[i];
            }

            double sumRelUncertSq = 0.0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == 0.0)
                    throw new ArgumentException($"Value at index {i} is zero; relative uncertainty is undefined.");
                double relUncert = uncertainties[i] / values[i];
                sumRelUncertSq += relUncert * relUncert;
            }

            return System.Math.Abs(product) * System.Math.Sqrt(sumRelUncertSq);
        }

        /// <summary>
        /// Propagates uncertainty through a general function using the delta method (first-order Taylor expansion).
        /// Computes partial derivatives numerically via central differences.
        /// </summary>
        /// <param name="values">The measured values at which to evaluate.</param>
        /// <param name="uncertainties">The standard uncertainties for each value.</param>
        /// <param name="func">The function to propagate through. Takes a double array and returns a scalar.</param>
        /// <returns>The propagated uncertainty of the function result.</returns>
        public static double PropagateGeneral(double[] values, double[] uncertainties, Func<double[], double> func)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (uncertainties == null) throw new ArgumentNullException(nameof(uncertainties));
            if (func == null) throw new ArgumentNullException(nameof(func));
            if (values.Length != uncertainties.Length)
                throw new ArgumentException("Values and uncertainties must have the same length.");
            if (values.Length == 0)
                throw new ArgumentException("Arrays must not be empty.");

            double delta = 1e-7;
            int n = values.Length;

            double sumSq = 0.0;
            for (int i = 0; i < n; i++)
            {
                double[] valuesPlus = new double[n];
                double[] valuesMinus = new double[n];
                for (int j = 0; j < n; j++)
                {
                    valuesPlus[j] = values[j];
                    valuesMinus[j] = values[j];
                }

                double h = delta * System.Math.Max(System.Math.Abs(values[i]), 1.0);
                valuesPlus[i] = values[i] + h;
                valuesMinus[i] = values[i] - h;

                double fPlus = func(valuesPlus);
                double fMinus = func(valuesMinus);
                double partialDeriv = (fPlus - fMinus) / (2.0 * h);

                sumSq += partialDeriv * partialDeriv * uncertainties[i] * uncertainties[i];
            }

            return System.Math.Sqrt(sumSq);
        }
    }
}
