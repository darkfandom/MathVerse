namespace MathVerse.Math.AI.GraphIntelligence;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

/// <summary>Result of a dependency graph analysis.</summary>
public sealed class DependencyAnalysisResult
{
    /// <summary>Gets whether any circular dependencies were detected.</summary>
    public bool HasCircularDependencies { get; init; }

    /// <summary>Gets the list of nodes involved in circular dependency chains.</summary>
    public List<List<string>> CircularChains { get; init; } = [];

    /// <summary>Gets the topological ordering of nodes (empty if cycles exist).</summary>
    public List<string> TopologicalOrder { get; init; } = [];

    /// <summary>Gets the depth of each node in the dependency tree (longest path from root).</summary>
    public Dictionary<string, int> NodeDepths { get; init; } = new();

    /// <summary>Gets the total number of nodes in the dependency graph.</summary>
    public int NodeCount { get; init; }

    /// <summary>Gets the total number of edges (dependencies) in the graph.</summary>
    public int EdgeCount { get; init; }

    /// <summary>Gets the maximum depth of the dependency tree.</summary>
    public int MaxDepth { get; init; }

    /// <summary>Gets nodes with no dependencies (roots).</summary>
    public List<string> RootNodes { get; init; } = [];

    /// <summary>Gets nodes with no dependents (leaves).</summary>
    public List<string> LeafNodes { get; init; } = [];

    /// <summary>Gets additional metrics from the analysis.</summary>
    public ImmutableDictionary<string, double> Metrics { get; init; } = ImmutableDictionary<string, double>.Empty;
}

/// <summary>
/// Analyzes dependency graphs for circular dependencies, topological ordering,
/// and depth calculation for evaluation scheduling.
/// </summary>
public static class DependencyGraphAnalysis
{
    /// <summary>
    /// Analyzes a dependency graph to detect cycles, compute topological order,
    /// and calculate node depths.
    /// </summary>
    /// <param name="dependencies">Adjacency list where each key maps to the list of items it depends on.</param>
    /// <returns>A <see cref="DependencyAnalysisResult"/> with the full analysis.</returns>
    public static DependencyAnalysisResult Analyze(Dictionary<string, List<string>> dependencies)
    {
        HashSet<string> allNodes = new HashSet<string>(dependencies.Keys);
        foreach (var kvp in dependencies)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
                allNodes.Add(kvp.Value[i]);
        }

        int edgeCount = 0;
        foreach (var kvp in dependencies)
            edgeCount += kvp.Value.Count;

        List<List<string>> circularChains = DetectCycles(dependencies, allNodes);

        List<string> topoOrder = [];
        if (circularChains.Count == 0)
        {
            topoOrder = ComputeTopologicalOrder(dependencies, allNodes);
        }

        Dictionary<string, int> depths = ComputeDepths(dependencies, allNodes);

        int maxDepth = 0;
        foreach (var kvp in depths)
        {
            if (kvp.Value > maxDepth)
                maxDepth = kvp.Value;
        }

        List<string> rootNodes = new List<string>();
        List<string> leafNodes = new List<string>();
        foreach (string node in allNodes)
        {
            if (!dependencies.ContainsKey(node) || dependencies[node].Count == 0)
            {
                rootNodes.Add(node);
            }

            bool hasDependent = false;
            foreach (var kvp in dependencies)
            {
                if (kvp.Value.Contains(node))
                {
                    hasDependent = true;
                    break;
                }
            }
            if (!hasDependent)
            {
                leafNodes.Add(node);
            }
        }

