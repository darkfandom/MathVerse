namespace MathVerse.Math.Distributed.MemoryManagement
{
    using System;
    using System.Buffers;
    using System.Collections.Concurrent;

    /// <summary>
    /// Generic buffer pool wrapping ArrayPool with tracking.
    /// </summary>
    public sealed class BufferPool
    {
        private readonly ConcurrentDictionary<Type, long> _rentedCounts = new();
        private readonly ConcurrentDictionary<Type, long> _returnedCounts = new();

        /// <summary>
        /// Rents a buffer of at least the specified length.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="minimumLength">Minimum array length.</param>
        /// <returns>A rented array.</returns>
        public T[] RentBuffer<T>(int minimumLength)
        {
            if (minimumLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(minimumLength), "Length must be positive.");

            _rentedCounts.AddOrUpdate(typeof(T), 1, (_, c) => c + 1);
            return ArrayPool<T>.Shared.Rent(minimumLength);
        }

        /// <summary>
        /// Returns a previously rented buffer.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="buffer">The array to return.</param>
        public void ReturnBuffer<T>(T[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            _returnedCounts.AddOrUpdate(typeof(T), 1, (_, c) => c + 1);
            ArrayPool<T>.Shared.Return(buffer);
        }

        /// <summary>
        /// Gets the number of buffers rented for a given type.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <returns>The count of rented buffers.</returns>
        public long GetRentedCount<T>() =>
            _rentedCounts.TryGetValue(typeof(T), out var count) ? count : 0;

        /// <summary>
        /// Gets the number of buffers returned for a given type.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <returns>The count of returned buffers.</returns>
        public long GetReturnedCount<T>() =>
            _returnedCounts.TryGetValue(typeof(T), out var count) ? count : 0;
    }
}
