namespace MathVerse.Math.Distributed.MemoryManagement
{
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Distributed memory abstraction for single-node simulation.
    /// </summary>
    public sealed class DistributedMemory
    {
        private readonly ConcurrentDictionary<string, byte[]> _nodes = new();
        private readonly object _lock = new();

        /// <summary>
        /// Allocates memory on a simulated node.
        /// </summary>
        /// <param name="nodeId">The node identifier.</param>
        /// <param name="size">Number of bytes to allocate.</param>
        public void Allocate(string nodeId, int size)
        {
            if (string.IsNullOrEmpty(nodeId))
                throw new ArgumentException("Node ID cannot be null or empty.", nameof(nodeId));
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), "Size must be positive.");

            _nodes[nodeId] = new byte[size];
        }

        /// <summary>
        /// Reads data from a simulated node.
        /// </summary>
        /// <param name="nodeId">The node identifier.</param>
        /// <param name="offset">Start offset.</param>
        /// <param name="size">Number of bytes to read.</param>
        /// <returns>A byte array containing the read data.</returns>
        public byte[] Read(string nodeId, int offset, int size)
        {
            if (!_nodes.TryGetValue(nodeId, out var buffer))
                throw new KeyNotFoundException($"Node '{nodeId}' not found.");
            if (offset < 0 || size < 0 || offset + size > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), "Read range is out of bounds.");

            var result = new byte[size];
            Array.Copy(buffer, offset, result, 0, size);
            return result;
        }

        /// <summary>
        /// Writes data to a simulated node.
        /// </summary>
        /// <param name="nodeId">The node identifier.</param>
        /// <param name="offset">Start offset.</param>
        /// <param name="data">Data to write.</param>
        public void Write(string nodeId, int offset, ReadOnlySpan<byte> data)
        {
            if (!_nodes.TryGetValue(nodeId, out var buffer))
                throw new KeyNotFoundException($"Node '{nodeId}' not found.");
            if (offset < 0 || offset + data.Length > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), "Write range is out of bounds.");

            data.CopyTo(buffer.AsSpan(offset));
        }

        /// <summary>
        /// Frees memory on a simulated node.
        /// </summary>
        /// <param name="nodeId">The node identifier.</param>
        public void Free(string nodeId)
        {
            _nodes.TryRemove(nodeId, out _);
        }
    }
}
