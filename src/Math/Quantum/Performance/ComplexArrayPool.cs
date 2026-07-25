using System.Buffers;
using System.Numerics;

namespace MathVerse.Math.Quantum.Performance;

/// <summary>
/// Provides static access to a shared pool of complex number arrays.
/// </summary>
public static class ComplexArrayPool
{
    /// <summary>
    /// Gets the shared complex array pool instance.
    /// </summary>
    public static ArrayPool<Complex> Shared { get; } = ArrayPool<Complex>.Shared;

    /// <summary>
    /// Rents a complex array of at least the specified minimum length from the shared pool.
    /// </summary>
    /// <param name="minimumLength">The minimum required length of the array.</param>
    /// <returns>A rented complex array.</returns>
    public static Complex[] Rent(int minimumLength)
    {
        return Shared.Rent(minimumLength);
    }

    /// <summary>
    /// Returns a previously rented complex array to the shared pool.
    /// </summary>
    /// <param name="array">The array to return.</param>
    public static void Return(Complex[] array)
    {
        if (array != null)
        {
            Shared.Return(array, clearArray: true);
        }
    }
}
