namespace MathVerse.Math.Distributed.GPU
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// High-level GPU executor that orchestrates kernel execution on GPU devices,
    /// providing a simplified interface for dispatching compute workloads.
    /// </summary>
    public sealed class GPUExecutor
    {
        /// <summary>
        /// The available GPU devices.
        /// </summary>
        private readonly List<GPUDevice> _devices;

        /// <summary>
        /// The current GPU context.
        /// </summary>
        private GPUContext? _context;

        /// <summary>
        /// The command queue for this executor.
        /// </summary>
        private readonly GPUCommandQueue _commandQueue;

        /// <summary>
        /// The memory manager for this executor.
        /// </summary>
        private readonly GPUMemory _memory;

        /// <summary>
        /// Initializes a new instance of the <see cref="GPUExecutor"/> class.
        /// </summary>
        public GPUExecutor()
        {
            _devices = new List<GPUDevice>();
            _commandQueue = new GPUCommandQueue();
            _memory = new GPUMemory();
        }

        /// <summary>
        /// Gets whether GPU execution is supported (at least one device is available).
        /// </summary>
        public bool IsSupported => _devices.Count > 0;

        /// <summary>
        /// Gets the current GPU context, or null if not initialized.
        /// </summary>
        public GPUContext? Context => _context;

        /// <summary>
        /// Gets the command queue associated with this executor.
        /// </summary>
        public GPUCommandQueue CommandQueue => _commandQueue;

        /// <summary>
        /// Gets the memory manager associated with this executor.
        /// </summary>
        public GPUMemory Memory => _memory;

        /// <summary>
        /// Gets the number of registered GPU devices.
        /// </summary>
        public int DeviceCount => _devices.Count;

        /// <summary>
        /// Registers a GPU device with this executor.
        /// </summary>
        /// <param name="device">The device to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="device"/> is null.</exception>
        public void RegisterDevice(GPUDevice device)
        {
            if (device is null)
                throw new ArgumentNullException(nameof(device));

            _devices.Add(device);
        }

        /// <summary>
        /// Gets the best available device based on available memory and device type preference.
        /// Prefers discrete GPUs, then integrated, then virtual.
        /// </summary>
        /// <returns>The best available device, or null if no devices are registered.</returns>
        public GPUDevice? GetBestDevice()
        {
            GPUDevice? best = null;

            for (int i = 0; i < _devices.Count; i++)
            {
                GPUDevice device = _devices[i];
                if (!device.IsAvailable)
                    continue;

                if (best is null)
                {
                    best = device;
                    continue;
                }

                if (device.DeviceType == DeviceType.Discrete && best.DeviceType != DeviceType.Discrete)
                {
                    best = device;
                }
                else if (device.DeviceType == best.DeviceType && device.FreeMemory > best.FreeMemory)
                {
                    best = device;
                }
            }

            return best;
        }

        /// <summary>
        /// Gets a device by its ID.
        /// </summary>
        /// <param name="deviceId">The device ID to look up.</param>
        /// <returns>The device with the matching ID, or null if not found.</returns>
        public GPUDevice? GetDevice(int deviceId)
        {
            for (int i = 0; i < _devices.Count; i++)
            {
                if (_devices[i].DeviceId == deviceId)
                    return _devices[i];
            }

            return null;
        }

        /// <summary>
        /// Initializes the executor with the best available device or a specific device.
        /// </summary>
        /// <param name="deviceId">
        /// The device ID to initialize with. If -1, uses the best available device.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no suitable device is available.
        /// </exception>
        public void Initialize(int deviceId = -1)
        {
            GPUDevice? device = deviceId == -1
                ? GetBestDevice()
                : GetDevice(deviceId);

            if (device is null)
                throw new InvalidOperationException("No suitable GPU device available.");

            _context = new GPUContext();
            _context.Initialize(device);
        }

        /// <summary>
        /// Executes a kernel on the GPU with the specified buffers and global work size.
        /// </summary>
        /// <param name="kernel">The kernel to execute.</param>
        /// <param name="buffers">The buffer arguments to pass to the kernel.</param>
        /// <param name="globalSize">
        /// The global work size as a 1D, 2D, or 3D array.
        /// </param>
        /// <returns>An execution result containing timing and output information.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="kernel"/> or <paramref name="buffers"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown when the executor is not initialized.</exception>
        public GPUExecutionResult ExecuteKernel(GPUKernel kernel, GPUBuffer[] buffers, int[] globalSize)
        {
            if (kernel is null)
                throw new ArgumentNullException(nameof(kernel));
            if (buffers is null)
                throw new ArgumentNullException(nameof(buffers));

            if (_context is null || !_context.IsInitialized)
                throw new InvalidOperationException("GPUExecutor is not initialized. Call Initialize first.");

            Stopwatch sw = Stopwatch.StartNew();

            kernel.GlobalWorkSize = globalSize;

            _commandQueue.EnqueueKernel(kernel, buffers);
            _commandQueue.Flush();
            _commandQueue.WaitForCompletion();

            sw.Stop();

            return GPUExecutionResult.CreateSuccess(
                sw.Elapsed.TotalMilliseconds,
                null);
        }

        /// <summary>
        /// Executes a kernel with automatically determined global work size based on buffer sizes.
        /// </summary>
        /// <param name="kernel">The kernel to execute.</param>
        /// <param name="buffers">The buffer arguments to pass to the kernel.</param>
        /// <param name="elementCount">The number of elements to process.</param>
        /// <returns>An execution result containing timing and output information.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="kernel"/> or <paramref name="buffers"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="elementCount"/> is non-positive.
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown when the executor is not initialized.</exception>
        public GPUExecutionResult ExecuteKernelAuto(GPUKernel kernel, GPUBuffer[] buffers, int elementCount)
        {
            if (elementCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(elementCount), "Element count must be positive.");

            int localSize = kernel.LocalWorkSize[0];
            int globalSize = ((elementCount + localSize - 1) / localSize) * localSize;

            return ExecuteKernel(kernel, buffers, new int[] { globalSize });
        }

        /// <summary>
        /// Releases all resources associated with this executor.
        /// </summary>
        public void Dispose()
        {
            _context?.Dispose();
            _context = null;
        }
    }
}
