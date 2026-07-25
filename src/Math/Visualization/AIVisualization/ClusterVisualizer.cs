namespace MathVerse.Math.Visualization.AIVisualization;

using System.Collections.Generic;

/// <summary>Represents a 2D projected data point with its cluster label.</summary>
public sealed record ClusterPoint
{
    /// <summary>X coordinate after projection.</summary>
    public required double X { get; init; }

    /// <summary>Y coordinate after projection.</summary>
    public required double Y { get; init; }

    /// <summary>Cluster label.</summary>
    public required int Label { get; init; }

    /// <summary>Original index in the input data.</summary>
    public required int OriginalIndex { get; init; }
}

/// <summary>Represents a cluster centroid in the projected space.</summary>
public sealed record ClusterCentroid
{
    /// <summary>X coordinate of the centroid.</summary>
    public required double X { get; init; }

    /// <summary>Y coordinate of the centroid.</summary>
    public required double Y { get; init; }

    /// <summary>Cluster label for this centroid.</summary>
    public required int Label { get; init; }
}

/// <summary>Complete data for a cluster visualization.</summary>
public sealed record ClusterVisualizationData
{
    /// <summary>Projected data points.</summary>
    public required IReadOnlyList<ClusterPoint> Points { get; init; }

    /// <summary>Cluster centroids (if provided).</summary>
    public required IReadOnlyList<ClusterCentroid> Centroids { get; init; }

    /// <summary>Number of unique clusters.</summary>
    public required int ClusterCount { get; init; }

    /// <summary>Whether PCA projection was applied.</summary>
    public required bool WasProjected { get; init; }
}

/// <summary>Visualizes clustering results, projecting to 2D if needed.</summary>
public sealed class ClusterVisualizer
{
    /// <summary>
    /// Creates a cluster visualization, projecting high-dimensional data to 2D via simple PCA if needed.
    /// </summary>
    /// <param name="data">Data points (each row is a point).</param>
    /// <param name="labels">Cluster label for each point.</param>
    /// <param name="centroids">Optional cluster centroids.</param>
    /// <param name="projectionDims">Target dimensions (must be 2).</param>
    /// <returns>Projected 2D visualization data.</returns>
    public ClusterVisualizationData Create(
        double[][] data,
        int[] labels,
        double[][]? centroids = null,
        int projectionDims = 2)
    {
        if (data == null || data.Length == 0 || labels == null || labels.Length == 0)
        {
            return new ClusterVisualizationData
            {
                Points = [],
                Centroids = [],
                ClusterCount = 0,
                WasProjected = false
            };
        }

        bool needsProjection = data[0].Length > projectionDims;
        double[][] projectedData = needsProjection ? ProjectTo2D(data, projectionDims) : data;

        var points = new List<ClusterPoint>();
        for (int i = 0; i < projectedData.Length; i++)
        {
            int label = i < labels.Length ? labels[i] : 0;
            points.Add(new ClusterPoint
            {
                X = projectedData[i][0],
                Y = projectedData.Length > 0 && projectedData[i].Length > 1 ? projectedData[i][1] : 0.0,
                Label = label,
                OriginalIndex = i
            });
        }

        var centroidPoints = new List<ClusterCentroid>();
        if (centroids != null)
        {
            double[][] projectedCentroids = needsProjection ? ProjectTo2D(centroids, projectionDims) : centroids;
            for (int i = 0; i < projectedCentroids.Length; i++)
            {
                centroidPoints.Add(new ClusterCentroid
                {
                    X = projectedCentroids[i][0],
                    Y = projectedCentroids[i].Length > 1 ? projectedCentroids[i][1] : 0.0,
                    Label = i
                });
            }
        }

        HashSet<int> uniqueLabels = new(labels);

        return new ClusterVisualizationData
        {
            Points = points,
            Centroids = centroidPoints,
            ClusterCount = uniqueLabels.Count,
            WasProjected = needsProjection
        };
    }

    private static double[][] ProjectTo2D(double[][] data, int targetDims)
    {
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

        double[,] covariance = new double[dims, dims];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < dims; j++)
            {
                for (int k = j; k < dims; k++)
                {
                    double val = (data[i][j] - mean[j]) * (data[i][k] - mean[k]);
                    covariance[j, k] += val;
                    if (j != k)
                        covariance[k, j] += val;
                }
            }
        }

        double scale = n > 1 ? 1.0 / (double)(n - 1) : 1.0;
        for (int j = 0; j < dims; j++)
            for (int k = 0; k < dims; k++)
                covariance[j, k] *= scale;

        double[][] eigenvectors = PowerIteration(covariance, dims, targetDims);

        double[][] result = new double[n][];
        for (int i = 0; i < n; i++)
        {
            result[i] = new double[targetDims];
            for (int d = 0; d < targetDims; d++)
            {
                double val = 0.0;
                for (int j = 0; j < dims; j++)
                    val += (data[i][j] - mean[j]) * eigenvectors[j][d];
                result[i][d] = val;
            }
        }

        return result;
    }

    private static double[][] PowerIteration(double[,] matrix, int dims, int numVectors)
    {
        var vectors = new double[numVectors][];

        for (int v = 0; v < numVectors; v++)
        {
            double[] vec = new double[dims];
            for (int i = 0; i < dims; i++)
                vec[i] = 1.0 / System.Math.Sqrt((double)dims);

            double[] deflate = new double[dims * dims];
            System.Array.Copy(matrix, deflate, dims * dims);

            for (int iter = 0; iter < 100; iter++)
            {
                double[] newVec = new double[dims];
                for (int i = 0; i < dims; i++)
                {
                    double sum = 0.0;
                    for (int j = 0; j < dims; j++)
                        sum += deflate[i * dims + j] * vec[j];
                    newVec[i] = sum;
                }

                double norm = 0.0;
                for (int i = 0; i < dims; i++)
                    norm += newVec[i] * newVec[i];
                norm = System.Math.Sqrt(norm);

                if (norm > 1e-15)
                    for (int i = 0; i < dims; i++)
                        newVec[i] /= norm;

                vec = newVec;
            }

            vectors[v] = vec;

            double eigenvalue = 0.0;
            for (int i = 0; i < dims; i++)
            {
                double sum = 0.0;
                for (int j = 0; j < dims; j++)
                    sum += deflate[i * dims + j] * vec[j];
                eigenvalue += vec[i] * sum;
            }

            for (int i = 0; i < dims; i++)
                for (int j = 0; j < dims; j++)
                    deflate[i * dims + j] -= eigenvalue * vec[i] * vec[j];
        }

        return vectors;
    }
}
