namespace MathVerse.Math.AI.MathematicalLearning;

using System;
using System.Collections.Generic;

/// <summary>Clusters mathematical expressions by structural similarity using embeddings and k-means.</summary>
public sealed class ExpressionClustering
{
    private readonly MathematicalEmbedding _embedding;
    private readonly Random _rng;

    /// <summary>Initializes a new expression clustering instance.</summary>
    /// <param name="seed">Random seed for reproducibility. Use -1 for non-deterministic.</param>
    public ExpressionClustering(int seed = -1)
    {
        _embedding = new MathematicalEmbedding();
        _rng = seed >= 0 ? new Random(seed) : new Random();
    }

    /// <summary>Clusters a list of mathematical expressions into the specified number of clusters.</summary>
    /// <param name="expressions">List of expression strings.</param>
    /// <param name="numClusters">Number of clusters to create.</param>
    /// <returns>Clustering result with cluster assignments and centroids.</returns>
    public ClusteringResult Cluster(List<string> expressions, int numClusters)
    {
        if (expressions == null || expressions.Count == 0)
            throw new ArgumentException("Expressions cannot be null or empty.", nameof(expressions));
        if (numClusters <= 0)
            throw new ArgumentException("Number of clusters must be positive.", nameof(numClusters));
        if (numClusters > expressions.Count)
            throw new ArgumentException("Number of clusters cannot exceed the number of expressions.", nameof(numClusters));

        int dimensions = 64;
        double[][] embeddings = new double[expressions.Count][];
        for (int i = 0; i < expressions.Count; i++)
            embeddings[i] = _embedding.Embed(expressions[i], dimensions);

        int maxIterations = 100;
        int[] assignments = KMeans(embeddings, numClusters, maxIterations);

        double[][] centroids = ComputeCentroids(embeddings, assignments, numClusters, dimensions);
        double silhouette = ComputeSilhouetteScore(embeddings, assignments, numClusters);

        Dictionary<int, List<int>> clusterMap = new();
        for (int c = 0; c < numClusters; c++)
            clusterMap[c] = new List<int>();
        for (int i = 0; i < assignments.Length; i++)
            clusterMap[assignments[i]].Add(i);

        List<ClusterInfo> clusters = new();
        for (int c = 0; c < numClusters; c++)
        {
            List<string> clusterExprs = new();
            foreach (int idx in clusterMap[c])
                clusterExprs.Add(expressions[idx]);

            clusters.Add(new ClusterInfo
            {
                ClusterId = c,
                Centroid = centroids[c],
                Members = clusterMap[c],
                MemberExpressions = clusterExprs
            });
        }

        return new ClusteringResult
        {
            Clusters = clusters,
            Assignments = assignments,
            SilhouetteScore = silhouette,
            ExpressionCount = expressions.Count,
            NumClusters = numClusters
        };
    }

    /// <summary>Determines the optimal number of clusters using the elbow method.</summary>
    /// <param name="expressions">List of expression strings.</param>
    /// <param name="maxK">Maximum number of clusters to test.</param>
    /// <returns>Recommended number of clusters.</returns>
    public int FindOptimalClusters(List<string> expressions, int maxK = 10)
    {
        if (expressions == null || expressions.Count == 0)
            throw new ArgumentException("Expressions cannot be null or empty.", nameof(expressions));

        int n = expressions.Count;
        maxK = System.Math.Min(maxK, n);

        if (maxK <= 1)
            return 1;

        int dimensions = 64;
        double[][] embeddings = new double[n][];
        for (int i = 0; i < n; i++)
            embeddings[i] = _embedding.Embed(expressions[i], dimensions);

        double prevSSE = double.MaxValue;
        double prevSilhouette = -1.0;
        int bestK = 2;

        for (int k = 2; k <= maxK; k++)
        {
            int[] assignments = KMeans(embeddings, k, 50);
            double sse = ComputeSSE(embeddings, assignments, k, dimensions);
            double silhouette = ComputeSilhouetteScore(embeddings, assignments, k);

            double sseImprovement = prevSSE - sse;
            if (k > 2 && sseImprovement < prevSSE * 0.1)
            {
                bestK = k - 1;
                break;
            }

            if (silhouette > prevSilhouette + 0.01)
            {
                bestK = k;
            }

            prevSSE = sse;
            prevSilhouette = silhouette;
        }

        return bestK;
    }

    /// <summary>Assigns a new expression to the nearest existing cluster.</summary>
    /// <param name="expression">The new expression.</param>
    /// <param name="centroids">Existing cluster centroids.</param>
    /// <param name="dimensions">Embedding dimensions.</param>
    /// <returns>Index of the nearest cluster.</returns>
    public int AssignToCluster(string expression, double[][] centroids, int dimensions = 64)
    {
        if (string.IsNullOrEmpty(expression))
            throw new ArgumentException("Expression cannot be null or empty.", nameof(expression));
        if (centroids == null || centroids.Length == 0)
            throw new ArgumentException("Centroids cannot be null or empty.", nameof(centroids));

        double[] emb = _embedding.Embed(expression, dimensions);
        int bestCluster = 0;
        double bestDist = double.MaxValue;

        for (int c = 0; c < centroids.Length; c++)
        {
            double dist = EuclideanDistance(emb, centroids[c]);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestCluster = c;
            }
        }

