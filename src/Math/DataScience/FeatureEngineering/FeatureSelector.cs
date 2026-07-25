namespace MathVerse.Math.DataScience.FeatureEngineering
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MathVerse.Math.DataScience.Core;
    using MathVerse.Math.DataScience.DatasetManagement;

    /// <summary>
    /// Selects features from datasets based on various statistical criteria.
    /// </summary>
    public sealed class FeatureSelector
    {
        /// <summary>
        /// Selects features whose variance exceeds the specified threshold.
        /// Low-variance features are considered uninformative.
        /// </summary>
        /// <param name="ds">The dataset to select features from.</param>
        /// <param name="threshold">The minimum variance threshold.</param>
        /// <returns>A list of column names that meet the variance threshold.</returns>
        public static List<string> ByVariance(Dataset ds, double threshold)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));

            List<string> selected = new();
            if (ds.Rows.Count == 0) return selected;

            List<string> numericCols = GetNumericColumns(ds);

            foreach (string col in numericCols)
            {
                List<double> values = GetNumericValues(ds, col);
                if (values.Count < 2) continue;

                double mean = 0.0;
                foreach (double v in values) mean += v;
                mean /= values.Count;

                double variance = 0.0;
                foreach (double v in values)
                {
                    double diff = v - mean;
                    variance += diff * diff;
                }
                variance /= values.Count;

                if (variance >= threshold)
                {
                    selected.Add(col);
                }
            }

            return selected;
        }

        /// <summary>
        /// Selects features whose absolute correlation with the target column exceeds the threshold.
        /// </summary>
        /// <param name="ds">The dataset to select features from.</param>
        /// <param name="target">The target column name.</param>
        /// <param name="threshold">The minimum absolute correlation threshold.</param>
        /// <returns>A list of column names that meet the correlation threshold.</returns>
        public static List<string> ByCorrelation(Dataset ds, string target, double threshold)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (string.IsNullOrEmpty(target)) throw new ArgumentException("Target column cannot be null or empty.", nameof(target));

            List<string> selected = new();
            List<double> targetValues = GetNumericValues(ds, target);
            if (targetValues.Count < 2) return selected;

            List<string> numericCols = GetNumericColumns(ds);

            foreach (string col in numericCols)
            {
                if (col == target) continue;

                List<double> colValues = GetPairedNumericValues(ds, col, target);
                if (colValues.Count < 2) continue;

                double corr = PearsonCorrelation(colValues, targetValues);
                if (System.Math.Abs(corr) >= threshold)
                {
                    selected.Add(col);
                }
            }

            return selected;
        }

        /// <summary>
        /// Selects the top K features based on mutual information with the target column.
        /// </summary>
        /// <param name="ds">The dataset to select features from.</param>
        /// <param name="target">The target column name.</param>
        /// <param name="topK">The number of top features to select.</param>
        /// <returns>A list of the top K column names by mutual information.</returns>
        public static List<string> ByMutualInformation(Dataset ds, string target, int topK)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (string.IsNullOrEmpty(target)) throw new ArgumentException("Target column cannot be null or empty.", nameof(target));

            List<string> numericCols = GetNumericColumns(ds);
            List<(string col, double mi)> scored = new();

            foreach (string col in numericCols)
            {
                if (col == target) continue;

                List<double> featureValues = GetNumericValues(ds, col);
                List<double> targetValues = GetNumericValues(ds, target);

                if (featureValues.Count < 2 || targetValues.Count < 2) continue;

                double mi = ComputeMutualInformation(featureValues, targetValues);
                scored.Add((col, mi));
            }

            return scored
                .OrderByDescending(s => s.mi)
                .Take(topK)
                .Select(s => s.col)
                .ToList();
        }

        /// <summary>
        /// Performs Principal Component Analysis and selects the top principal components.
        /// </summary>
        /// <param name="ds">The dataset to perform PCA on.</param>
        /// <param name="numComponents">The number of principal components to retain.</param>
        /// <returns>A new dataset containing only the selected principal components.</returns>
        public static Dataset ByPCA(Dataset ds, int numComponents)
        {
            if (ds is null) throw new ArgumentNullException(nameof(ds));
            if (numComponents <= 0) throw new ArgumentException("Number of components must be positive.", nameof(numComponents));

            List<string> numericCols = GetNumericColumns(ds);
            if (numericCols.Count == 0 || ds.Rows.Count == 0) return ds;

            int n = ds.Rows.Count;
            int p = numericCols.Count;
            numComponents = System.Math.Min(numComponents, p);

            double[][] data = new double[n][];
            for (int i = 0; i < n; i++)
            {
                data[i] = new double[p];
                for (int j = 0; j < p; j++)
                {
                    if (ds.Rows[i].TryGetValue(numericCols[j], out object? val) && val is not null && IsNumeric(val))
                        data[i][j] = Convert.ToDouble(val);
                }
            }

            double[] means = new double[p];
            for (int j = 0; j < p; j++)
            {
                double sum = 0.0;
                for (int i = 0; i < n; i++) sum += data[i][j];
                means[j] = sum / n;
                for (int i = 0; i < n; i++) data[i][j] -= means[j];
            }

            double[,] covMatrix = ComputeCovarianceMatrix(data, n, p);
            double[][] eigenvectors = PowerIteration(covMatrix, p, numComponents);

            Dataset result = new();
            for (int c = 0; c < numComponents; c++)
            {
                result.Schema.AddColumn($"PC{c + 1}", ColumnType.Double);
            }

            for (int i = 0; i < n; i++)
            {
                Dictionary<string, object?> row = new();
                for (int c = 0; c < numComponents; c++)
                {
                    double proj = 0.0;
                    for (int j = 0; j < p; j++)
                    {
                        proj += data[i][j] * eigenvectors[j][c];
                    }
                    row[$"PC{c + 1}"] = proj;
                }
                result.Rows.Add(row);
            }

            return result;
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

        private static double[,] ComputeCovarianceMatrix(double[][] data, int n, int p)
        {
            double[,] cov = new double[p, p];
            for (int i = 0; i < p; i++)
            {
                for (int j = i; j < p; j++)
                {
                    double sum = 0.0;
                    for (int k = 0; k < n; k++)
                    {
                        sum += data[k][i] * data[k][j];
                    }
                    cov[i, j] = sum / n;
                    cov[j, i] = cov[i, j];
                }
            }
            return cov;
        }

        private static double[][] PowerIteration(double[,] matrix, int size, int numComponents)
        {
            double[][] result = new double[size][];
            for (int i = 0; i < size; i++)
                result[i] = new double[numComponents];

            double[,] work = new double[size, size];
            Array.Copy(matrix, work, matrix.Length);

            for (int comp = 0; comp < numComponents; comp++)
            {
                double[] vector = new double[size];
                Random rng = new(42 + comp);
                for (int i = 0; i < size; i++)
                    vector[i] = rng.NextDouble();

                double norm = 0.0;
                for (int i = 0; i < size; i++) norm += vector[i] * vector[i];
                norm = System.Math.Sqrt(norm);
                for (int i = 0; i < size; i++) vector[i] /= norm;

                for (int iter = 0; iter < 1000; iter++)
                {
                    double[] newVec = new double[size];
                    for (int i = 0; i < size; i++)
                    {
                        double sum = 0.0;
                        for (int j = 0; j < size; j++)
                            sum += work[i, j] * vector[j];
                        newVec[i] = sum;
                    }

                    norm = 0.0;
                    for (int i = 0; i < size; i++) norm += newVec[i] * newVec[i];
                    norm = System.Math.Sqrt(norm);

                    if (norm > 0.0)
                    {
                        for (int i = 0; i < size; i++) newVec[i] /= norm;
                    }

                    double diff = 0.0;
                    for (int i = 0; i < size; i++)
                    {
                        double d = System.Math.Abs(newVec[i] - vector[i]);
                        if (d > diff) diff = d;
                    }

                    vector = newVec;
                    if (diff < 1e-10) break;
                }

                double eigenvalue = 0.0;
                for (int i = 0; i < size; i++)
                {
                    double sum = 0.0;
                    for (int j = 0; j < j + 1 && j < size; j++)
                        sum += matrix[i, j] * vector[j];
                    for (int j = i + 1; j < size; j++)
                        sum += matrix[i, j] * vector[j];
                    eigenvalue += vector[i] * sum;
                }

                for (int i = 0; i < size; i++)
                {
                    result[i][comp] = vector[i];
                }

                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        work[i, j] -= eigenvalue * vector[i] * vector[j];
                    }
                }
            }

            return result;
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

        private static List<double> GetPairedNumericValues(Dataset ds, string col1, string col2)
        {
            List<double> values = new();
            foreach (Dictionary<string, object?> row in ds.Rows)
            {
                if (row.TryGetValue(col1, out object? v1) && v1 is not null && IsNumeric(v1) &&
                    row.TryGetValue(col2, out object? v2) && v2 is not null && IsNumeric(v2))
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