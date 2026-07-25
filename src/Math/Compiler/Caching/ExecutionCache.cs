namespace MathVerse.Math.Compiler.Caching;

using System;
using System.Collections.Concurrent;

/// <summary>Caches execution results for pure functions with known inputs.
/// Thread-safe; supports invalidation of dependent entries.</summary>
public sealed class ExecutionCache
{
    private readonly ConcurrentDictionary<string, ExecutionCacheEntry> _cache = new();

    /// <summary>Event raised when a cache entry is invalidated.</summary>
    public event Action<string>? EntryInvalidated;

    /// <summary>Gets a cached execution result or executes and caches a new one.</summary>
    /// <param name="key">The unique cache key composed of function name and inputs hash.</param>
    /// <param name="execute">The function to execute if not cached.</param>
    /// <param name="inputs">The input array to the function.</param>
    /// <returns>The execution result.</returns>
    public double GetOrExecute(string key, Func<double[], double> execute, double[] inputs)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (execute is null) throw new ArgumentNullException(nameof(execute));
        if (inputs is null) throw new ArgumentNullException(nameof(inputs));

        var entry = _cache.GetOrAdd(key, _ => new ExecutionCacheEntry(execute(inputs), DateTime.UtcNow));
        return entry.Value;
    }

    /// <summary>Invalidates all entries with keys that start with the given prefix.</summary>
    /// <param name="keyPrefix">The prefix to match against cache keys.</param>
    public void InvalidateDependents(string keyPrefix)
    {
        if (keyPrefix is null) throw new ArgumentNullException(nameof(keyPrefix));
        foreach (var kv in _cache)
        {
            if (kv.Key.StartsWith(keyPrefix, StringComparison.Ordinal))
            {
                _cache.TryRemove(kv.Key, out _);
                EntryInvalidated?.Invoke(kv.Key);
            }
        }
    }

    /// <summary>Clears all entries from the cache.</summary>
    public void Clear()
    {
        _cache.Clear();
    }

    /// <summary>Gets the number of cached entries.</summary>
    public int Count => _cache.Count;

    /// <summary>Entry stored in the execution cache.</summary>
    private sealed class ExecutionCacheEntry
    {
        public double Value { get; }
        public DateTime Timestamp { get; }

        public ExecutionCacheEntry(double value, DateTime timestamp)
        {
            Value = value;
            Timestamp = timestamp;
        }
    }
}
