namespace MathVerse.Math.Performance.Caching;

/// <summary>
/// Generic memoization engine that caches computation results to avoid redundant evaluation.
/// Thread-safe and compatible with Native AOT.
/// </summary>
public sealed class MemoizationEngine
{
    private readonly ConcurrentDictionary<string, object?> _cache = new(StringComparer.Ordinal);
    private int _hits;
    private int _misses;

    /// <summary>Memoizes a computation under the specified key, returning the cached result on subsequent calls.</summary>
    /// <typeparam name="TResult">The type of the computation result.</typeparam>
    /// <param name="key">The unique key identifying this computation.</param>
    /// <param name="compute">The computation function to execute on cache miss.</param>
    /// <returns>The cached or freshly computed result.</returns>
    public TResult Memoize<TResult>(string key, Func<TResult> compute)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(compute);

        if (_cache.TryGetValue(key, out var cached) && cached is TResult typedResult)
        {
            Interlocked.Increment(ref _hits);
            return typedResult;
        }

        Interlocked.Increment(ref _misses);
        var result = compute();
        _cache[key] = result;
        return result;
    }

    /// <summary>Memoizes a computation keyed by its argument, returning the cached result on subsequent calls.</summary>
    /// <typeparam name="TArg">The type of the computation argument.</typeparam>
    /// <typeparam name="TResult">The type of the computation result.</typeparam>
    /// <param name="arg">The argument that uniquely identifies this computation.</param>
    /// <param name="compute">The computation function to execute on cache miss.</param>
    /// <returns>The cached or freshly computed result.</returns>
    public TResult Memoize<TArg, TResult>(TArg arg, Func<TArg, TResult> compute)
    {
        ArgumentNullException.ThrowIfNull(compute);

        var key = arg?.GetHashCode().ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "null";

        if (_cache.TryGetValue(key, out var cached) && cached is TResult typedResult)
        {
            Interlocked.Increment(ref _hits);
            return typedResult;
        }

        Interlocked.Increment(ref _misses);
        var result = compute(arg!);
        _cache[key] = result;
        return result;
    }

    /// <summary>Removes the cached result for the specified key.</summary>
    /// <param name="key">The cache key to invalidate.</param>
    public void Invalidate(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        _cache.TryRemove(key, out _);
    }

    /// <summary>Removes all cached results and resets statistics.</summary>
    public void ClearAll()
    {
        _cache.Clear();
        Interlocked.Exchange(ref _hits, 0);
        Interlocked.Exchange(ref _misses, 0);
    }

    /// <summary>Gets the current memoization statistics.</summary>
    public CacheStatistics Statistics => new()
    {
        Hits = Volatile.Read(ref _hits),
        Misses = Volatile.Read(ref _misses),
        Count = _cache.Count,
        Capacity = int.MaxValue
    };
}
