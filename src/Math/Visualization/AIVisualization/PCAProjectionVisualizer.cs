namespace MathVerse.Math.Visualization.AIVisualization;

using System.Collections.Generic;

/// <summary>Represents a single point in the PCA scatter plot.</summary>
public sealed record PCAPoint
{
    /// <summary>PC1 value (x coordinate).</summary>
    public required double PC1 { get; init; }

    /// <summary>PC2 value (y coordinate).</summary>
    public required double PC2 { get; init; }

    /// <summary>Optional cluster/class label.</summary>
    public int? Label { get; init; }

    /// <summary>Original index in the input data.</summary>
    public required int OriginalIndex { get; init; }
}

/// <summary>PCA analysis result with projected data and explained variance.</summary>
public sealed record PCAVisualizationData
{
    /// <summary>Projected 2D points.</summary>
    public required IReadOnlyList<PCAPoint> Points { get; init; }

    /// <summary>Explained variance ratio for PC1.</summary>
    public required double PC1VarianceRatio { get; init; }

    /// <summary>Explained variance ratio for PC2.</summary>
    public required double PC2VarianceRatio { get; init; }

    /// <summary>PC1 eigenvector (loadings).</summary>
    public required IReadOnlyList<double> PC1Loadings { get; init; }

    /// <summary>PC2 eigenvector (loadings).</summary>
    public required IReadOnlyList<double> PC2Loadings { get; init; }
}

