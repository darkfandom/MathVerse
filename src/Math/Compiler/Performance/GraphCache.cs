namespace MathVerse.Math.Compiler.Performance;

using System;
using System.Collections.Concurrent;
using ComputationGraph = MathVerse.Math.Compiler.Graph.ComputationGraph;

/// <summary>Caches computation graph structures for reuse across compilations.</summary>
public sealed class GraphCache
{
    private readonly ConcurrentDictionary<string, ComputationGraph> _cache = new();

    /// <summary>Gets an existing <see cref="ComputationGraph"/> or builds and caches a new one.</summary>
    /// <param name="key">The unique key for the graph.</param>
    /// <param name="builder">Factory to build the graph if not cached.</param>
    /// <returns>The cached or newly built computation graph.</returns>
    public ComputationGraph GetOrBuild(string key, Func<ComputationGraph> builder)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (builder is null) throw new ArgumentNullException(nameof(builder));

        return _cache.GetOrAdd(key, _ => builder());
    }

    /// <summary>Attempts to retrieve a cached graph.</summary>
    public bool TryGet(string key, out ComputationGraph? graph)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        return _cache.TryGetValue(key, out graph);
    }

    /// <summary>Removes a specific graph from the cache.</summary>
    public void Remove(string key)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        _cache.TryRemove(key, out _);
    }

    /// <summary>Clears all cached graphs.</summary>
    public void Clear()
    {
        _cache.Clear();
    }

    /// <summary>Gets the number of cached graphs.</summary>
    public int Count => _cache.Count;
}
