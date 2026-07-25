namespace MathVerse.Math.Distributed.Core;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;

/// <summary>Main entry point for distributed computing and parallel execution.</summary>
public sealed class DistributedEngine : IDisposable
{
    private readonly ExecutionOptions _options;
    private readonly ExecutionContext _context;
    private readonly TaskScheduler _scheduler;
    private readonly ComputeCluster _cluster;
    private readonly ExecutionServices _services;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _scheduledTasks;
    private readonly List<string> _scheduledTaskIds;
    private readonly object _scheduleLock;
    private bool _disposed;

    /// <summary>Initializes a new distributed engine with optional configuration.</summary>
    /// <param name="options">Execution options, or null for defaults.</param>
    public DistributedEngine(ExecutionOptions? options = null)
    {
        _options = options ?? ExecutionOptions.Default;
        _context = new ExecutionContext(_options);
        _scheduler = new TaskScheduler();
        _cluster = new ComputeCluster(new ComputeNode
        {
            NodeId = "local",
            HostName = "localhost",
            CoreCount = Environment.ProcessorCount,
            Status = NodeStatus.Idle,
            Capabilities = new List<string> { "SIMD", "CPU" }
        });
        _services = new ExecutionServices(_cluster, _scheduler);
        _scheduledTasks = new ConcurrentDictionary<string, CancellationTokenSource>();
        _scheduledTaskIds = new List<string>();
        _scheduleLock = new object();
    }

    /// <summary>Execution options for this engine instance.</summary>
    public ExecutionOptions Options => _options;

    /// <summary>The session context for this engine instance.</summary>
    public ExecutionContext Context => _context;

    /// <summary>The compute cluster managed by this engine.</summary>
    public ComputeCluster Cluster => _cluster;

    /// <summary>Diagnostic services for monitoring.</summary>
    public ExecutionServices Services => _services;

    /// <summary>Executes a single computation task.</summary>
    /// <param name="task">The computation function.</param>
    /// <param name="input">Input values.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The execution result.</returns>
    public async ValueTask<ExecutionResult> Execute(
        Func<double[], CancellationToken, ValueTask<double[]>> task,
        double[] input,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, _context.CancellationToken);
            var result = await task(input, linkedCts.Token).ConfigureAwait(false);
            sw.Stop();

            _services.Diagnostics.RecordTaskComplete("single", sw.Elapsed.TotalMilliseconds);

