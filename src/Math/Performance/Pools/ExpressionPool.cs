namespace MathVerse.Math.Performance.Pools;

/// <summary>
/// Poolable expression container that can be borrowed from and returned to an <see cref="ObjectPool{T}"/>.
/// </summary>
public sealed class ExpressionPool
{
    private Expression? _current;

    /// <summary>
    /// Gets or sets the expression held by this pool entry.
    /// </summary>
    public Expression? Current
    {
        get => _current;
        set => _current = value;
    }

    /// <summary>
    /// Resets the pool entry to its default state.
    /// </summary>
    public void Reset()
    {
        _current = null;
    }
}
