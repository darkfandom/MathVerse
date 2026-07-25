namespace MathVerse.Math.Performance.Caching;

/// <summary>
/// Thread-safe cache mapping original expressions to their rewritten equivalents
/// using structural expression equality.
/// </summary>
public sealed class RewriteCache
{
    private readonly ConcurrentDictionary<Expression, Expression> _cache = new();
    private int _hits;
    private int _misses;

    /// <summary>Gets the cached rewritten expression for the given original, or null if not cached.</summary>
    /// <param name="original">The original expression to look up.</param>
    /// <returns>The rewritten expression if cached; otherwise, null.</returns>
    public Expression? GetRewritten(Expression original)
    {
        ArgumentNullException.ThrowIfNull(original);

        if (_cache.TryGetValue(original, out var rewritten))
        {
            Interlocked.Increment(ref _hits);
            return rewritten;
        }

        Interlocked.Increment(ref _misses);
        return null;
    }

    /// <summary>Stores the result of rewriting the given expression.</summary>
    /// <param name="original">The original expression before rewriting.</param>
    /// <param name="rewritten">The rewritten expression result.</param>
    public void Store(Expression original, Expression rewritten)
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(rewritten);

        _cache[original] = rewritten;
    }

    /// <summary>Determines whether the given expression has a cached rewrite result.</summary>
    /// <param name="original">The expression to check.</param>
    /// <returns>True if a rewrite result is cached for this expression.</returns>
    public bool IsCached(Expression original)
    {
        ArgumentNullException.ThrowIfNull(original);
        return _cache.ContainsKey(original);
    }

    /// <summary>Removes all entries from the cache and resets statistics.</summary>
    public void Clear()
    {
        _cache.Clear();
        Interlocked.Exchange(ref _hits, 0);
        Interlocked.Exchange(ref _misses, 0);
    }

    /// <summary>Gets the current cache statistics.</summary>
    public CacheStatistics Statistics => new()
    {
        Hits = Volatile.Read(ref _hits),
        Misses = Volatile.Read(ref _misses),
        Count = _cache.Count,
        Capacity = int.MaxValue
    };
}
