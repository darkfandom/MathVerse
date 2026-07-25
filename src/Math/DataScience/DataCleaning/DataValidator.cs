namespace MathVerse.Math.DataScience.DataCleaning
{
    using System;
    using System.Collections.Generic;
    using MathVerse.Math.DataScience.Core;

    /// <summary>
    /// Specifies the expected data type for a column.
    /// </summary>
    public enum ColumnType
    {
        /// <summary>Integer numeric type.</summary>
        Integer,

        /// <summary>Floating-point numeric type.</summary>
        Double,

        /// <summary>String/text type.</summary>
        String,

        /// <summary>Boolean type.</summary>
        Boolean,

        /// <summary>Date/time type.</summary>
        DateTime
    }

    /// <summary>
    /// Validates data types and value ranges in datasets.
    /// </summary>
    public sealed class DataValidator
    {
        /// <summary>
        /// Validates that all non-null values in the specified column match the expected type.
        /// </summary>
        /// <param name="ds">The dataset to validate.</param>
        /// <param name="col">The column name to validate.</param>
        /// <param name="expectedType">The expected data type for the column.</param>
        /// <returns>A list of row indices containing invalid values.</returns>
        public static List<int> ValidateColumn(Dataset ds, string col, ColumnType expectedType)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (string.IsNullOrEmpty(col)) throw new ArgumentException("Column name cannot be null or empty.", nameof(col));

            List<int> invalidIndices = new();

            for (int i = 0; i < ds.Rows.Count; i++)
            {
                if (!ds.Rows[i].TryGetValue(col, out object? val) || val is null) continue;

                bool valid = expectedType switch
                {
                    ColumnType.Integer => val is int or long or short or byte,
                    ColumnType.Double => val is float or double or decimal or int or long,
                    ColumnType.String => val is string,
                    ColumnType.Boolean => val is bool,
                    ColumnType.DateTime => val is DateTime,
                    _ => false
                };

                if (!valid)
                {
                    invalidIndices.Add(i);
                }
            }

            return invalidIndices;
        }

        /// <summary>
        /// Validates that all non-null numeric values in the specified column fall within the given range.
        /// </summary>
        /// <param name="ds">The dataset to validate.</param>
        /// <param name="col">The column name to validate.</param>
        /// <param name="min">The minimum allowed value (inclusive).</param>
        /// <param name="max">The maximum allowed value (inclusive).</param>
        /// <returns>A list of row indices containing out-of-range values.</returns>
        public static List<int> ValidateRange(Dataset ds, string col, double min, double max)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (string.IsNullOrEmpty(col)) throw new ArgumentException("Column name cannot be null or empty.", nameof(col));

            List<int> invalidIndices = new();

            for (int i = 0; i < ds.Rows.Count; i++)
            {
                if (ds.Rows[i].TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                {
                    double d = Convert.ToDouble(val);
                    if (d < min || d > max)
                    {
                        invalidIndices.Add(i);
                    }
                }
            }

            return invalidIndices;
        }

        /// <summary>
        /// Validates that the specified columns are not null or empty in any row.
        /// </summary>
        /// <param name="ds">The dataset to validate.</param>
        /// <param name="cols">The column names to check for non-null values.</param>
        /// <returns>A list of row indices containing null values in the specified columns.</returns>
        public static List<int> ValidateNotNull(Dataset ds, string[] cols)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (cols is null || cols.Length == 0) throw new ArgumentException("Columns array cannot be null or empty.", nameof(cols));

            List<int> invalidIndices = new();

            for (int i = 0; i < ds.Rows.Count; i++)
            {
                foreach (string col in cols)
                {
                    if (!ds.Rows[i].ContainsKey(col) || ds.Rows[i][col] is null)
                    {
                        invalidIndices.Add(i);
                        break;
                    }
                }
            }

            return invalidIndices;
        }

        private static bool IsNumeric(object value)
        {
            return value is int or long or float or double or decimal or short or byte;
        }
    }
}