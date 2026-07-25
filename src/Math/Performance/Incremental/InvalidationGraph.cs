namespace MathVerse.Math.Performance.Incremental;

/// <summary>
/// Computes the transitive closure of invalidations through a dependency graph using BFS.
/// </summary>
public sealed class InvalidationGraph
{
    private DependencyTracker? _tracker;

    /// <summary>Sets the dependency tracker used for invalidation propagation.</summary>
    /// <param name="tracker">The dependency tracker instance.</param>
    public void SetTracker(DependencyTracker tracker)
    {
        _tracker = tracker ?? throw new ArgumentNullException(nameof(tracker));
    }

    /// <summary>Propagates initial changes through the dependency graph, returning all affected nodes.</summary>
    /// <param name="initialChanges">The initial set of directly changed nodes.</param>
    /// <returns>A <see cref="ChangeSet"/> containing both the original changes and all transitively affected nodes.</returns>
    public ChangeSet Propagate(ChangeSet initialChanges)
    {
        ArgumentNullException.ThrowIfNull(initialChanges);

        if (_tracker is null)
            throw new InvalidOperationException("Dependency tracker has not been set. Call SetTracker first.");

        var affected = new HashSet<int>(initialChanges.ChangedNodes);
        var queue = new Queue<int>();

        foreach (var nodeId in initialChanges.ChangedNodes)
            queue.Enqueue(nodeId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            var node = _tracker.GetNode(currentId);

            if (node is null)
                continue;

            foreach (var dependentId in node.Dependents)
            {
                if (affected.Add(dependentId))
                {
                    _tracker.MarkDirty(dependentId);
                    queue.Enqueue(dependentId);
                }
            }
        }

        return new ChangeSet(initialChanges.ChangedNodes, affected);
    }
}
