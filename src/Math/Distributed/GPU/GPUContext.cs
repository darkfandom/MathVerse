namespace MathVerse.Math.Distributed.GPU
{
    using System;

    /// <summary>
    /// Manages a GPU execution context bound to a specific device.
    /// A context must be initialized before any GPU operations can be performed.
    /// </summary>
    public sealed class GPUContext : IDisposable
    {
        /// <summary>
        /// The device associated with this context.
        /// </summary>
        private GPUDevice? _device;

        /// <summary>
        /// Tracks whether this context has been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Gets the device associated with this context, or null if not initialized.
        /// </summary>
        public GPUDevice? Device => _device;

        /// <summary>
        /// Gets whether this context has been initialized and is ready for use.
        /// </summary>
        public bool IsInitialized => _device is not null && !_disposed;

        /// <summary>
        /// Initializes this context for the specified GPU device.
        /// </summary>
        /// <param name="device">The GPU device to bind this context to.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="device"/> is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the device is not available or the context is already initialized.
        /// </exception>
        public void Initialize(GPUDevice device)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(GPUContext));

            if (device is null)
                throw new ArgumentNullException(nameof(device));

            if (!device.IsAvailable)
                throw new InvalidOperationException($"Device '{device.Name}' is not available.");

            if (IsInitialized)
                throw new InvalidOperationException("Context is already initialized. Dispose first.");

            _device = device;
        }

        /// <summary>
        /// Releases all resources associated with this GPU context.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _device = null;
            GC.SuppressFinalize(this);
        }
    }
}
