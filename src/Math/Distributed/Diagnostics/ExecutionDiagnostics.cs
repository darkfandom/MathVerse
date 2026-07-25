namespace MathVerse.Math.Distributed.Diagnostics;

using System.Collections.Concurrent;

/// <summary>Summary of execution diagnostics data.</summary>
public sealed class ExecutionDiagnosticsSummary
{
    /// <summary>Total number of tasks recorded.</summary>
    public int TotalTasks { get; init; }

    /// <summary>Number of successfully completed tasks.</summary>
    public int CompletedTasks { get; init; }

    /// <summary>Number of failed tasks.</summary>
    public int FailedTasks { get; init; }

    /// <summary>Total execution time in milliseconds.</summary>
    public double TotalExecutionTimeMs { get; init; }

    /// <summary>Average execution time per task in milliseconds.</summary>
    public double AverageExecutionTimeMs { get; init; }

    /// <summary>Minimum task execution time in milliseconds.</summary>
    public double MinExecutionTimeMs { get; init; }

    /// <summary>Maximum task execution time in milliseconds.</summary>
    public double MaxExecutionTimeMs { get; init; }
}

/// <summary>Records and reports on execution events across the distributed system.</summary>
public sealed class ExecutionDiagnostics : IDisposable
{
    private readonly ConcurrentBag<TaskEvent> _events;
    private bool _disposed;

    /// <summary>Represents a recorded task event.</summary>
    public sealed class TaskEvent
    {
        /// <summary>The task identifier.</summary>
        public string TaskId { get; init; } = "";

        /// <summary>The event type (Start, Complete, Failed).</summary>
        public string EventType { get; init; } = "";

        /// <summary>Timestamp of the event.</summary>
        public DateTime Timestamp { get; init; }

        /// <summary>Execution time in milliseconds (for completion events).</summary>
        public double ElapsedMs { get; init; }

        /// <summary>Error message (for failure events).</summary>
        public string? ErrorMessage { get; init; }

        /// <summary>Exception (for failure events).</summary>
        public Exception? Exception { get; init; }
    }

    /// <summary>Initializes a new execution diagnostics instance.</summary>
    public ExecutionDiagnostics()
    {
        _events = new ConcurrentBag<TaskEvent>();
    }

    /// <summary>Records a task start event.</summary>
    /// <param name="taskId">The task identifier.</param>
    public void RecordTaskStart(string taskId)
    {
        _events.Add(new TaskEvent
        {
            TaskId = taskId,
            EventType = "Start",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>Records a task completion event.</summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="elapsedMs">Elapsed time in milliseconds.</param>
    public void RecordTaskComplete(string taskId, double elapsedMs)
    {
        _events.Add(new TaskEvent
        {
            TaskId = taskId,
            EventType = "Complete",
            Timestamp = DateTime.UtcNow,
            ElapsedMs = elapsedMs
        });
    }

    /// <summary>Records a task failure event.</summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="ex">The exception that caused the failure.</param>
    public void RecordTaskFailed(string taskId, Exception? ex = null)
    {
        _events.Add(new TaskEvent
        {
            TaskId = taskId,
            EventType = "Failed",
            Timestamp = DateTime.UtcNow,
            ErrorMessage = ex?.Message,
            Exception = ex
        });
    }

    /// <summary>Gets a summary of all recorded execution events.</summary>
    /// <returns>An diagnostics summary with aggregated metrics.</returns>
    public ExecutionDiagnosticsSummary GetSummary()
    {
        int total = 0;
        int completed = 0;
        int failed = 0;
        double totalTime = 0.0;
        double minTime = double.MaxValue;
        double maxTime = 0.0;

        foreach (var evt in _events)
        {
            total++;
            if (evt.EventType == "Complete")
            {
                completed++;
                totalTime += evt.ElapsedMs;
                if (evt.ElapsedMs < minTime) minTime = evt.ElapsedMs;
                if (evt.ElapsedMs > maxTime) maxTime = evt.ElapsedMs;
            }
            else if (evt.EventType == "Failed")
            {
                failed++;
            }
        }

        return new ExecutionDiagnosticsSummary
        {
            TotalTasks = total,
            CompletedTasks = completed,
            FailedTasks = failed,
            TotalExecutionTimeMs = totalTime,
            AverageExecutionTimeMs = completed > 0 ? totalTime / completed : 0.0,
            MinExecutionTimeMs = completed > 0 ? minTime : 0.0,
            MaxExecutionTimeMs = completed > 0 ? maxTime : 0.0
        };
    }

    /// <summary>Gets all recorded events.</summary>
    /// <returns>Array of all task events.</returns>
    public TaskEvent[] GetEvents()
    {
        return _events.ToArray();
    }

    /// <summary>Clears all recorded events.</summary>
    public void Clear()
    {
        while (_events.TryTake(out _)) { }
    }

    /// <summary>Disposes the diagnostics instance.</summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            Clear();
            _disposed = true;
        }
    }
}
