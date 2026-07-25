namespace MathVerse.Math.Performance.Incremental;

/// <summary>
/// Top-level incremental computation engine that coordinates dependency tracking,
/// invalidation propagation, and cached evaluation of mathematical expressions.
/// </summary>
public sealed class IncrementalEngine
{
    private readonly DependencyTracker _tracker = new();
    private readonly InvalidationGraph _invalidationGraph = new();
    private readonly IncrementalEvaluator _evaluator = new();
    private readonly ConcurrentDictionary<Expression, Expression> _resultCache = new();

    /// <summary>Initializes a new incremental engine.</summary>
    public IncrementalEngine()
    {
        _invalidationGraph.SetTracker(_tracker);
    }

    /// <summary>Gets the dependency tracker for this engine.</summary>
    public DependencyTracker Dependencies => _tracker;

    /// <summary>Evaluates the given expression incrementally, returning a cached result if valid.</summary>
    /// <param name="expr">The expression to evaluate.</param>
    /// <returns>The evaluated expression.</returns>
    public Expression Evaluate(Expression expr)
    {
        ArgumentNullException.ThrowIfNull(expr);

        if (_resultCache.TryGetValue(expr, out var cached))
            return cached;

        var result = _evaluator.Evaluate(expr);
        _resultCache[expr] = result;
        return result;
    }

    /// <summary>Processes pending changes and returns a change set describing all affected nodes.</summary>
    /// <returns>A <see cref="ChangeSet"/> describing the changes and their transitive effects.</returns>
    public ChangeSet Update()
    {
        var dirtyNodes = _tracker.GetDirtyNodes();
        if (dirtyNodes.Count == 0)
            return ChangeSet.Empty;

        var changedSet = new HashSet<int>(dirtyNodes);
        var initialChanges = new ChangeSet(changedSet, changedSet);
        var propagated = _invalidationGraph.Propagate(initialChanges);

        foreach (var affectedId in propagated.AffectedNodes)
        {
            var node = _tracker.GetNode(affectedId);
            if (node is not null)
                node.MarkClean();
        }

        return propagated;
    }

    /// <summary>Invalidates the cached result for the specified expression and propagates the invalidation.</summary>
    /// <param name="expr">The expression to invalidate.</param>
    public void Invalidate(Expression expr)
    {
        ArgumentNullException.ThrowIfNull(expr);

        _evaluator.Invalidate(expr);
        _resultCache.TryRemove(expr, out _);
    }

    /// <summary>Resets the engine, clearing all caches, dependencies, and evaluation state.</summary>
    public void Reset()
    {
        _resultCache.Clear();
        _evaluator.InvalidateAll();
    }
}
