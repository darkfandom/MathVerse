namespace MathVerse.Math.Distributed.GPU
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Manages a queue of GPU commands (kernel launches, barriers, memory operations)
    /// that are submitted for asynchronous execution on the device.
    /// </summary>
    public sealed class GPUCommandQueue
    {
        /// <summary>
        /// The internal command queue.
        /// </summary>
        private readonly Queue<GPUCommand> _commands;

        /// <summary>
        /// Lock object for thread-safe queue access.
        /// </summary>
        private readonly object _lock;

#pragma warning disable CS0414
        private bool _hasPendingWork;
#pragma warning restore CS0414

        /// <summary>
        /// Initializes a new instance of the <see cref="GPUCommandQueue"/> class.
        /// </summary>
        public GPUCommandQueue()
        {
            _commands = new Queue<GPUCommand>();
            _lock = new object();
            _hasPendingWork = false;
        }

        /// <summary>
        /// Gets the number of commands currently queued.
        /// </summary>
        public int PendingCommandCount
        {
            get
            {
                lock (_lock)
                {
                    return _commands.Count;
                }
            }
        }

        /// <summary>
        /// Gets whether there are any pending commands in the queue.
        /// </summary>
        public bool HasPendingCommands
        {
            get
            {
                lock (_lock)
                {
                    return _commands.Count > 0;
                }
            }
        }

        /// <summary>
        /// Enqueues a kernel execution command with the specified kernel and argument buffers.
        /// </summary>
        /// <param name="kernel">The kernel to execute.</param>
        /// <param name="args">The buffer arguments to pass to the kernel.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="kernel"/> or <paramref name="args"/> is null.
        /// </exception>
        public void EnqueueKernel(GPUKernel kernel, GPUBuffer[] args)
        {
            if (kernel is null)
                throw new ArgumentNullException(nameof(kernel));
            if (args is null)
                throw new ArgumentNullException(nameof(args));

            lock (_lock)
            {
                _commands.Enqueue(new GPUCommand
                {
                    Type = GPUCommandType.Kernel,
                    Kernel = kernel,
                    Buffers = args
                });
                _hasPendingWork = true;
            }
        }

        /// <summary>
        /// Enqueues a barrier command that ensures all previous commands complete
        /// before subsequent commands begin execution.
        /// </summary>
        public void EnqueueBarrier()
        {
            lock (_lock)
            {
                _commands.Enqueue(new GPUCommand
                {
                    Type = GPUCommandType.Barrier
                });
                _hasPendingWork = true;
            }
        }

        /// <summary>
        /// Submits all queued commands to the device for execution.
        /// After flushing, the internal queue is cleared.
        /// </summary>
        /// <returns>The number of commands that were flushed.</returns>
        public int Flush()
        {
            lock (_lock)
            {
                int count = _commands.Count;
                _commands.Clear();
                _hasPendingWork = false;
                return count;
            }
        }

        /// <summary>
        /// Waits for all submitted commands to complete execution.
        /// In this abstract implementation, this is a no-op since there is no real device.
        /// </summary>
        public void WaitForCompletion()
        {
            lock (_lock)
            {
                _hasPendingWork = false;
            }
        }

        /// <summary>
        /// Returns all queued commands as an array without clearing the queue.
        /// </summary>
        /// <returns>An array of pending commands.</returns>
        public GPUCommand[] PeekAll()
        {
            lock (_lock)
            {
                return _commands.ToArray();
            }
        }

        /// <summary>
        /// Represents a single command in the GPU command queue.
        /// </summary>
        public sealed class GPUCommand
        {
            /// <summary>
            /// Gets or sets the type of this command.
            /// </summary>
            public GPUCommandType Type { get; set; }

            /// <summary>
            /// Gets or sets the kernel for kernel execution commands.
            /// </summary>
            public GPUKernel? Kernel { get; set; }

            /// <summary>
            /// Gets or sets the buffer arguments for kernel execution commands.
            /// </summary>
            public GPUBuffer[] Buffers { get; set; } = Array.Empty<GPUBuffer>();
        }

        /// <summary>
        /// Represents the type of a GPU command.
        /// </summary>
        public enum GPUCommandType
        {
            /// <summary>A kernel execution command.</summary>
            Kernel,

            /// <summary>A synchronization barrier command.</summary>
            Barrier
        }
    }
}