        return bestCluster;
    }

    private int[] KMeans(double[][] data, int k, int maxIterations)
    {
        int n = data.Length;
        int dims = data[0].Length;
        int[] assignments = new int[n];

        double[][] centroids = InitializeCentroids(data, k);

        for (int iter = 0; iter < maxIterations; iter++)
        {
            bool changed = false;

            for (int i = 0; i < n; i++)
            {
                int bestCluster = 0;
                double bestDist = double.MaxValue;
                for (int c = 0; c < k; c++)
                {
                    double dist = EuclideanDistance(data[i], centroids[c]);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestCluster = c;
                    }
                }
                if (assignments[i] != bestCluster)
                {
                    assignments[i] = bestCluster;
                    changed = true;
                }
            }

            if (!changed)
                break;

            centroids = ComputeCentroids(data, assignments, k, dims);
        }

        return assignments;
    }

    private double[][] InitializeCentroids(double[][] data, int k)
    {
        int n = data.Length;
        int dims = data[0].Length;
        double[][] centroids = new double[k][];

        bool[] selected = new bool[n];
        int first = _rng.Next(n);
        centroids[0] = (double[])data[first].Clone();
        selected[first] = true;

        for (int c = 1; c < k; c++)
        {
            double[] dists = new double[n];
            double totalDist = 0.0;

            for (int i = 0; i < n; i++)
            {
                if (selected[i])
                {
                    dists[i] = 0.0;
                }
                else
                {
                    double minDist = double.MaxValue;
                    for (int j = 0; j < c; j++)
                    {
                        double dist = EuclideanDistance(data[i], centroids[j]);
                        if (dist < minDist)
                            minDist = dist;
                    }
                    dists[i] = minDist;
                    totalDist += minDist;
                }
            }

            double target = _rng.NextDouble() * totalDist;
            double cumulative = 0.0;
            int chosen = 0;
            for (int i = 0; i < n; i++)
            {
                cumulative += dists[i];
                if (cumulative >= target)
                {
                    chosen = i;
                    break;
                }
            }

            centroids[c] = (double[])data[chosen].Clone();
            selected[chosen] = true;
        }

        return centroids;
    }

    private static double[][] ComputeCentroids(double[][] data, int[] assignments, int k, int dims)
    {
        int[] counts = new int[k];
        double[][] centroids = new double[k][];
        for (int c = 0; c < k; c++)
            centroids[c] = new double[dims];

        for (int i = 0; i < data.Length; i++)
        {
            int cluster = assignments[i];
            counts[cluster]++;
            for (int d = 0; d < dims; d++)
                centroids[cluster][d] += data[i][d];
        }

        for (int c = 0; c < k; c++)
        {
            if (counts[c] > 0)
            {
                for (int d = 0; d < dims; d++)
                    centroids[c][d] /= counts[c];
            }
        }

        return centroids;
    }

    private static double ComputeSSE(double[][] data, int[] assignments, int k, int dims)
    {
        double[][] centroids = ComputeCentroids(data, assignments, k, dims);
        double sse = 0.0;

        for (int i = 0; i < data.Length; i++)
        {
            double dist = EuclideanDistance(data[i], centroids[assignments[i]]);
            sse += dist * dist;
        }

        return sse;
    }

    private static double ComputeSilhouetteScore(double[][] data, int[] assignments, int k)
    {
        int n = data.Length;
        if (n <= 1 || k <= 1)
            return 0.0;

        double totalScore = 0.0;
        int validCount = 0;

        for (int i = 0; i < n; i++)
        {
            int myCluster = assignments[i];

            double intraSum = 0.0;
            int intraCount = 0;
            for (int j = 0; j < n; j++)
            {
                if (j != i && assignments[j] == myCluster)
                {
                    intraSum += EuclideanDistance(data[i], data[j]);
                    intraCount++;
                }
            }
            double a = intraCount > 0 ? intraSum / intraCount : 0.0;

            double minInter = double.MaxValue;
            for (int c = 0; c < k; c++)
            {
                if (c == myCluster)
                    continue;

                double interSum = 0.0;
                int interCount = 0;
                for (int j = 0; j < n; j++)
                {
                    if (assignments[j] == c)
                    {
                        interSum += EuclideanDistance(data[i], data[j]);
                        interCount++;
                    }
                }
                if (interCount > 0)
                {
                    double b = interSum / interCount;
                    if (b < minInter)
                        minInter = b;
                }
            }

            if (minInter == double.MaxValue)
                continue;

            double s = (minInter - a) / System.Math.Max(a, minInter);
            totalScore += s;
            validCount++;
        }

        return validCount > 0 ? totalScore / validCount : 0.0;
    }

    private static double EuclideanDistance(double[] a, double[] b)
    {
        int len = System.Math.Min(a.Length, b.Length);
        double sum = 0.0;
        for (int i = 0; i < len; i++)
        {
            double diff = a[i] - b[i];
            sum += diff * diff;
        }
        return System.Math.Sqrt(sum);
    }
}

/// <summary>Result of clustering mathematical expressions.</summary>
public sealed class ClusteringResult
{
    /// <summary>Gets the list of cluster information.</summary>
    public List<ClusterInfo> Clusters { get; init; } = new();

    /// <summary>Gets the cluster assignment for each expression.</summary>
    public int[] Assignments { get; init; } = Array.Empty<int>();

    /// <summary>Gets the silhouette score (-1 to 1, higher is better).</summary>
    public double SilhouetteScore { get; init; }

    /// <summary>Gets the total number of expressions clustered.</summary>
    public int ExpressionCount { get; init; }

    /// <summary>Gets the number of clusters.</summary>
    public int NumClusters { get; init; }
}

/// <summary>Information about a single cluster.</summary>
public sealed class ClusterInfo
{
    /// <summary>Gets the cluster identifier.</summary>
    public int ClusterId { get; init; }

    /// <summary>Gets the centroid embedding vector.</summary>
    public double[] Centroid { get; init; } = Array.Empty<double>();

    /// <summary>Gets the indices of expressions in this cluster.</summary>
    public List<int> Members { get; init; } = new();

    /// <summary>Gets the expression strings in this cluster.</summary>
    public List<string> MemberExpressions { get; init; } = new();
}
