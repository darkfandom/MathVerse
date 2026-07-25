namespace MathVerse.Math.AI.MachineLearning.Clustering;

using System;

/// <summary>K-Means clustering algorithm with K-Means++ initialization and Lloyd's iteration.</summary>
public sealed class KMeansClustering
{
    private const double Tolerance = 1e-6;

    /// <summary>Partitions data into k clusters using Lloyd's algorithm with K-Means++ seeding.</summary>
    /// <param name="data">Array of data points, each represented as a double array.</param>
    /// <param name="k">Number of clusters to form.</param>
    /// <param name="maxIterations">Maximum number of iterations allowed.</param>
    /// <param name="seed">Random seed for reproducibility.</param>
    /// <returns>A <see cref="ClusteringResult"/> containing labels, centroids, and diagnostics.</returns>
    /// <exception cref="ArgumentException">Thrown when data is empty or k is invalid.</exception>
    public ClusteringResult Cluster(double[][] data, int k, int maxIterations = 100, int seed = 42)
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty.", nameof(data));
        if (k <= 0 || k > data.Length)
            throw new ArgumentException($"k must be between 1 and {data.Length}.", nameof(k));

        int n = data.Length;
        int d = data[0].Length;

        double[][] centroids = InitializeCentroidsPlusPlus(data, k, seed);
        int[] labels = new int[n];
        int iterationsExecuted = 0;
        bool converged = false;

        for (int iter = 0; iter < maxIterations; iter++)
        {
            iterationsExecuted = iter + 1;

            // Assignment step: assign each point to nearest centroid
            AssignClusters(data, centroids, labels);

            // Update step: recompute centroids
            double[][] newCentroids = ComputeCentroids(data, labels, k, d);

            // Check convergence
            double movement = 0.0;
            for (int c = 0; c < k; c++)
            {
                for (int j = 0; j < d; j++)
                {
                    double diff = newCentroids[c][j] - centroids[c][j];
                    movement += diff * diff;
                }
            }

            centroids = newCentroids;

            if (System.Math.Sqrt(movement) < Tolerance)
            {
                converged = true;
                break;
            }
        }

        double inertia = ComputeInertia(data, centroids, labels);

        return new ClusteringResult
        {
            Labels = labels,
            Centroids = centroids,
            NumClusters = k,
            Inertia = inertia,
            IterationsExecuted = iterationsExecuted,
            Converged = converged
        };
    }

    /// <summary>Initializes centroids using the K-Means++ algorithm.</summary>
    /// <param name="data">The data points.</param>
    /// <param name="k">Number of centroids to initialize.</param>
    /// <param name="seed">Random seed.</param>
    /// <returns>Initial centroid positions.</returns>
    private static double[][] InitializeCentroidsPlusPlus(double[][] data, int k, int seed)
    {
        int n = data.Length;
        int d = data[0].Length;
        var rng = new Random(seed);
        double[][] centroids = new double[k][];

        // Choose first centroid uniformly at random
        centroids[0] = (double[])data[rng.Next(n)].Clone();

        double[] distances = new double[n];

        for (int c = 1; c < k; c++)
        {
            // Compute squared distance from each point to nearest existing centroid
            double totalDistance = 0.0;
            for (int i = 0; i < n; i++)
            {
                double minDist = double.MaxValue;
                for (int j = 0; j < c; j++)
                {
                    double dist = SquaredDistance(data[i], centroids[j]);
                    if (dist < minDist)
                        minDist = dist;
                }
                distances[i] = minDist;
                totalDistance += minDist;
            }

            // Choose next centroid with probability proportional to squared distance
            double r = rng.NextDouble() * totalDistance;
            double cumulative = 0.0;
            int chosen = n - 1;
            for (int i = 0; i < n; i++)
            {
                cumulative += distances[i];
                if (cumulative >= r)
                {
                    chosen = i;
                    break;
                }
            }

            centroids[c] = (double[])data[chosen].Clone();
        }

        return centroids;
    }

    /// <summary>Assigns each data point to the nearest centroid.</summary>
    /// <param name="data">The data points.</param>
    /// <param name="centroids">Current centroid positions.</param>
    /// <param name="labels">Output array for cluster labels.</param>
    private static void AssignClusters(double[][] data, double[][] centroids, int[] labels)
    {
        int n = data.Length;
        int k = centroids.Length;

        for (int i = 0; i < n; i++)
        {
            double minDist = double.MaxValue;
            int bestCluster = 0;
            for (int c = 0; c < k; c++)
            {
                double dist = SquaredDistance(data[i], centroids[c]);
                if (dist < minDist)
                {
                    minDist = dist;
                    bestCluster = c;
                }
            }
            labels[i] = bestCluster;
        }
    }

    /// <summary>Recomputes centroids as the mean of assigned points.</summary>
    /// <param name="data">The data points.</param>
    /// <param name="labels">Cluster labels for each point.</param>
    /// <param name="k">Number of clusters.</param>
    /// <param name="d">Dimensionality of the data.</param>
    /// <returns>Updated centroid positions.</returns>
    private static double[][] ComputeCentroids(double[][] data, int[] labels, int k, int d)
    {
        double[][] centroids = new double[k][];
        int[] counts = new int[k];

        for (int c = 0; c < k; c++)
        {
            centroids[c] = new double[d];
        }

        for (int i = 0; i < data.Length; i++)
        {
            int c = labels[i];
            counts[c]++;
            for (int j = 0; j < d; j++)
            {
                centroids[c][j] += data[i][j];
            }
        }

        for (int c = 0; c < k; c++)
        {
            if (counts[c] > 0)
            {
                for (int j = 0; j < d; j++)
                {
                    centroids[c][j] /= counts[c];
                }
            }
        }

        return centroids;
    }

    /// <summary>Computes the within-cluster sum of squares (inertia).</summary>
    /// <param name="data">The data points.</param>
    /// <param name="centroids">Centroid positions.</param>
    /// <param name="labels">Cluster labels.</param>
    /// <returns>Total inertia value.</returns>
    private static double ComputeInertia(double[][] data, double[][] centroids, int[] labels)
    {
        double inertia = 0.0;
        for (int i = 0; i < data.Length; i++)
        {
            inertia += SquaredDistance(data[i], centroids[labels[i]]);
        }
        return inertia;
    }

    /// <summary>Computes the squared Euclidean distance between two points.</summary>
    /// <param name="a">First point.</param>
    /// <param name="b">Second point.</param>
    /// <returns>Squared Euclidean distance.</returns>
    private static double SquaredDistance(double[] a, double[] b)
    {
        double sum = 0.0;
        for (int j = 0; j < a.Length; j++)
        {
            double diff = a[j] - b[j];
            sum += diff * diff;
        }
        return sum;
    }
}
