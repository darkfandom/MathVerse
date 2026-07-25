namespace MathVerse.Math.Performance.Pools;

/// <summary>
/// Pools instances of builder objects that implement a reset pattern.
/// </summary>
/// <typeparam name="T">The builder type to pool.</typeparam>
public sealed class BuilderPool<T> where T : class, new()
{
    private readonly ObjectPool<T> _pool;

    /// <summary>
    /// Initializes a new builder pool.
    /// </summary>
    public BuilderPool()
    {
        _pool = new ObjectPool<T>();
    }

    /// <summary>
    /// Rents a builder instance from the pool.
    /// </summary>
    /// <returns>A builder instance ready for use.</returns>
    public T Rent() => _pool.Rent(static () => new T());

    /// <summary>
    /// Returns a builder instance to the pool after resetting it.
    /// </summary>
    /// <param name="builder">The builder to return.</param>
    public void Return(T builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        _pool.Return(builder);
    }
}
