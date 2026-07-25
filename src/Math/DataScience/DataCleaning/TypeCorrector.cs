namespace MathVerse.Math.DataScience.DataCleaning
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using MathVerse.Math.DataScience.Core;

    /// <summary>
    /// Automatically detects and corrects data types in datasets by converting string columns to numeric types.
    /// </summary>
    public sealed class TypeCorrector
    {
        /// <summary>
        /// Detects string columns in the dataset and attempts to convert them to numeric types.
        /// If all non-null values in a string column can be parsed as numbers, the column is converted.
        /// </summary>
        /// <param name="ds">The dataset to correct types in.</param>
        /// <returns>The modified dataset with converted types.</returns>
        public static Dataset DetectAndConvert(Dataset ds)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));

            if (ds.Rows.Count == 0) return ds;

            List<string> columns = new(ds.Rows[0].Keys);

            foreach (string col in columns)
            {
                TryConvertColumn(ds, col);
            }

            return ds;
        }

        private static void TryConvertColumn(Dataset ds, string col)
        {
            bool allConvertible = true;
            bool hasDouble = false;

            foreach (Dictionary<string, object?> row in ds.Rows)
            {
                if (!row.TryGetValue(col, out object? val) || val is null) continue;

                if (val is not string strVal)
                {
                    if (IsNumeric(val)) continue;
                    allConvertible = false;
                    break;
                }

                if (string.IsNullOrWhiteSpace(strVal))
                {
                    continue;
                }

                if (double.TryParse(strVal, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                {
                    hasDouble = true;
                }
                else
                {
                    allConvertible = false;
                    break;
                }
            }

            if (!allConvertible) return;

            bool hasInteger = true;
            if (hasDouble)
            {
                foreach (Dictionary<string, object?> row in ds.Rows)
                {
                    if (!row.TryGetValue(col, out object? val) || val is null) continue;
                    if (val is string strVal && !string.IsNullOrWhiteSpace(strVal))
                    {
                        if (!int.TryParse(strVal, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                        {
                            hasInteger = false;
                            break;
                        }
                    }
                }
            }

            foreach (Dictionary<string, object?> row in ds.Rows)
            {
                if (!row.TryGetValue(col, out object? val) || val is null) continue;
                if (val is string strVal && !string.IsNullOrWhiteSpace(strVal))
                {
                    if (hasDouble)
                    {
                        if (hasInteger && int.TryParse(strVal, NumberStyles.Any, CultureInfo.InvariantCulture, out int intResult))
                        {
                            row[col] = intResult;
                        }
                        else if (double.TryParse(strVal, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleResult))
                        {
                            row[col] = doubleResult;
                        }
                    }
                }
            }
        }

        private static bool IsNumeric(object value)
        {
            return value is int or long or float or double or decimal or short or byte;
        }
    }
}