        return new DependencyAnalysisResult
        {
            HasCircularDependencies = circularChains.Count > 0,
            CircularChains = circularChains,
            TopologicalOrder = topoOrder,
            NodeDepths = depths,
            NodeCount = allNodes.Count,
            EdgeCount = edgeCount,
            MaxDepth = maxDepth,
            RootNodes = rootNodes,
            LeafNodes = leafNodes,
            Metrics = ImmutableDictionary<string, double>.Empty
                .Add("nodeCount", allNodes.Count)
                .Add("edgeCount", edgeCount)
                .Add("maxDepth", maxDepth)
                .Add("rootCount", rootNodes.Count)
                .Add("leafCount", leafNodes.Count)
                .Add("hasCycles", circularChains.Count > 0 ? 1.0 : 0.0)
        };
    }

    private static List<List<string>> DetectCycles(
        Dictionary<string, List<string>> dependencies, HashSet<string> allNodes)
    {
        List<List<string>> cycles = new List<List<string>>();
        int WHITE = 0, GRAY = 1, BLACK = 2;
        Dictionary<string, int> color = new Dictionary<string, int>();
        Dictionary<string, string?> parent = new Dictionary<string, string?>();
        foreach (string node in allNodes)
        {
            color[node] = WHITE;
            parent[node] = null;
        }

        foreach (string node in allNodes)
        {
            if (color[node] != WHITE)
                continue;

            Stack<(string current, int neighborIdx)> stack = new Stack<(string, int)>();
            stack.Push((node, 0));
            color[node] = GRAY;
            List<string> path = [node];

            while (stack.Count > 0)
            {
                var (current, idx) = stack.Pop();

                if (!dependencies.TryGetValue(current, out List<string>? deps) || idx >= deps.Count)
                {
                    color[current] = BLACK;
                    if (path.Count > 0 && path[^1] == current)
                        path.RemoveAt(path.Count - 1);
                    continue;
                }

                string neighbor = deps[idx];
                stack.Push((current, idx + 1));

                if (!color.ContainsKey(neighbor))
                    color[neighbor] = WHITE;

                if (color[neighbor] == GRAY)
                {
                    List<string> cycle = new List<string>();
                    cycle.Add(neighbor);
                    for (int i = path.Count - 1; i >= 0; i--)
                    {
                        cycle.Add(path[i]);
                        if (path[i] == neighbor)
                            break;
                    }
                    cycle.Reverse();
                    cycles.Add(cycle);
                }
                else if (color[neighbor] == WHITE)
                {
                    color[neighbor] = GRAY;
                    parent[neighbor] = current;
                    path.Add(neighbor);
                    stack.Push((neighbor, 0));
                }
            }
        }

        return cycles;
    }

    private static List<string> ComputeTopologicalOrder(
        Dictionary<string, List<string>> dependencies, HashSet<string> allNodes)
    {
        Dictionary<string, int> inDegree = new Dictionary<string, int>();
        foreach (string node in allNodes)
            inDegree[node] = 0;

        foreach (var kvp in dependencies)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                string dep = kvp.Value[i];
                if (!inDegree.ContainsKey(dep))
                    inDegree[dep] = 0;
            }
            if (!inDegree.ContainsKey(kvp.Key))
                inDegree[kvp.Key] = 0;
        }

        foreach (var kvp in dependencies)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                inDegree[kvp.Key]++;
            }
        }

        Queue<string> queue = new Queue<string>();
        foreach (var kvp in inDegree)
        {
            if (kvp.Value == 0)
                queue.Enqueue(kvp.Key);
        }

        List<string> result = new List<string>();
        while (queue.Count > 0)
        {
            string node = queue.Dequeue();
            result.Add(node);

            if (dependencies.TryGetValue(node, out List<string>? deps))
            {
                foreach (var kvp in dependencies)
                {
                    if (kvp.Value.Contains(node))
                    {
                        inDegree[kvp.Key]--;
                        if (inDegree[kvp.Key] == 0)
                            queue.Enqueue(kvp.Key);
                    }
                }
            }
        }

        return result;
    }

    private static Dictionary<string, int> ComputeDepths(
        Dictionary<string, List<string>> dependencies, HashSet<string> allNodes)
    {
        Dictionary<string, int> depths = new Dictionary<string, int>();
        foreach (string node in allNodes)
            depths[node] = 0;

        Dictionary<string, int> visited = new Dictionary<string, int>();
        foreach (string node in allNodes)
            visited[node] = 0;

        foreach (string node in allNodes)
        {
            depths[node] = ComputeNodeDepth(node, dependencies, depths, visited);
        }

        return depths;
    }

    private static int ComputeNodeDepth(
        string node,
        Dictionary<string, List<string>> dependencies,
        Dictionary<string, int> depths,
        Dictionary<string, int> visited)
    {
        if (visited[node] == 1)
            return depths[node];

        visited[node] = 1;

        if (!dependencies.TryGetValue(node, out List<string>? deps) || deps.Count == 0)
        {
            depths[node] = 0;
            visited[node] = 2;
            return 0;
        }

        int maxChildDepth = 0;
        for (int i = 0; i < deps.Count; i++)
        {
            string dep = deps[i];
            if (!depths.ContainsKey(dep))
                depths[dep] = 0;

            int childDepth;
            if (visited[dep] == 1)
            {
                childDepth = depths[dep];
            }
            else
            {
                childDepth = ComputeNodeDepth(dep, dependencies, depths, visited);
            }

            if (childDepth > maxChildDepth)
                maxChildDepth = childDepth;
        }

        depths[node] = maxChildDepth + 1;
        visited[node] = 2;
        return depths[node];
    }
}
