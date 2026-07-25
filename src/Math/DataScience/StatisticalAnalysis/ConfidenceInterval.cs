namespace MathVerse.Math.DataScience.StatisticalAnalysis
{
    using System;

    /// <summary>
    /// Represents a confidence interval with lower and upper bounds.
    /// </summary>
    public sealed class ConfidenceIntervalResult
    {
        /// <summary>Gets or sets the lower bound of the confidence interval.</summary>
        public double Lower { get; set; }

        /// <summary>Gets or sets the upper bound of the confidence interval.</summary>
        public double Upper { get; set; }

        /// <summary>Gets or sets the point estimate.</summary>
        public double PointEstimate { get; set; }

        /// <summary>Gets or sets the confidence level (e.g., 0.95 for 95%).</summary>
        public double ConfidenceLevel { get; set; }
    }

    /// <summary>
    /// Computes confidence intervals for means and proportions.
    /// </summary>
    public sealed class ConfidenceInterval
    {
        /// <summary>
        /// Computes a confidence interval for the population mean using the t-distribution.
        /// </summary>
        /// <param name="data">The sample data.</param>
        /// <param name="confidence">The confidence level (default 0.95 for 95%).</param>
        /// <returns>A ConfidenceIntervalResult with the interval bounds.</returns>
        public static ConfidenceIntervalResult Mean(double[] data, double confidence = 0.95)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));
            if (data.Length < 2) throw new ArgumentException("At least two data points are required.", nameof(data));
            if (confidence <= 0.0 || confidence >= 1.0)
                throw new ArgumentOutOfRangeException(nameof(confidence), "Confidence must be between 0 and 1.");

            int n = data.Length;
            double sum = 0.0;
            foreach (double v in data) sum += v;
            double mean = sum / n;

            double variance = 0.0;
            foreach (double v in data)
            {
                double diff = v - mean;
                variance += diff * diff;
            }
            double stdDev = System.Math.Sqrt(variance / (n - 1));
            double stdError = stdDev / System.Math.Sqrt(n);

            double alpha = 1.0 - confidence;
            double tValue = InverseTDistribution(alpha / 2.0, n - 1);

            double margin = tValue * stdError;

            return new ConfidenceIntervalResult
            {
                PointEstimate = mean,
                Lower = mean - margin,
                Upper = mean + margin,
                ConfidenceLevel = confidence
            };
        }

        /// <summary>
        /// Computes a confidence interval for a population proportion using the normal approximation.
        /// </summary>
        /// <param name="successes">The number of successes.</param>
        /// <param name="total">The total number of trials.</param>
        /// <param name="confidence">The confidence level (default 0.95 for 95%).</param>
        /// <returns>A ConfidenceIntervalResult with the interval bounds.</returns>
        public static ConfidenceIntervalResult Proportion(double successes, double total, double confidence = 0.95)
        {
            if (total <= 0) throw new ArgumentException("Total must be positive.", nameof(total));
            if (successes < 0 || successes > total)
                throw new ArgumentOutOfRangeException(nameof(successes), "Successes must be between 0 and total.");
            if (confidence <= 0.0 || confidence >= 1.0)
                throw new ArgumentOutOfRangeException(nameof(confidence), "Confidence must be between 0 and 1.");

            double p = successes / total;
            double alpha = 1.0 - confidence;
            double z = InverseNormalDistribution(1.0 - alpha / 2.0);

            double margin = z * System.Math.Sqrt(p * (1.0 - p) / total);

            return new ConfidenceIntervalResult
            {
                PointEstimate = p,
                Lower = System.Math.Max(0.0, p - margin),
                Upper = System.Math.Min(1.0, p + margin),
                ConfidenceLevel = confidence
            };
        }

        private static double InverseTDistribution(double p, int df)
        {
            if (df <= 0) throw new ArgumentException("Degrees of freedom must be positive.", nameof(df));

            double x = InverseNormalDistribution(p);
            double x2 = x * x;

            double a1 = 0.25 * x * (x2 + 1.0);
            double a2 = (1.0 / 48.0) * x * (5.0 * x2 * x2 + 16.0 * x2 + 3.0);
            double a3 = (1.0 / 48.0) * x * (3.0 * x2 * x2 * x2 + 19.0 * x2 * x2 + 17.0 * x2 - 15.0);
            double a4 = (1.0 / 3840.0) * x * (79.0 * x2 * x2 * x2 * x2 + 776.0 * x2 * x2 * x2 + 1482.0 * x2 * x2 - 1920.0 * x2 - 945.0);
            double a5 = (1.0 / 92160.0) * x * (27.0 * System.Math.Pow(x2, 5) + 339.0 * x2 * x2 * x2 * x2 + 930.0 * x2 * x2 * x2 - 1782.0 * x2 * x2 - 765.0 * x2 + 17955.0);

            double dfD = df;
            return x + a1 / dfD + a2 / (dfD * dfD) + a3 / (dfD * dfD * dfD) + a4 / (dfD * dfD * dfD * dfD) + a5 / (dfD * dfD * dfD * dfD * dfD);
        }

        private static double InverseNormalDistribution(double p)
        {
            if (p <= 0.0 || p >= 1.0)
                throw new ArgumentOutOfRangeException(nameof(p), "Probability must be between 0 and 1.");

            if (p < 0.5) return -InverseNormalDistribution(1.0 - p);

            double[] a = new double[]
            {
                -3.969683028665376e+01,
                 2.209460984245205e+02,
                -2.759285104469687e+02,
                 1.383577518672690e+02,
                -3.066479806614716e+01,
                 2.506628277459239e+00
            };

            double[] b = new double[]
            {
                -5.447609879822406e+01,
                 1.615858368580409e+02,
                -1.556989798598866e+02,
                 6.680131188771972e+01,
                -1.328068155288572e+01
            };

            double[] c = new double[]
            {
                -7.784894002430293e-03,
                -3.223964580411365e-01,
                -2.400758277161838e+00,
                -2.549732539343734e+00,
                 4.374664141464968e+00,
                 2.938163982698783e+00
            };

            double[] d = new double[]
            {
                 7.784695709041462e-03,
                 3.224671290700398e-01,
                 2.445134137142996e+00,
                 3.754408661907416e+00
            };

            const double pLow = 0.02425;
            const double pHigh = 1.0 - pLow;
            double q, r;

            if (p < pLow)
            {
                q = System.Math.Sqrt(-2.0 * System.Math.Log(p));
                return (((((c[0] * q + c[1]) * q + c[2]) * q + c[3]) * q + c[4]) * q + c[5]) /
                       ((((d[0] * q + d[1]) * q + d[2]) * q + d[3]) * q + 1.0);
            }
            else if (p <= pHigh)
            {
                q = p - 0.5;
                r = q * q;
                return (((((a[0] * r + a[1]) * r + a[2]) * r + a[3]) * r + a[4]) * r + a[5]) * q /
                       (((((b[0] * r + b[1]) * r + b[2]) * r + b[3]) * r + b[4]) * r + 1.0);
            }
            else
            {
                q = System.Math.Sqrt(-2.0 * System.Math.Log(1.0 - p));
                return -(((((c[0] * q + c[1]) * q + c[2]) * q + c[3]) * q + c[4]) * q + c[5]) /
                        ((((d[0] * q + d[1]) * q + d[2]) * q + d[3]) * q + 1.0);
            }
        }
    }
}