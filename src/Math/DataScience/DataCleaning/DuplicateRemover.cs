namespace MathVerse.Math.DataScience.DataCleaning
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MathVerse.Math.DataScience.Core;

    /// <summary>
    /// Removes duplicate rows from datasets using various strategies.
    /// </summary>
    public sealed class DuplicateRemover
    {
        /// <summary>
        /// Removes rows that are exact duplicates across all columns.
        /// </summary>
        /// <param name="ds">The dataset to deduplicate.</param>
        /// <returns>The dataset with exact duplicate rows removed, keeping the first occurrence.</returns>
        public static Dataset RemoveExact(Dataset ds)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));

            HashSet<string> seen = new();
            List<Dictionary<string, object?>> unique = new();

            foreach (Dictionary<string, object?> row in ds.Rows)
            {
                string key = RowToKey(row);
                if (seen.Add(key))
                {
                    unique.Add(row);
                }
            }

            ds.Rows.Clear();
            foreach (Dictionary<string, object?> row in unique)
            {
                ds.Rows.Add(row);
            }

            return ds;
        }

        /// <summary>
        /// Removes duplicate rows based on specified key columns.
        /// </summary>
        /// <param name="ds">The dataset to deduplicate.</param>
        /// <param name="keyColumns">The column names to use as the deduplication key.</param>
        /// <returns>The dataset with duplicates removed, keeping the first occurrence per key.</returns>
        public static Dataset RemoveByKey(Dataset ds, string[] keyColumns)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (keyColumns is null || keyColumns.Length == 0) throw new ArgumentException("Key columns cannot be null or empty.", nameof(keyColumns));

            HashSet<string> seen = new();
            List<Dictionary<string, object?>> unique = new();

            foreach (Dictionary<string, object?> row in ds.Rows)
            {
                string key = RowToKeyByColumns(row, keyColumns);
                if (seen.Add(key))
                {
                    unique.Add(row);
                }
            }

            ds.Rows.Clear();
            foreach (Dictionary<string, object?> row in unique)
            {
                ds.Rows.Add(row);
            }

            return ds;
        }

        /// <summary>
        /// Removes duplicates using a custom key selector function.
        /// </summary>
        /// <param name="ds">The dataset to deduplicate.</param>
        /// <param name="keySelector">A function that generates a deduplication key from a row.</param>
        /// <returns>The dataset with duplicates removed, keeping the first occurrence per key.</returns>
        public static Dataset RemoveDuplicates(Dataset ds, Func<Dictionary<string, object?>, string> keySelector)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (keySelector is null) throw new ArgumentNullException(nameof(keySelector));

            HashSet<string> seen = new();
            List<Dictionary<string, object?>> unique = new();

            foreach (Dictionary<string, object?> row in ds.Rows)
            {
                string key = keySelector(row);
                if (seen.Add(key))
                {
                    unique.Add(row);
                }
            }

            ds.Rows.Clear();
            foreach (Dictionary<string, object?> row in unique)
            {
                ds.Rows.Add(row);
            }

            return ds;
        }

        private static string RowToKey(Dictionary<string, object?> row)
        {
            return string.Join("|", row.OrderBy(kv => kv.Key).Select(kv => kv.Value?.ToString() ?? "NULL"));
        }

        private static string RowToKeyByColumns(Dictionary<string, object?> row, string[] columns)
        {
            List<string> parts = new(columns.Length);
            foreach (string col in columns)
            {
                if (row.TryGetValue(col, out object? val))
                    parts.Add(val?.ToString() ?? "NULL");
                else
                    parts.Add("NULL");
            }
            return string.Join("|", parts);
        }
    }
}