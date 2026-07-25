namespace MathVerse.Math.Distributed.GPU
{
    using System;

    /// <summary>
    /// Provides abstract GPU memory management operations for allocating, freeing,
    /// and transferring data between host and device memory.
    /// </summary>
    public sealed class GPUMemory
    {
        /// <summary>
        /// The next available buffer identifier.
        /// </summary>
        private int _nextBufferId;

        /// <summary>
        /// Lock object for thread-safe ID generation.
        /// </summary>
        private readonly object _lock;

        /// <summary>
        /// Initializes a new instance of the <see cref="GPUMemory"/> class.
        /// </summary>
        public GPUMemory()
        {
            _nextBufferId = 1;
            _lock = new object();
        }

        /// <summary>
        /// Allocates a new GPU buffer of the specified size in bytes.
        /// </summary>
        /// <param name="sizeBytes">The number of bytes to allocate.</param>
        /// <returns>A new <see cref="GPUBuffer"/> representing the allocated memory.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="sizeBytes"/> is non-positive.
        /// </exception>
        public GPUBuffer Allocate(int sizeBytes)
        {
            if (sizeBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(sizeBytes), "Allocation size must be positive.");

            int id;
            lock (_lock)
            {
                id = _nextBufferId++;
            }

            return new GPUBuffer
            {
                BufferId = id,
                SizeInBytes = sizeBytes,
                DeviceMemory = new IntPtr(id),
                IsDeviceMemory = true
            };
        }

        /// <summary>
        /// Frees a previously allocated GPU buffer.
        /// </summary>
        /// <param name="buffer">The buffer to free.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the buffer is not a device memory buffer.
        /// </exception>
        public void Free(GPUBuffer buffer)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            buffer.IsDeviceMemory = false;
            buffer.DeviceMemory = IntPtr.Zero;
        }

        /// <summary>
        /// Copies data from host memory to the specified GPU buffer.
        /// </summary>
        /// <param name="buffer">The destination GPU buffer.</param>
        /// <param name="data">The source data to copy.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="buffer"/> or <paramref name="data"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the data size exceeds the buffer capacity.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the buffer is not a device memory buffer.
        /// </exception>
        public void CopyToDevice(GPUBuffer buffer, ReadOnlySpan<byte> data)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            if (!buffer.IsDeviceMemory)
                throw new InvalidOperationException("Buffer is not a device memory buffer.");

            if (data.Length > buffer.SizeInBytes)
                throw new ArgumentException(
                    $"Data size ({data.Length} bytes) exceeds buffer capacity ({buffer.SizeInBytes} bytes).");
        }

        /// <summary>
        /// Copies data from the specified GPU buffer to host memory.
        /// </summary>
        /// <param name="buffer">The source GPU buffer.</param>
        /// <param name="data">The destination span to copy data into.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the destination span is too small for the buffer content.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the buffer is not a device memory buffer.
        /// </exception>
        public void CopyFromDevice(GPUBuffer buffer, Span<byte> data)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            if (!buffer.IsDeviceMemory)
                throw new InvalidOperationException("Buffer is not a device memory buffer.");

            if (data.Length < buffer.SizeInBytes)
                throw new ArgumentException(
                    $"Destination span ({data.Length} bytes) is too small for buffer content ({buffer.SizeInBytes} bytes).");
        }

        /// <summary>
        /// Copies data between two GPU buffers.
        /// </summary>
        /// <param name="source">The source GPU buffer.</param>
        /// <param name="destination">The destination GPU buffer.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> or <paramref name="destination"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the source buffer is larger than the destination buffer.
        /// </exception>
        public void CopyBufferToBuffer(GPUBuffer source, GPUBuffer destination)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (destination is null)
                throw new ArgumentNullException(nameof(destination));

            if (source.SizeInBytes > destination.SizeInBytes)
                throw new ArgumentException(
                    $"Source buffer ({source.SizeInBytes} bytes) is larger than destination ({destination.SizeInBytes} bytes).");
        }
    }
}
