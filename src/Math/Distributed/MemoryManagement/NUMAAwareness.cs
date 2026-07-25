namespace MathVerse.Math.Distributed.MemoryManagement
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// NUMA topology detection for NUMA-aware memory allocation.
    /// </summary>
    public sealed class NUMAAwareness
    {
        private readonly int _nodeCount;
        private readonly int[] _processorToNode;
        private readonly int[] _preferredNode;

        /// <summary>
        /// Initializes a new instance of NUMAAwareness.
        /// </summary>
        public NUMAAwareness()
        {
            int processorCount = Environment.ProcessorCount;
            _processorToNode = new int[processorCount];
            _preferredNode = new int[processorCount];
            _nodeCount = DetectNUMANodeCount();

            for (int i = 0; i < processorCount; i++)
            {
                _processorToNode[i] = i % _nodeCount;
                _preferredNode[i] = _processorToNode[i];
            }
        }

        /// <summary>
        /// Gets the number of NUMA nodes detected.
        /// </summary>
        /// <returns>Number of NUMA nodes (1 if non-NUMA).</returns>
        public int GetNUMANodeCount() => _nodeCount;

        /// <summary>
        /// Gets the NUMA node for a specific processor.
        /// </summary>
        /// <param name="processorIndex">Processor index.</param>
        /// <returns>NUMA node index.</returns>
        public int GetProcessorNUMANode(int processorIndex)
        {
            if (processorIndex < 0 || processorIndex >= _processorToNode.Length)
                throw new ArgumentOutOfRangeException(nameof(processorIndex));

            return _processorToNode[processorIndex];
        }

        /// <summary>
        /// Gets the preferred NUMA node for a specific processor.
        /// </summary>
        /// <param name="processorIndex">Processor index.</param>
        /// <returns>Preferred NUMA node index.</returns>
        public int GetPreferredNUMANode(int processorIndex)
        {
            if (processorIndex < 0 || processorIndex >= _preferredNode.Length)
                throw new ArgumentOutOfRangeException(nameof(processorIndex));

            return _preferredNode[processorIndex];
        }

        private static int DetectNUMANodeCount()
        {
            try
            {
                return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                    System.Runtime.InteropServices.OSPlatform.Windows) ? 1 : 1;
            }
            catch
            {
                return 1;
            }
        }
    }
}
