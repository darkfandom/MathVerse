namespace MathVerse.Math.HPC.MemorySystem;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

public sealed class SlabAllocator : IDisposable
{
    private readonly int _blockSize;
    private readonly int _blocksPerSlab;
    private readonly System.Buffers.ArrayPool<byte> _pool;
    private readonly ConcurrentStack<IntPtr> _freeBlocks;
    private readonly List<byte[]> _slabs;
    private readonly object _slabLock = new();
    private bool _disposed;

    public SlabStats Stats { get; private set; }

    public SlabAllocator(int blockSize, int blocksPerSlab = 64, System.Buffers.ArrayPool<byte>? pool = null)
    {
        if (blockSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(blockSize), "Block size must be positive");
        if (blocksPerSlab <= 0)
            throw new ArgumentOutOfRangeException(nameof(blocksPerSlab), "Blocks per slab must be positive");

        _blockSize = (blockSize + 7) & ~7;
        _blocksPerSlab = blocksPerSlab;
        _pool = pool ?? System.Buffers.ArrayPool<byte>.Shared;
        _freeBlocks = new ConcurrentStack<IntPtr>();
        _slabs = new List<byte[]>();
        Stats = new SlabStats { BlockSize = _blockSize, BlocksPerSlab = _blocksPerSlab };
    }

[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IntPtr Allocate()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SlabAllocator));

        if (_freeBlocks.TryPop(out var ptr))
        {
            var stats = Stats;
            stats.Allocations++;
            stats.CurrentUsage++;
            stats.PeakUsage = Math.Max(stats.PeakUsage, stats.CurrentUsage);
            Stats = stats;
            return ptr;
        }

        return AllocateNewSlab();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IntPtr AllocateNewSlab()
    {
        var slabSize = _blockSize * _blocksPerSlab;
        var slab = _pool.Rent(slabSize);
        var slabPtr = Marshal.UnsafeAddrOfPinnedArrayElement(slab, 0);

        lock (_slabLock)
        {
            _slabs.Add(slab);
            var stats = Stats;
            stats.SlabCount++;
            stats.TotalBlocks += _blocksPerSlab;
            Stats = stats;
        }

        var offset = _blockSize;
        for (int i = 1; i < _blocksPerSlab; i++, offset += _blockSize)
        {
            var blockPtr = IntPtr.Add(slabPtr, offset);
            _freeBlocks.Push(blockPtr);
        }

        var stats2 = Stats;
        stats2.Allocations++;
        stats2.CurrentUsage++;
        stats2.PeakUsage = Math.Max(stats2.PeakUsage, stats2.CurrentUsage);
        Stats = stats2;

        return slabPtr;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Free(IntPtr ptr)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SlabAllocator));

        if (ptr == IntPtr.Zero)
            return;

        _freeBlocks.Push(ptr);
        var stats3 = Stats;
        stats3.CurrentUsage--;
        stats3.Frees++;
        Stats = stats3;
    }

    public SlabStats GetStats()
    {
        var stats = Stats;
        stats.FreeBlocks = _freeBlocks.Count;
        return stats;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        lock (_slabLock)
        {
            foreach (var slab in _slabs)
            {
                _pool.Return(slab);
            }
            _slabs.Clear();
            _freeBlocks.Clear();
            Stats = new SlabStats { BlockSize = _blockSize, BlocksPerSlab = _blocksPerSlab };
        }
    }
}

public struct SlabStats
{
    public int BlockSize;
    public int BlocksPerSlab;
    public int SlabCount;
    public long TotalBlocks;
    public long Allocations;
    public long Frees;
    public long CurrentUsage;
    public long PeakUsage;
    public int FreeBlocks;

    public double Utilization => TotalBlocks > 0 ? (double)CurrentUsage / TotalBlocks : 0;
    public long TotalBytes => TotalBlocks * BlockSize;
    public long UsedBytes => CurrentUsage * BlockSize;
}
