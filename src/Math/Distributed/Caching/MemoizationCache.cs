namespace MathVerse.Math.Distributed.Caching;

using System.Collections.Concurrent;
using System.Diagnostics;

/// <summary>Provides generic memoization with LRU eviction for function results.</summary>
public sealed class MemoizationCache
{
    /// <summary>Creates a memoized version of the specified function with LRU eviction.</summary>
    /// <typeparam name="TArg">The type of the function argument.</typeparam>
    /// <typeparam name="TResult">The type of the function result.</typeparam>
    /// <param name="func">The function to memoize.</param>
    /// <param name="maxSize">The maximum number of cached results.</param>
    /// <returns>A memoized function wrapper.</returns>
    public MemoizedFunction<TArg, TResult> Memoize<TArg, TResult>(Func<TArg, TResult> func, int maxSize = 256)
        where TArg : notnull
    {
        return new MemoizedFunction<TArg, TResult>(func, maxSize);
    }
}

/// <summary>A memoized function wrapper that caches results with LRU eviction.</summary>
/// <typeparam name="TArg">The type of the function argument.</typeparam>
/// <typeparam name="TResult">The type of the function result.</typeparam>
public sealed class MemoizedFunction<TArg, TResult>
    where TArg : notnull
{
    private readonly Func<TArg, TResult> _func;
    private readonly int _maxSize;
    private readonly ConcurrentDictionary<TArg, CacheEntry> _cache = new();
    private long _hits;
    private long _misses;

    /// <summary>Represents a cached result entry with access timestamp.</summary>
    private sealed class CacheEntry
    {
        /// <summary>The cached result value.</summary>
        public TResult Value { get; init; } = default!;

        /// <summary>The timestamp of the last access.</summary>
        public long LastAccessTimestamp { get; set; } = Stopwatch.GetTimestamp();
    }

    /// <summary>Initializes a new instance of the <see cref="MemoizedFunction{TArg, TResult}"/> class.</summary>
    /// <param name="func">The function to memoize.</param>
    /// <param name="maxSize">The maximum number of cached results.</param>
    public MemoizedFunction(Func<TArg, TResult> func, int maxSize)
    {
        _func = func;
        _maxSize = maxSize > 0 ? maxSize : 256;
    }

    /// <summary>Gets the current number of cached entries.</summary>
    public int Count => _cache.Count;

    /// <summary>Gets the total number of cache hits.</summary>
    public long Hits => Interlocked.Read(ref _hits);

    /// <summary>Gets the total number of cache misses.</summary>
    public long Misses => Interlocked.Read(ref _misses);

    /// <summary>Invokes the memoized function, returning a cached result if available.</summary>
    /// <param name="arg">The function argument.</param>
    /// <returns>The cached or freshly computed result.</returns>
    public TResult Invoke(TArg arg)
    {
        if (_cache.TryGetValue(arg, out var entry))
        {
            Interlocked.Increment(ref _hits);
            entry.LastAccessTimestamp = Stopwatch.GetTimestamp();
            return entry.Value;
        }

        Interlocked.Increment(ref _misses);

        if (_cache.Count >= _maxSize)
        {
            EvictLru(1);
        }

        TResult result = _func(arg);
        _cache[arg] = new CacheEntry { Value = result };
        return result;
    }

    /// <summary>Invalidates the cached result for the specified argument.</summary>
    /// <param name="arg">The argument whose cached result should be removed.</param>
    /// <returns>True if the entry was found and removed.</returns>
    public bool Invalidate(TArg arg)
    {
        return _cache.TryRemove(arg, out _);
    }

    /// <summary>Clears all cached results.</summary>
    public void Clear()
    {
        _cache.Clear();
        Interlocked.Exchange(ref _hits, 0);
        Interlocked.Exchange(ref _misses, 0);
    }

    /// <summary>Returns the cache hit rate as a value between 0 and 1.</summary>
    /// <returns>The hit rate, or 0 if no invocations have been made.</returns>
    public double GetHitRate()
    {
        long total = Interlocked.Read(ref _hits) + Interlocked.Read(ref _misses);
        if (total == 0)
        {
            return 0.0;
        }
        return (double)Interlocked.Read(ref _hits) / (double)total;
    }

    /// <summary>Returns whether the cache contains a result for the specified argument.</summary>
    /// <param name="arg">The argument to check.</param>
    /// <returns>True if a cached result exists.</returns>
    public bool Contains(TArg arg)
    {
        return _cache.ContainsKey(arg);
    }

    private void EvictLru(int count)
    {
        var keysToEvict = _cache
            .OrderBy(kvp => kvp.Value.LastAccessTimestamp)
            .Take(count)
            .Select(kvp => kvp.Key)
            .ToArray();

        foreach (var key in keysToEvict)
        {
            _cache.TryRemove(key, out _);
        }
    }
}
