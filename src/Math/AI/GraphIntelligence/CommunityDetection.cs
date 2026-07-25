namespace MathVerse.Math.AI.GraphIntelligence;
using System;
using System.Collections.Generic;

/// <summary>
/// Provides community detection via label propagation and modularity computation.
/// </summary>
public static class CommunityDetection
{
    private static readonly Random _rng = new Random(42);

    /// <summary>
    /// Detects communities in the graph using the label propagation algorithm.
    /// Each node adopts the label that is most common among its neighbors.
    /// </summary>
    /// <param name="graph">Adjacency list representation of the graph.</param>
    /// <returns>A dictionary mapping each node to its community label.</returns>
    public static Dictionary<int, int> DetectCommunities(Dictionary<int, List<int>> graph)
    {
        HashSet<int> allNodes = new HashSet<int>(graph.Keys);
        foreach (var kvp in graph)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
                allNodes.Add(kvp.Value[i]);
        }

        Dictionary<int, int> labels = new Dictionary<int, int>();
        int labelId = 0;
        foreach (int node in allNodes)
        {
            labels[node] = labelId++;
        }

        int maxIterations = allNodes.Count * 10;
        for (int iter = 0; iter < maxIterations; iter++)
        {
            bool changed = false;
            List<int> nodes = new List<int>(allNodes);

            for (int idx = 0; idx < nodes.Count; idx++)
            {
                int node = nodes[idx];
                if (!graph.TryGetValue(node, out List<int>? neighbors) || neighbors.Count == 0)
                    continue;

                Dictionary<int, int> labelCounts = new Dictionary<int, int>();
                for (int i = 0; i < neighbors.Count; i++)
                {
                    int neighborLabel = labels[neighbors[i]];
                    if (!labelCounts.ContainsKey(neighborLabel))
                        labelCounts[neighborLabel] = 0;
                    labelCounts[neighborLabel]++;
                }

                int bestLabel = labels[node];
                int bestCount = 0;
                foreach (var kvp in labelCounts)
                {
                    if (kvp.Value > bestCount)
                    {
                        bestCount = kvp.Value;
                        bestLabel = kvp.Key;
                    }
                    else if (kvp.Value == bestCount && kvp.Key < bestLabel)
                    {
                        bestLabel = kvp.Key;
                    }
                }

                if (bestLabel != labels[node])
                {
                    labels[node] = bestLabel;
                    changed = true;
                }
            }

            if (!changed)
                break;
        }

        Dictionary<int, int> compressedLabels = new Dictionary<int, int>();
        Dictionary<int, int> mapping = new Dictionary<int, int>();
        int nextLabel = 0;
        foreach (int node in allNodes)
        {
            if (!mapping.ContainsKey(labels[node]))
            {
                mapping[labels[node]] = nextLabel++;
            }
            compressedLabels[node] = mapping[labels[node]];
        }

        return compressedLabels;
    }

    /// <summary>
    /// Computes the modularity of a partition of the graph into communities.
    /// </summary>
    /// <param name="graph">Adjacency list representation of the graph.</param>
    /// <param name="communities">A dictionary mapping each node to its community ID.</param>
    /// <returns>The modularity value Q in the range [-0.5, 1.0].</returns>
    public static double Modularity(Dictionary<int, List<int>> graph, Dictionary<int, int> communities)
    {
        double m = 0.0;
        Dictionary<int, int> degree = new Dictionary<int, int>();

        foreach (var kvp in graph)
        {
            if (!degree.ContainsKey(kvp.Key))
                degree[kvp.Key] = 0;
            degree[kvp.Key] += kvp.Value.Count;
            m += kvp.Value.Count;

            for (int i = 0; i < kvp.Value.Count; i++)
            {
                if (!degree.ContainsKey(kvp.Value[i]))
                    degree[kvp.Value[i]] = 0;
            }
        }

        if (m < 1e-14)
            return 0.0;

        double Q = 0.0;

        foreach (var kvp in graph)
        {
            int i = kvp.Key;
            if (!communities.ContainsKey(i))
                continue;

            for (int j = 0; j < kvp.Value.Count; j++)
            {
                int neighbor = kvp.Value[j];
                if (!communities.ContainsKey(neighbor))
                    continue;

                double aij = 1.0;
                double ki = degree.ContainsKey(i) ? degree[i] : 0.0;
                double kj = degree.ContainsKey(neighbor) ? degree[neighbor] : 0.0;

                double delta = communities[i] == communities[neighbor] ? 1.0 : 0.0;
                Q += aij - (ki * kj) / m;
            }
        }

        return Q / m;
    }
}
