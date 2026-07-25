namespace MathVerse.Math.DataScience.FeatureEngineering
{
    using System;
    using System.Collections.Generic;
    using MathVerse.Math.DataScience.Core;
    using MathVerse.Math.DataScience.DatasetManagement;

    /// <summary>
    /// Generates interaction (cross) features by computing pairwise products of specified columns.
    /// </summary>
    public sealed class InteractionFeatureGenerator
    {
        /// <summary>
        /// Generates all pairwise interaction features from the specified columns.
        /// Each new column is named col1_x_col2 and contains the product of the two column values.
        /// </summary>
        /// <param name="ds">The dataset to augment.</param>
        /// <param name="columns">The column names to generate interactions from.</param>
        /// <returns>The modified dataset with interaction features added.</returns>
        public static Dataset Generate(Dataset ds, string[] columns)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (columns is null || columns.Length < 2) throw new ArgumentException("At least two columns are required.", nameof(columns));

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

            return ds;
        }

        private static bool IsNumeric(object value)
        {
            return value is int or long or float or double or decimal or short or byte;
        }
    }
}