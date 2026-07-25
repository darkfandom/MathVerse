namespace MathVerse.Math.DataScience.FeatureEngineering
{
    using System;
    using System.Collections.Generic;
    using MathVerse.Math.DataScience.Core;
    using MathVerse.Math.DataScience.DatasetManagement;

    /// <summary>
    /// Generates derived features by applying mathematical transformations to existing columns.
    /// </summary>
    public sealed class DimensionalFeatureGenerator
    {
        /// <summary>
        /// Generates natural logarithm features for the specified columns.
        /// Adds new columns named col_log containing ln(x) for each column.
        /// </summary>
        /// <param name="ds">The dataset to augment.</param>
        /// <param name="cols">The column names to transform.</param>
        /// <returns>The modified dataset with logarithmic features added.</returns>
        public static Dataset Log(Dataset ds, string[] cols)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (cols is null || cols.Length == 0) throw new ArgumentException("Columns array cannot be null or empty.", nameof(cols));

            foreach (string col in cols)
            {
                string newName = $"{col}_log";
                ds.Schema.AddColumn(newName, ColumnType.Double);

                foreach (Dictionary<string, object?> row in ds.Rows)
                {
                    if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                    {
                        double d = Convert.ToDouble(val);
                        row[newName] = d > 0.0 ? System.Math.Log(d) : (object?)null;
                    }
                    else
                    {
                        row[newName] = null;
                    }
                }
            }

            return ds;
        }

        /// <summary>
        /// Generates square root features for the specified columns.
        /// Adds new columns named col_sqrt containing sqrt(x) for each column.
        /// </summary>
        /// <param name="ds">The dataset to augment.</param>
        /// <param name="cols">The column names to transform.</param>
        /// <returns>The modified dataset with square root features added.</returns>
        public static Dataset Sqrt(Dataset ds, string[] cols)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (cols is null || cols.Length == 0) throw new ArgumentException("Columns array cannot be null or empty.", nameof(cols));

            foreach (string col in cols)
            {
                string newName = $"{col}_sqrt";
                ds.Schema.AddColumn(newName, ColumnType.Double);

                foreach (Dictionary<string, object?> row in ds.Rows)
                {
                    if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                    {
                        double d = Convert.ToDouble(val);
                        row[newName] = d >= 0.0 ? System.Math.Sqrt(d) : (object?)null;
                    }
                    else
                    {
                        row[newName] = null;
                    }
                }
            }

            return ds;
        }

        /// <summary>
        /// Generates power features for the specified columns.
        /// Adds new columns named col_pow containing x^exp for each column.
        /// </summary>
        /// <param name="ds">The dataset to augment.</param>
        /// <param name="cols">The column names to transform.</param>
        /// <param name="exp">The exponent to raise each value to.</param>
        /// <returns>The modified dataset with power features added.</returns>
        public static Dataset Power(Dataset ds, string[] cols, double exp)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (cols is null || cols.Length == 0) throw new ArgumentException("Columns array cannot be null or empty.", nameof(cols));

            foreach (string col in cols)
            {
                string newName = $"{col}_pow{exp.ToString("G")}";
                ds.Schema.AddColumn(newName, ColumnType.Double);

                foreach (Dictionary<string, object?> row in ds.Rows)
                {
                    if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                    {
                        double d = Convert.ToDouble(val);
                        row[newName] = System.Math.Pow(d, exp);
                    }
                    else
                    {
                        row[newName] = null;
                    }
                }
            }

            return ds;
        }

        /// <summary>
        /// Generates a ratio feature from two columns.
        /// Adds a new column named col1_div_col2 containing col1/col2.
        /// </summary>
        /// <param name="ds">The dataset to augment.</param>
        /// <param name="col1">The numerator column name.</param>
        /// <param name="col2">The denominator column name.</param>
        /// <returns>The modified dataset with the ratio feature added.</returns>
        public static Dataset Ratio(Dataset ds, string col1, string col2)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (string.IsNullOrEmpty(col1)) throw new ArgumentException("Column 1 cannot be null or empty.", nameof(col1));
            if (string.IsNullOrEmpty(col2)) throw new ArgumentException("Column 2 cannot be null or empty.", nameof(col2));

            string newName = $"{col1}_div_{col2}";
            ds.Schema.AddColumn(newName, ColumnType.Double);

            foreach (Dictionary<string, object?> row in ds.Rows)
            {
                if (row.TryGetValue(col1, out object? v1) && v1 is not null && IsNumeric(v1) &&
                    row.TryGetValue(col2, out object? v2) && v2 is not null && IsNumeric(v2))
                {
                    double denom = Convert.ToDouble(v2);
                    row[newName] = denom != 0.0 ? Convert.ToDouble(v1) / denom : (object?)null;
                }
                else
                {
                    row[newName] = null;
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