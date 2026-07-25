namespace MathVerse.Math.AI.GraphIntelligence;
using System;
using System.Collections.Generic;

/// <summary>
/// Generates simplified node embeddings via random walks and co-occurrence based
/// dimensionality reduction, inspired by DeepWalk/Node2Vec style approaches.
/// </summary>
public static class GraphEmbeddings
{
    /// <summary>
    /// Computes embeddings for all nodes in the graph using random walks and
    /// a simplified SVD-like approach on the co-occurrence matrix.
    /// </summary>
    /// <param name="graph">Adjacency list representation of the graph.</param>
    /// <param name="dimensions">Dimensionality of the output embeddings.</param>
    /// <param name="walksPerNode">Number of random walks to start from each node.</param>
    /// <param name="walkLength">Length of each random walk.</param>
    /// <param name="windowSize">Context window size for co-occurrence counting.</param>
    /// <param name="seed">Random seed for reproducibility.</param>
    /// <returns>A dictionary mapping each node to its embedding vector.</returns>
    public static Dictionary<int, double[]> Embed(
        Dictionary<int, List<int>> graph,
        int dimensions = 64,
        int walksPerNode = 10,
        int walkLength = 40,
        int windowSize = 5,
        int seed = 42)
    {
        Random rng = new Random(seed);

        HashSet<int> allNodes = new HashSet<int>(graph.Keys);
        foreach (var kvp in graph)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
                allNodes.Add(kvp.Value[i]);
        }

        List<int> nodeList = new List<int>(allNodes);
        int n = nodeList.Count;
        Dictionary<int, int> nodeIndex = new Dictionary<int, int>();
        for (int i = 0; i < n; i++)
            nodeIndex[nodeList[i]] = i;

        if (n == 0)
            return new Dictionary<int, double[]>();

        double[][] cooccurrence = new double[n][];
        for (int i = 0; i < n; i++)
            cooccurrence[i] = new double[n];

        for (int nodeIdx = 0; nodeIdx < n; nodeIdx++)
        {
            int startNode = nodeList[nodeIdx];
            for (int w = 0; w < walksPerNode; w++)
            {
                List<int> walk = RandomWalk(graph, startNode, walkLength, rng);
                for (int i = 0; i < walk.Count; i++)
                {
                    if (!nodeIndex.ContainsKey(walk[i]))
                        continue;
                    int ci = nodeIndex[walk[i]];

                    for (int j = i + 1; j < System.Math.Min(i + windowSize + 1, walk.Count); j++)
                    {
                        if (!nodeIndex.ContainsKey(walk[j]))
                            continue;
                        int cj = nodeIndex[walk[j]];
                        double weight = 1.0 / (j - i);
                        cooccurrence[ci][cj] += weight;
                        cooccurrence[cj][ci] += weight;
                    }
                }
            }
        }

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                cooccurrence[i][j] = System.Math.Log(1.0 + cooccurrence[i][j]);
            }
        }

        int effectiveDim = System.Math.Min(dimensions, n);
        double[][] embeddings = new double[n][];
        for (int i = 0; i < n; i++)
            embeddings[i] = new double[effectiveDim];

        double[] singularValues = new double[effectiveDim];
        for (int d = 0; d < effectiveDim; d++)
        {
            double[] u = new double[n];
            double[] v = new double[n];
            for (int i = 0; i < n; i++)
            {
                u[i] = rng.NextDouble() - 0.5;
            }

            for (int powerIter = 0; powerIter < 20; powerIter++)
            {
                for (int i = 0; i < n; i++)
                {
                    v[i] = 0.0;
                    for (int j = 0; j < n; j++)
                        v[i] += cooccurrence[i][j] * u[j];
                }

                for (int i = 0; i < n; i++)
                {
                    u[i] = 0.0;
                    for (int j = 0; j < n; j++)
                        u[i] += cooccurrence[j][i] * v[j];
                }
            }

            double normU = 0.0;
            for (int i = 0; i < n; i++)
                normU += u[i] * u[i];
            normU = System.Math.Sqrt(normU);

            double sigma = 0.0;
            if (normU > 1e-14)
            {
                for (int i = 0; i < n; i++)
                    u[i] /= normU;

                for (int i = 0; i < n; i++)
                {
                    double val = 0.0;
                    for (int j = 0; j < n; j++)
                        val += cooccurrence[i][j] * u[j];
                    sigma += val * val;
                }
                sigma = System.Math.Sqrt(sigma);
            }

            singularValues[d] = sigma;

            for (int i = 0; i < n; i++)
                embeddings[i][d] = u[d < u.Length ? d : 0] * System.Math.Sqrt(sigma > 0.0 ? sigma : 1e-10);

            if (sigma > 1e-14)
            {
                double[][] residual = new double[n][];
                for (int i = 0; i < n; i++)
                {
                    residual[i] = new double[n];
                    for (int j = 0; j < n; j++)
                        residual[i][j] = cooccurrence[i][j] - sigma * u[i] * u[j];
                }
                cooccurrence = residual;
            }
        }

        Dictionary<int, double[]> result = new Dictionary<int, double[]>();
        for (int i = 0; i < n; i++)
        {
            result[nodeList[i]] = embeddings[i];
        }

        return result;
    }

    private static List<int> RandomWalk(
        Dictionary<int, List<int>> graph,
        int startNode,
        int length,
        Random rng)
    {
        List<int> walk = [startNode];
        int current = startNode;

        for (int step = 0; step < length - 1; step++)
        {
            if (!graph.TryGetValue(current, out List<int>? neighbors) || neighbors.Count == 0)
                break;

            int nextIdx = rng.Next(neighbors.Count);
            current = neighbors[nextIdx];
            walk.Add(current);
        }

        return walk;
    }
}
