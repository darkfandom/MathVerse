using System.Buffers;

namespace MathVerse.Math.Performance.Memory;

/// <summary>
/// Thread-safe manager for reusable byte buffers backed by <see cref="ArrayPool{T}"/>.
/// </summary>
public sealed class BufferManager
{
    private readonly ArrayPool<byte> _pool;
    private int _activeBuffers;
    private int _pooledBuffers;

    /// <summary>Initializes a new buffer manager with default pool settings.</summary>
    public BufferManager()
        : this(ArrayPool<byte>.Shared)
    {
    }

    /// <summary>Initializes a new buffer manager with the specified array pool.</summary>
    /// <param name="pool">The array pool to rent from.</param>
    public BufferManager(ArrayPool<byte> pool)
    {
        _pool = pool ?? throw new ArgumentNullException(nameof(pool));
    }

    /// <summary>Gets the number of buffers currently rented out.</summary>
    public int ActiveBuffers => Volatile.Read(ref _activeBuffers);

    /// <summary>Gets the number of buffers currently in the pool.</summary>
    public int PooledBuffers => Volatile.Read(ref _pooledBuffers);

    /// <summary>Rents a buffer with at least the specified minimum size.</summary>
    /// <param name="minimumSize">The minimum required capacity in bytes.</param>
    /// <returns>A byte array of at least the requested size.</returns>
    public byte[] RentBuffer(int minimumSize)
    {
        if (minimumSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(minimumSize), "Minimum size must be positive.");

        var buffer = _pool.Rent(minimumSize);
        Interlocked.Increment(ref _activeBuffers);
        return buffer;
    }

    /// <summary>Returns a previously rented buffer to the pool for reuse.</summary>
    /// <param name="buffer">The buffer to return.</param>
    public void ReturnBuffer(byte[] buffer)
    {
        if (buffer is null)
            throw new ArgumentNullException(nameof(buffer));

        Interlocked.Decrement(ref _activeBuffers);
        Interlocked.Increment(ref _pooledBuffers);
        _pool.Return(buffer, clearArray: true);
    }

    /// <summary>Clears all pooled buffers, releasing them for garbage collection.</summary>
    public void Clear()
    {
        Volatile.Write(ref _pooledBuffers, 0);
    }
}
