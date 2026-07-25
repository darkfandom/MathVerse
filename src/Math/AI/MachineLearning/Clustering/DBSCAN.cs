namespace MathVerse.Math.AI.MachineLearning.Clustering;

using System;
using System.Collections.Generic;

/// <summary>DBSCAN (Density-Based Spatial Clustering of Applications with Noise) algorithm.</summary>
public sealed class DBSCAN
{
    /// <summary>Clusters data using the DBSCAN algorithm.</summary>
    /// <param name="data">Array of data points, each represented as a double array.</param>
    /// <param name="epsilon">Maximum distance between two points to be considered neighbors.</param>
    /// <param name="minPoints">Minimum number of points required to form a core point.</param>
    /// <returns>A <see cref="ClusteringResult"/> with labels (noise = -1) and no centroids.</returns>
    /// <exception cref="ArgumentException">Thrown when data is empty or parameters are invalid.</exception>
    public ClusteringResult Cluster(double[][] data, double epsilon, int minPoints)
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty.", nameof(data));
        if (epsilon <= 0.0)
            throw new ArgumentException("Epsilon must be positive.", nameof(epsilon));
        if (minPoints <= 0)
            throw new ArgumentException("MinPoints must be positive.", nameof(minPoints));

        int n = data.Length;
        int[] labels = new int[n];
        bool[] visited = new bool[n];

        // Initialize all labels to noise (-1)
        for (int i = 0; i < n; i++)
            labels[i] = -1;

        int clusterId = 0;

        for (int i = 0; i < n; i++)
        {
            if (visited[i])
                continue;

            visited[i] = true;
            int[] neighbors = RegionQuery(data, i, epsilon);

            if (neighbors.Length < minPoints)
            {
                // Point is noise; label stays -1
                continue;
            }

            // Start a new cluster
            labels[i] = clusterId;
            ExpandCluster(data, i, neighbors, clusterId, epsilon, minPoints, labels, visited);
            clusterId++;
        }

        return new ClusteringResult
        {
            Labels = labels,
            Centroids = [],
            NumClusters = clusterId,
            Inertia = ComputeInertia(data, labels, clusterId),
            IterationsExecuted = 1,
            Converged = true
        };
    }

    /// <summary>Finds all points within epsilon distance of the given point (epsilon neighborhood).</summary>
    /// <param name="data">The data points.</param>
    /// <param name="pointIndex">Index of the query point.</param>
    /// <param name="epsilon">Distance threshold.</param>
    /// <returns>Array of indices of neighboring points (including the query point).</returns>
    private static int[] RegionQuery(double[][] data, int pointIndex, double epsilon)
    {
        List<int> neighbors = new();
        double[] p = data[pointIndex];
        double epsSq = epsilon * epsilon;

        for (int i = 0; i < data.Length; i++)
        {
            double distSq = SquaredDistance(p, data[i]);
            if (distSq <= epsSq)
                neighbors.Add(i);
        }

        return neighbors.ToArray();
    }

    /// <summary>Expands a cluster by adding density-reachable points.</summary>
    /// <param name="data">The data points.</param>
    /// <param name="seedIndex">The seed point index (core point).</param>
    /// <param name="seedNeighbors">Neighbors of the seed point.</param>
    /// <param name="clusterId">Current cluster identifier.</param>
    /// <param name="epsilon">Distance threshold.</param>
    /// <param name="minPoints">Minimum points for a core point.</param>
    /// <param name="labels">Label array to modify in-place.</param>
    /// <param name="visited">Visited array to modify in-place.</param>
    private static void ExpandCluster(
        double[][] data,
        int seedIndex,
        int[] seedNeighbors,
        int clusterId,
        double epsilon,
        int minPoints,
        int[] labels,
        bool[] visited)
    {
        Queue<int> queue = new(seedNeighbors);

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();

            if (!visited[current])
            {
                visited[current] = true;
                int[] neighbors = RegionQuery(data, current, epsilon);
                if (neighbors.Length >= minPoints)
                {
                    for (int j = 0; j < neighbors.Length; j++)
                    {
                        if (!visited[neighbors[j]] || labels[neighbors[j]] == -1)
                        {
                            queue.Enqueue(neighbors[j]);
                        }
                    }
                }
            }

            if (labels[current] == -1)
            {
                labels[current] = clusterId;
            }
        }
    }

    /// <summary>Computes total within-cluster sum of squares for non-noise points.</summary>
    /// <param name="data">Data points.</param>
    /// <param name="labels">Cluster labels.</param>
    /// <param name="numClusters">Number of clusters.</param>
    /// <returns>Total inertia value.</returns>
    private static double ComputeInertia(double[][] data, int[] labels, int numClusters)
    {
        int d = data[0].Length;
        double[][] centroids = new double[numClusters][];
        int[] counts = new int[numClusters];

        for (int c = 0; c < numClusters; c++)
            centroids[c] = new double[d];

        for (int i = 0; i < data.Length; i++)
        {
            if (labels[i] < 0) continue;
            counts[labels[i]]++;
            for (int j = 0; j < d; j++)
                centroids[labels[i]][j] += data[i][j];
        }

        for (int c = 0; c < numClusters; c++)
        {
            if (counts[c] > 0)
            {
                for (int j = 0; j < d; j++)
                    centroids[c][j] /= counts[c];
            }
        }

        double inertia = 0.0;
        for (int i = 0; i < data.Length; i++)
        {
            if (labels[i] < 0) continue;
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
