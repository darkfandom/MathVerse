namespace MathVerse.Math.Compiler.Caching;

using System;
using System.Collections.Concurrent;
using IRModule = MathVerse.Math.Compiler.IR.IRModule;

/// <summary>Caches optimization results for specific IR pattern hashes, avoiding redundant processing.</summary>
public sealed class OptimizationCache
{
    private readonly ConcurrentDictionary<string, IRModule> _cache = new();

    /// <summary>Caches an optimized module for a given pattern hash.</summary>
    /// <param name="patternHash">Hash key uniquely identifying the IR pattern.</param>
    /// <param name="optimized">The optimized IR module.</param>
    public void CacheResult(string patternHash, IRModule optimized)
    {
        if (patternHash is null) throw new ArgumentNullException(nameof(patternHash));
        if (optimized is null) throw new ArgumentNullException(nameof(optimized));
        _cache[patternHash] = optimized;
    }

    /// <summary>Attempts to get a cached optimized result for a pattern hash.</summary>
    /// <param name="patternHash">Hash key for the IR pattern.</param>
    /// <param name="optimized">The cached optimized module, if found.</param>
    /// <returns>True if an optimized result was found.</returns>
    public bool TryGetOptimized(string patternHash, out IRModule? optimized)
    {
        if (patternHash is null) throw new ArgumentNullException(nameof(patternHash));
        return _cache.TryGetValue(patternHash, out optimized);
    }

    /// <summary>Removes a specific pattern hash from the cache.</summary>
    public void Remove(string patternHash)
    {
        if (patternHash is null) throw new ArgumentNullException(nameof(patternHash));
        _cache.TryRemove(patternHash, out _);
    }

    /// <summary>Clears all entries from the cache.</summary>
    public void Clear()
    {
        _cache.Clear();
    }
}
