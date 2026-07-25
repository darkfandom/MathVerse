namespace MathVerse.Math.Distributed.GPU
{
    using System;

    /// <summary>
    /// Abstract representation of a memory buffer allocated on a GPU device.
    /// </summary>
    public sealed class GPUBuffer
    {
        /// <summary>
        /// Gets or sets the unique identifier for this buffer.
        /// </summary>
        public int BufferId { get; set; }

        /// <summary>
        /// Gets or sets the size of this buffer in bytes.
        /// </summary>
        public int SizeInBytes { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the device memory where this buffer resides.
        /// </summary>
        public IntPtr DeviceMemory { get; set; }

        /// <summary>
        /// Gets or sets whether this buffer is backed by device (GPU) memory.
        /// </summary>
        public bool IsDeviceMemory { get; set; }

        /// <summary>
        /// Gets the size of this buffer in kilobytes.
        /// </summary>
        public double SizeInKilobytes => (double)SizeInBytes / 1024.0;

        /// <summary>
        /// Returns a string representation of this buffer.
        /// </summary>
        /// <returns>A string containing the buffer ID and size information.</returns>
        public override string ToString()
        {
            return $"Buffer[{BufferId}] - {SizeInKilobytes:F2} KB";
        }
    }

    /// <summary>
    /// A strongly-typed GPU buffer that tracks the element type stored in the buffer.
    /// </summary>
    /// <typeparam name="T">The element type stored in this buffer.</typeparam>
    public sealed class GPUBuffer<T> where T : struct
    {
        /// <summary>
        /// Gets or sets the underlying untyped GPU buffer.
        /// </summary>
        public GPUBuffer Buffer { get; set; } = new GPUBuffer();

        /// <summary>
        /// Gets or sets the number of elements of type <typeparamref name="T"/> in this buffer.
        /// </summary>
        public int ElementCount { get; set; }

        /// <summary>
        /// Gets the size of each element in bytes.
        /// </summary>
        public int ElementSize => System.Runtime.InteropServices.Marshal.SizeOf<T>();

        /// <summary>
        /// Gets the total size of this buffer in bytes.
        /// </summary>
        public int TotalSizeInBytes => ElementCount * ElementSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="GPUBuffer{T}"/> class with the specified element count.
        /// </summary>
        /// <param name="elementCount">The number of elements to allocate.</param>
        public GPUBuffer(int elementCount)
        {
            ElementCount = elementCount;
            Buffer.SizeInBytes = TotalSizeInBytes;
        }

        /// <summary>
        /// Returns a string representation of this typed buffer.
        /// </summary>
        /// <returns>A string containing the element type, count, and total size.</returns>
        public override string ToString()
        {
            return $"GPUBuffer<{typeof(T).Name}>[{ElementCount}] - {Buffer.SizeInKilobytes:F2} KB";
        }
    }
}
