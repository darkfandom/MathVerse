using System.Buffers;
using System.Numerics;

namespace MathVerse.Math.Quantum.Performance;

/// <summary>
/// Provides pooled tensor arrays of complex numbers to reduce allocations.
/// </summary>
public sealed class TensorPool
{
    private readonly ArrayPool<Complex> _pool;

    /// <summary>
    /// Initializes a new instance of the <see cref="TensorPool"/> class.
    /// </summary>
    public TensorPool()
    {
        _pool = ArrayPool<Complex>.Shared;
    }

    /// <summary>
    /// Rents a complex array of at least the specified minimum length.
    /// </summary>
    /// <param name="minimumLength">The minimum required length of the array.</param>
    /// <returns>A rented complex array.</returns>
    public Complex[] Rent(int minimumLength)
    {
        return _pool.Rent(minimumLength);
    }

    /// <summary>
    /// Returns a previously rented complex array to the pool.
    /// </summary>
    /// <param name="array">The array to return.</param>
    public void Return(Complex[] array)
    {
        if (array != null)
        {
            _pool.Return(array, clearArray: true);
        }
    }

    /// <summary>
    /// Clears all pooled arrays.
    /// </summary>
    public void Clear()
    {
    }
}
