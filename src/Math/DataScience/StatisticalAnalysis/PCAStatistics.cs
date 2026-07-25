namespace MathVerse.Math.DataScience.StatisticalAnalysis
{
    using System;

    /// <summary>
    /// Contains the results of Principal Component Analysis.
    /// </summary>
    public sealed class PCAResult
    {
        /// <summary>Gets or sets the eigenvalues in descending order.</summary>
        public double[] Eigenvalues { get; set; } = Array.Empty<double>();

        /// <summary>Gets or sets the eigenvectors as columns of a 2D array.</summary>
        public double[][] Eigenvectors { get; set; } = Array.Empty<double[]>();

        /// <summary>Gets or sets the proportion of variance explained by each component.</summary>
        public double[] ExplainedVariance { get; set; } = Array.Empty<double>();

        /// <summary>Gets or sets the cumulative proportion of variance explained.</summary>
        public double[] CumulativeExplainedVariance { get; set; } = Array.Empty<double>();
    }

    /// <summary>
    /// Performs Principal Component Analysis and computes related statistics.
    /// </summary>
    public sealed class PCAStatistics
    {
        /// <summary>
        /// Performs PCA on the data and returns eigenvalues, eigenvectors, and explained variance.
        /// Uses power iteration with deflation to find eigenvalues.
        /// </summary>
        /// <param name="data">A 2D array where each row is an observation and each column is a variable.</param>
        /// <param name="numComponents">The number of principal components to compute.</param>
        /// <returns>A PCAResult containing the analysis results.</returns>
        public static PCAResult Compute(double[][] data, int numComponents)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));
            if (data.Length == 0) throw new ArgumentException("Data must contain at least one observation.", nameof(data));
            if (numComponents <= 0) throw new ArgumentException("Number of components must be positive.", nameof(numComponents));

            int n = data.Length;
            int p = data[0].Length;
            numComponents = System.Math.Min(numComponents, p);

            double[] means = new double[p];
            for (int j = 0; j < p; j++)
            {
                double sum = 0.0;
                for (int i = 0; i < n; i++) sum += data[i][j];
                means[j] = sum / n;
            }

            double[][] centered = new double[n][];
            for (int i = 0; i < n; i++)
            {
                centered[i] = new double[p];
                for (int j = 0; j < p; j++)
                {
                    centered[i][j] = data[i][j] - means[j];
                }
            }

            double[,] covMatrix = new double[p, p];
            for (int i = 0; i < p; i++)
            {
                for (int j = i; j < p; j++)
                {
                    double sum = 0.0;
                    for (int k = 0; k < n; k++)
                    {
                        sum += centered[k][i] * centered[k][j];
                    }
                    covMatrix[i, j] = sum / n;
                    covMatrix[j, i] = covMatrix[i, j];
                }
            }

            double[] eigenvalues = new double[numComponents];
            double[][] eigenvectors = new double[numComponents][];
            double[,] work = new double[p, p];
            Array.Copy(covMatrix, work, covMatrix.Length);

            for (int comp = 0; comp < numComponents; comp++)
            {
                double[] vector = new double[p];
                Random rng = new(42 + comp);
                double norm = 0.0;
                for (int i = 0; i < p; i++)
                {
                    vector[i] = rng.NextDouble();
                    norm += vector[i] * vector[i];
                }
                norm = System.Math.Sqrt(norm);
                for (int i = 0; i < p; i++) vector[i] /= norm;

                for (int iter = 0; iter < 1000; iter++)
                {
                    double[] newVec = new double[p];
                    for (int i = 0; i < p; i++)
                    {
                        double sum = 0.0;
                        for (int j = 0; j < p; j++)
                            sum += work[i, j] * vector[j];
                        newVec[i] = sum;
                    }

                    norm = 0.0;
                    for (int i = 0; i < p; i++) norm += newVec[i] * newVec[i];
                    norm = System.Math.Sqrt(norm);

                    if (norm > 0.0)
                    {
                        for (int i = 0; i < p; i++) newVec[i] /= norm;
                    }

                    double maxDiff = 0.0;
                    for (int i = 0; i < p; i++)
                    {
                        double d = System.Math.Abs(newVec[i] - vector[i]);
                        if (d > maxDiff) maxDiff = d;
                    }

                    vector = newVec;
                    if (maxDiff < 1e-10) break;
                }

                double eigenvalue = 0.0;
                for (int i = 0; i < p; i++)
                {
                    double sum = 0.0;
                    for (int j = 0; j < p; j++)
                        sum += covMatrix[i, j] * vector[j];
                    eigenvalue += vector[i] * sum;
                }

                eigenvalues[comp] = eigenvalue;
                eigenvectors[comp] = vector;

                for (int i = 0; i < p; i++)
                {
                    for (int j = 0; j < p; j++)
                    {
                        work[i, j] -= eigenvalue * vector[i] * vector[j];
                    }
                }
            }

            double totalVariance = 0.0;
            for (int i = 0; i < p; i++) totalVariance += covMatrix[i, i];

            double[] explainedVariance = new double[numComponents];
            double[] cumulativeExplainedVariance = new double[numComponents];
            double cumulative = 0.0;

            for (int i = 0; i < numComponents; i++)
            {
                explainedVariance[i] = totalVariance > 0.0 ? eigenvalues[i] / totalVariance : 0.0;
                cumulative += explainedVariance[i];
                cumulativeExplainedVariance[i] = cumulative;
            }

            return new PCAResult
            {
                Eigenvalues = eigenvalues,
                Eigenvectors = eigenvectors,
                ExplainedVariance = explainedVariance,
                CumulativeExplainedVariance = cumulativeExplainedVariance
            };
        }
    }
}