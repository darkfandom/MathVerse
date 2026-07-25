namespace MathVerse.Math.DataScience.FeatureEngineering
{
    using System;
    using System.Collections.Generic;
    using MathVerse.Math.DataScience.Core;

    /// <summary>
    /// Specifies the scaling method to apply to features.
    /// </summary>
    public enum ScaleMethod
    {
        /// <summary>Min-Max scaling to [0, 1].</summary>
        MinMax,

        /// <summary>Z-score standardization (mean=0, stddev=1).</summary>
        Standard,

        /// <summary>Robust scaling using median and IQR.</summary>
        Robust
    }

    /// <summary>
    /// Scales features in a dataset using various scaling methods.
    /// </summary>
    public sealed class Scaler
    {
        /// <summary>
        /// Applies Min-Max scaling to the specified columns, transforming values to [0, 1].
        /// Formula: (x - min) / (max - min).
        /// </summary>
        /// <param name="ds">The dataset to scale.</param>
        /// <param name="cols">The column names to scale.</param>
        /// <returns>The modified dataset with scaled columns.</returns>
        public static Dataset MinMaxScale(Dataset ds, string[] cols)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (cols is null || cols.Length == 0) throw new ArgumentException("Columns array cannot be null or empty.", nameof(cols));

            foreach (string col in cols)
            {
                double min = double.MaxValue;
                double max = double.MinValue;

                foreach (Dictionary<string, object?> row in ds.Rows)
                {
                    if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                    {
                        double d = Convert.ToDouble(val);
                        if (d < min) min = d;
                        if (d > max) max = d;
                    }
                }

                double range = max - min;
                if (range == 0.0) continue;

                foreach (Dictionary<string, object?> row in ds.Rows)
                {
                    if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                    {
                        row[col] = (Convert.ToDouble(val) - min) / range;
                    }
                }
            }

            return ds;
        }

        /// <summary>
        /// Applies Z-score standardization to the specified columns.
        /// Formula: (x - mean) / stddev.
        /// </summary>
        /// <param name="ds">The dataset to scale.</param>
        /// <param name="cols">The column names to scale.</param>
        /// <returns>The modified dataset with standardized columns.</returns>
        public static Dataset StandardScale(Dataset ds, string[] cols)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (cols is null || cols.Length == 0) throw new ArgumentException("Columns array cannot be null or empty.", nameof(cols));

            foreach (string col in cols)
            {
                List<double> values = new();
                foreach (Dictionary<string, object?> row in ds.Rows)
                {
                    if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                    {
                        values.Add(Convert.ToDouble(val));
                    }
                }

                if (values.Count < 2) continue;

                double sum = 0.0;
                foreach (double v in values) sum += v;
                double mean = sum / values.Count;

                double variance = 0.0;
                foreach (double v in values)
                {
                    double diff = v - mean;
                    variance += diff * diff;
                }
                double stdDev = System.Math.Sqrt(variance / values.Count);

                if (stdDev == 0.0) continue;

                foreach (Dictionary<string, object?> row in ds.Rows)
                {
                    if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                    {
                        row[col] = (Convert.ToDouble(val) - mean) / stdDev;
                    }
                }
            }

            return ds;
        }

        /// <summary>
        /// Applies robust scaling to the specified columns using the median and IQR.
        /// Formula: (x - median) / IQR.
        /// </summary>
        /// <param name="ds">The dataset to scale.</param>
        /// <param name="cols">The column names to scale.</param>
        /// <returns>The modified dataset with robustly scaled columns.</returns>
        public static Dataset RobustScale(Dataset ds, string[] cols)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (cols is null || cols.Length == 0) throw new ArgumentException("Columns array cannot be null or empty.", nameof(cols));

            foreach (string col in cols)
            {
                List<double> values = new();
                foreach (Dictionary<string, object?> row in ds.Rows)
                {
                    if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                    {
                        values.Add(Convert.ToDouble(val));
                    }
                }

                if (values.Count < 4) continue;

                values.Sort();
                int n = values.Count;
                double median = n % 2 == 0
                    ? (values[n / 2 - 1] + values[n / 2]) / 2.0
                    : values[n / 2];

                double q1 = Percentile(values, 25.0);
                double q3 = Percentile(values, 75.0);
                double iqr = q3 - q1;

                if (iqr == 0.0) continue;

                foreach (Dictionary<string, object?> row in ds.Rows)
                {
                    if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                    {
                        row[col] = (Convert.ToDouble(val) - median) / iqr;
                    }
                }
            }

            return ds;
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

        private static bool IsNumeric(object value)
        {
            return value is int or long or float or double or decimal or short or byte;
        }
    }
}