namespace MathVerse.Math.Performance.Pools;

/// <summary>
/// Thread-safe pooled dictionary that borrows a <see cref="Dictionary{TKey,TValue}"/> from a pool and returns it on disposal.
/// </summary>
/// <typeparam name="TKey">The key type.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
public sealed class PooledDictionary<TKey, TValue> : IDisposable where TKey : notnull
{
    private readonly ObjectPool<Dictionary<TKey, TValue>> _pool;
    private readonly Dictionary<TKey, TValue> _dictionary;
    private bool _disposed;

    /// <summary>
    /// Initializes a pooled dictionary backed by the specified pool.
    /// </summary>
    /// <param name="pool">The object pool to borrow from.</param>
    public PooledDictionary(ObjectPool<Dictionary<TKey, TValue>> pool)
    {
        _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        _dictionary = pool.Rent(static () => []);
    }

    /// <summary>Gets the underlying dictionary.</summary>
    public Dictionary<TKey, TValue> Dictionary => _dictionary;

    /// <summary>Gets the number of entries.</summary>
    public int Count => _dictionary.Count;

    /// <summary>Adds or updates an entry.</summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public void Add(TKey key, TValue value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _dictionary[key] = value;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _dictionary.Clear();
            _pool.Return(_dictionary);
        }
    }
}
