namespace MathVerse.Math.DataScience.DataCleaning
{
    using System;
    using System.Collections.Generic;
    using MathVerse.Math.DataScience.Core;

    /// <summary>
    /// Winsorizes columns in a dataset by capping extreme values at specified percentiles.
    /// </summary>
    public sealed class Winsorizer
    {
        /// <summary>
        /// Winsorizes the specified columns by capping values below the lower percentile
        /// and above the upper percentile to the respective percentile values.
        /// </summary>
        /// <param name="ds">The dataset to winsorize.</param>
        /// <param name="cols">The column names to winsorize.</param>
        /// <param name="lowerPercentile">The lower percentile threshold (default 0.05).</param>
        /// <param name="upperPercentile">The upper percentile threshold (default 0.95).</param>
        /// <returns>The modified dataset with winsorized columns.</returns>
        public static Dataset Apply(Dataset ds, string[] cols, double lowerPercentile = 0.05, double upperPercentile = 0.95)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (cols is null || cols.Length == 0) throw new ArgumentException("Columns array cannot be null or empty.", nameof(cols));

            foreach (string col in cols)
            {
                List<double> values = new();
                foreach (Dictionary<string, object?> row in ds.Rows)
                {
                    if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                    {
                        values.Add(Convert.ToDouble(val));
                    }
                }

                if (values.Count < 2) continue;

                values.Sort();
                double lower = Percentile(values, lowerPercentile * 100.0);
                double upper = Percentile(values, upperPercentile * 100.0);

                foreach (Dictionary<string, object?> row in ds.Rows)
                {
                    if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                    {
                        double d = Convert.ToDouble(val);
                        if (d < lower)
                            row[col] = lower;
                        else if (d > upper)
                            row[col] = upper;
                    }
                }
            }

            return ds;
        }

        private static double Percentile(List<double> sortedValues, double percentile)
        {
            double index = (percentile / 100.0) * (sortedValues.Count - 1);
            int lower = (int)System.Math.Floor(index);
            int upper = (int)System.Math.Ceiling(index);
            if (lower == upper) return sortedValues[lower];
            double fraction = index - lower;
            return sortedValues[lower] + fraction * (sortedValues[upper] - sortedValues[lower]);
        }

        private static bool IsNumeric(object value)
        {
            return value is int or long or float or double or decimal or short or byte;
        }
    }
}