namespace MathVerse.Math.Distributed.MemoryManagement
{
    using System;
    using System.Buffers;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// High-level memory management wrapping ArrayPool and SharedMemoryPool.
    /// </summary>
    public sealed class MemoryManager
    {
        private readonly SharedMemoryPool _sharedPool = new();
        private readonly ConcurrentDictionary<string, long> _taggedAllocations = new();
        private readonly ConcurrentDictionary<int, SharedMemoryPool> _customPools = new();
        private long _totalAllocated;

        /// <summary>
        /// Gets or creates a SharedMemoryPool for a specific bucket size.
        /// </summary>
        /// <param name="bucketSize">The bucket size for the pool.</param>
        /// <returns>A SharedMemoryPool for the given bucket size.</returns>
        public SharedMemoryPool GetPool(int bucketSize)
        {
            return _customPools.GetOrAdd(bucketSize, _ => new SharedMemoryPool());
        }

        /// <summary>
        /// Tracks an allocation under a specific tag.
        /// </summary>
        /// <param name="tag">Tag identifier for the allocation.</param>
        /// <param name="bytes">Number of bytes allocated.</param>
        public void TrackAllocation(string tag, long bytes)
        {
            if (string.IsNullOrEmpty(tag))
                throw new ArgumentException("Tag cannot be null or empty.", nameof(tag));
            if (bytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(bytes), "Bytes must be positive.");

            _taggedAllocations.AddOrUpdate(tag, bytes, (_, existing) => existing + bytes);
            System.Threading.Interlocked.Add(ref _totalAllocated, bytes);
        }

        /// <summary>
        /// Releases all allocations under a specific tag.
        /// </summary>
        /// <param name="tag">Tag identifier to release.</param>
        public void ReleaseTag(string tag)
        {
            if (_taggedAllocations.TryRemove(tag, out var bytes))
            {
                System.Threading.Interlocked.Add(ref _totalAllocated, -bytes);
            }
        }

        /// <summary>
        /// Gets current memory statistics.
        /// </summary>
        /// <returns>A dictionary containing memory stats.</returns>
        public Dictionary<string, long> GetMemoryStats()
        {
            return new Dictionary<string, long>
            {
                ["TotalAllocated"] = System.Threading.Interlocked.Read(ref _totalAllocated),
                ["RentedBuffers"] = _sharedPool.RentedCount,
                ["ReturnedBuffers"] = _sharedPool.ReturnedCount,
                ["TrackedTags"] = _taggedAllocations.Count
            };
        }

        /// <summary>
        /// Rents a buffer from the shared pool.
        /// </summary>
        /// <param name="size">Minimum size in bytes.</param>
        /// <returns>A rented memory buffer.</returns>
        public Memory<byte> RentBuffer(int size) => _sharedPool.Rent(size);

        /// <summary>
        /// Returns a buffer to the shared pool.
        /// </summary>
        /// <param name="buffer">The buffer to return.</param>
        public void ReturnBuffer(Memory<byte> buffer) => _sharedPool.Return(buffer);
    }
}
