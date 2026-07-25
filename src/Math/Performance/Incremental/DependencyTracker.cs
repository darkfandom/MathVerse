namespace MathVerse.Math.Performance.Incremental;

/// <summary>
/// Thread-safe dependency graph that tracks relationships between computation nodes
/// and supports dirty propagation.
/// </summary>
public sealed class DependencyTracker
{
    private readonly ConcurrentDictionary<int, DependencyNode> _nodes = new();
    private int _nextId;

    /// <summary>Gets the number of nodes in the graph.</summary>
    public int NodeCount => _nodes.Count;

    /// <summary>Adds a new node with the specified name and returns its identifier.</summary>
    /// <param name="name">The human-readable name for the node.</param>
    /// <returns>The unique identifier assigned to the new node.</returns>
    public int AddNode(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var id = Interlocked.Increment(ref _nextId);
        var node = new DependencyNode(id, name);
        _nodes[id] = node;
        return id;
    }

    /// <summary>Establishes a dependency edge: <paramref name="nodeId"/> depends on <paramref name="dependsOnId"/>.</summary>
    /// <param name="nodeId">The dependent node identifier.</param>
    /// <param name="dependsOnId">The dependency node identifier.</param>
    public void AddDependency(int nodeId, int dependsOnId)
    {
        if (!_nodes.TryGetValue(nodeId, out var node))
            throw new ArgumentException($"Node {nodeId} not found.", nameof(nodeId));

        if (!_nodes.TryGetValue(dependsOnId, out var dependency))
            throw new ArgumentException($"Node {dependsOnId} not found.", nameof(dependsOnId));

        node.AddDependency(dependsOnId);
        dependency.AddDependent(nodeId);
    }

    /// <summary>Marks the specified node and all its transitive dependents as dirty.</summary>
    /// <param name="nodeId">The identifier of the node that changed.</param>
    public void MarkDirty(int nodeId)
    {
        if (!_nodes.TryGetValue(nodeId, out var node))
            return;

        var visited = new HashSet<int>();
        var queue = new Queue<int>();
        queue.Enqueue(nodeId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            if (!visited.Add(currentId))
                continue;

            if (_nodes.TryGetValue(currentId, out var currentNode))
            {
                currentNode.MarkDirty();

                foreach (var dependentId in currentNode.Dependents)
                    queue.Enqueue(dependentId);
            }
        }
    }

    /// <summary>Returns all nodes currently marked as dirty.</summary>
    /// <returns>A list of dirty node identifiers.</returns>
    public IReadOnlyList<int> GetDirtyNodes()
    {
        var dirty = new List<int>();
        foreach (var kvp in _nodes)
        {
            if (kvp.Value.IsDirty)
                dirty.Add(kvp.Key);
        }
        return dirty;
    }

    /// <summary>Marks all nodes as clean.</summary>
    public void MarkAllClean()
    {
        foreach (var kvp in _nodes)
            kvp.Value.MarkClean();
    }

    /// <summary>Gets the node with the specified identifier, or null if not found.</summary>
    /// <param name="nodeId">The node identifier to look up.</param>
    /// <returns>The <see cref="DependencyNode"/> if found; otherwise, null.</returns>
    public DependencyNode? GetNode(int nodeId)
    {
        _nodes.TryGetValue(nodeId, out var node);
        return node;
    }

    /// <summary>Removes the specified node and all its dependency edges.</summary>
    /// <param name="nodeId">The identifier of the node to remove.</param>
    public void RemoveNode(int nodeId)
    {
        if (!_nodes.TryRemove(nodeId, out var node))
            return;

        foreach (var depId in node.Dependencies)
        {
            if (_nodes.TryGetValue(depId, out var dep))
                dep.RemoveDependent(nodeId);
        }

        foreach (var depId in node.Dependents)
        {
            if (_nodes.TryGetValue(depId, out var dep))
                dep.RemoveDependency(nodeId);
        }
    }
}
