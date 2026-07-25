namespace MathVerse.Math.DataScience.ScientificComputing
{
    using System;

    /// <summary>
    /// Provides full uncertainty propagation including linear propagation with correlations
    /// and Monte Carlo propagation for nonlinear functions.
    /// </summary>
    public sealed class ErrorPropagation
    {
        /// <summary>
        /// Propagates uncertainties through a linear combination y = sum(c_i * x_i) with optional pairwise correlation.
        /// </summary>
        /// <param name="coefficients">The coefficients c_i of the linear combination.</param>
        /// <param name="uncertainties">The standard uncertainties of each input variable.</param>
        /// <param name="correlation">The pairwise correlation coefficient between all inputs (default is 0 for independent variables). Must be in [-1, 1].</param>
        /// <returns>The propagated uncertainty of the linear combination.</returns>
        public static double LinearPropagation(double[] coefficients, double[] uncertainties, double correlation = 0.0)
        {
            if (coefficients == null) throw new ArgumentNullException(nameof(coefficients));
            if (uncertainties == null) throw new ArgumentNullException(nameof(uncertainties));
            if (coefficients.Length != uncertainties.Length)
                throw new ArgumentException("Coefficients and uncertainties must have the same length.");
            if (coefficients.Length == 0)
                throw new ArgumentException("Arrays must not be empty.");
            if (correlation < -1.0 || correlation > 1.0)
                throw new ArgumentException("Correlation must be between -1 and 1.");

            int n = coefficients.Length;

            double sumVariance = 0.0;
            for (int i = 0; i < n; i++)
            {
                sumVariance += coefficients[i] * coefficients[i] * uncertainties[i] * uncertainties[i];
            }

            if (correlation != 0.0)
            {
                double sumCovariance = 0.0;
                for (int i = 0; i < n; i++)
                {
                    for (int j = i + 1; j < n; j++)
                    {
                        sumCovariance += 2.0 * coefficients[i] * coefficients[j] *
                                         uncertainties[i] * uncertainties[j] * correlation;
                    }
                }
                sumVariance += sumCovariance;
            }

            return System.Math.Sqrt(sumVariance);
        }

        /// <summary>
        /// Propagates uncertainties through a general nonlinear function using Monte Carlo simulation.
        /// Generates random samples from the input distributions, evaluates the function, and computes
        /// the standard deviation of the output distribution.
        /// </summary>
        /// <param name="values">The nominal input values (means of the distributions).</param>
        /// <param name="uncertainties">The standard deviations of the input distributions.</param>
        /// <param name="func">The function to propagate through. Takes a double array and returns a scalar.</param>
        /// <param name="samples">The number of Monte Carlo samples. Default is 10000.</param>
        /// <returns>The Monte Carlo estimated uncertainty (standard deviation of the output distribution).</returns>
        public static double MonteCarloPropagation(double[] values, double[] uncertainties, Func<double[], double> func, int samples = 10000)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (uncertainties == null) throw new ArgumentNullException(nameof(uncertainties));
            if (func == null) throw new ArgumentNullException(nameof(func));
            if (values.Length != uncertainties.Length)
                throw new ArgumentException("Values and uncertainties must have the same length.");
            if (values.Length == 0)
                throw new ArgumentException("Arrays must not be empty.");
            if (samples <= 0) throw new ArgumentException("Number of samples must be positive.");

            int n = values.Length;
            double[] outputValues = new double[samples];
            Random rng = new Random(42);

            for (int s = 0; s < samples; s++)
            {
                double[] sampled = new double[n];
                for (int i = 0; i < n; i++)
                {
                    sampled[i] = values[i] + SampleNormal(rng) * uncertainties[i];
                }
                outputValues[s] = func(sampled);
            }

            double mean = 0.0;
            for (int i = 0; i < samples; i++)
            {
                mean += outputValues[i];
            }
            mean /= samples;

            double variance = 0.0;
            for (int i = 0; i < samples; i++)
            {
                double d = outputValues[i] - mean;
                variance += d * d;
            }
            variance /= (samples - 1);

            return System.Math.Sqrt(variance);
        }

        private static double SampleNormal(Random rng)
        {
            double u1 = 1.0 - rng.NextDouble();
            double u2 = 1.0 - rng.NextDouble();
            return System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Sin(2.0 * System.Math.PI * u2);
        }
    }
}
