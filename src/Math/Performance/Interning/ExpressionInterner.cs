namespace MathVerse.Math.Performance.Interning;

/// <summary>
/// Thread-safe expression interning service that deduplicates structurally equal expressions,
/// ensuring that identical expression trees share the same object reference.
/// </summary>
public sealed class ExpressionInterner
{
    private readonly ExpressionCache _cache = new();
    private int _totalInterns;

    /// <summary>
    /// Gets the number of unique interned expressions.
    /// </summary>
    public int Count => _cache.Count;

    /// <summary>
    /// Interns the given expression, returning the canonical instance if one already exists
    /// with the same structural content.
    /// </summary>
    /// <param name="expression">The expression to intern.</param>
    /// <returns>The interned (canonical) expression instance.</returns>
    public Expression Intern(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        if (_cache.TryGet(expression, out var existing) && existing is not null)
        {
            Interlocked.Increment(ref _totalInterns);
            return existing;
        }

        _cache.Add(expression);
        return expression;
    }

    /// <summary>
    /// Removes all interned expressions.
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
        Interlocked.Exchange(ref _totalInterns, 0);
    }

    /// <summary>
    /// Gets the current interning statistics.
    /// </summary>
    public InternStatistics Statistics => _cache.Statistics;
}
