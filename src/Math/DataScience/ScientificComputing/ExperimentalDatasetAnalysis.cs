namespace MathVerse.Math.DataScience.ScientificComputing
{
    using System;

    /// <summary>
    /// Represents the result of experimental dataset analysis including descriptive statistics and error bar information.
    /// </summary>
    public sealed class ExperimentalAnalysisResult
    {
        /// <summary>
        /// Gets the arithmetic mean of the measurements.
        /// </summary>
        public double Mean { get; }

        /// <summary>
        /// Gets the standard deviation of the measurements.
        /// </summary>
        public double StandardDeviation { get; }

        /// <summary>
        /// Gets the standard error of the mean.
        /// </summary>
        public double StandardError { get; }

        /// <summary>
        /// Gets the 95% confidence interval half-width.
        /// </summary>
        public double ConfidenceInterval95 { get; }

        /// <summary>
        /// Gets the minimum measured value.
        /// </summary>
        public double Min { get; }

        /// <summary>
        /// Gets the maximum measured value.
        /// </summary>
        public double Max { get; }

        /// <summary>
        /// Gets the median of the measurements.
        /// </summary>
        public double Median { get; }

        /// <summary>
        /// Gets the total number of measurements.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Gets the propagated uncertainties for each measurement, or null if not provided.
        /// </summary>
        public double[]? Uncertainties { get; }

        /// <summary>
        /// Gets the combined uncertainty of the mean.
        /// </summary>
        public double CombinedUncertainty { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExperimentalAnalysisResult"/> class.
        /// </summary>
        public ExperimentalAnalysisResult(
            double mean, double standardDeviation, double standardError,
            double confidenceInterval95, double min, double max, double median,
            int count, double[]? uncertainties, double combinedUncertainty)
        {
            Mean = mean;
            StandardDeviation = standardDeviation;
            StandardError = standardError;
            ConfidenceInterval95 = confidenceInterval95;
            Min = min;
            Max = max;
            Median = median;
            Count = count;
            Uncertainties = uncertainties;
            CombinedUncertainty = combinedUncertainty;
        }
    }

    /// <summary>
    /// Provides analysis of experimental datasets including descriptive statistics, uncertainty propagation,
    /// and confidence interval estimation.
    /// </summary>
    public sealed class ExperimentalDatasetAnalysis
    {
        private static readonly double[] TTable95 = { 12.706, 4.303, 3.182, 2.776, 2.571, 2.447, 2.365, 2.306, 2.262, 2.228, 2.201, 2.179, 2.160, 2.145, 2.131, 2.120, 2.110, 2.101, 2.093, 2.086 };

        /// <summary>
        /// Analyzes an experimental dataset, computing descriptive statistics and uncertainty information.
        /// If individual measurement uncertainties are provided, they are combined into a total uncertainty for the mean.
        /// </summary>
        /// <param name="measurements">The array of experimental measurements.</param>
        /// <param name="uncertainties">Optional per-measurement uncertainties (standard deviations). If provided, must have the same length as measurements.</param>
        /// <returns>An <see cref="ExperimentalAnalysisResult"/> containing all computed statistics.</returns>
        public static ExperimentalAnalysisResult Analyze(double[] measurements, double[]? uncertainties = null)
        {
            if (measurements == null) throw new ArgumentNullException(nameof(measurements));
            if (measurements.Length == 0) throw new ArgumentException("Measurements must not be empty.");
            if (uncertainties != null && uncertainties.Length != measurements.Length)
                throw new ArgumentException("Uncertainties array must have the same length as measurements.");

            int n = measurements.Length;

            double mean = 0.0;
            for (int i = 0; i < n; i++)
            {
                mean += measurements[i];
            }
            mean /= n;

            double variance = 0.0;
            for (int i = 0; i < n; i++)
            {
                double d = measurements[i] - mean;
                variance += d * d;
            }
            variance /= (n - 1);
            double stdDev = System.Math.Sqrt(variance);

            double stdError = stdDev / System.Math.Sqrt(n);

            double tValue = GetTValue95(n - 1);
            double ci95 = tValue * stdError;

            double min = measurements[0];
            double max = measurements[0];
            for (int i = 1; i < n; i++)
            {
                if (measurements[i] < min) min = measurements[i];
                if (measurements[i] > max) max = measurements[i];
            }

            double[] sorted = new double[n];
            for (int i = 0; i < n; i++) sorted[i] = measurements[i];
            Array.Sort(sorted);
            double median;
            if (n % 2 == 1)
            {
                median = sorted[n / 2];
            }
            else
            {
                median = (sorted[n / 2 - 1] + sorted[n / 2]) / 2.0;
            }

            double combinedUncertainty;
            if (uncertainties != null)
            {
                double sumUncertSq = 0.0;
                for (int i = 0; i < n; i++)
                {
                    sumUncertSq += uncertainties[i] * uncertainties[i];
                }
                combinedUncertainty = System.Math.Sqrt(sumUncertSq) / n;
            }
            else
            {
                combinedUncertainty = stdError;
            }

            return new ExperimentalAnalysisResult(
                mean, stdDev, stdError, ci95, min, max, median,
                n, uncertainties, combinedUncertainty);
        }

        private static double GetTValue95(int degreesOfFreedom)
        {
            if (degreesOfFreedom <= 0)
                throw new ArgumentException("Degrees of freedom must be positive.");

            if (degreesOfFreedom <= TTable95.Length)
            {
                return TTable95[degreesOfFreedom - 1];
            }

            return 1.96;
        }
    }
}
