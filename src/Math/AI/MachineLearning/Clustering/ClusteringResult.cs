namespace MathVerse.Math.AI.MachineLearning.Clustering;
using System.Collections.Immutable;

/// <summary>Result of a clustering operation.</summary>
public sealed class ClusteringResult
{
    /// <summary>Cluster label assigned to each data point. Noise points are labeled -1.</summary>
    public int[] Labels { get; init; } = [];

    /// <summary>Centroid coordinates for each cluster.</summary>
    public double[][] Centroids { get; init; } = [];

    /// <summary>Total number of clusters identified.</summary>
    public int NumClusters { get; init; }

    /// <summary>Within-cluster sum of squares (inertia).</summary>
    public double Inertia { get; init; }

    /// <summary>Number of iterations actually executed.</summary>
    public int IterationsExecuted { get; init; }

    /// <summary>Whether the algorithm converged within tolerance.</summary>
    public bool Converged { get; init; }
}
