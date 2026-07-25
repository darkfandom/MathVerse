namespace MathVerse.Math.Distributed.Parallel
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A work-stealing task scheduler that distributes tasks across a fixed thread pool
    /// using thread-local deques with work stealing from other threads' queues.
    /// </summary>
    public sealed class WorkStealingScheduler : TaskScheduler, IDisposable
    {
        /// <summary>
        /// The fixed number of threads managed by this scheduler.
        /// </summary>
        private readonly int _threadCount;

        /// <summary>
        /// The thread-local work deques, indexed by thread.
        /// </summary>
        private readonly ConcurrentBag<ConcurrentQueue<Task>> _queues;

        /// <summary>
        /// The worker threads.
        /// </summary>
        private readonly Thread[] _threads;

        /// <summary>
        /// Signal to stop the worker threads.
        /// </summary>
        private readonly ManualResetEventSlim _shutdownSignal;

        /// <summary>
        /// Lock for the global fallback queue.
        /// </summary>
        private readonly object _globalLock;

        /// <summary>
        /// Global fallback queue for tasks that cannot be placed in a local deque.
        /// </summary>
        private readonly Queue<Task> _globalQueue;

        /// <summary>
        /// The current thread index mapping.
        /// </summary>
        private readonly ThreadLocal<int> _threadIndex;

        /// <summary>
        /// Tracks whether this scheduler has been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkStealingScheduler"/> class
        /// with the specified number of worker threads.
        /// </summary>
        /// <param name="threadCount">
        /// The number of worker threads. Uses <see cref="Environment.ProcessorCount"/> if -1.
        /// </param>
        public WorkStealingScheduler(int threadCount = -1)
        {
            _threadCount = threadCount == -1
                ? Environment.ProcessorCount
                : threadCount;

            if (_threadCount < 1)
                throw new ArgumentOutOfRangeException(nameof(threadCount), "Thread count must be positive.");

            _queues = new ConcurrentBag<ConcurrentQueue<Task>>();
            _threads = new Thread[_threadCount];
            _shutdownSignal = new ManualResetEventSlim(false);
            _globalLock = new object();
            _globalQueue = new Queue<Task>();
            _threadIndex = new ThreadLocal<int>(false);

            for (int i = 0; i < _threadCount; i++)
            {
                _queues.Add(new ConcurrentQueue<Task>());
            }

            for (int i = 0; i < _threadCount; i++)
            {
                int index = i;
                _threads[i] = new Thread(WorkerLoop)
                {
                    IsBackground = true,
                    Name = $"WorkStealingWorker-{index}"
                };
                _threads[i].Start(index);
            }
        }

        /// <summary>
        /// Gets the number of worker threads in this scheduler.
        /// </summary>
        public int ThreadCount => _threadCount;

        /// <summary>
        /// Gets the approximate number of tasks waiting to be executed across all queues.
        /// </summary>
        public int PendingTaskCount
        {
            get
            {
                int count = _globalQueue.Count;
                foreach (ConcurrentQueue<Task> queue in _queues)
                {
                    count += queue.Count;
                }
                return count;
            }
        }

        /// <summary>
        /// Gets the maximum concurrency level of this scheduler.
        /// </summary>
        public override int MaximumConcurrencyLevel => _threadCount;

        /// <summary>
        /// Queues a task to the scheduler.
        /// </summary>
        /// <param name="task">The task to queue.</param>
        protected override void QueueTask(Task task)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(WorkStealingScheduler));

            int? index = _threadIndex.Value;
            if (index.HasValue && index.Value >= 0 && index.Value < _threadCount)
            {
                _queues.ElementAt(index.Value).Enqueue(task);
            }
            else
            {
                lock (_globalLock)
                {
                    _globalQueue.Enqueue(task);
                }
            }
        }

        /// <summary>
        /// Attempts to inline the queued task on the current thread, if possible.
        /// </summary>
        /// <param name="task">The task to execute.</param>
        /// <param name="taskWasPreviouslyQueued">Whether the task was previously queued.</param>
        /// <returns>True if the task was inlined; otherwise, false.</returns>
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (taskWasPreviouslyQueued)
                return false;

            int? index = _threadIndex.Value;
            if (!index.HasValue)
                return false;

            return TryExecuteTask(task);
        }

        /// <summary>
        /// Returns all tasks currently scheduled.
        /// </summary>
        /// <returns>An enumerable of all queued tasks.</returns>
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            List<Task> allTasks = new List<Task>();

            lock (_globalLock)
            {
                allTasks.AddRange(_globalQueue);
            }

            foreach (ConcurrentQueue<Task> queue in _queues)
            {
                allTasks.AddRange(queue);
            }

            return allTasks;
        }

        /// <summary>
        /// The worker thread loop that processes tasks from local and stolen queues.
        /// </summary>
        /// <param name="state">The thread index as a boxed integer.</param>
        private void WorkerLoop(object? state)
        {
            int myIndex = (int)state!;
            _threadIndex.Value = myIndex;

            while (!_shutdownSignal.IsSet)
            {
                Task? task = TryGetTask(myIndex);

                if (task is not null)
                {
                    TryExecuteTask(task);
                }
                else
                {
                    Thread.Sleep(1);
                }
            }

            while (TryGetTask(myIndex) is Task remainingTask)
            {
                TryExecuteTask(remainingTask);
            }
        }

        /// <summary>
        /// Attempts to get a task from the local queue, global queue, or by stealing from others.
        /// </summary>
        /// <param name="myIndex">The index of the current thread.</param>
        /// <returns>A task to execute, or null if none found.</returns>
        private Task? TryGetTask(int myIndex)
        {
            if (myIndex >= 0 && myIndex < _threadCount)
            {
                ConcurrentQueue<Task> localQueue = _queues.ElementAt(myIndex);
                if (localQueue.TryDequeue(out Task? localTask))
                    return localTask;
            }

            lock (_globalLock)
            {
                if (_globalQueue.Count > 0)
                    return _globalQueue.Dequeue();
            }

            for (int i = 0; i < _threadCount; i++)
            {
                if (i == myIndex)
                    continue;

                ConcurrentQueue<Task> otherQueue = _queues.ElementAt(i);
                if (otherQueue.TryDequeue(out Task? stolenTask))
                    return stolenTask;
            }

            return null;
        }

        /// <summary>
        /// Disposes the scheduler and joins all worker threads.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _shutdownSignal.Set();

            for (int i = 0; i < _threads.Length; i++)
            {
                _threads[i].Join();
            }

            _shutdownSignal.Dispose();
            _threadIndex.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
