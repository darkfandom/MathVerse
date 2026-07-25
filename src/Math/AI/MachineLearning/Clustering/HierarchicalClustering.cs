namespace MathVerse.Math.AI.MachineLearning.Clustering;

using System;
using System.Collections.Generic;

/// <summary>Defines the linkage criterion for hierarchical clustering.</summary>
public enum LinkageType
{
    /// <summary>Single linkage: minimum distance between any two points in different clusters.</summary>
    Single,

    /// <summary>Complete linkage: maximum distance between any two points in different clusters.</summary>
    Complete,

    /// <summary>Average linkage: mean distance between all pairs of points in different clusters.</summary>
    Average
}

/// <summary>Agglomerative hierarchical clustering with single, complete, or average linkage.</summary>
public sealed class HierarchicalClustering
{
    private const double Infinity = double.MaxValue;

    /// <summary>Performs agglomerative hierarchical clustering.</summary>
    /// <param name="data">Array of data points, each represented as a double array.</param>
    /// <param name="numClusters">Desired number of final clusters.</param>
    /// <param name="linkage">Linkage criterion to use for cluster distance.</param>
    /// <returns>A <see cref="ClusteringResult"/> containing cluster labels and cluster centroids.</returns>
    /// <exception cref="ArgumentException">Thrown when data is empty or parameters are invalid.</exception>
    public ClusteringResult Cluster(double[][] data, int numClusters, LinkageType linkage = LinkageType.Single)
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty.", nameof(data));
        if (numClusters <= 0 || numClusters > data.Length)
            throw new ArgumentException($"numClusters must be between 1 and {data.Length}.", nameof(numClusters));

        int n = data.Length;
        int d = data[0].Length;

        // Each point starts as its own cluster; clusterIds[i] = current cluster id for point i
        int[] clusterIds = new int[n];
        for (int i = 0; i < n; i++)
            clusterIds[i] = i;

        // Track which points belong to which active cluster
        List<HashSet<int>> clusterMembers = new();
        for (int i = 0; i < n; i++)
        {
            HashSet<int> set = new() { i };
            clusterMembers.Add(set);
        }

        // Compute initial pairwise distance matrix
        int activeCount = n;
        double[][] distMatrix = new double[n][];
        for (int i = 0; i < n; i++)
        {
            distMatrix[i] = new double[n];
            for (int j = 0; j < n; j++)
                distMatrix[i][j] = EuclideanDistance(data[i], data[j]);
        }

        // Active cluster indices
        List<int> activeClusters = new();
        for (int i = 0; i < n; i++)
            activeClusters.Add(i);

        int currentClusterId = n;

        while (activeCount > numClusters)
        {
            // Find closest pair of active clusters
            double minDist = Infinity;
            int mergeA = -1;
            int mergeB = -1;

            for (int ai = 0; ai < activeClusters.Count; ai++)
            {
                for (int bi = ai + 1; bi < activeClusters.Count; bi++)
                {
                    int ci = activeClusters[ai];
                    int cj = activeClusters[bi];
                    double dist = distMatrix[ci][cj];
                    if (dist < minDist)
                    {
                        minDist = dist;
                        mergeA = ai;
                        mergeB = bi;
                    }
                }
            }

            int clusterA = activeClusters[mergeA];
            int clusterB = activeClusters[mergeB];

            // Create new cluster combining A and B
            HashSet<int> merged = new(clusterMembers[clusterA]);
            merged.UnionWith(clusterMembers[clusterB]);
            clusterMembers.Add(merged);

            int newClusterId = currentClusterId;
            currentClusterId++;

            // Extend distMatrix for the new cluster
            double[] newRow = new double[currentClusterId];
            for (int i = 0; i < currentClusterId; i++)
                newRow[i] = 0.0;

            // Compute distances from the new cluster to all existing clusters
            for (int ai = 0; ai < activeClusters.Count; ai++)
            {
                int ci = activeClusters[ai];
                if (ci == clusterA || ci == clusterB) continue;

                double dist = ComputeLinkageDistance(
                    clusterMembers[newClusterId], clusterMembers[ci], data, linkage);
                newRow[ci] = dist;

                // Ensure the reverse is available in distMatrix
                if (newClusterId < distMatrix.Length && ci < distMatrix.Length)
                {
                    // rows may need extending
                }
            }

            // Grow distMatrix if needed
            if (newClusterId >= distMatrix.Length)
            {
                double[][] expanded = new double[newClusterId + 1][];
                for (int i = 0; i < distMatrix.Length; i++)
                    expanded[i] = distMatrix[i];
                for (int i = distMatrix.Length; i <= newClusterId; i++)
                    expanded[i] = new double[newClusterId + 1];
                distMatrix = expanded;
            }

            // Fill in the new row and column
            for (int ai = 0; ai < activeClusters.Count; ai++)
            {
                int ci = activeClusters[ai];
                if (ci == clusterA || ci == clusterB) continue;

                double dist = ComputeLinkageDistance(
                    clusterMembers[newClusterId], clusterMembers[ci], data, linkage);
                distMatrix[newClusterId][ci] = dist;
                distMatrix[ci][newClusterId] = dist;
            }
            distMatrix[newClusterId][newClusterId] = 0.0;

            // Remove merged clusters from active list, add new one
            // Remove higher index first to preserve lower index
            activeClusters.RemoveAt(mergeB);
            activeClusters.RemoveAt(mergeA);
            activeClusters.Add(newClusterId);

            activeCount--;
        }

