namespace MathVerse.Math.Compiler.Performance;

using System;
using System.Buffers;
using System.Collections.Concurrent;
using IRInstruction = MathVerse.Math.Compiler.IR.IRInstruction;

/// <summary>Pools and caches frequently used <see cref="IRInstruction"/> objects.
/// Uses <see cref="ArrayPool{T}"/> internally for efficient array reuse.</summary>
public sealed class InstructionCache
{
    private readonly ConcurrentQueue<IRInstruction> _pool = new();
    private readonly object _lock = new();
    private int _count;

    /// <summary>Rents an array of <see cref="IRInstruction"/> from the shared <see cref="ArrayPool{T}"/>.</summary>
    /// <param name="minimumLength">The minimum length of the array.</param>
    /// <returns>An array of at least the requested length.</returns>
    public IRInstruction[] Rent(int minimumLength)
    {
        if (minimumLength < 0) throw new ArgumentOutOfRangeException(nameof(minimumLength));
        return ArrayPool<IRInstruction>.Shared.Rent(minimumLength);
    }

    /// <summary>Returns an array to the shared <see cref="ArrayPool{T}"/>.</summary>
    /// <param name="array">The array to return.</param>
    public void Return(IRInstruction[] array)
    {
        if (array is null) throw new ArgumentNullException(nameof(array));
        ArrayPool<IRInstruction>.Shared.Return(array, clearArray: true);
    }

    /// <summary>Attempts to retrieve a pooled instruction instance.</summary>
    /// <param name="instruction">The retrieved instruction, or null if the pool is empty.</param>
    /// <returns>True if an instruction was available.</returns>
    public bool TryGet(out IRInstruction? instruction)
    {
        if (_pool.TryDequeue(out instruction))
        {
            Interlocked.Decrement(ref _count);
            return true;
        }
        return false;
    }

    /// <summary>Returns an unused instruction to the pool for reuse.</summary>
    public void Return(IRInstruction instruction)
    {
        if (instruction is null) throw new ArgumentNullException(nameof(instruction));
        _pool.Enqueue(instruction);
        Interlocked.Increment(ref _count);
    }

    /// <summary>Clears all pooled instructions.</summary>
    public void Clear()
    {
        while (_pool.TryDequeue(out _)) { }
        Interlocked.Exchange(ref _count, 0);
    }

    /// <summary>Gets the number of cached/pooled instructions.</summary>
    public int Count => Interlocked.Read(ref _count);

    private static class Interlocked
    {
        public static int Increment(ref int value) => System.Threading.Interlocked.Increment(ref value);
        public static int Decrement(ref int value) => System.Threading.Interlocked.Decrement(ref value);
        public static int Exchange(ref int value, int newVal) => System.Threading.Interlocked.Exchange(ref value, newVal);
        public static int Read(ref int value) => System.Threading.Interlocked.CompareExchange(ref value, 0, 0);
    }
}
