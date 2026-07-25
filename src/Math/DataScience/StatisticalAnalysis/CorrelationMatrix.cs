namespace MathVerse.Math.DataScience.StatisticalAnalysis
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Computes correlation matrices using Pearson, Spearman, and Kendall correlation methods.
    /// </summary>
    public sealed class CorrelationMatrix
    {
        /// <summary>
        /// Computes the Pearson correlation matrix.
        /// Pearson correlation = sum((xi - meanX)(yi - meanY)) / (sqrt(sum((xi-meanX)^2)) * sqrt(sum((yi-meanY)^2))).
        /// </summary>
        /// <param name="data">A 2D array where each row is an observation and each column is a variable.</param>
        /// <returns>The Pearson correlation matrix as a 2D array.</returns>
        public static double[,] Compute(double[][] data)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));
            if (data.Length == 0) throw new ArgumentException("Data must contain at least one observation.", nameof(data));

            int n = data.Length;
            int p = data[0].Length;

            double[] means = new double[p];
            for (int j = 0; j < p; j++)
            {
                double sum = 0.0;
                for (int i = 0; i < n; i++) sum += data[i][j];
                means[j] = sum / n;
            }

            double[] stds = new double[p];
            for (int j = 0; j < p; j++)
            {
                double var = 0.0;
                for (int i = 0; i < n; i++)
                {
                    double diff = data[i][j] - means[j];
                    var += diff * diff;
                }
                stds[j] = System.Math.Sqrt(var);
            }

            double[,] result = new double[p, p];
            for (int i = 0; i < p; i++)
            {
                result[i, i] = 1.0;
                for (int j = i + 1; j < p; j++)
                {
                    double sum = 0.0;
                    for (int k = 0; k < n; k++)
                    {
                        sum += (data[k][i] - means[i]) * (data[k][j] - means[j]);
                    }

                    double denom = stds[i] * stds[j];
                    double corr = denom > 0.0 ? sum / denom : 0.0;
                    result[i, j] = corr;
                    result[j, i] = corr;
                }
            }

            return result;
        }

        /// <summary>
        /// Computes the Spearman rank correlation matrix.
        /// Uses linear interpolation for tied ranks.
        /// </summary>
        /// <param name="data">A 2D array where each row is an observation and each column is a variable.</param>
        /// <returns>The Spearman correlation matrix as a 2D array.</returns>
        public static double[,] SpearmanCorrelation(double[][] data)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));
            if (data.Length == 0) throw new ArgumentException("Data must contain at least one observation.", nameof(data));

            int n = data.Length;
            int p = data[0].Length;

            double[][] ranks = new double[p][];
            for (int j = 0; j < p; j++)
            {
                ranks[j] = ComputeRanks(data, j);
            }

            double[] rankMeans = new double[p];
            for (int j = 0; j < p; j++)
            {
                double sum = 0.0;
                for (int i = 0; i < n; i++) sum += ranks[j][i];
                rankMeans[j] = sum / n;
            }

            double[] rankStds = new double[p];
            for (int j = 0; j < p; j++)
            {
                double var = 0.0;
                for (int i = 0; i < n; i++)
                {
                    double diff = ranks[j][i] - rankMeans[j];
                    var += diff * diff;
                }
                rankStds[j] = System.Math.Sqrt(var);
            }

            double[,] result = new double[p, p];
            for (int i = 0; i < p; i++)
            {
                result[i, i] = 1.0;
                for (int j = i + 1; j < p; j++)
                {
                    double sum = 0.0;
                    for (int k = 0; k < n; k++)
                    {
                        sum += (ranks[i][k] - rankMeans[i]) * (ranks[j][k] - rankMeans[j]);
                    }

                    double denom = rankStds[i] * rankStds[j];
                    double corr = denom > 0.0 ? sum / denom : 0.0;
                    result[i, j] = corr;
                    result[j, i] = corr;
                }
            }

            return result;
        }

        /// <summary>
        /// Computes the Kendall tau-b rank correlation matrix.
        /// </summary>
        /// <param name="data">A 2D array where each row is an observation and each column is a variable.</param>
        /// <returns>The Kendall correlation matrix as a 2D array.</returns>
        public static double[,] KendallCorrelation(double[][] data)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));
            if (data.Length == 0) throw new ArgumentException("Data must contain at least one observation.", nameof(data));

            int n = data.Length;
            int p = data[0].Length;

            double[,] result = new double[p, p];
            for (int i = 0; i < p; i++)
            {
                result[i, i] = 1.0;
                for (int j = i + 1; j < p; j++)
                {
                    double tau = ComputeKendallTau(data, i, j);
                    result[i, j] = tau;
                    result[j, i] = tau;
                }
            }

            return result;
        }

        private static double[] ComputeRanks(double[][] data, int col)
        {
            int n = data.Length;
            double[] ranks = new double[n];
            (double value, int index)[] indexed = new (double, int)[n];

            for (int i = 0; i < n; i++)
            {
                indexed[i] = (data[i][col], i);
            }

            Array.Sort(indexed, (a, b) => a.value.CompareTo(b.value));

            int i2 = 0;
            while (i2 < n)
            {
                int j = i2;
                while (j < n - 1 && indexed[j + 1].value == indexed[j].value)
                    j++;

                double avgRank = (i2 + j) / 2.0 + 1.0;
                for (int k = i2; k <= j; k++)
                {
                    ranks[indexed[k].index] = avgRank;
                }
                i2 = j + 1;
            }

            return ranks;
        }

        private static double ComputeKendallTau(double[][] data, int col1, int col2)
        {
            int n = data.Length;
            int concordant = 0;
            int discordant = 0;
            int tiedX = 0;
            int tiedY = 0;
            int tiedBoth = 0;

            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    double dx = data[i][col1] - data[j][col1];
                    double dy = data[i][col2] - data[j][col2];

                    bool tieX = dx == 0.0;
                    bool tieY = dy == 0.0;

                    if (tieX && tieY)
                    {
                        tiedBoth++;
                    }
                    else if (tieX)
                    {
                        tiedX++;
                    }
                    else if (tieY)
                    {
                        tiedY++;
                    }
                    else if ((dx > 0.0 && dy > 0.0) || (dx < 0.0 && dy < 0.0))
                    {
                        concordant++;
                    }
                    else
                    {
                        discordant++;
                    }
                }
            }

            int nonTiedPairs = concordant + discordant;
            if (nonTiedPairs == 0) return 0.0;

            return (double)(concordant - discordant) / nonTiedPairs;
        }
    }
}