namespace MathVerse.Math.DataScience.FeatureEngineering
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MathVerse.Math.DataScience.Core;
    using MathVerse.Math.DataScience.DatasetManagement;

    /// <summary>
    /// Encodes categorical columns using one-hot encoding, creating binary dummy columns.
    /// </summary>
    public sealed class OneHotEncoder
    {
        /// <summary>
        /// Encodes the specified column using one-hot encoding.
        /// Each unique category becomes a new binary column (0 or 1).
        /// </summary>
        /// <param name="ds">The dataset to encode.</param>
        /// <param name="column">The column name to one-hot encode.</param>
        /// <returns>A new dataset with the original column replaced by binary dummy columns.</returns>
        public static Dataset Encode(Dataset ds, string column)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (string.IsNullOrEmpty(column)) throw new ArgumentException("Column name cannot be null or empty.", nameof(column));

            SortedSet<string> categories = new();
            foreach (Dictionary<string, object?> row in ds.Rows)
            {
                if (row.TryGetValue(column, out object? val))
                {
                    categories.Add(val?.ToString() ?? "NULL");
                }
                else
                {
                    categories.Add("NULL");
                }
            }

            Dataset result = new();
            foreach (string existingCol in ds.Schema.ColumnNames)
            {
                if (existingCol != column)
                    result.Schema.AddColumn(existingCol, ColumnType.String);
            }

            List<string> catList = categories.ToList();
            foreach (string cat in catList)
            {
                string safeName = SanitizeColumnName(column, cat);
                result.Schema.AddColumn(safeName, ColumnType.String);
            }

            foreach (Dictionary<string, object?> row in ds.Rows)
            {
                Dictionary<string, object?> newRow = new();
                foreach (string existingCol in ds.Schema.ColumnNames)
                {
                    if (existingCol != column)
                    {
                        newRow[existingCol] = row.ContainsKey(existingCol) ? row[existingCol] : null;
                    }
                }

                string rowCat;
                if (row.TryGetValue(column, out object? val))
                    rowCat = val?.ToString() ?? "NULL";
                else
                    rowCat = "NULL";

                foreach (string cat in catList)
                {
                    string safeName = SanitizeColumnName(column, cat);
                    newRow[safeName] = cat == rowCat ? 1 : 0;
                }

                result.Rows.Add(newRow);
            }

            return result;
        }

        private static string SanitizeColumnName(string column, string category)
        {
            string safe = category.Replace(" ", "_").Replace(".", "_").Replace("-", "_");
            return $"{column}_{safe}";
        }
    }
}