namespace MathVerse.Math.DataScience.VisualizationIntegration;

using System;
using System.Collections.Generic;

/// <summary>
/// Projects high-dimensional data onto 2D or 3D space using Principal Component Analysis.
/// </summary>
public sealed class PCAVisualizer
{
    /// <summary>
    /// Represents a single projected point in reduced-dimensional space.
    /// </summary>
    public sealed class ProjectedPoint
    {
        /// <summary>
        /// Gets or sets the component values for this point.
        /// </summary>
        public double[] Components { get; set; } = Array.Empty<double>();

        /// <summary>
        /// Gets or sets the original row index in the dataset.
        /// </summary>
        public int OriginalIndex { get; set; }
    }

    /// <summary>
    /// Represents the result of a PCA projection.
    /// </summary>
    public sealed class PCAResult
    {
        /// <summary>
        /// Gets or sets the projected data points.
        /// </summary>
        public List<ProjectedPoint> Points { get; set; } = new();

        /// <summary>
        /// Gets or sets the explained variance ratio for each component.
        /// </summary>
        public double[] ExplainedVarianceRatio { get; set; } = Array.Empty<double>();

        /// <summary>
        /// Gets or sets the principal component axes (eigenvectors).
        /// </summary>
        public double[][] Components { get; set; } = Array.Empty<double[]>();

        /// <summary>
        /// Gets or sets the total variance explained by all selected components.
        /// </summary>
        public double TotalVarianceExplained { get; set; }

        /// <summary>
        /// Gets or sets the number of components selected.
        /// </summary>
        public int NumComponents { get; set; }
    }

    /// <summary>
    /// Projects data onto a lower-dimensional space using PCA via power iteration.
    /// </summary>
    /// <param name="data">The input data where each row is an observation and each column is a feature.</param>
    /// <param name="numComponents">The number of principal components to retain (default 2).</param>
    /// <returns>A <see cref="PCAResult"/> containing the projected points and explained variance.</returns>
    public static PCAResult Project(double[][] data, int numComponents = 2)
    {
        if (data is null) throw new ArgumentNullException(nameof(data));
        if (data.Length < 2) throw new ArgumentException("Data must contain at least 2 observations.", nameof(data));
        if (data[0].Length < numComponents)
            throw new ArgumentException(
                $"Data features ({data[0].Length}) must be >= numComponents ({numComponents}).",
                nameof(numComponents));
        if (numComponents < 1)
            throw new ArgumentOutOfRangeException(nameof(numComponents), numComponents, "Must be at least 1.");

        int n = data.Length;
        int p = data[0].Length;

        double[] means = new double[p];
        for (int j = 0; j < p; j++)
        {
            double sum = 0.0;
            for (int i = 0; i < n; i++)
            {
                sum += data[i][j];
            }
            means[j] = sum / n;
        }

        double[] scaledData = new double[n * p];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < p; j++)
            {
                scaledData[i * p + j] = data[i][j] - means[j];
            }
        }

        double totalVariance = 0.0;
        for (int j = 0; j < p; j++)
        {
            double colVar = 0.0;
            for (int i = 0; i < n; i++)
            {
                double v = scaledData[i * p + j];
                colVar += v * v;
            }
            totalVariance += colVar / n;
        }

        double[][] eigenvectors = new double[numComponents][];
        double[] eigenvalues = new double[numComponents];

        for (int comp = 0; comp < numComponents; comp++)
        {
            double[] vector = new double[p];
            var rng = new Random(comp * 31 + 7);
            double norm = 0.0;
            for (int j = 0; j < p; j++)
            {
                vector[j] = rng.NextDouble() - 0.5;
                norm += vector[j] * vector[j];
            }
            norm = System.Math.Sqrt(norm);
            if (norm > 1e-15)
            {
                for (int j = 0; j < p; j++) vector[j] /= norm;
            }

            for (int iter = 0; iter < 100; iter++)
            {
                double[] newVec = new double[p];
                for (int j = 0; j < p; j++)
                {
                    double sum = 0.0;
                    for (int k = 0; k < p; k++)
                    {
                        double cov = 0.0;
                        for (int i = 0; i < n; i++)
                        {
                            cov += scaledData[i * p + j] * scaledData[i * p + k];
                        }
                        cov /= n;
                        sum += cov * vector[k];
                    }
                    newVec[j] = sum;
                }

                for (int j = 0; j < p; j++)
                {
                    for (int k = 0; k < comp; k++)
                    {
                        double dot = 0.0;
                        for (int j2 = 0; j2 < p; j2++)
                        {
                            dot += newVec[j2] * eigenvectors[k][j2];
                        }
                        for (int j2 = 0; j2 < p; j2++)
                        {
                            newVec[j2] -= dot * eigenvectors[k][j2];
                        }
                    }
                }

                norm = 0.0;
                for (int j = 0; j < p; j++)
                {
                    norm += newVec[j] * newVec[j];
                }
                norm = System.Math.Sqrt(norm);
                if (norm < 1e-15) break;
                for (int j = 0; j < p; j++) newVec[j] /= norm;

                vector = newVec;
            }

            double eigenval = 0.0;
            for (int j = 0; j < p; j++)
            {
                double sum = 0.0;
                for (int k = 0; k < p; k++)
                {
                    double cov = 0.0;
                    for (int i = 0; i < n; i++)
                    {
                        cov += scaledData[i * p + j] * scaledData[i * p + k];
                    }
                    cov /= n;
                    sum += cov * vector[k];
                }
                eigenval += vector[j] * sum;
            }

            eigenvectors[comp] = vector;
            eigenvalues[comp] = eigenval;
        }

        double[] explainedRatio = new double[numComponents];
        double cumulativeRatio = 0.0;
        for (int c = 0; c < numComponents; c++)
        {
            explainedRatio[c] = totalVariance > 1e-15 ? eigenvalues[c] / totalVariance : 0.0;
            cumulativeRatio += explainedRatio[c];
        }

        List<ProjectedPoint> points = new(n);
        for (int i = 0; i < n; i++)
        {
            double[] projection = new double[numComponents];
            for (int c = 0; c < numComponents; c++)
            {
                double dot = 0.0;
                for (int j = 0; j < p; j++)
                {
                    dot += scaledData[i * p + j] * eigenvectors[c][j];
                }
                projection[c] = dot;
            }
            points.Add(new ProjectedPoint
            {
                Components = projection,
                OriginalIndex = i
            });
        }

        return new PCAResult
        {
            Points = points,
            ExplainedVarianceRatio = explainedRatio,
            Components = eigenvectors,
            TotalVarianceExplained = cumulativeRatio,
            NumComponents = numComponents
        };
    }
}
