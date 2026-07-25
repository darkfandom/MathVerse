namespace MathVerse.Math.Distributed.GPU
{
    using System;

    /// <summary>
    /// Represents the type of GPU device available in the system.
    /// </summary>
    public enum DeviceType
    {
        /// <summary>A dedicated discrete GPU with its own memory.</summary>
        Discrete,

        /// <summary>An integrated GPU sharing system memory with the CPU.</summary>
        Integrated,

        /// <summary>A virtual or emulated GPU device.</summary>
        Virtual,

        /// <summary>An unknown or unrecognized device type.</summary>
        Unknown
    }

    /// <summary>
    /// Abstract representation of a GPU device providing hardware information
    /// and capability queries.
    /// </summary>
    public sealed class GPUDevice
    {
        /// <summary>
        /// Gets or sets the unique identifier for this device.
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the human-readable name of this device.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the compute capability version of this device (e.g., "8.6").
        /// </summary>
        public string ComputeCapability { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total available device memory in bytes.
        /// </summary>
        public long MaxMemory { get; set; }

        /// <summary>
        /// Gets or sets the amount of device memory currently in use in bytes.
        /// </summary>
        public long CurrentMemoryUsed { get; set; }

        /// <summary>
        /// Gets or sets whether this device is currently available for computation.
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Gets or sets the type of this GPU device.
        /// </summary>
        public DeviceType DeviceType { get; set; } = DeviceType.Unknown;

        /// <summary>
        /// Gets the amount of free device memory in bytes.
        /// </summary>
        public long FreeMemory => MaxMemory - CurrentMemoryUsed;

        /// <summary>
        /// Gets the memory utilization as a value between 0.0 and 1.0.
        /// </summary>
        public double MemoryUtilization =>
            MaxMemory > 0 ? (double)CurrentMemoryUsed / MaxMemory : 0.0;

        /// <summary>
        /// Returns a string representation of this device.
        /// </summary>
        /// <returns>A string containing the device name, type, and memory information.</returns>
        public override string ToString()
        {
            return $"{Name} ({DeviceType}) - {FreeMemory / (1024 * 1024)}MB free / {MaxMemory / (1024 * 1024)}MB total";
        }
    }
}
