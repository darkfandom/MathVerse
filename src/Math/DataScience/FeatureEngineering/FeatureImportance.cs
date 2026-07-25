namespace MathVerse.Math.DataScience.FeatureEngineering
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MathVerse.Math.DataScience.Core;

    /// <summary>
    /// Represents the importance score of a feature.
    /// </summary>
    public sealed class FeatureImportanceResult
    {
        /// <summary>Gets or sets the feature name.</summary>
        public string Feature { get; set; } = string.Empty;

        /// <summary>Gets or sets the importance score.</summary>
        public double Score { get; set; }
    }

    /// <summary>
    /// Computes feature importance scores using various statistical methods.
    /// </summary>
    public sealed class FeatureImportance
    {
        /// <summary>
        /// Computes feature importance based on variance.
        /// Higher variance indicates more informative features.
        /// </summary>
        /// <param name="ds">The dataset containing features.</param>
        /// <param name="featureCols">The feature column names to evaluate.</param>
        /// <returns>A list of feature importance results sorted by score in descending order.</returns>
        public static List<FeatureImportanceResult> ByVariance(Dataset ds, string[] featureCols)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (featureCols is null || featureCols.Length == 0) throw new ArgumentException("Feature columns cannot be null or empty.", nameof(featureCols));

            List<FeatureImportanceResult> results = new();

            foreach (string col in featureCols)
            {
                List<double> values = GetNumericValues(ds, col);
                if (values.Count < 2)
                {
                    results.Add(new FeatureImportanceResult { Feature = col, Score = 0.0 });
                    continue;
                }

                double sum = 0.0;
                foreach (double v in values) sum += v;
                double mean = sum / values.Count;

                double variance = 0.0;
                foreach (double v in values)
                {
                    double diff = v - mean;
                    variance += diff * diff;
                }
                variance /= values.Count;

                results.Add(new FeatureImportanceResult { Feature = col, Score = variance });
            }

            return results.OrderByDescending(r => r.Score).ToList();
        }

        /// <summary>
        /// Computes feature importance based on absolute Pearson correlation with the target.
        /// Higher absolute correlation indicates more predictive features.
        /// </summary>
        /// <param name="ds">The dataset containing features.</param>
        /// <param name="targetCol">The target column name.</param>
        /// <returns>A list of feature importance results sorted by score in descending order.</returns>
        public static List<FeatureImportanceResult> ByCorrelation(Dataset ds, string targetCol)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (string.IsNullOrEmpty(targetCol)) throw new ArgumentException("Target column cannot be null or empty.", nameof(targetCol));

            List<double> targetValues = GetNumericValues(ds, targetCol);
            List<FeatureImportanceResult> results = new();
            List<string> numericCols = GetNumericColumns(ds);

            foreach (string col in numericCols)
            {
                if (col == targetCol) continue;

                List<double> featureValues = GetPairedNumericValues(ds, col, targetCol);
                if (featureValues.Count < 2)
                {
                    results.Add(new FeatureImportanceResult { Feature = col, Score = 0.0 });
                    continue;
                }

                double corr = PearsonCorrelation(featureValues, targetValues);
                results.Add(new FeatureImportanceResult { Feature = col, Score = System.Math.Abs(corr) });
            }

            return results.OrderByDescending(r => r.Score).ToList();
        }

        /// <summary>
        /// Computes feature importance based on mutual information with the target.
        /// Higher mutual information indicates stronger dependency.
        /// </summary>
        /// <param name="ds">The dataset containing features.</param>
        /// <param name="targetCol">The target column name.</param>
        /// <returns>A list of feature importance results sorted by score in descending order.</returns>
        public static List<FeatureImportanceResult> ByMutualInformation(Dataset ds, string targetCol)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (string.IsNullOrEmpty(targetCol)) throw new ArgumentException("Target column cannot be null or empty.", nameof(targetCol));

            List<double> targetValues = GetNumericValues(ds, targetCol);
            List<FeatureImportanceResult> results = new();
            List<string> numericCols = GetNumericColumns(ds);

            foreach (string col in numericCols)
            {
                if (col == targetCol) continue;

                List<double> featureValues = GetNumericValues(ds, col);
                if (featureValues.Count < 2)
                {
                    results.Add(new FeatureImportanceResult { Feature = col, Score = 0.0 });
                    continue;
                }

                double mi = ComputeMutualInformation(featureValues, targetValues);
                results.Add(new FeatureImportanceResult { Feature = col, Score = mi });
            }

            return results.OrderByDescending(r => r.Score).ToList();
        }

        private static double PearsonCorrelation(List<double> x, List<double> y)
        {
            int n = System.Math.Min(x.Count, y.Count);
            if (n < 2) return 0.0;

            double sumX = 0.0, sumY = 0.0;
            for (int i = 0; i < n; i++)
            {
                sumX += x[i];
                sumY += y[i];
            }
            double meanX = sumX / n;
            double meanY = sumY / n;

            double sumXY = 0.0, sumX2 = 0.0, sumY2 = 0.0;
            for (int i = 0; i < n; i++)
            {
                double dx = x[i] - meanX;
                double dy = y[i] - meanY;
                sumXY += dx * dy;
                sumX2 += dx * dx;
                sumY2 += dy * dy;
            }

            double denom = System.Math.Sqrt(sumX2 * sumY2);
            if (denom == 0.0) return 0.0;
            return sumXY / denom;
        }

        private static double ComputeMutualInformation(List<double> x, List<double> y)
        {
            int bins = System.Math.Max(2, (int)System.Math.Sqrt(x.Count));
            double xMin = double.MaxValue, xMax = double.MinValue;
            double yMin = double.MaxValue, yMax = double.MinValue;

            for (int i = 0; i < x.Count; i++)
            {
                if (x[i] < xMin) xMin = x[i];
                if (x[i] > xMax) xMax = x[i];
                if (y[i] < yMin) yMin = y[i];
                if (y[i] > yMax) yMax = y[i];
            }

            double xRange = xMax - xMin;
            double yRange = yMax - yMin;
            if (xRange == 0.0 || yRange == 0.0) return 0.0;

            int n = x.Count;
            int[,] joint = new int[bins, bins];
            int[] xMarginal = new int[bins];
            int[] yMarginal = new int[bins];

            for (int i = 0; i < n; i++)
            {
                int xi = System.Math.Min((int)((x[i] - xMin) / xRange * (bins - 1)), bins - 1);
                int yi = System.Math.Min((int)((y[i] - yMin) / yRange * (bins - 1)), bins - 1);
                joint[xi, yi]++;
                xMarginal[xi]++;
                yMarginal[yi]++;
            }

            double mi = 0.0;
            for (int i = 0; i < bins; i++)
            {
                for (int j = 0; j < bins; j++)
                {
                    if (joint[i, j] == 0) continue;
                    double pJoint = (double)joint[i, j] / n;
                    double pX = (double)xMarginal[i] / n;
                    double pY = (double)yMarginal[j] / n;
                    mi += pJoint * System.Math.Log(pJoint / (pX * pY));
                }
            }

            return mi;
        }

        private static List<string> GetNumericColumns(Dataset ds)
        {
            List<string> cols = new();
            if (ds.Rows.Count == 0) return cols;

            foreach (string col in ds.Rows[0].Keys)
            {
                bool isNumeric = false;
                foreach (Dictionary<string, object?> row in ds.Rows)
                {
                    if (row.TryGetValue(col, out object? val) && val is not null && IsNumeric(val))
                    {
                        isNumeric = true;
                        break;
                    }
                }
                if (isNumeric) cols.Add(col);
            }
            return cols;
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

        private static List<double> GetPairedNumericValues(Dataset ds, string col, string target)
        {
            List<double> values = new();
            foreach (Dictionary<string, object?> row in ds.Rows)
            {
                if (row.TryGetValue(col, out object? v1) && v1 is not null && IsNumeric(v1) &&
                    row.TryGetValue(target, out object? v2) && v2 is not null && IsNumeric(v2))
                {
                    values.Add(Convert.ToDouble(v1));
                }
            }
            return values;
        }

        private static bool IsNumeric(object value)
        {
            return value is int or long or float or double or decimal or short or byte;
        }
    }
}