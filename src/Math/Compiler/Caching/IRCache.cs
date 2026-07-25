namespace MathVerse.Math.Compiler.Caching;

using System;
using System.Collections.Concurrent;
using IRModule = MathVerse.Math.Compiler.IR.IRModule;

/// <summary>Caches IRModule transformations. Provides memoization for repeated transforms.</summary>
public sealed class IRCache
{
    private readonly ConcurrentDictionary<string, IRModule> _cache = new();

    /// <summary>Gets a cached transform result or computes and caches it.</summary>
    /// <param name="key">Unique key for this transform and input combination.</param>
    /// <param name="transform">The transformation function.</param>
    /// <param name="input">The input IR module to transform.</param>
    /// <returns>The IRModule from cache or the newly computed result.</returns>
    public IRModule GetOrTransform(string key, Func<IRModule, IRModule> transform, IRModule input)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (transform is null) throw new ArgumentNullException(nameof(transform));
        if (input is null) throw new ArgumentNullException(nameof(input));

        return _cache.GetOrAdd(key, _ => transform(input));
    }

    /// <summary>Attempts to get a cached module for the key.</summary>
    public bool TryGet(string key, out IRModule? module)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        return _cache.TryGetValue(key, out module);
    }

    /// <summary>Removes a specific key from the cache.</summary>
    public void Remove(string key)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        _cache.TryRemove(key, out _);
    }

    /// <summary>Clears all entries from the cache.</summary>
    public void Clear()
    {
        _cache.Clear();
    }
}
