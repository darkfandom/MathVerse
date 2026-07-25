namespace MathVerse.Math.DataScience.DataCleaning
{
    using System;
    using System.Collections.Generic;
    using MathVerse.Math.DataScience.Core;

    /// <summary>
    /// Standardizes columns in a dataset using Z-score standardization.
    /// </summary>
    public sealed class Standardizer
    {
        /// <summary>
        /// Applies Z-score standardization to the specified columns.
        /// Transforms values using: (x - mean) / stddev.
        /// </summary>
        /// <param name="ds">The dataset to standardize.</param>
        /// <param name="cols">The column names to standardize.</param>
        /// <returns>The modified dataset with standardized columns.</returns>
        public static Dataset ZScore(Dataset ds, string[] cols)
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

                if (stdDev == 0.0)
                {
                    foreach (Dictionary<string, object?> row in ds.Rows)
                    {
                        if (row.ContainsKey(col) && row[col] is not null && IsNumeric(row[col]!))
                            row[col] = 0.0;
                    }
                }
                else
                {
                    foreach (Dictionary<string, object?> row in ds.Rows)
                    {
                        if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                        {
                            row[col] = (Convert.ToDouble(val) - mean) / stdDev;
                        }
                    }
                }
            }

            return ds;
        }

        private static bool IsNumeric(object value)
        {
            return value is int or long or float or double or decimal or short or byte;
        }
    }
}