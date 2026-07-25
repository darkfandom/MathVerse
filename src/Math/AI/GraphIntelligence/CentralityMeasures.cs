namespace MathVerse.Math.AI.GraphIntelligence;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Provides graph centrality measures including degree, betweenness, closeness,
/// and eigenvector centrality.
/// </summary>
public static class CentralityMeasures
{
    /// <summary>
    /// Computes the degree centrality for each node in the graph.
    /// Normalizes by dividing each node's degree by (n - 1).
    /// </summary>
    /// <param name="graph">Adjacency list representation of the graph.</param>
    /// <returns>A dictionary mapping each node to its degree centrality.</returns>
    public static Dictionary<int, double> DegreeCentrality(Dictionary<int, List<int>> graph)
    {
        HashSet<int> allNodes = GetAllNodes(graph);
        int n = allNodes.Count;
        double normalization = n > 1 ? 1.0 / (n - 1) : 1.0;

        Dictionary<int, double> centrality = new Dictionary<int, double>();
        foreach (int node in allNodes)
        {
            int degree = 0;
            if (graph.TryGetValue(node, out List<int>? neighbors))
            {
                degree = neighbors.Count;
            }
            centrality[node] = degree * normalization;
        }

        return centrality;
    }

    /// <summary>
    /// Computes betweenness centrality using Brandes' algorithm.
    /// </summary>
    /// <param name="graph">Adjacency list representation of the graph.</param>
    /// <returns>A dictionary mapping each node to its betweenness centrality.</returns>
    public static Dictionary<int, double> BetweennessCentrality(Dictionary<int, List<int>> graph)
    {
        HashSet<int> allNodes = GetAllNodes(graph);
        Dictionary<int, double> centrality = new Dictionary<int, double>();
        foreach (int node in allNodes)
            centrality[node] = 0.0;

        foreach (int s in allNodes)
        {
            Stack<int> stack = new Stack<int>();
            Dictionary<int, List<int>> predecessors = new Dictionary<int, List<int>>();
            Dictionary<int, double> sigma = new Dictionary<int, double>();
            Dictionary<int, double> distance = new Dictionary<int, double>();
            Queue<int> queue = new Queue<int>();

            foreach (int node in allNodes)
            {
                predecessors[node] = [];
                sigma[node] = 0.0;
                distance[node] = -1.0;
            }

            sigma[s] = 1.0;
            distance[s] = 0.0;
            queue.Enqueue(s);

            while (queue.Count > 0)
            {
                int v = queue.Dequeue();
                stack.Push(v);

                if (!graph.TryGetValue(v, out List<int>? neighbors))
                    continue;

                for (int i = 0; i < neighbors.Count; i++)
                {
                    int w = neighbors[i];
                    if (!allNodes.Contains(w))
                        continue;

                    if (distance[w] < 0.0)
                    {
                        distance[w] = distance[v] + 1.0;
                        queue.Enqueue(w);
                    }

                    if (System.Math.Abs(distance[w] - (distance[v] + 1.0)) < 1e-10)
                    {
                        sigma[w] += sigma[v];
                        predecessors[w].Add(v);
                    }
                }
            }

            Dictionary<int, double> delta = new Dictionary<int, double>();
            foreach (int node in allNodes)
                delta[node] = 0.0;

            while (stack.Count > 0)
            {
                int w = stack.Pop();
                for (int i = 0; i < predecessors[w].Count; i++)
                {
                    int v = predecessors[w][i];
                    if (System.Math.Abs(sigma[w]) > 1e-14)
                    {
                        delta[v] += (sigma[v] / sigma[w]) * (1.0 + delta[w]);
                    }
                }
                if (w != s)
                {
                    centrality[w] += delta[w];
                }
            }
        }

        int nodeCount = allNodes.Count;
        if (nodeCount > 2)
        {
            double scale = 1.0 / ((nodeCount - 1.0) * (nodeCount - 2.0));
            List<int> keys = new List<int>(centrality.Keys);
            foreach (int k in keys)
                centrality[k] *= scale;
        }

        return centrality;
    }

