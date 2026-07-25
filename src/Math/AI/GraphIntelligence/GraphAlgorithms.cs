namespace MathVerse.Math.AI.GraphIntelligence;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Provides core graph algorithms including traversal, connectivity analysis,
/// topological ordering, cycle detection, and shortest path computation.
/// </summary>
public static class GraphAlgorithms
{
    /// <summary>
    /// Performs breadth-first search starting from the given node.
    /// </summary>
    /// <param name="graph">Adjacency list representation of the graph.</param>
    /// <param name="start">The starting node.</param>
    /// <returns>A list of nodes in BFS traversal order.</returns>
    public static List<int> BFS(Dictionary<int, List<int>> graph, int start)
    {
        List<int> visited = [];
        HashSet<int> seen = new HashSet<int>();
        Queue<int> queue = new Queue<int>();

        queue.Enqueue(start);
        seen.Add(start);

        while (queue.Count > 0)
        {
            int node = queue.Dequeue();
            visited.Add(node);

            if (!graph.TryGetValue(node, out List<int>? neighbors))
                continue;

            for (int i = 0; i < neighbors.Count; i++)
            {
                int neighbor = neighbors[i];
                if (seen.Add(neighbor))
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        return visited;
    }

    /// <summary>
    /// Performs depth-first search starting from the given node.
    /// </summary>
    /// <param name="graph">Adjacency list representation of the graph.</param>
    /// <param name="start">The starting node.</param>
    /// <returns>A list of nodes in DFS traversal order.</returns>
    public static List<int> DFS(Dictionary<int, List<int>> graph, int start)
    {
        List<int> visited = [];
        HashSet<int> seen = new HashSet<int>();
        Stack<int> stack = new Stack<int>();

        stack.Push(start);

        while (stack.Count > 0)
        {
            int node = stack.Pop();
            if (!seen.Add(node))
                continue;

            visited.Add(node);

            if (!graph.TryGetValue(node, out List<int>? neighbors))
                continue;

            for (int i = neighbors.Count - 1; i >= 0; i--)
            {
                if (!seen.Contains(neighbors[i]))
                {
                    stack.Push(neighbors[i]);
                }
            }
        }

        return visited;
    }

    /// <summary>
    /// Finds all connected components in the graph.
    /// </summary>
    /// <param name="graph">Adjacency list representation of the graph.</param>
    /// <returns>A dictionary mapping each node to its component ID.</returns>
    public static Dictionary<int, int> ConnectedComponents(Dictionary<int, List<int>> graph)
    {
        Dictionary<int, int> components = new Dictionary<int, int>();
        int componentId = 0;
        HashSet<int> visited = new HashSet<int>();

        List<int> allNodes = new List<int>(graph.Keys);
        foreach (var kvp in graph)
        {
            foreach (int n in kvp.Value)
            {
                if (!allNodes.Contains(n))
                    allNodes.Add(n);
            }
        }

        foreach (int node in allNodes)
        {
            if (visited.Contains(node))
                continue;

            Queue<int> queue = new Queue<int>();
            queue.Enqueue(node);
            visited.Add(node);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                components[current] = componentId;

                if (graph.TryGetValue(current, out List<int>? neighbors))
                {
                    for (int i = 0; i < neighbors.Count; i++)
                    {
                        if (visited.Add(neighbors[i]))
                        {
                            queue.Enqueue(neighbors[i]);
                        }
                    }
                }
            }
            componentId++;
        }

        return components;
    }

    /// <summary>
    /// Computes a topological ordering of the graph using Kahn's algorithm.
    /// </summary>
    /// <param name="graph">Adjacency list representation of a directed acyclic graph.</param>
    /// <returns>A list of nodes in topological order.</returns>
    public static List<int> TopologicalSort(Dictionary<int, List<int>> graph)
    {
        Dictionary<int, int> inDegree = new Dictionary<int, int>();

        foreach (var kvp in graph)
        {
            if (!inDegree.ContainsKey(kvp.Key))
                inDegree[kvp.Key] = 0;

            for (int i = 0; i < kvp.Value.Count; i++)
            {
                if (!inDegree.ContainsKey(kvp.Value[i]))
                    inDegree[kvp.Value[i]] = 0;
                inDegree[kvp.Value[i]]++;
            }
        }

        Queue<int> queue = new Queue<int>();
        foreach (var kvp in inDegree)
        {
            if (kvp.Value == 0)
                queue.Enqueue(kvp.Key);
        }

        List<int> result = [];
        while (queue.Count > 0)
        {
            int node = queue.Dequeue();
            result.Add(node);

            if (graph.TryGetValue(node, out List<int>? neighbors))
            {
                for (int i = 0; i < neighbors.Count; i++)
                {
                    inDegree[neighbors[i]]--;
                    if (inDegree[neighbors[i]] == 0)
                        queue.Enqueue(neighbors[i]);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Determines whether the graph contains a cycle.
    /// </summary>
    /// <param name="graph">Adjacency list representation of the graph.</param>
    /// <returns>True if a cycle exists; otherwise false.</returns>
    public static bool HasCycle(Dictionary<int, List<int>> graph)
    {
        HashSet<int> allNodes = new HashSet<int>(graph.Keys);
        foreach (var kvp in graph)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
                allNodes.Add(kvp.Value[i]);
        }

        int WHITE = 0, GRAY = 1, BLACK = 2;
        Dictionary<int, int> color = new Dictionary<int, int>();
        foreach (int node in allNodes)
            color[node] = WHITE;

        foreach (int node in allNodes)
        {
            if (color[node] != WHITE)
                continue;

            Stack<(int node, int neighborIndex)> stack = new Stack<(int, int)>();
            stack.Push((node, 0));
            color[node] = GRAY;

            while (stack.Count > 0)
            {
                var (current, idx) = stack.Pop();

                if (!graph.TryGetValue(current, out List<int>? neighbors))
                {
                    color[current] = BLACK;
                    continue;
                }

                if (idx < neighbors.Count)
                {
                    stack.Push((current, idx + 1));
                    int neighbor = neighbors[idx];

                    if (!color.ContainsKey(neighbor))
                        color[neighbor] = WHITE;

                    if (color[neighbor] == GRAY)
                        return true;

                    if (color[neighbor] == WHITE)
                    {
                        color[neighbor] = GRAY;
                        stack.Push((neighbor, 0));
                    }
                }
                else
                {
                    color[current] = BLACK;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Computes shortest paths from the source node to all reachable nodes using Dijkstra's algorithm.
    /// </summary>
    /// <param name="graph">Weighted adjacency list where each entry maps a neighbor to its edge weight.</param>
    /// <param name="source">The source node.</param>
    /// <returns>A dictionary mapping each reachable node to its shortest distance from the source.</returns>
    public static Dictionary<int, double> ShortestPaths(
        Dictionary<int, List<(int neighbor, double weight)>> graph,
        int source)
    {
        Dictionary<int, double> distances = new Dictionary<int, double>();
        HashSet<int> visited = new HashSet<int>();

        PriorityQueue<(int node, double dist), double> pq = new PriorityQueue<(int, double), double>();

        distances[source] = 0.0;
        pq.Enqueue((source, 0.0), 0.0);

        while (pq.Count > 0)
        {
            var (node, dist) = pq.Dequeue();

            if (!visited.Add(node))
                continue;

            if (!graph.TryGetValue(node, out List<(int neighbor, double weight)>? neighbors))
                continue;

            for (int i = 0; i < neighbors.Count; i++)
            {
                int neighbor = neighbors[i].neighbor;
                double weight = neighbors[i].weight;
                double newDist = dist + weight;

                if (!distances.TryGetValue(neighbor, out double currentDist) || newDist < currentDist)
                {
                    distances[neighbor] = newDist;
                    pq.Enqueue((neighbor, newDist), newDist);
                }
            }
        }

        return distances;
    }
}
