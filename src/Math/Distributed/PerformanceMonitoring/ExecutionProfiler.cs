namespace MathVerse.Math.Distributed.PerformanceMonitoring;

using System.Collections.Concurrent;
using System.Diagnostics;
using MathVerse.Math.Distributed.Core;

/// <summary>Represents timing data for a single task within an execution plan.</summary>
public sealed class TaskTiming
{
    /// <summary>The task identifier.</summary>
    public int TaskId { get; init; }

    /// <summary>The human-readable task name.</summary>
    public string TaskName { get; init; } = "";

    /// <summary>The elapsed time for this task.</summary>
    public TimeSpan Elapsed { get; init; }

    /// <summary>The estimated cost specified in the execution plan.</summary>
    public double EstimatedCost { get; init; }

    /// <summary>The ratio of actual elapsed time to estimated cost.</summary>
    public double OverheadRatio => EstimatedCost > 0.0
        ? Elapsed.TotalSeconds / EstimatedCost
        : 0.0;
}

/// <summary>Profiles execution plans to obtain per-task timing data.</summary>
public sealed class ExecutionProfiler
{
    private readonly ConcurrentBag<TaskTiming> _recordedTimings = new();
    private long _totalProfiledTasks;

    /// <summary>Gets the total number of tasks profiled across all execution plans.</summary>
    public long TotalProfiledTasks => Interlocked.Read(ref _totalProfiledTasks);

    /// <summary>Profiles an execution plan by executing each task sequentially and recording timing data.</summary>
    /// <param name="plan">The execution plan to profile.</param>
    /// <returns>A list of timing data for each task in topological order.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the plan contains a cycle.</exception>
    public List<TaskTiming> Profile(ExecutionPlan plan)
    {
        if (plan is null)
        {
            throw new ArgumentNullException(nameof(plan));
        }

        var sorted = plan.TopologicalSort();
        var timings = new List<TaskTiming>();

        foreach (var task in sorted)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                task.Execute(CancellationToken.None).AsTask().GetAwaiter().GetResult();
            }
            catch
            {
                // Swallow exceptions during profiling.
            }

            sw.Stop();

            var timing = new TaskTiming
            {
                TaskId = task.TaskId,
                TaskName = task.Name,
                Elapsed = sw.Elapsed,
                EstimatedCost = task.EstimatedCost
            };

            _recordedTimings.Add(timing);
            timings.Add(timing);
            Interlocked.Increment(ref _totalProfiledTasks);
        }

        return timings;
    }

    /// <summary>Profiles an execution plan without executing the tasks, using estimated costs only.</summary>
    /// <param name="plan">The execution plan to estimate.</param>
    /// <returns>A list of estimated timing data.</returns>
    public List<TaskTiming> EstimateProfile(ExecutionPlan plan)
    {
        if (plan is null)
        {
            throw new ArgumentNullException(nameof(plan));
        }

        var sorted = plan.TopologicalSort();
        var timings = new List<TaskTiming>();

        foreach (var task in sorted)
        {
            var timing = new TaskTiming
            {
                TaskId = task.TaskId,
                TaskName = task.Name,
                Elapsed = TimeSpan.FromSeconds(task.EstimatedCost),
                EstimatedCost = task.EstimatedCost
            };

            timings.Add(timing);
        }

        return timings;
    }

    /// <summary>Returns the slowest tasks across all profiled data.</summary>
    /// <param name="count">The number of slowest tasks to return.</param>
    /// <returns>An array of the slowest task timings, ordered by elapsed time descending.</returns>
    public TaskTiming[] GetSlowestTasks(int count)
    {
        return _recordedTimings
            .OrderByDescending(t => t.Elapsed)
            .Take(count)
            .ToArray();
    }

    /// <summary>Returns the fastest tasks across all profiled data.</summary>
    /// <param name="count">The number of fastest tasks to return.</param>
    /// <returns>An array of the fastest task timings, ordered by elapsed time ascending.</returns>
    public TaskTiming[] GetFastestTasks(int count)
    {
        return _recordedTimings
            .OrderBy(t => t.Elapsed)
            .Take(count)
            .ToArray();
    }

    /// <summary>Returns the tasks with the highest overhead ratio (actual vs estimated).</summary>
    /// <param name="count">The number of tasks to return.</param>
    /// <returns>An array of task timings with overhead ratio above 1.0, ordered descending.</returns>
    public TaskTiming[] GetMostOverestimatedTasks(int count)
    {
        return _recordedTimings
            .Where(t => t.EstimatedCost > 0.0)
            .OrderByDescending(t => t.OverheadRatio)
            .Take(count)
            .ToArray();
    }

    /// <summary>Clears all recorded profiling data.</summary>
    public void Clear()
    {
        while (_recordedTimings.TryTake(out _)) { }
        Interlocked.Exchange(ref _totalProfiledTasks, 0);
    }
}
