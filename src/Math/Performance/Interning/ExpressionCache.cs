namespace MathVerse.Math.Performance.Interning;

/// <summary>
/// Thread-safe cache for interned expressions using structural equality.
/// </summary>
public sealed class ExpressionCache
{
    private readonly ConcurrentDictionary<Expression, Expression> _cache = new();
    private int _hits;
    private int _misses;

    /// <summary>
    /// Gets the number of unique expressions in the cache.
    /// </summary>
    public int Count => _cache.Count;

    /// <summary>
    /// Attempts to retrieve a cached expression structurally equal to the given one.
    /// </summary>
    /// <param name="expression">The expression to look up.</param>
    /// <param name="interned">The interned expression if found.</param>
    /// <returns>True if a matching expression was found.</returns>
    public bool TryGet(Expression expression, out Expression? interned)
    {
        ArgumentNullException.ThrowIfNull(expression);

        if (_cache.TryGetValue(expression, out var cached))
        {
            Interlocked.Increment(ref _hits);
            interned = cached;
            return true;
        }

        Interlocked.Increment(ref _misses);
        interned = null;
        return false;
    }

    /// <summary>
    /// Adds an expression to the cache.
    /// </summary>
    /// <param name="expression">The expression to intern.</param>
    public void Add(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        _cache[expression] = expression;
    }

    /// <summary>
    /// Removes all entries from the cache.
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
        Interlocked.Exchange(ref _hits, 0);
        Interlocked.Exchange(ref _misses, 0);
    }

    /// <summary>
    /// Gets the current interning statistics.
    /// </summary>
    public InternStatistics Statistics => new()
    {
        TotalLookups = Volatile.Read(ref _hits) + Volatile.Read(ref _misses),
        Hits = Volatile.Read(ref _hits),
        Misses = Volatile.Read(ref _misses),
        UniqueCount = _cache.Count
    };
}
