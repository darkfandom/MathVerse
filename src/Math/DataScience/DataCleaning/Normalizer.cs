namespace MathVerse.Math.DataScience.DataCleaning
{
    using System;
    using System.Collections.Generic;
    using MathVerse.Math.DataScience.Core;

    /// <summary>
    /// Normalizes columns in a dataset using various normalization techniques.
    /// </summary>
    public sealed class Normalizer
    {
        /// <summary>
        /// Applies Min-Max normalization to the specified columns.
        /// Transforms values to the range [0, 1] using: (x - min) / (max - min).
        /// </summary>
        /// <param name="ds">The dataset to normalize.</param>
        /// <param name="cols">The column names to normalize.</param>
        /// <returns>The modified dataset with normalized columns.</returns>
        public static Dataset MinMax(Dataset ds, string[] cols)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (cols is null || cols.Length == 0) throw new ArgumentException("Columns array cannot be null or empty.", nameof(cols));

            foreach (string col in cols)
            {
                double min = double.MaxValue;
                double max = double.MinValue;
                bool found = false;

                foreach (Dictionary<string, object?> row in ds.Rows)
                {
                    if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                    {
                        double d = Convert.ToDouble(val);
                        if (d < min) min = d;
                        if (d > max) max = d;
                        found = true;
                    }
                }

                if (!found) continue;

                double range = max - min;
                if (range == 0.0)
                {
                    foreach (Dictionary<string, object?> row in ds.Rows)
                    {
                        if (row.ContainsKey(col) && row[col] is not null)
                            row[col] = 0.0;
                    }
                }
                else
                {
                    foreach (Dictionary<string, object?> row in ds.Rows)
                    {
                        if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                        {
                            row[col] = (Convert.ToDouble(val) - min) / range;
                        }
                    }
                }
            }

            return ds;
        }

        /// <summary>
        /// Applies Max-Abs normalization to the specified columns.
        /// Transforms values by dividing by the maximum absolute value.
        /// </summary>
        /// <param name="ds">The dataset to normalize.</param>
        /// <param name="cols">The column names to normalize.</param>
        /// <returns>The modified dataset with normalized columns.</returns>
        public static Dataset MaxAbs(Dataset ds, string[] cols)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (cols is null || cols.Length == 0) throw new ArgumentException("Columns array cannot be null or empty.", nameof(cols));

            foreach (string col in cols)
            {
                double maxAbs = 0.0;
                foreach (Dictionary<string, object?> row in ds.Rows)
                {
                    if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                    {
                        double abs = System.Math.Abs(Convert.ToDouble(val));
                        if (abs > maxAbs) maxAbs = abs;
                    }
                }

                if (maxAbs == 0.0) continue;

                foreach (Dictionary<string, object?> row in ds.Rows)
                {
                    if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                    {
                        row[col] = Convert.ToDouble(val) / maxAbs;
                    }
                }
            }

            return ds;
        }

        /// <summary>
        /// Applies L2 normalization to the specified columns.
        /// Transforms values by dividing by the Euclidean norm of the column vector.
        /// </summary>
        /// <param name="ds">The dataset to normalize.</param>
        /// <param name="cols">The column names to normalize.</param>
        /// <returns>The modified dataset with L2-normalized columns.</returns>
        public static Dataset L2Norm(Dataset ds, string[] cols)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (cols is null || cols.Length == 0) throw new ArgumentException("Columns array cannot be null or empty.", nameof(cols));

            foreach (string col in cols)
            {
                double sumSquares = 0.0;
                foreach (Dictionary<string, object?> row in ds.Rows)
                {
                    if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                    {
                        double d = Convert.ToDouble(val);
                        sumSquares += d * d;
                    }
                }

                double norm = System.Math.Sqrt(sumSquares);
                if (norm == 0.0) continue;

                foreach (Dictionary<string, object?> row in ds.Rows)
                {
                    if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                    {
                        row[col] = Convert.ToDouble(val) / norm;
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