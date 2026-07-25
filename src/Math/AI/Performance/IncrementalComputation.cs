namespace MathVerse.Math.AI.Performance;

using System.Collections.Concurrent;

/// <summary>Tracks dirty flags for incremental computation, enabling recomputation only when inputs actually change.</summary>
public sealed class IncrementalComputation
{
    private readonly ConcurrentDictionary<string, DirtyTracker> _trackers = new(StringComparer.OrdinalIgnoreCase);
    private long _recomputeCount;
    private long _skippedCount;

    /// <summary>Gets the total number of recomputations triggered.</summary>
    public long RecomputeCount => Volatile.Read(ref _recomputeCount);

    /// <summary>Gets the total number of recomputations skipped due to clean state.</summary>
    public long SkippedCount => Volatile.Read(ref _skippedCount);

    /// <summary>Gets the number of tracked computation nodes.</summary>
    public int TrackedCount => _trackers.Count;

    /// <summary>Marks a computation node as dirty, indicating its output needs recomputation.</summary>
    /// <param name="nodeId">The unique identifier for the computation node.</param>
    public void MarkDirty(string nodeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        var tracker = _trackers.GetOrAdd(nodeId, _ => new DirtyTracker());
        tracker.IsDirty = true;
    }

    /// <summary>Marks a computation node and all its dependents as dirty.</summary>
    /// <param name="nodeId">The node whose dependents should also be marked dirty.</param>
    /// <param name="dependencies">A mapping from node ID to its list of dependent node IDs.</param>
    public void MarkDirtyWithDependents(string nodeId, ImmutableDictionary<string, List<string>> dependencies)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        MarkDirty(nodeId);

        if (dependencies.TryGetValue(nodeId, out var dependents))
        {
            foreach (var dependent in dependents)
            {
                MarkDirty(dependent);
            }
        }
    }

    /// <summary>Checks whether a computation node needs recomputation.</summary>
    /// <param name="nodeId">The computation node identifier.</param>
    /// <returns>true if the node is dirty and needs recomputation; false if it can be skipped.</returns>
    public bool NeedsRecompute(string nodeId)
    {
        if (_trackers.TryGetValue(nodeId, out var tracker))
        {
            return tracker.IsDirty;
        }

        return true;
    }

    /// <summary>Executes a computation only if the node is dirty, then marks it clean.</summary>
    /// <param name="nodeId">The computation node identifier.</param>
    /// <param name="compute">The computation function to execute if dirty.</param>
    /// <returns>The result of the computation, or the cached value if clean.</returns>
    public T ComputeIfDirty<T>(string nodeId, Func<T> compute)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);
        ArgumentNullException.ThrowIfNull(compute);

        var tracker = _trackers.GetOrAdd(nodeId, _ => new DirtyTracker());

        if (!tracker.IsDirty && tracker.CachedValue is T cached)
        {
            Interlocked.Increment(ref _skippedCount);
            return cached;
        }

        Interlocked.Increment(ref _recomputeCount);
        T result = compute();
        tracker.CachedValue = result;
        tracker.IsDirty = false;
        return result;
    }

    /// <summary>Registers an input dependency for a computation node.</summary>
    /// <param name="nodeId">The computation node identifier.</param>
    /// <param name="inputId">The input identifier that this node depends on.</param>
    public void RegisterInput(string nodeId, string inputId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(inputId);

        var tracker = _trackers.GetOrAdd(nodeId, _ => new DirtyTracker());
        lock (tracker.InputLock)
        {
            tracker.Inputs.Add(inputId);
        }
    }

    /// <summary>Propagates dirty flags from changed inputs to all dependent computation nodes.</summary>
    /// <param name="changedInputIds">The identifiers of inputs that have changed.</param>
    public void PropagateDirty(IEnumerable<string> changedInputIds)
    {
        foreach (var inputId in changedInputIds)
        {
            foreach (var kvp in _trackers)
            {
                lock (kvp.Value.InputLock)
                {
                    if (kvp.Value.Inputs.Contains(inputId))
                    {
                        kvp.Value.IsDirty = true;
                    }
                }
            }
        }
    }

    /// <summary>Gets the list of node identifiers that are currently dirty.</summary>
    /// <returns>A list of dirty node identifiers.</returns>
    public List<string> GetDirtyNodes()
    {
        var dirty = new List<string>();
        foreach (var kvp in _trackers)
        {
            if (kvp.Value.IsDirty)
            {
                dirty.Add(kvp.Key);
            }
        }
        return dirty;
    }

    /// <summary>Resets all tracking state, marking all nodes as dirty.</summary>
    public void ResetAll()
    {
        foreach (var kvp in _trackers)
        {
            kvp.Value.IsDirty = true;
            kvp.Value.CachedValue = null;
        }

        Interlocked.Exchange(ref _recomputeCount, 0);
        Interlocked.Exchange(ref _skippedCount, 0);
    }

    /// <summary>Clears all tracked computation nodes.</summary>
    public void Clear()
    {
        _trackers.Clear();
        Interlocked.Exchange(ref _recomputeCount, 0);
        Interlocked.Exchange(ref _skippedCount, 0);
    }

    /// <summary>Internal dirty tracker for a single computation node.</summary>
    private sealed class DirtyTracker
    {
        /// <summary>Gets or sets whether the node needs recomputation.</summary>
        public volatile bool IsDirty = true;

        /// <summary>Gets or sets the cached result value.</summary>
        public object? CachedValue { get; set; }

        /// <summary>Gets the set of input identifiers this node depends on.</summary>
        public HashSet<string> Inputs { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Gets the lock object for thread-safe input access.</summary>
        public object InputLock { get; } = new();
    }
}