/// <summary>Visualizes data projected onto principal components as a scatter plot.</summary>
public sealed class PCAProjectionVisualizer
{
    /// <summary>
    /// Creates a PCA scatter plot by computing the top 2 principal components and projecting the data.
    /// </summary>
    /// <param name="data">High-dimensional data points.</param>
    /// <param name="labels">Optional labels for coloring.</param>
    /// <param name="pc1">Index of the first principal component (default 0).</param>
    /// <param name="pc2">Index of the second principal component (default 1).</param>
    /// <returns>PCA visualization data with projected points and variance info.</returns>
    public PCAVisualizationData Create(double[][] data, int[]? labels = null, int pc1 = 0, int pc2 = 1)
    {
        if (data == null || data.Length < 2)
        {
            return new PCAVisualizationData
            {
                Points = [],
                PC1VarianceRatio = 0.0,
                PC2VarianceRatio = 0.0,
                PC1Loadings = [],
                PC2Loadings = []
            };
        }

        int n = data.Length;
        int dims = data[0].Length;

        double[] mean = new double[dims];
        for (int j = 0; j < dims; j++)
        {
            double sum = 0.0;
            for (int i = 0; i < n; i++)
                sum += data[i][j];
            mean[j] = sum / (double)n;
        }

        double[][] centered = new double[n][];
        for (int i = 0; i < n; i++)
        {
            centered[i] = new double[dims];
            for (int j = 0; j < dims; j++)
                centered[i][j] = data[i][j] - mean[j];
        }

        double[,] covariance = new double[dims, dims];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < dims; j++)
            {
                for (int k = j; k < dims; k++)
                {
                    covariance[j, k] += centered[i][j] * centered[i][k];
                }
            }
        }

        double scale = 1.0 / (double)(n - 1);
        for (int j = 0; j < dims; j++)
        {
            for (int k = j; k < dims; k++)
            {
                covariance[j, k] *= scale;
                covariance[k, j] = covariance[j, k];
            }
        }

        int maxComp = pc1 > pc2 ? pc1 : pc2;
        var eigenvectors = EigenDecomposition(covariance, dims, maxComp + 1);
        var eigenvalues = Eigenvalues(covariance, dims, maxComp + 1);

        double totalVariance = 0.0;
        for (int i = 0; i < dims; i++)
            totalVariance += covariance[i, i];

        double pc1Var = totalVariance > 1e-15 ? eigenvalues[pc1] / totalVariance : 0.0;
        double pc2Var = totalVariance > 1e-15 ? eigenvalues[pc2] / totalVariance : 0.0;

        var pc1Vec = eigenvectors[pc1];
        var pc2Vec = eigenvectors[pc2];

        var points = new List<PCAPoint>();
        for (int i = 0; i < n; i++)
        {
            double val1 = 0.0;
            double val2 = 0.0;
            for (int j = 0; j < dims; j++)
            {
                val1 += centered[i][j] * pc1Vec[j];
                val2 += centered[i][j] * pc2Vec[j];
            }

            points.Add(new PCAPoint
            {
                PC1 = val1,
                PC2 = val2,
                Label = labels != null && i < labels.Length ? labels[i] : null,
                OriginalIndex = i
            });
        }

        var pc1Loadings = new List<double>();
        var pc2Loadings = new List<double>();
        for (int j = 0; j < dims; j++)
        {
            pc1Loadings.Add(pc1Vec[j]);
            pc2Loadings.Add(pc2Vec[j]);
        }

        return new PCAVisualizationData
        {
            Points = points,
            PC1VarianceRatio = pc1Var,
            PC2VarianceRatio = pc2Var,
            PC1Loadings = pc1Loadings,
            PC2Loadings = pc2Loadings
        };
    }

    private static double[] Eigenvalues(double[,] matrix, int dims, int count)
    {
        var values = new double[dims];
        double[,] work = new double[dims, dims];
        System.Array.Copy(matrix, work, dims * dims);

        for (int iter = 0; iter < 200; iter++)
        {
            double maxOff = 0.0;
            int p = 0, q = 1;
            for (int i = 0; i < dims; i++)
            {
                for (int j = i + 1; j < dims; j++)
                {
                    double absVal = System.Math.Abs(work[i, j]);
                    if (absVal > maxOff)
                    {
                        maxOff = absVal;
                        p = i;
                        q = j;
                    }
                }
            }

            if (maxOff < 1e-12) break;

            double theta = 0.5 * System.Math.Atan2(2.0 * work[p, q], work[p, p] - work[q, q]);
            double c = System.Math.Cos(theta);
            double s = System.Math.Sin(theta);

            double[,] rotation = new double[dims, dims];
            for (int i = 0; i < dims; i++)
                rotation[i, i] = 1.0;
            rotation[p, p] = c;
            rotation[q, q] = c;
            rotation[p, q] = s;
            rotation[q, p] = -s;

            double[,] temp = new double[dims, dims];
            for (int i = 0; i < dims; i++)
                for (int j = 0; j < dims; j++)
                    for (int k = 0; k < dims; k++)
                        temp[i, j] += rotation[k, i] * work[k, j];

            for (int i = 0; i < dims; i++)
                for (int j = 0; j < dims; j++)
                    work[i, j] = 0.0;

            for (int i = 0; i < dims; i++)
                for (int j = 0; j < dims; j++)
                    for (int k = 0; k < dims; k++)
                        work[i, j] += temp[i, k] * rotation[k, j];
        }

        for (int i = 0; i < dims; i++)
            values[i] = work[i, i];

        System.Array.Sort(values);
        System.Array.Reverse(values);

        return values;
    }

    private static double[][] EigenDecomposition(double[,] matrix, int dims, int count)
    {
        var vectors = new double[dims][];
        double[,] work = new double[dims, dims];
        System.Array.Copy(matrix, work, dims * dims);

        for (int iter = 0; iter < 200; iter++)
        {
            double maxOff = 0.0;
            int p = 0, q = 1;
            for (int i = 0; i < dims; i++)
            {
                for (int j = i + 1; j < dims; j++)
                {
                    double absVal = System.Math.Abs(work[i, j]);
                    if (absVal > maxOff)
                    {
                        maxOff = absVal;
                        p = i;
                        q = j;
                    }
                }
            }

            if (maxOff < 1e-12) break;

            double theta = 0.5 * System.Math.Atan2(2.0 * work[p, q], work[p, p] - work[q, q]);
            double c = System.Math.Cos(theta);
            double s = System.Math.Sin(theta);

            double[,] rotation = new double[dims, dims];
            for (int i = 0; i < dims; i++)
                rotation[i, i] = 1.0;
            rotation[p, p] = c;
            rotation[q, q] = c;
            rotation[p, q] = s;
            rotation[q, p] = -s;

            double[,] temp = new double[dims, dims];
            for (int i = 0; i < dims; i++)
                for (int j = 0; j < dims; j++)
                    for (int k = 0; k < dims; k++)
                        temp[i, j] += rotation[k, i] * work[k, j];

            for (int i = 0; i < dims; i++)
                for (int j = 0; j < dims; j++)
                    work[i, j] = 0.0;

            for (int i = 0; i < dims; i++)
                for (int j = 0; j < dims; j++)
                    for (int k = 0; k < dims; k++)
                        work[i, j] += temp[i, k] * rotation[k, j];
        }

        var eigenvalues = new double[dims];
        for (int i = 0; i < dims; i++)
            eigenvalues[i] = work[i, i];

        var indexed = new (double value, int index)[dims];
        for (int i = 0; i < dims; i++)
            indexed[i] = (eigenvalues[i], i);
        System.Array.Sort(indexed, (a, b) => b.value.CompareTo(a.value));

        for (int v = 0; v < dims; v++)
        {
            int origIdx = indexed[v].index;
            vectors[v] = new double[dims];
            for (int i = 0; i < dims; i++)
                vectors[v][i] = work[i, origIdx];
        }

        return vectors;
    }
}