        // Assign final labels
        int[] labels = new int[n];
        for (int i = 0; i < activeClusters.Count; i++)
        {
            int cid = activeClusters[i];
            foreach (int pt in clusterMembers[cid])
                labels[pt] = i;
        }

        int finalK = activeClusters.Count;
        double[][] centroids = ComputeCentroids(data, labels, finalK, d);

        return new ClusteringResult
        {
            Labels = labels,
            Centroids = centroids,
            NumClusters = finalK,
            Inertia = ComputeInertia(data, centroids, labels),
            IterationsExecuted = n - finalK,
            Converged = true
        };
    }

    /// <summary>Computes the linkage distance between two clusters based on the specified criterion.</summary>
    /// <param name="clusterA">Member indices of the first cluster.</param>
    /// <param name="clusterB">Member indices of the second cluster.</param>
    /// <param name="data">The data points.</param>
    /// <param name="linkage">The linkage criterion.</param>
    /// <returns>The linkage distance.</returns>
    private static double ComputeLinkageDistance(
        HashSet<int> clusterA,
        HashSet<int> clusterB,
        double[][] data,
        LinkageType linkage)
    {
        switch (linkage)
        {
            case LinkageType.Single:
            {
                double minDist = Infinity;
                foreach (int i in clusterA)
                {
                    foreach (int j in clusterB)
                    {
                        double dist = EuclideanDistance(data[i], data[j]);
                        if (dist < minDist)
                            minDist = dist;
                    }
                }
                return minDist;
            }
            case LinkageType.Complete:
            {
                double maxDist = 0.0;
                foreach (int i in clusterA)
                {
                    foreach (int j in clusterB)
                    {
                        double dist = EuclideanDistance(data[i], data[j]);
                        if (dist > maxDist)
                            maxDist = dist;
                    }
                }
                return maxDist;
            }
            case LinkageType.Average:
            {
                double totalDist = 0.0;
                int count = 0;
                foreach (int i in clusterA)
                {
                    foreach (int j in clusterB)
                    {
                        totalDist += EuclideanDistance(data[i], data[j]);
                        count++;
                    }
                }
                return count > 0 ? totalDist / count : 0.0;
            }
            default:
                throw new ArgumentException($"Unknown linkage type: {linkage}");
        }
    }

    /// <summary>Computes Euclidean distance between two points.</summary>
    /// <param name="a">First point.</param>
    /// <param name="b">Second point.</param>
    /// <returns>Euclidean distance.</returns>
    private static double EuclideanDistance(double[] a, double[] b)
    {
        double sum = 0.0;
        for (int j = 0; j < a.Length; j++)
        {
            double diff = a[j] - b[j];
            sum += diff * diff;
        }
        return System.Math.Sqrt(sum);
    }

    /// <summary>Computes centroids for each cluster.</summary>
    /// <param name="data">Data points.</param>
    /// <param name="labels">Cluster labels.</param>
    /// <param name="k">Number of clusters.</param>
    /// <param name="d">Dimensionality.</param>
    /// <returns>Centroid array.</returns>
    private static double[][] ComputeCentroids(double[][] data, int[] labels, int k, int d)
    {
        double[][] centroids = new double[k][];
        int[] counts = new int[k];

        for (int c = 0; c < k; c++)
            centroids[c] = new double[d];

        for (int i = 0; i < data.Length; i++)
        {
            int c = labels[i];
            counts[c]++;
            for (int j = 0; j < d; j++)
                centroids[c][j] += data[i][j];
        }

        for (int c = 0; c < k; c++)
        {
            if (counts[c] > 0)
            {
                for (int j = 0; j < d; j++)
                    centroids[c][j] /= counts[c];
            }
        }

        return centroids;
    }

    /// <summary>Computes within-cluster sum of squares.</summary>
    /// <param name="data">Data points.</param>
    /// <param name="centroids">Centroid positions.</param>
    /// <param name="labels">Cluster labels.</param>
    /// <returns>Total inertia.</returns>
    private static double ComputeInertia(double[][] data, double[][] centroids, int[] labels)
    {
        double inertia = 0.0;
        for (int i = 0; i < data.Length; i++)
        {
            double sum = 0.0;
            double[] c = centroids[labels[i]];
            for (int j = 0; j < data[i].Length; j++)
            {
                double diff = data[i][j] - c[j];
                sum += diff * diff;
            }
            inertia += sum;
        }
        return inertia;
    }
}
