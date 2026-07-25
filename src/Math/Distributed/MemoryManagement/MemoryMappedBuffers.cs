namespace MathVerse.Math.Distributed.MemoryManagement
{
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Memory-mapped file abstraction using in-memory simulation.
    /// </summary>
    public sealed class MemoryMappedBuffers
    {
        private readonly ConcurrentDictionary<string, byte[]> _mappings = new();

        /// <summary>
        /// Creates a simulated memory mapping.
        /// </summary>
        /// <param name="name">Mapping name.</param>
        /// <param name="size">Size in bytes.</param>
        public void CreateMapping(string name, long size)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be null or empty.", nameof(name));
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), "Size must be positive.");

            _mappings[name] = new byte[size];
        }

        /// <summary>
        /// Gets a view of a mapping as a Memory segment.
        /// </summary>
        /// <param name="name">Mapping name.</param>
        /// <returns>A Memory view of the mapping.</returns>
        public Memory<byte> GetView(string name)
        {
            if (!_mappings.TryGetValue(name, out var buffer))
                throw new KeyNotFoundException($"Mapping '{name}' not found.");

            return new Memory<byte>(buffer);
        }

        /// <summary>
        /// Disposes and removes a mapping.
        /// </summary>
        /// <param name="name">Mapping name.</param>
        public void DisposeMapping(string name)
        {
            _mappings.TryRemove(name, out _);
        }
    }
}