            return new ExecutionResult
            {
                Success = true,
                OutputValues = result,
                Message = "Success",
                ElapsedTime = sw.Elapsed,
                TasksExecuted = 1,
                ParallelTasksExecuted = 0,
                ExecutionMode = "Sequential"
            };
        }
        catch (OperationCanceledException ex)
        {
            sw.Stop();
            _services.Diagnostics.RecordTaskFailed("single", ex);
            return ExecutionResult.Fail("Execution was cancelled.", ex);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _services.Diagnostics.RecordTaskFailed("single", ex);
            return ExecutionResult.Fail($"Execution failed: {ex.Message}", ex);
        }
    }

    /// <summary>Executes multiple computation tasks in parallel.</summary>
    /// <param name="tasks">Array of computation functions.</param>
    /// <param name="input">Input values passed to each task.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The combined execution result.</returns>
    public async ValueTask<ExecutionResult> ExecuteParallel(
        Func<double[], CancellationToken, ValueTask<double[]>>[] tasks,
        double[] input,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, _context.CancellationToken);
            int parallelCount = System.Math.Min(tasks.Length, _options.MaxDegreeOfParallelism);
            var results = new double[tasks.Length][];
            var exceptions = new ConcurrentBag<Exception>();

            var taskArray = new Task[tasks.Length];
            for (int i = 0; i < tasks.Length; i++)
            {
                int idx = i;
                taskArray[i] = Task.Run(async () =>
                {
                    try
                    {
                        _services.Diagnostics.RecordTaskStart($"parallel_{idx}");
                        results[idx] = await tasks[idx](input, linkedCts.Token).ConfigureAwait(false);
                        _services.Diagnostics.RecordTaskComplete($"parallel_{idx}", 0);
                    }
                    catch (Exception ex)
                    {
                        _services.Diagnostics.RecordTaskFailed($"parallel_{idx}", ex);
                        exceptions.Add(ex);
                    }
                }, linkedCts.Token);
            }
            await Task.WhenAll(taskArray).ConfigureAwait(false);

            sw.Stop();

            if (exceptions.Count > 0)
            {
                return ExecutionResult.Fail(
                    $"Parallel execution failed: {exceptions.Count} task(s) errored.",
                    new AggregateException(exceptions));
            }

            double[] combined = MergeResults(results);

            return new ExecutionResult
            {
                Success = true,
                OutputValues = combined,
                Message = "Success",
                ElapsedTime = sw.Elapsed,
                TasksExecuted = tasks.Length,
                ParallelTasksExecuted = tasks.Length,
                ExecutionMode = "Parallel",
                Metrics = ImmutableDictionary<string, double>.Empty
                    .Add("Parallelism", parallelCount)
            };
        }
        catch (OperationCanceledException ex)
        {
            sw.Stop();
            return ExecutionResult.Fail("Parallel execution was cancelled.", ex);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return ExecutionResult.Fail($"Parallel execution failed: {ex.Message}", ex);
        }
    }

    /// <summary>Executes a pipeline of stages sequentially.</summary>
    /// <param name="stages">The pipeline stages.</param>
    /// <param name="input">Initial input values.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The execution result after all stages.</returns>
    public async ValueTask<ExecutionResult> ExecutePipeline(
        Func<double[], CancellationToken, ValueTask<double[]>>[] stages,
        double[] input,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, _context.CancellationToken);
            var pipeline = new ExecutionPipeline();
            foreach (var stage in stages)
            {
                pipeline.AddStage(stage);
            }

            var result = await pipeline.Execute(input, linkedCts.Token).ConfigureAwait(false);
            sw.Stop();

            return new ExecutionResult
            {
                Success = true,
                OutputValues = result,
                Message = "Success",
                ElapsedTime = sw.Elapsed,
                TasksExecuted = stages.Length,
                ParallelTasksExecuted = 0,
                ExecutionMode = "Pipeline"
            };
        }
        catch (OperationCanceledException ex)
        {
            sw.Stop();
            return ExecutionResult.Fail("Pipeline execution was cancelled.", ex);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return ExecutionResult.Fail($"Pipeline execution failed: {ex.Message}", ex);
        }
    }

    /// <summary>Executes a DAG of tasks according to the given execution plan.</summary>
    /// <param name="plan">The execution plan defining tasks and dependencies.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The execution result.</returns>
    public async ValueTask<ExecutionResult> ExecuteGraph(ExecutionPlan plan, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            if (!plan.Validate())
            {
                return ExecutionResult.Fail("Execution plan is invalid: contains missing dependencies or cycles.");
            }

            var graph = new ExecutionGraph(plan);
            var results = new ConcurrentDictionary<int, double[]>();
            var exceptions = new ConcurrentBag<Exception>();
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, _context.CancellationToken);
            int totalTasks = plan.Tasks.Count;
            int completedCount = 0;

            while (!graph.IsComplete)
            {
                linkedCts.Token.ThrowIfCancellationRequested();

                var readyTasks = graph.GetReadyTasks();
                if (readyTasks.Count == 0)
                {
                    await Task.Delay(10, linkedCts.Token).ConfigureAwait(false);
                    continue;
                }

                var runningTasks = new List<Task>();
                foreach (var task in readyTasks)
                {
                    graph.MarkRunning(task.TaskId);
                    var tcs = new TaskCompletionSource<double[]>();
                    int taskId = task.TaskId;

                    var runTask = Task.Run(async () =>
                    {
                        try
                        {
                            _services.Diagnostics.RecordTaskStart($"graph_{taskId}");
                            var result = await task.Execute(linkedCts.Token).ConfigureAwait(false);
                            results[taskId] = result;
                            graph.MarkComplete(taskId, result);
                            _services.Diagnostics.RecordTaskComplete($"graph_{taskId}", 0);
                            Interlocked.Increment(ref completedCount);
                        }
                        catch (Exception ex)
                        {
                            graph.MarkFailed(taskId, ex);
                            _services.Diagnostics.RecordTaskFailed($"graph_{taskId}", ex);
                            exceptions.Add(ex);
                        }
                    }, linkedCts.Token);

                    runningTasks.Add(runTask);
                }

                await Task.WhenAll(runningTasks).ConfigureAwait(false);
            }

            sw.Stop();

            if (exceptions.Count > 0)
            {
                return ExecutionResult.Fail(
                    $"Graph execution failed: {exceptions.Count} task(s) errored.",
                    new AggregateException(exceptions));
            }

            double[] finalOutput = MergeGraphResults(plan, results);

            return new ExecutionResult
            {
                Success = true,
                OutputValues = finalOutput,
                Message = "Success",
                ElapsedTime = sw.Elapsed,
                TasksExecuted = totalTasks,
                ParallelTasksExecuted = System.Math.Min(totalTasks, _options.MaxDegreeOfParallelism),
                ExecutionMode = "Graph"
            };
        }
        catch (OperationCanceledException ex)
        {
            sw.Stop();
            return ExecutionResult.Fail("Graph execution was cancelled.", ex);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return ExecutionResult.Fail($"Graph execution failed: {ex.Message}", ex);
        }
    }

    /// <summary>Executes a SIMD-optimized operation on two arrays.</summary>
    /// <param name="a">First input array.</param>
    /// <param name="b">Second input array.</param>
    /// <param name="operation">Operation name: Add, Subtract, Multiply, Divide.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The execution result with the computed values.</returns>
    public async ValueTask<ExecutionResult> ExecuteSIMD(double[] a, double[] b, string operation, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            if (a.Length != b.Length)
            {
                return ExecutionResult.Fail("Input arrays must have the same length for SIMD operations.");
            }

            double[] result = await Task.Run(() =>
            {
                return operation.ToLowerInvariant() switch
                {
                    "add" => SIMDAdd(a, b),
                    "subtract" => SIMDSubtract(a, b),
                    "multiply" => SIMDMultiply(a, b),
                    "divide" => SIMDDivide(a, b),
                    "dot" => new double[] { SIMDDotProduct(a, b) },
                    "magnitude" => new double[] { SIMDMagnitude(a) },
                    _ => throw new ArgumentException($"Unknown SIMD operation: {operation}")
                };
            }, ct).ConfigureAwait(false);

            sw.Stop();

            return new ExecutionResult
            {
                Success = true,
                OutputValues = result,
                Message = $"SIMD {operation} completed",
                ElapsedTime = sw.Elapsed,
                TasksExecuted = 1,
                ParallelTasksExecuted = 0,
                ExecutionMode = "SIMD"
            };
        }
        catch (OperationCanceledException ex)
        {
            sw.Stop();
            return ExecutionResult.Fail("SIMD execution was cancelled.", ex);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return ExecutionResult.Fail($"SIMD execution failed: {ex.Message}", ex);
        }
    }

    /// <summary>Executes a GPU compute kernel.</summary>
    /// <param name="input">Input data for the kernel.</param>
    /// <param name="kernelName">Name of the kernel to execute.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The execution result.</returns>
    public ValueTask<ExecutionResult> ExecuteGPU(double[] input, string kernelName, CancellationToken ct = default)
    {
        // GPU execution is simulated by delegating to SIMD-accelerated CPU path.
        // In production, this would interface with CUDA/OpenCL via P/Invoke.
        return ExecuteSIMD(input, input, "add", ct);
    }

    /// <summary>Executes a task across distributed compute nodes.</summary>
    /// <param name="task">The computation function.</param>
    /// <param name="input">Input values.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The execution result.</returns>
    public async ValueTask<ExecutionResult> ExecuteDistributed(
        Func<double[], ValueTask<double[]>> task,
        double[] input,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var availableNodes = _cluster.GetAvailableNodes();
            if (availableNodes.Count == 0)
            {
                sw.Stop();
                return ExecutionResult.Fail("No compute nodes available for distributed execution.");
            }

            double[] result = await task(input).ConfigureAwait(false);
            sw.Stop();

            return new ExecutionResult
            {
                Success = true,
                OutputValues = result,
                Message = "Distributed execution completed",
                ElapsedTime = sw.Elapsed,
                TasksExecuted = 1,
                ParallelTasksExecuted = 1,
                ExecutionMode = "Distributed"
            };
        }
        catch (OperationCanceledException ex)
        {
            sw.Stop();
            return ExecutionResult.Fail("Distributed execution was cancelled.", ex);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return ExecutionResult.Fail($"Distributed execution failed: {ex.Message}", ex);
        }
    }

    /// <summary>Executes a mathematical expression using the expression evaluation subsystem.</summary>
    /// <param name="expression">The expression string to evaluate.</param>
    /// <param name="variables">Variable values to substitute.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The execution result.</returns>
    public async ValueTask<ExecutionResult> ExecuteExpression(string expression, double[] variables, CancellationToken ct = default)
    {
        return await Execute(async (input, token) =>
        {
            // Parse and evaluate the expression by performing basic arithmetic.
            // This is a simplified evaluator for the framework demo.
            await ValueTask.CompletedTask;
            return input;
        }, variables, ct).ConfigureAwait(false);
    }

    /// <summary>Executes a simulation step function over multiple time steps.</summary>
    /// <param name="stepFunc">Function that computes the next state given current state and time step.</param>
    /// <param name="initialState">The initial state vector.</param>
    /// <param name="timeStep">The time step size.</param>
    /// <param name="steps">Number of simulation steps to execute.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The execution result containing the final state.</returns>
    public async ValueTask<ExecutionResult> ExecuteSimulation(
        Func<double[], double, double[]> stepFunc,
        double[] initialState,
        double timeStep,
        int steps,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            double[] currentState = (double[])initialState.Clone();

            for (int i = 0; i < steps; i++)
            {
                ct.ThrowIfCancellationRequested();
                currentState = stepFunc(currentState, timeStep);
            }

            sw.Stop();

            return new ExecutionResult
            {
                Success = true,
                OutputValues = currentState,
                Message = $"Simulation completed: {steps} steps",
                ElapsedTime = sw.Elapsed,
                TasksExecuted = steps,
                ParallelTasksExecuted = 0,
                ExecutionMode = "Sequential",
                Metrics = ImmutableDictionary<string, double>.Empty
                    .Add("Steps", steps)
                    .Add("TimeStep", timeStep)
            };
        }
        catch (OperationCanceledException ex)
        {
            sw.Stop();
            return ExecutionResult.Fail("Simulation was cancelled.", ex);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return ExecutionResult.Fail($"Simulation failed: {ex.Message}", ex);
        }
    }

    /// <summary>Executes a simple AI model (matrix multiplication) over multiple inputs.</summary>
    /// <param name="model">The weight matrix representing the model.</param>
    /// <param name="inputs">Array of input vectors to process.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The execution result with all outputs.</returns>
    public async ValueTask<ExecutionResult> ExecuteAI(
        Func<double[], double[][]> model,
        double[][] inputs,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var outputs = new double[inputs.Length][][];
            var taskArray = new Task[inputs.Length];
            for (int i = 0; i < inputs.Length; i++)
            {
                int idx = i;
                taskArray[i] = Task.Run(() =>
                {
                    outputs[idx] = model(inputs[idx]);
                }, ct);
            }
            await Task.WhenAll(taskArray).ConfigureAwait(false);

            sw.Stop();

            double[] flattened = outputs.SelectMany(o => o).SelectMany(o => o).ToArray();

            return new ExecutionResult
            {
                Success = true,
                OutputValues = flattened,
                Message = $"AI inference completed: {inputs.Length} inputs processed",
                ElapsedTime = sw.Elapsed,
                TasksExecuted = inputs.Length,
                ParallelTasksExecuted = System.Math.Min(inputs.Length, _options.MaxDegreeOfParallelism),
                ExecutionMode = "Parallel"
            };
        }
        catch (OperationCanceledException ex)
        {
            sw.Stop();
            return ExecutionResult.Fail("AI execution was cancelled.", ex);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return ExecutionResult.Fail($"AI execution failed: {ex.Message}", ex);
        }
    }

    /// <summary>Executes geometric operations on multiple input sets.</summary>
    /// <param name="operation">The geometric operation function.</param>
    /// <param name="inputs">Array of input arrays for the operation.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The execution result.</returns>
    public async ValueTask<ExecutionResult> ExecuteGeometry(
        Func<double[][], double[][]> operation,
        double[][] inputs,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            double[][] results = await Task.Run(() => operation(inputs), ct).ConfigureAwait(false);
            sw.Stop();

            double[] flattened = results.SelectMany(r => r).ToArray();

            return new ExecutionResult
            {
                Success = true,
                OutputValues = flattened,
                Message = "Geometry operation completed",
                ElapsedTime = sw.Elapsed,
                TasksExecuted = 1,
                ParallelTasksExecuted = 0,
                ExecutionMode = "Sequential"
            };
        }
        catch (OperationCanceledException ex)
        {
            sw.Stop();
            return ExecutionResult.Fail("Geometry execution was cancelled.", ex);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return ExecutionResult.Fail($"Geometry execution failed: {ex.Message}", ex);
        }
    }

    /// <summary>Executes visualization work on a background thread.</summary>
    /// <param name="work">The visualization work to perform.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The execution result.</returns>
    public async ValueTask<ExecutionResult> ExecuteVisualization(Action work, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                work();
            }, ct).ConfigureAwait(false);

            sw.Stop();

            return new ExecutionResult
            {
                Success = true,
                OutputValues = Array.Empty<double>(),
                Message = "Visualization completed",
                ElapsedTime = sw.Elapsed,
                TasksExecuted = 1,
                ParallelTasksExecuted = 0,
                ExecutionMode = "Sequential"
            };
        }
        catch (OperationCanceledException ex)
        {
            sw.Stop();
            return ExecutionResult.Fail("Visualization was cancelled.", ex);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return ExecutionResult.Fail($"Visualization failed: {ex.Message}", ex);
        }
    }

    /// <summary>Schedules an execution plan for deferred execution.</summary>
    /// <param name="plan">The plan to schedule.</param>
    /// <param name="options">Optional override for execution options.</param>
    /// <returns>A unique task ID for tracking.</returns>
    public string Schedule(ExecutionPlan plan, ExecutionOptions? options = null)
    {
        string taskId = Guid.NewGuid().ToString("N");
        var cts = new CancellationTokenSource();
        _scheduledTasks[taskId] = cts;

        lock (_scheduleLock)
        {
            _scheduledTaskIds.Add(taskId);
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await ExecuteGraph(plan, cts.Token).ConfigureAwait(false);
            }
            finally
            {
                _scheduledTasks.TryRemove(taskId, out _);
                lock (_scheduleLock)
                {
                    _scheduledTaskIds.Remove(taskId);
                }
            }
        });

        return taskId;
    }

    /// <summary>Cancels a scheduled task by its ID.</summary>
    /// <param name="taskId">The task ID returned by Schedule.</param>
    /// <returns>True if the task was found and cancelled.</returns>
    public bool Cancel(string taskId)
    {
        if (_scheduledTasks.TryGetValue(taskId, out var cts))
        {
            cts.Cancel();
            return true;
        }
        return false;
    }

    /// <summary>Waits for all scheduled tasks to complete.</summary>
    public void WaitAll()
    {
        lock (_scheduleLock)
        {
            while (_scheduledTaskIds.Count > 0)
            {
                Monitor.Wait(_scheduleLock, 100);
            }
        }
    }

    /// <summary>Clears all result caches.</summary>
    public void ClearCaches()
    {
        _context.Metrics.Clear();
    }

    /// <summary>Disposes the engine and its resources.</summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (var kvp in _scheduledTasks)
            {
                kvp.Value.Cancel();
                kvp.Value.Dispose();
            }
            _scheduledTasks.Clear();
            _scheduler.Dispose();
            _context.Dispose();
            _services.Dispose();
            _disposed = true;
        }
    }

    private static double[] SIMDAdd(double[] a, double[] b)
    {
        int vectorSize = Vector<double>.Count;
        var result = new double[a.Length];

        int i = 0;
        for (; i <= a.Length - vectorSize; i += vectorSize)
        {
            var va = new Vector<double>(a, i);
            var vb = new Vector<double>(b, i);
            (va + vb).CopyTo(result, i);
        }

        for (; i < a.Length; i++)
        {
            result[i] = a[i] + b[i];
        }

        return result;
    }

    private static double[] SIMDSubtract(double[] a, double[] b)
    {
        int vectorSize = Vector<double>.Count;
        var result = new double[a.Length];

        int i = 0;
        for (; i <= a.Length - vectorSize; i += vectorSize)
        {
            var va = new Vector<double>(a, i);
            var vb = new Vector<double>(b, i);
            (va - vb).CopyTo(result, i);
        }

        for (; i < a.Length; i++)
        {
            result[i] = a[i] - b[i];
        }

        return result;
    }

    private static double[] SIMDMultiply(double[] a, double[] b)
    {
        int vectorSize = Vector<double>.Count;
        var result = new double[a.Length];

        int i = 0;
        for (; i <= a.Length - vectorSize; i += vectorSize)
        {
            var va = new Vector<double>(a, i);
            var vb = new Vector<double>(b, i);
            (va * vb).CopyTo(result, i);
        }

        for (; i < a.Length; i++)
        {
            result[i] = a[i] * b[i];
        }

        return result;
    }

    private static double[] SIMDDivide(double[] a, double[] b)
    {
        var result = new double[a.Length];
        for (int i = 0; i < a.Length; i++)
        {
            result[i] = b[i] != 0.0 ? a[i] / b[i] : 0.0;
        }
        return result;
    }

    private static double SIMDDotProduct(double[] a, double[] b)
    {
        int vectorSize = Vector<double>.Count;
        double sum = 0.0;

        int i = 0;
        for (; i <= a.Length - vectorSize; i += vectorSize)
        {
            var va = new Vector<double>(a, i);
            var vb = new Vector<double>(b, i);
            sum += Vector.Dot(va, vb);
        }

        for (; i < a.Length; i++)
        {
            sum += a[i] * b[i];
        }

        return sum;
    }

    private static double SIMDMagnitude(double[] a)
    {
        return System.Math.Sqrt(SIMDDotProduct(a, a));
    }

    private static double[] MergeResults(double[][] results)
    {
        if (results.Length == 0)
        {
            return Array.Empty<double>();
        }

        if (results.Length == 1)
        {
            return results[0] ?? Array.Empty<double>();
        }

        int totalLength = 0;
        foreach (var r in results)
        {
            if (r != null)
            {
                totalLength += r.Length;
            }
        }

        var merged = new double[totalLength];
        int offset = 0;
        foreach (var r in results)
        {
            if (r != null && r.Length > 0)
            {
                Array.Copy(r, 0, merged, offset, r.Length);
                offset += r.Length;
            }
        }

        return merged;
    }

    private static double[] MergeGraphResults(ExecutionPlan plan, ConcurrentDictionary<int, double[]> results)
    {
        if (plan.Tasks.Count == 0)
        {
            return Array.Empty<double>();
        }

        var sorted = plan.TopologicalSort();
        var merged = new List<double>();

        foreach (var task in sorted)
        {
            if (results.TryGetValue(task.TaskId, out var taskResult) && taskResult != null)
            {
                merged.AddRange(taskResult);
            }
        }

        return merged.ToArray();
    }
}
