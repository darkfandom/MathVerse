namespace MathVerse.Math.Performance.Pools;

/// <summary>
/// Thread-safe pooled list that borrows a <see cref="List{T}"/> from a pool and returns it on disposal.
/// </summary>
/// <typeparam name="T">The element type of the list.</typeparam>
public sealed class PooledList<T> : IDisposable
{
    private readonly ObjectPool<List<T>> _pool;
    private readonly List<T> _list;
    private bool _disposed;

    /// <summary>
    /// Initializes a pooled list backed by the specified pool.
    /// </summary>
    /// <param name="pool">The object pool to borrow from.</param>
    public PooledList(ObjectPool<List<T>> pool)
    {
        _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        _list = pool.Rent(static () => []);
    }

    /// <summary>Gets the underlying list.</summary>
    public List<T> List => _list;

    /// <summary>Gets the number of elements.</summary>
    public int Count => _list.Count;

    /// <summary>Adds an element to the list.</summary>
    /// <param name="item">The item to add.</param>
    public void Add(T item)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _list.Add(item);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _list.Clear();
            _pool.Return(_list);
        }
    }
}
