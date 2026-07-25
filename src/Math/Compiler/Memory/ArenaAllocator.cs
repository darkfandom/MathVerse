namespace MathVerse.Math.Compiler.Memory;

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading;

/// <summary>
/// Fast bump allocator for compilation temporaries. Allocates from large ArrayPool-backed blocks
/// using a lock-free bump pointer per thread for thread safety.
/// </summary>
public sealed class ArenaAllocator : IDisposable
{
    private const int DefaultBlockSize = 65536;
    private const int MaxBlockSize = 1048576;

    private readonly int _blockSize;
    private readonly ThreadLocal<ArenaThreadState> _threadState;
    private long _totalAllocated;
    private long _allocationCount;
    private bool _disposed;

    /// <summary>Total bytes allocated across all threads.</summary>
    public long TotalAllocated => Interlocked.Read(ref _totalAllocated);

    /// <summary>Total number of allocation requests.</summary>
    public long AllocationCount => Interlocked.Read(ref _allocationCount);

    /// <summary>
    /// Initializes a new arena allocator with the specified block size.
    /// </summary>
    /// <param name="blockSize">The size of each backing buffer block in bytes. Defaults to 64KB.</param>
    public ArenaAllocator(int blockSize = DefaultBlockSize)
    {
        _blockSize = Math.Clamp(blockSize, 1024, MaxBlockSize);
        _threadState = new ThreadLocal<ArenaThreadState>(() => new ArenaThreadState(_blockSize), trackAllValues: false);
    }

    /// <summary>
    /// Allocates a contiguous block of memory of the specified size from the arena.
    /// Returns the offset within the current block.
    /// </summary>
    /// <param name="size">The number of bytes to allocate.</param>
    /// <returns>The byte offset of the allocated memory within the arena's address space.</returns>
    public int Allocate(int size)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size), size, "Size must be positive.");

        Interlocked.Increment(ref _allocationCount);

        var state = _threadState.Value!;

        // Try bump allocation in current block
        var currentOffset = Volatile.Read(ref state.Offset);
        var alignedSize = Align(size, 8);

        if (currentOffset + alignedSize <= state.Buffer.Length)
        {
            var newOffset = Interlocked.Add(ref state.Offset, alignedSize) - alignedSize;
            if (newOffset + alignedSize <= state.Buffer.Length)
            {
                Interlocked.Add(ref _totalAllocated, alignedSize);
                return newOffset;
            }
        }

        // Current block is full, allocate a new one
        return AllocateNewBlock(state, alignedSize);
    }

    /// <summary>
    /// Resets the arena, making all previously allocated memory available for reuse.
    /// Thread-safe: each thread's state is reset independently.
    /// </summary>
    public void Reset()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Reset current thread state
        var state = _threadState.Value;
        if (state != null)
        {
            Volatile.Write(ref state.Offset, 0);
        }

        Interlocked.Exchange(ref _totalAllocated, 0);
        Interlocked.Exchange(ref _allocationCount, 0);
    }

    /// <summary>
    /// Returns all rented buffers to the ArrayPool and resets the allocator.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        var state = _threadState.Value;
        if (state != null)
        {
            state.ReturnBuffers();
        }

        _threadState.Dispose();
    }

    private int AllocateNewBlock(ArenaThreadState state, int alignedSize)
    {
        // Return old buffer to pool
        if (state.Buffer != null && state.Buffer != Array.Empty<byte>())
        {
            ArrayPool<byte>.Shared.Return(state.Buffer);
        }

        // Rent a new buffer, potentially larger
        var newSize = Math.Max(_blockSize, alignedSize);
        state.Buffer = ArrayPool<byte>.Shared.Rent(newSize);
        Volatile.Write(ref state.Offset, 0);

        var offset = Interlocked.Add(ref state.Offset, alignedSize) - alignedSize;
        Interlocked.Add(ref _totalAllocated, alignedSize);

        return offset;
    }

    private static int Align(int value, int alignment)
    {
        return (value + alignment - 1) & ~(alignment - 1);
    }

    /// <summary>
    /// Thread-local state for bump allocation.
    /// </summary>
    private sealed class ArenaThreadState
    {
        public byte[] Buffer;
        public int Offset;

        public ArenaThreadState(int initialSize)
        {
            Buffer = ArrayPool<byte>.Shared.Rent(initialSize);
            Offset = 0;
        }

        public void ReturnBuffers()
        {
            if (Buffer != null && Buffer != Array.Empty<byte>())
            {
                ArrayPool<byte>.Shared.Return(Buffer);
                Buffer = Array.Empty<byte>();
            }
        }
    }
}
