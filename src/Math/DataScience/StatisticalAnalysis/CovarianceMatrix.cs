namespace MathVerse.Math.DataScience.StatisticalAnalysis
{
    using System;

    /// <summary>
    /// Computes the covariance matrix for multivariate datasets.
    /// </summary>
    public sealed class CovarianceMatrix
    {
        /// <summary>
        /// Computes the covariance matrix for the given data.
        /// Each row in the input array represents an observation, each column a variable.
        /// </summary>
        /// <param name="data">A 2D array of observations where each row is an observation and each column is a variable.</param>
        /// <returns>The covariance matrix as a 2D array.</returns>
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

            double[,] result = new double[p, p];

            for (int i = 0; i < p; i++)
            {
                for (int j = i; j < p; j++)
                {
                    double sum = 0.0;
                    for (int k = 0; k < n; k++)
                    {
                        sum += (data[k][i] - means[i]) * (data[k][j] - means[j]);
                    }
                    result[i, j] = sum / n;
                    result[j, i] = result[i, j];
                }
            }

            return result;
        }
    }
}