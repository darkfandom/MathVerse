namespace MathVerse.Math.AI.GraphIntelligence;
using System;
using System.Collections.Generic;

/// <summary>
/// Computes PageRank scores for nodes in a directed graph using power iteration.
/// Handles dangling nodes by redistributing their rank equally among all nodes.
/// </summary>
public static class PageRank
{
    /// <summary>
    /// Computes PageRank scores for all nodes in the graph.
    /// </summary>
    /// <param name="graph">Directed adjacency list where each key points to its outgoing neighbors.</param>
    /// <param name="iterations">Number of power iteration steps.</param>
    /// <param name="damping">Damping factor (typically 0.85).</param>
    /// <returns>A dictionary mapping each node to its PageRank score.</returns>
    public static Dictionary<int, double> ComputeRank(
        Dictionary<int, List<int>> graph,
        int iterations = 100,
        double damping = 0.85)
    {
        HashSet<int> allNodes = new HashSet<int>(graph.Keys);
        foreach (var kvp in graph)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
                allNodes.Add(kvp.Value[i]);
        }

        int n = allNodes.Count;
        if (n == 0)
            return new Dictionary<int, double>();

        double[] rank = new double[n];
        double[] newRank = new double[n];
        List<int> nodeList = new List<int>(allNodes);
        Dictionary<int, int> nodeIndex = new Dictionary<int, int>();
        for (int i = 0; i < n; i++)
        {
            nodeIndex[nodeList[i]] = i;
            rank[i] = 1.0 / n;
        }

        List<int> danglingNodes = new List<int>();
        for (int i = 0; i < n; i++)
        {
            int node = nodeList[i];
            if (!graph.TryGetValue(node, out List<int>? neighbors) || neighbors.Count == 0)
            {
                danglingNodes.Add(i);
            }
        }

        for (int iter = 0; iter < iterations; iter++)
        {
            double danglingSum = 0.0;
            for (int i = 0; i < danglingNodes.Count; i++)
            {
                danglingSum += rank[danglingNodes[i]];
            }

            for (int i = 0; i < n; i++)
                newRank[i] = (1.0 - damping) / n + damping * danglingSum / n;

            for (int i = 0; i < n; i++)
            {
                int node = nodeList[i];
                if (!graph.TryGetValue(node, out List<int>? neighbors) || neighbors.Count == 0)
                    continue;

                double share = rank[i] / neighbors.Count;
                for (int j = 0; j < neighbors.Count; j++)
                {
                    if (nodeIndex.TryGetValue(neighbors[j], out int idx))
                    {
                        newRank[idx] += damping * share;
                    }
                }
            }

            double[] temp = rank;
            rank = newRank;
            newRank = temp;
        }

        Dictionary<int, double> result = new Dictionary<int, double>();
        for (int i = 0; i < n; i++)
        {
            result[nodeList[i]] = rank[i];
        }

        return result;
    }
}
