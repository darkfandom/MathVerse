namespace MathVerse.Math.Interop.Performance;

using System;
using System.Buffers;

/// <summary>
/// Manages memory pools for serialization operations to minimize GC pressure.
/// </summary>
public sealed class MemoryPoolManager
{
    private readonly int _blockSize;

    /// <summary>
    /// Gets the block size used for memory pool allocations.
    /// </summary>
    public int BlockSize => _blockSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryPoolManager"/> class.
    /// </summary>
    /// <param name="blockSize">The block size for pool allocations.</param>
    public MemoryPoolManager(int blockSize = 4096)
    {
        _blockSize = blockSize > 0 ? blockSize : throw new ArgumentOutOfRangeException(nameof(blockSize));
    }

    /// <summary>
    /// Rents a memory block from the shared pool.
    /// </summary>
    /// <param name="minSize">The minimum required size.</param>
    /// <returns>A memory owner that should be disposed when done.</returns>
    public IMemoryOwner<byte> Rent(int minSize)
    {
        return MemoryPool<byte>.Shared.Rent(minSize);
    }

    /// <summary>
    /// Rents a memory block with the default block size.
    /// </summary>
    /// <returns>A memory owner that should be disposed when done.</returns>
    public IMemoryOwner<byte> Rent()
    {
        return MemoryPool<byte>.Shared.Rent(_blockSize);
    }
}
