namespace MathVerse.Math.Performance.Pools;

/// <summary>
/// Thread-safe generic object pool that manages reusable instances of reference types.
/// </summary>
/// <typeparam name="T">The type of pooled objects.</typeparam>
public sealed class ObjectPool<T> where T : class
{
    private readonly ConcurrentBag<T> _items = [];
    private int _count;

    /// <summary>
    /// Gets the number of objects available in the pool.
    /// </summary>
    public int Count => Volatile.Read(ref _count);

    /// <summary>
    /// Returns an object from the pool, or creates a new one using the factory if the pool is empty.
    /// </summary>
    /// <param name="factory">The factory function to create a new instance when the pool is empty.</param>
    /// <returns>A pooled or newly created object.</returns>
    public T Rent(Func<T> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        if (_items.TryTake(out var item))
        {
            Interlocked.Decrement(ref _count);
            return item;
        }

        return factory();
    }

    /// <summary>
    /// Returns an object to the pool for future reuse.
    /// </summary>
    /// <param name="item">The object to return to the pool.</param>
    public void Return(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        _items.Add(item);
        Interlocked.Increment(ref _count);
    }

    /// <summary>
    /// Clears all objects from the pool.
    /// </summary>
    public void Clear()
    {
        while (_items.TryTake(out _))
        {
            Interlocked.Decrement(ref _count);
        }
    }
}
