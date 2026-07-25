namespace MathVerse.Math.Distributed.MemoryManagement
{
    using System;
    using System.Buffers;
    using System.Collections.Concurrent;
    using System.Threading;

    /// <summary>
    /// Thread-safe pool of shared memory buffers with bucketed sizes (power of 2).
    /// </summary>
    public sealed class SharedMemoryPool
    {
        private readonly ConcurrentDictionary<int, ConcurrentBag<Memory<byte>>> _pools = new();
        private long _rentedCount;
        private long _returnedCount;

        /// <summary>
        /// Rents a memory buffer of at least the specified size.
        /// </summary>
        /// <param name="size">Minimum size in bytes.</param>
        /// <returns>A rented memory buffer.</returns>
        public Memory<byte> Rent(int size)
        {
            int bucketSize = GetBucketSize(size);
            var pool = _pools.GetOrAdd(bucketSize, _ => new ConcurrentBag<Memory<byte>>());
            if (pool.TryTake(out var buffer))
            {
                Interlocked.Increment(ref _rentedCount);
                return buffer;
            }
            Interlocked.Increment(ref _rentedCount);
            return new byte[bucketSize];
        }

        /// <summary>
        /// Returns a previously rented buffer to the pool.
        /// </summary>
        /// <param name="buffer">The buffer to return.</param>
        public void Return(Memory<byte> buffer)
        {
            int bucketSize = GetBucketSize(buffer.Length);
            var pool = _pools.GetOrAdd(bucketSize, _ => new ConcurrentBag<Memory<byte>>());
            pool.Add(buffer);
            Interlocked.Increment(ref _returnedCount);
        }

        /// <summary>
        /// Gets the total number of rented buffers.
        /// </summary>
        public long RentedCount => Interlocked.Read(ref _rentedCount);

        /// <summary>
        /// Gets the total number of returned buffers.
        /// </summary>
        public long ReturnedCount => Interlocked.Read(ref _returnedCount);

        /// <summary>
        /// Clears all pools.
        /// </summary>
        public void Clear()
        {
            foreach (var pool in _pools.Values)
            {
                while (pool.TryTake(out _)) { }
            }
        }

        private static int GetBucketSize(int size)
        {
            int bucket = 1;
            while (bucket < size)
            {
                bucket <<= 1;
            }
            return bucket;
        }
    }
}
