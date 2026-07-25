namespace MathVerse.Math.Compiler.Performance;

using System;
using System.Buffers;

/// <summary>Centralized buffer management using <see cref="ArrayPool{T}"/> for double, float, and int arrays.</summary>
public static class ReusableBuffers
{
    /// <summary>Rents a double array of at least the specified size from the shared pool.</summary>
    public static double[] RentDouble(int size)
    {
        if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));
        return ArrayPool<double>.Shared.Rent(size);
    }

    /// <summary>Returns a double array to the shared pool.</summary>
    public static void ReturnDouble(double[] buffer)
    {
        if (buffer is null) throw new ArgumentNullException(nameof(buffer));
        ArrayPool<double>.Shared.Return(buffer, clearArray: true);
    }

    /// <summary>Rents a float array of at least the specified size from the shared pool.</summary>
    public static float[] RentFloat(int size)
    {
        if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));
        return ArrayPool<float>.Shared.Rent(size);
    }

    /// <summary>Returns a float array to the shared pool.</summary>
    public static void ReturnFloat(float[] buffer)
    {
        if (buffer is null) throw new ArgumentNullException(nameof(buffer));
        ArrayPool<float>.Shared.Return(buffer, clearArray: true);
    }

    /// <summary>Rents an int array of at least the specified size from the shared pool.</summary>
    public static int[] RentInt(int size)
    {
        if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));
        return ArrayPool<int>.Shared.Rent(size);
    }

    /// <summary>Returns an int array to the shared pool.</summary>
    public static void ReturnInt(int[] buffer)
    {
        if (buffer is null) throw new ArgumentNullException(nameof(buffer));
        ArrayPool<int>.Shared.Return(buffer, clearArray: true);
    }

    /// <summary>Rents a buffer of the given element type.</summary>
    public static T[] Rent<T>(int size)
    {
        if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));
        return ArrayPool<T>.Shared.Rent(size);
    }

    /// <summary>Returns a buffer of the given element type.</summary>
    public static void Return<T>(T[] buffer)
    {
        if (buffer is null) throw new ArgumentNullException(nameof(buffer));
        ArrayPool<T>.Shared.Return(buffer, clearArray: true);
    }
}
