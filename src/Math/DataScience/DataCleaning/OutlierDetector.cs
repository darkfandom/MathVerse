namespace MathVerse.Math.DataScience.DataCleaning
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MathVerse.Math.DataScience.Core;

    /// <summary>
    /// Detects and handles outliers in datasets using various statistical methods.
    /// </summary>
    public sealed class OutlierDetector
    {
        /// <summary>
        /// Detects outliers using the Interquartile Range (IQR) method.
        /// Outliers are values below Q1 - factor*IQR or above Q3 + factor*IQR.
        /// </summary>
        /// <param name="ds">The dataset to analyze.</param>
        /// <param name="col">The column name to check for outliers.</param>
        /// <param name="factor">The IQR multiplier (default 1.5).</param>
        /// <returns>A list of row indices that contain outliers.</returns>
        public static List<int> DetectIQR(Dataset ds, string col, double factor = 1.5)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (string.IsNullOrEmpty(col)) throw new ArgumentException("Column name cannot be null or empty.", nameof(col));

            List<double> values = GetNumericValues(ds, col);
            if (values.Count < 4) return new List<int>();

            values.Sort();
            double q1 = Percentile(values, 25.0);
            double q3 = Percentile(values, 75.0);
            double iqr = q3 - q1;
            double lowerBound = q1 - factor * iqr;
            double upperBound = q3 + factor * iqr;

            List<int> outlierIndices = new();
            for (int i = 0; i < ds.Rows.Count; i++)
            {
                if (ds.Rows[i].TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                {
                    double d = Convert.ToDouble(val);
                    if (d < lowerBound || d > upperBound)
                    {
                        outlierIndices.Add(i);
                    }
                }
            }

            return outlierIndices;
        }

        /// <summary>
        /// Detects outliers using the Z-score method.
        /// Outliers are values with |z-score| greater than the threshold.
        /// </summary>
        /// <param name="ds">The dataset to analyze.</param>
        /// <param name="col">The column name to check for outliers.</param>
        /// <param name="threshold">The z-score threshold (default 3.0).</param>
        /// <returns>A list of row indices that contain outliers.</returns>
        public static List<int> DetectZScore(Dataset ds, string col, double threshold = 3.0)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (string.IsNullOrEmpty(col)) throw new ArgumentException("Column name cannot be null or empty.", nameof(col));

            List<double> values = GetNumericValues(ds, col);
            if (values.Count < 2) return new List<int>();

            double mean = values.Average();
            double variance = 0.0;
            foreach (double v in values)
            {
                double diff = v - mean;
                variance += diff * diff;
            }
            double stdDev = System.Math.Sqrt(variance / values.Count);

            if (stdDev == 0.0) return new List<int>();

            List<int> outlierIndices = new();
            for (int i = 0; i < ds.Rows.Count; i++)
            {
                if (ds.Rows[i].TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                {
                    double d = Convert.ToDouble(val);
                    double z = System.Math.Abs((d - mean) / stdDev);
                    if (z > threshold)
                    {
                        outlierIndices.Add(i);
                    }
                }
            }

            return outlierIndices;
        }

        /// <summary>
        /// Detects outliers using the Median Absolute Deviation (MAD) method.
        /// Outliers are values with modified z-score greater than the threshold.
        /// </summary>
        /// <param name="ds">The dataset to analyze.</param>
        /// <param name="col">The column name to check for outliers.</param>
        /// <param name="threshold">The modified z-score threshold (default 3.0).</param>
        /// <returns>A list of row indices that contain outliers.</returns>
        public static List<int> DetectMAD(Dataset ds, string col, double threshold = 3.0)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (string.IsNullOrEmpty(col)) throw new ArgumentException("Column name cannot be null or empty.", nameof(col));

            List<double> values = GetNumericValues(ds, col);
            if (values.Count < 2) return new List<int>();

            values.Sort();
            double median = values.Count % 2 == 0
                ? (values[values.Count / 2 - 1] + values[values.Count / 2]) / 2.0
                : values[values.Count / 2];

            List<double> absDeviations = new(values.Count);
            foreach (double v in values)
            {
                absDeviations.Add(System.Math.Abs(v - median));
            }
            absDeviations.Sort();
            double mad = absDeviations.Count % 2 == 0
                ? (absDeviations[absDeviations.Count / 2 - 1] + absDeviations[absDeviations.Count / 2]) / 2.0
                : absDeviations[absDeviations.Count / 2];

            if (mad == 0.0) return new List<int>();

            const double k = 1.4826;
            List<int> outlierIndices = new();
            for (int i = 0; i < ds.Rows.Count; i++)
            {
                if (ds.Rows[i].TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                {
                    double d = Convert.ToDouble(val);
                    double modifiedZ = k * System.Math.Abs(d - median) / mad;
                    if (modifiedZ > threshold)
                    {
                        outlierIndices.Add(i);
                    }
                }
            }

            return outlierIndices;
        }

        /// <summary>
        /// Clips outliers in the specified column to the IQR boundaries.
        /// Values below Q1 - factor*IQR are set to the lower bound,
        /// values above Q3 + factor*IQR are set to the upper bound.
        /// </summary>
        /// <param name="ds">The dataset to modify.</param>
        /// <param name="col">The column name to clip outliers in.</param>
        /// <param name="factor">The IQR multiplier (default 1.5).</param>
        /// <returns>The modified dataset with clipped outlier values.</returns>
        public static Dataset ClipOutliers(Dataset ds, string col, double factor = 1.5)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (string.IsNullOrEmpty(col)) throw new ArgumentException("Column name cannot be null or empty.", nameof(col));

            List<double> values = GetNumericValues(ds, col);
            if (values.Count < 4) return ds;

            values.Sort();
            double q1 = Percentile(values, 25.0);
            double q3 = Percentile(values, 75.0);
            double iqr = q3 - q1;
            double lowerBound = q1 - factor * iqr;
            double upperBound = q3 + factor * iqr;

            for (int i = 0; i < ds.Rows.Count; i++)
            {
                if (ds.Rows[i].TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                {
                    double d = Convert.ToDouble(val);
                    if (d < lowerBound)
                        ds.Rows[i][col] = lowerBound;
                    else if (d > upperBound)
                        ds.Rows[i][col] = upperBound;
                }
            }

            return ds;
        }

        private static List<double> GetNumericValues(Dataset ds, string col)
        {
            List<double> values = new();
            foreach (Dictionary<string, object?> row in ds.Rows)
            {
                if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                {
                    values.Add(Convert.ToDouble(val));
                }
            }
            return values;
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