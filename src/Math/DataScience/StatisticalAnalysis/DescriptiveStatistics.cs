namespace MathVerse.Math.DataScience.StatisticalAnalysis
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Contains the results of descriptive statistical analysis.
    /// </summary>
    public sealed class DescriptiveStatsResult
    {
        /// <summary>Gets or sets the arithmetic mean.</summary>
        public double Mean { get; set; }

        /// <summary>Gets or sets the median (50th percentile).</summary>
        public double Median { get; set; }

        /// <summary>Gets or sets the standard deviation (population).</summary>
        public double StdDev { get; set; }

        /// <summary>Gets or sets the variance (population).</summary>
        public double Variance { get; set; }

        /// <summary>Gets or sets the minimum value.</summary>
        public double Min { get; set; }

        /// <summary>Gets or sets the maximum value.</summary>
        public double Max { get; set; }

        /// <summary>Gets or sets the first quartile (25th percentile).</summary>
        public double Q1 { get; set; }

        /// <summary>Gets or sets the third quartile (75th percentile).</summary>
        public double Q3 { get; set; }

        /// <summary>Gets or sets the interquartile range (Q3 - Q1).</summary>
        public double IQR { get; set; }

        /// <summary>Gets or sets the skewness measure.</summary>
        public double Skewness { get; set; }

        /// <summary>Gets or sets the excess kurtosis measure (kurtosis - 3).</summary>
        public double Kurtosis { get; set; }

        /// <summary>Gets or sets the total count of non-null values.</summary>
        public int Count { get; set; }

        /// <summary>Gets or sets the count of missing (null) values.</summary>
        public int MissingCount { get; set; }
    }

    /// <summary>
    /// Computes descriptive statistics for datasets.
    /// </summary>
    public sealed class DescriptiveStatistics
    {
        /// <summary>
        /// Computes comprehensive descriptive statistics for the given data array.
        /// </summary>
        /// <param name="data">The data array to analyze. Null values are ignored.</param>
        /// <returns>A DescriptiveStatsResult containing all computed statistics.</returns>
        public static DescriptiveStatsResult Compute(double[] data)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));

            List<double> valid = new();
            int missingCount = 0;

            foreach (double v in data)
            {
                if (double.IsNaN(v) || double.IsInfinity(v))
                    missingCount++;
                else
                    valid.Add(v);
            }

            DescriptiveStatsResult result = new()
            {
                Count = valid.Count,
                MissingCount = missingCount
            };

            if (valid.Count == 0) return result;

            valid.Sort();
            int n = valid.Count;

            double sum = 0.0;
            foreach (double v in valid) sum += v;
            result.Mean = sum / n;

            result.Median = n % 2 == 0
                ? (valid[n / 2 - 1] + valid[n / 2]) / 2.0
                : valid[n / 2];

            result.Min = valid[0];
            result.Max = valid[n - 1];

            result.Q1 = Percentile(valid, 25.0);
            result.Q3 = Percentile(valid, 75.0);
            result.IQR = result.Q3 - result.Q1;

            double variance = 0.0;
            foreach (double v in valid)
            {
                double diff = v - result.Mean;
                variance += diff * diff;
            }
            result.Variance = variance / n;
            result.StdDev = System.Math.Sqrt(result.Variance);

            if (result.StdDev > 0.0 && n >= 3)
            {
                double skewness = 0.0;
                double m4 = 0.0;
                double invStd = 1.0 / result.StdDev;
                foreach (double v in valid)
                {
                    double z = (v - result.Mean) * invStd;
                    double z2 = z * z;
                    skewness += z2 * z;
                    m4 += z2 * z2;
                }
                result.Skewness = skewness / n;
                result.Kurtosis = m4 / n - 3.0;
            }

            return result;
        }

        private static double Percentile(List<double> sortedValues, double percentile)
        {
            double index = (percentile / 100.0) * (sortedValues.Count - 1);
            int lower = (int)System.Math.Floor(index);
            int upper = (int)System.Math.Ceiling(index);
            if (lower == upper) return sortedValues[lower];
            double fraction = index - lower;
            return sortedValues[lower] + fraction * (sortedValues[upper] - sortedValues[lower]);
        }
    }
}