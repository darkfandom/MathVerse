namespace MathVerse.Math.DataScience.DataCleaning
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MathVerse.Math.DataScience.Core;

    /// <summary>
    /// Handles missing values in datasets through various imputation and removal strategies.
    /// </summary>
    public sealed class MissingValueHandler
    {
        /// <summary>
        /// Imputes missing values in the specified column with the mean of existing values.
        /// </summary>
        /// <param name="ds">The dataset to modify.</param>
        /// <param name="col">The column name to impute.</param>
        /// <returns>The modified dataset with missing values replaced by the mean.</returns>
        public static Dataset ImputeMean(Dataset ds, string col)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (string.IsNullOrEmpty(col)) throw new ArgumentException("Column name cannot be null or empty.", nameof(col));

            double sum = 0.0;
            int count = 0;
            foreach (Dictionary<string, object?> row in ds.Rows)
            {
                if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                {
                    sum += Convert.ToDouble(val);
                    count++;
                }
            }

            if (count == 0) return ds;

            double mean = sum / count;

            foreach (Dictionary<string, object?> row in ds.Rows)
            {
                if (!row.ContainsKey(col) || row[col] is null || !IsNumeric(row[col]!))
                {
                    row[col] = mean;
                }
            }

            return ds;
        }

        /// <summary>
        /// Imputes missing values in the specified column with the median of existing values.
        /// </summary>
        /// <param name="ds">The dataset to modify.</param>
        /// <param name="col">The column name to impute.</param>
        /// <returns>The modified dataset with missing values replaced by the median.</returns>
        public static Dataset ImputeMedian(Dataset ds, string col)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (string.IsNullOrEmpty(col)) throw new ArgumentException("Column name cannot be null or empty.", nameof(col));

            List<double> values = new();
            foreach (Dictionary<string, object?> row in ds.Rows)
            {
                if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                {
                    values.Add(Convert.ToDouble(val));
                }
            }

            if (values.Count == 0) return ds;

            values.Sort();
            double median;
            int n = values.Count;
            if (n % 2 == 0)
            {
                median = (values[n / 2 - 1] + values[n / 2]) / 2.0;
            }
            else
            {
                median = values[n / 2];
            }

            foreach (Dictionary<string, object?> row in ds.Rows)
            {
                if (!row.ContainsKey(col) || row[col] is null || !IsNumeric(row[col]!))
                {
                    row[col] = median;
                }
            }

            return ds;
        }

        /// <summary>
        /// Imputes missing values in the specified column with the mode (most frequent value).
        /// </summary>
        /// <param name="ds">The dataset to modify.</param>
        /// <param name="col">The column name to impute.</param>
        /// <returns>The modified dataset with missing values replaced by the mode.</returns>
        public static Dataset ImputeMode(Dataset ds, string col)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (string.IsNullOrEmpty(col)) throw new ArgumentException("Column name cannot be null or empty.", nameof(col));

            Dictionary<object, int> frequency = new();
            foreach (Dictionary<string, object?> row in ds.Rows)
            {
                if (row.TryGetValue(col, out object? val) && val is not null)
                {
                    if (frequency.ContainsKey(val))
                        frequency[val]++;
                    else
                        frequency[val] = 1;
                }
            }

            if (frequency.Count == 0) return ds;

            object mode = frequency.OrderByDescending(kv => kv.Value).First().Key;

            foreach (Dictionary<string, object?> row in ds.Rows)
            {
                if (!row.ContainsKey(col) || row[col] is null)
                {
                    row[col] = mode;
                }
            }

            return ds;
        }

        /// <summary>
        /// Imputes missing values in the specified column with a constant value.
        /// </summary>
        /// <param name="ds">The dataset to modify.</param>
        /// <param name="col">The column name to impute.</param>
        /// <param name="value">The constant value to use for imputation.</param>
        /// <returns>The modified dataset with missing values replaced by the constant.</returns>
        public static Dataset ImputeConstant(Dataset ds, string col, object value)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (string.IsNullOrEmpty(col)) throw new ArgumentException("Column name cannot be null or empty.", nameof(col));

            foreach (Dictionary<string, object?> row in ds.Rows)
            {
                if (!row.ContainsKey(col) || row[col] is null)
                {
                    row[col] = value;
                }
            }

            return ds;
        }

        /// <summary>
        /// Drops rows that have missing values in any of the specified columns.
        /// </summary>
        /// <param name="ds">The dataset to modify.</param>
        /// <param name="cols">The column names to check for missing values.</param>
        /// <returns>The modified dataset with rows containing missing values removed.</returns>
        public static Dataset DropRows(Dataset ds, string[] cols)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (cols is null || cols.Length == 0) throw new ArgumentException("Columns array cannot be null or empty.", nameof(cols));

            List<Dictionary<string, object?>> rowsToRemove = new();
            foreach (Dictionary<string, object?> row in ds.Rows)
            {
                foreach (string col in cols)
                {
                    if (!row.ContainsKey(col) || row[col] is null)
                    {
                        rowsToRemove.Add(row);
                        break;
                    }
                }
            }

            foreach (Dictionary<string, object?> row in rowsToRemove)
            {
                ds.Rows.Remove(row);
            }

            return ds;
        }

        /// <summary>
        /// Imputes missing values using linear interpolation based on the row index.
        /// </summary>
        /// <param name="ds">The dataset to modify.</param>
        /// <param name="col">The column name to interpolate.</param>
        /// <returns>The modified dataset with missing values interpolated linearly.</returns>
        public static Dataset InterpolateLinear(Dataset ds, string col)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (string.IsNullOrEmpty(col)) throw new ArgumentException("Column name cannot be null or empty.", nameof(col));

            int rowCount = ds.Rows.Count;
            if (rowCount == 0) return ds;

            double?[] values = new double?[rowCount];
            for (int i = 0; i < rowCount; i++)
            {
                if (ds.Rows[i].TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                {
                    values[i] = Convert.ToDouble(val);
                }
                else
                {
                    values[i] = null;
                }
            }

            for (int i = 0; i < rowCount; i++)
            {
                if (values[i].HasValue) continue;

                int prevIdx = -1;
                int nextIdx = -1;

                for (int j = i - 1; j >= 0; j--)
                {
                    if (values[j].HasValue)
                    {
                        prevIdx = j;
                        break;
                    }
                }

                for (int j = i + 1; j < rowCount; j++)
                {
                    if (values[j].HasValue)
                    {
                        nextIdx = j;
                        break;
                    }
                }

                if (prevIdx >= 0 && nextIdx >= 0)
                {
                    double t = (double)(i - prevIdx) / (nextIdx - prevIdx);
                    double interpolated = values[prevIdx]!.Value + t * (values[nextIdx]!.Value - values[prevIdx]!.Value);
                    ds.Rows[i][col] = interpolated;
                }
                else if (prevIdx >= 0)
                {
                    ds.Rows[i][col] = values[prevIdx]!.Value;
                }
                else if (nextIdx >= 0)
                {
                    ds.Rows[i][col] = values[nextIdx]!.Value;
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