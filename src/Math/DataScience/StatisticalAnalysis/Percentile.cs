namespace MathVerse.Math.DataScience.StatisticalAnalysis
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Computes percentiles for datasets using linear interpolation.
    /// </summary>
    public sealed class Percentile
    {
        /// <summary>
        /// Computes the specified percentile of the data using linear interpolation.
        /// The percentile parameter should be between 0 and 100 (e.g., 50 for the median).
        /// </summary>
        /// <param name="data">The data array to compute the percentile from.</param>
        /// <param name="percentile">The percentile to compute (0-100).</param>
        /// <returns>The computed percentile value.</returns>
        public static double Compute(double[] data, double percentile)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));
            if (percentile < 0.0 || percentile > 100.0)
                throw new ArgumentOutOfRangeException(nameof(percentile), "Percentile must be between 0 and 100.");

            List<double> valid = new();
            foreach (double v in data)
            {
                if (!double.IsNaN(v) && !double.IsInfinity(v))
                    valid.Add(v);
            }

            if (valid.Count == 0) return double.NaN;

            valid.Sort();
            int n = valid.Count;

            if (n == 1) return valid[0];

            double index = (percentile / 100.0) * (n - 1);
            int lower = (int)System.Math.Floor(index);
            int upper = (int)System.Math.Ceiling(index);

            if (lower == upper) return valid[lower];

            double fraction = index - lower;
            return valid[lower] + fraction * (valid[upper] - valid[lower]);
        }
    }
}