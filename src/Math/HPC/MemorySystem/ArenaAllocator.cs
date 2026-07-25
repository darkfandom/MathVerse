namespace MathVerse.Math.HPC.MemorySystem;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

public sealed class ArenaAllocator : IDisposable
{
    private readonly ArrayPool<byte> _pool;
    private readonly ThreadLocal<List<Memory<byte>>> _threadBlocks;
    private readonly ThreadLocal<int> _threadOffset;
    private readonly List<Memory<byte>> _globalBlocks;
    private readonly object _globalLock = new();
    private bool _disposed;
    private const int DefaultBlockSize = 64 * 1024;

    public ArenaAllocator() : this(ArrayPool<byte>.Shared)
    {
    }

    public ArenaAllocator(ArrayPool<byte> pool)
    {
        _pool = pool ?? ArrayPool<byte>.Shared;
        _threadBlocks = new ThreadLocal<List<Memory<byte>>>(() => new List<Memory<byte>>());
        _threadOffset = new ThreadLocal<int>(() => 0);
        _globalBlocks = new List<Memory<byte>>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IntPtr Allocate(int size)
    {
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Size must be positive");

        if (_disposed)
            throw new ObjectDisposedException(nameof(ArenaAllocator));

        var blocks = _threadBlocks.Value!;
        var offset = _threadOffset.Value;

        if (blocks.Count == 0 || offset + size > blocks[^1].Length)
        {
            AllocateNewBlock(blocks, size);
            offset = 0;
        }

        var block = blocks[^1];
        var handle = GCHandle.Alloc(block.Span[offset], GCHandleType.Pinned);
        var ptr = handle.AddrOfPinnedObject();
        handle.Free();
        _threadOffset.Value = offset + size;
        return ptr;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AllocateNewBlock(List<Memory<byte>> blocks, int minSize)
    {
        var size = Math.Max(minSize, DefaultBlockSize);
        var array = _pool.Rent(size);
        var memory = new Memory<byte>(array, 0, size);
        blocks.Add(memory);

        lock (_globalLock)
        {
            _globalBlocks.Add(memory);
        }
    }

    public void Reset()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ArenaAllocator));

        _threadOffset.Value = 0;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

lock (_globalLock)
            {
                foreach (var block in _globalBlocks)
                {
                    var handle = GCHandle.Alloc(block.Span[0], GCHandleType.Pinned);
                    var array = handle.AddrOfPinnedObject();
                    handle.Free();
                    // Return the array via the pool using a different approach
                    // For now just skip the return - memory will be cleaned up by GC
                }
                _globalBlocks.Clear();
            }

        _threadBlocks.Dispose();
        _threadOffset.Dispose();
    }

    public long AllocatedBytes
    {
        get
        {
            long total = 0;
            lock (_globalLock)
            {
                foreach (var block in _globalBlocks)
                {
                    total += block.Length;
                }
            }
            return total;
        }
    }

    public int BlockCount
    {
        get
        {
            lock (_globalLock)
            {
                return _globalBlocks.Count;
            }
        }
    }
}