    /// <summary>
    /// Computes the closeness centrality for each node in the graph.
    /// </summary>
    /// <param name="graph">Adjacency list representation of the graph.</param>
    /// <returns>A dictionary mapping each node to its closeness centrality.</returns>
    public static Dictionary<int, double> ClosenessCentrality(Dictionary<int, List<int>> graph)
    {
        HashSet<int> allNodes = GetAllNodes(graph);
        Dictionary<int, double> centrality = new Dictionary<int, double>();

        foreach (int node in allNodes)
        {
            double totalDist = 0.0;
            int reachableCount = 0;

            Dictionary<int, double> dist = BFSdistances(graph, node, allNodes);
            foreach (var kvp in dist)
            {
                if (kvp.Key != node && kvp.Value > 0.0)
                {
                    totalDist += kvp.Value;
                    reachableCount++;
                }
            }

            if (reachableCount > 0 && totalDist > 0.0)
            {
                centrality[node] = reachableCount / totalDist;
            }
            else
            {
                centrality[node] = 0.0;
            }
        }

        return centrality;
    }

    /// <summary>
    /// Computes eigenvector centrality using the power iteration method.
    /// </summary>
    /// <param name="graph">Adjacency list representation of the graph.</param>
    /// <param name="iterations">Number of power iteration steps.</param>
    /// <returns>A dictionary mapping each node to its eigenvector centrality.</returns>
    public static Dictionary<int, double> EigenvectorCentrality(Dictionary<int, List<int>> graph, int iterations = 100)
    {
        HashSet<int> allNodes = GetAllNodes(graph);
        List<int> nodeList = new List<int>(allNodes);
        int n = nodeList.Count;

        Dictionary<int, int> nodeIndex = new Dictionary<int, int>();
        for (int i = 0; i < n; i++)
            nodeIndex[nodeList[i]] = i;

        double[] centrality = new double[n];
        for (int i = 0; i < n; i++)
            centrality[i] = 1.0;

        for (int iter = 0; iter < iterations; iter++)
        {
            double[] newCentrality = new double[n];
            for (int i = 0; i < n; i++)
            {
                int node = nodeList[i];
                if (graph.TryGetValue(node, out List<int>? neighbors))
                {
                    for (int j = 0; j < neighbors.Count; j++)
                    {
                        if (nodeIndex.TryGetValue(neighbors[j], out int idx))
                        {
                            newCentrality[i] += centrality[idx];
                        }
                    }
                }
            }

            double norm = 0.0;
            for (int i = 0; i < n; i++)
            {
                norm += newCentrality[i] * newCentrality[i];
            }
            norm = System.Math.Sqrt(norm);

            if (norm > 1e-14)
            {
                for (int i = 0; i < n; i++)
                    newCentrality[i] /= norm;
            }

            centrality = newCentrality;
        }

        Dictionary<int, double> result = new Dictionary<int, double>();
        for (int i = 0; i < n; i++)
        {
            result[nodeList[i]] = centrality[i];
        }

        return result;
    }

    private static HashSet<int> GetAllNodes(Dictionary<int, List<int>> graph)
    {
        HashSet<int> nodes = new HashSet<int>(graph.Keys);
        foreach (var kvp in graph)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
                nodes.Add(kvp.Value[i]);
        }
        return nodes;
    }

    private static Dictionary<int, double> BFSdistances(
        Dictionary<int, List<int>> graph, int source, HashSet<int> allNodes)
    {
        Dictionary<int, double> distances = new Dictionary<int, double>();
        foreach (int node in allNodes)
            distances[node] = -1.0;

        Queue<int> queue = new Queue<int>();
        distances[source] = 0.0;
        queue.Enqueue(source);

        while (queue.Count > 0)
        {
            int node = queue.Dequeue();
            if (graph.TryGetValue(node, out List<int>? neighbors))
            {
                for (int i = 0; i < neighbors.Count; i++)
                {
                    int neighbor = neighbors[i];
                    if (distances.ContainsKey(neighbor) && distances[neighbor] < 0.0)
                    {
                        distances[neighbor] = distances[node] + 1.0;
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        return distances;
    }
}
