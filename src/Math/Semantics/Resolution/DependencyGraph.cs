namespace MathVerse.Math.Semantics.Resolution;

/// <summary>
/// Detects circular dependencies between symbols using DFS-based cycle detection.
/// </summary>
public sealed class DependencyGraph
{
    private readonly Dictionary<string, HashSet<string>> _edges = new(StringComparer.Ordinal);

    /// <summary>Adds a dependency edge (from depends on to).</summary>
    public void AddDependency(string from, string to)
    {
        if (!_edges.TryGetValue(from, out var deps))
        {
            deps = [];
            _edges[from] = deps;
        }
        deps.Add(to);
    }

    /// <summary>Detects whether there are any circular dependencies.</summary>
    public bool HasCycles() => FindCycles().Count > 0;

    /// <summary>Finds all circular dependencies.</summary>
    public IReadOnlyList<IReadOnlyList<string>> FindCycles()
    {
        var cycles = new List<List<string>>();
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var inStack = new HashSet<string>(StringComparer.Ordinal);
        var path = new List<string>();

        foreach (var node in _edges.Keys)
        {
            if (!visited.Contains(node))
                Dfs(node, visited, inStack, path, cycles);
        }

        return cycles;
    }

    /// <summary>Gets the direct dependencies of a symbol.</summary>
    public IReadOnlySet<string> GetDependencies(string symbol) =>
        _edges.TryGetValue(symbol, out var deps) ? deps : new HashSet<string>(StringComparer.Ordinal);

    /// <summary>Gets the transitive dependencies of a symbol.</summary>
    public IReadOnlySet<string> GetTransitiveDependencies(string symbol)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        var queue = new Queue<string>();
        foreach (var dep in GetDependencies(symbol))
            queue.Enqueue(dep);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (result.Add(current))
            {
                foreach (var next in GetDependencies(current))
                    queue.Enqueue(next);
            }
        }
        return result;
    }

    /// <summary>Gets all symbols with no outgoing dependencies.</summary>
    public IReadOnlyList<string> GetLeafNodes()
    {
        var allNodes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var kv in _edges)
        {
            allNodes.Add(kv.Key);
            foreach (var target in kv.Value)
                allNodes.Add(target);
        }
        return allNodes.Where(n => !_edges.ContainsKey(n) || _edges[n].Count == 0).ToList();
    }

    private void Dfs(string node, HashSet<string> visited,
        HashSet<string> inStack, List<string> path, List<List<string>> cycles)
    {
        visited.Add(node);
        inStack.Add(node);
        path.Add(node);

        if (_edges.TryGetValue(node, out var deps))
        {
            foreach (var dep in deps)
            {
                if (!visited.Contains(dep))
                    Dfs(dep, visited, inStack, path, cycles);
                else if (inStack.Contains(dep))
                {
                    int idx = path.IndexOf(dep);
                    if (idx >= 0)
                        cycles.Add(path.Skip(idx).ToList());
                }
            }
        }

        path.RemoveAt(path.Count - 1);
        inStack.Remove(node);
    }
}
