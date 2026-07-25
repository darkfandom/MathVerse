using System.Buffers;

namespace MathVerse.Math.Performance.Pools;

/// <summary>
/// Adapter that wraps <see cref="ArrayPool{T}"/> for use as a reusable buffer pool.
/// </summary>
public sealed class ArrayPoolAdapter
{
    private readonly ArrayPool<byte> _pool;

    /// <summary>
    /// Initializes an adapter wrapping the shared <see cref="ArrayPool{Byte}"/>.
    /// </summary>
    public ArrayPoolAdapter()
        : this(ArrayPool<byte>.Shared)
    {
    }

    /// <summary>
    /// Initializes an adapter wrapping the specified array pool.
    /// </summary>
    /// <param name="pool">The array pool to wrap.</param>
    public ArrayPoolAdapter(ArrayPool<byte> pool)
    {
        _pool = pool ?? throw new ArgumentNullException(nameof(pool));
    }

    /// <summary>
    /// Rents a byte array with at least the specified minimum length.
    /// </summary>
    /// <param name="minimumLength">The minimum required length.</param>
    /// <returns>A rented byte array.</returns>
    public byte[] Rent(int minimumLength)
    {
        if (minimumLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(minimumLength), "Must be positive.");

        return _pool.Rent(minimumLength);
    }

    /// <summary>
    /// Returns a previously rented byte array to the pool.
    /// </summary>
    /// <param name="array">The array to return.</param>
    public void Return(byte[] array)
    {
        ArgumentNullException.ThrowIfNull(array);
        _pool.Return(array, clearArray: true);
    }
}
