namespace MathVerse.Math.DataScience.FeatureEngineering
{
    using System;
    using System.Collections.Generic;
    using MathVerse.Math.DataScience.Core;
    using MathVerse.Math.DataScience.DatasetManagement;

    /// <summary>
    /// Generates polynomial and interaction features from existing numeric columns.
    /// </summary>
    public sealed class PolynomialFeatureGenerator
    {
        /// <summary>
        /// Generates polynomial features for the specified columns up to the given degree.
        /// For degree 2, adds x^2 terms and interaction terms x1*x2 for all column pairs.
        /// </summary>
        /// <param name="ds">The dataset to augment.</param>
        /// <param name="columns">The column names to generate polynomial features from.</param>
        /// <param name="degree">The maximum polynomial degree (default 2).</param>
        /// <returns>The modified dataset with polynomial features added.</returns>
        public static Dataset Generate(Dataset ds, string[] columns, int degree = 2)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (columns is null || columns.Length == 0) throw new ArgumentException("Columns array cannot be null or empty.", nameof(columns));
            if (degree < 1) throw new ArgumentException("Degree must be at least 1.", nameof(degree));

            foreach (string col in columns)
            {
                for (int d = 2; d <= degree; d++)
                {
                    string newName = $"{col}_^{d}";
                    ds.Schema.AddColumn(newName, ColumnType.Double);

                    foreach (Dictionary<string, object?> row in ds.Rows)
                    {
                        if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                        {
                            double baseVal = Convert.ToDouble(val);
                            row[newName] = System.Math.Pow(baseVal, d);
                        }
                        else
                        {
                            row[newName] = null;
                        }
                    }
                }
            }

            if (degree >= 2)
            {
                for (int i = 0; i < columns.Length; i++)
                {
                    for (int j = i + 1; j < columns.Length; j++)
                    {
                        string newName = $"{columns[i]}_x_{columns[j]}";
                        ds.Schema.AddColumn(newName, ColumnType.Double);

                        foreach (Dictionary<string, object?> row in ds.Rows)
                        {
                            if (row.TryGetValue(columns[i], out object? v1) && v1 is not null && IsNumeric(v1) &&
                                row.TryGetValue(columns[j], out object? v2) && v2 is not null && IsNumeric(v2))
                            {
                                row[newName] = Convert.ToDouble(v1) * Convert.ToDouble(v2);
                            }
                            else
                            {
                                row[newName] = null;
                            }
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