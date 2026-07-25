namespace MathVerse.Math.Interop.Performance;

using System;
using System.Buffers;
using System.Collections.Concurrent;

/// <summary>
/// Generic object pool for reducing allocations in serialization pipelines.
/// </summary>
/// <typeparam name="T">The pooled type.</typeparam>
public sealed class InteropObjectPool<T> where T : class
{
    private readonly ConcurrentBag<T> _pool = new();
    private readonly Func<T> _factory;
    private readonly Action<T>? _reset;
    private readonly int _maxSize;

    /// <summary>
    /// Gets the approximate number of objects in the pool.
    /// </summary>
    public int ApproximateCount => _pool.Count;

    /// <summary>
    /// Initializes a new instance of the <see cref="InteropObjectPool{T}"/> class.
    /// </summary>
    /// <param name="factory">The factory function to create new instances.</param>
    /// <param name="reset">Optional reset action to prepare instances for reuse.</param>
    /// <param name="maxSize">The maximum pool size.</param>
    public InteropObjectPool(Func<T> factory, Action<T>? reset = null, int maxSize = 1024)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _reset = reset;
        _maxSize = maxSize;
    }

    /// <summary>
    /// Rents an object from the pool or creates a new one.
    /// </summary>
    /// <returns>An object instance.</returns>
    public T Rent()
    {
        if (_pool.TryTake(out var item))
        {
            return item;
        }
        return _factory();
    }

    /// <summary>
    /// Returns an object to the pool.
    /// </summary>
    /// <param name="item">The object to return.</param>
    public void Return(T item)
    {
        _ = item ?? throw new ArgumentNullException(nameof(item));
        if (_pool.Count < _maxSize)
        {
            _reset?.Invoke(item);
            _pool.Add(item);
        }
    }

    /// <summary>
    /// Clears the pool.
    /// </summary>
    public void Clear()
    {
        while (_pool.TryTake(out _)) { }
    }
}
