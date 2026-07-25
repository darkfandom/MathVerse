namespace MathVerse.Math.DataScience.FeatureEngineering
{
    using System;
    using System.Collections.Generic;
    using MathVerse.Math.DataScience.Core;

    /// <summary>
    /// Encodes categorical columns using label encoding, mapping each unique category to an integer.
    /// </summary>
    public sealed class LabelEncoder
    {
        /// <summary>
        /// Encodes the specified column using label encoding.
        /// Categories are sorted alphabetically and mapped to integers starting from 0.
        /// </summary>
        /// <param name="ds">The dataset to encode.</param>
        /// <param name="column">The column name to label encode.</param>
        /// <returns>The modified dataset with the column values replaced by integer labels.</returns>
        public static Dataset Encode(Dataset ds, string column)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (string.IsNullOrEmpty(column)) throw new ArgumentException("Column name cannot be null or empty.", nameof(column));

            SortedDictionary<string, int> mapping = new();
            int nextLabel = 0;

            foreach (Dictionary<string, object?> row in ds.Rows)
            {
                string key;
                if (row.TryGetValue(column, out object? val))
                    key = val?.ToString() ?? "NULL";
                else
                    key = "NULL";

                if (!mapping.ContainsKey(key))
                {
                    mapping[key] = nextLabel++;
                }
            }

            foreach (Dictionary<string, object?> row in ds.Rows)
            {
                string key;
                if (row.TryGetValue(column, out object? val))
                    key = val?.ToString() ?? "NULL";
                else
                    key = "NULL";

                row[column] = mapping[key];
            }

            return ds;
        }
    }
}