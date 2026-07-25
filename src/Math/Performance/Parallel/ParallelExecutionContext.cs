namespace MathVerse.Math.Performance.Parallel;

/// <summary>
/// Context object for a single task within a parallel evaluation run.
/// </summary>
public sealed class ParallelExecutionContext
{
    private readonly ConcurrentBag<object> _results = [];

    private ParallelExecutionContext(CancellationToken token)
    {
        Token = token;
    }

    /// <summary>
    /// Gets the cancellation token for this parallel evaluation run.
    /// </summary>
    public CancellationToken Token { get; }

    /// <summary>
    /// Gets the unique task identifier.
    /// </summary>
    public int TaskId { get; private set; }

    /// <summary>
    /// Gets the thread-safe collection of results produced by this task.
    /// </summary>
    public ConcurrentBag<object> Results => _results;

    /// <summary>
    /// Gets the exception that caused the task to fail, if any.
    /// </summary>
    public Exception? Error { get; private set; }

    /// <summary>
    /// Gets whether this task has completed (successfully or otherwise).
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Creates a new parallel execution context.
    /// </summary>
    /// <param name="token">The cancellation token for the run.</param>
    /// <returns>A new <see cref="ParallelExecutionContext"/> instance.</returns>
    public static ParallelExecutionContext Create(CancellationToken token) => new(token);

    /// <summary>
    /// Adds a result to this context's result bag.
    /// </summary>
    /// <param name="result">The result to add.</param>
    public void AddResult(object result)
    {
        Token.ThrowIfCancellationRequested();
        _results.Add(result);
    }

    /// <summary>
    /// Marks the task as completed successfully.
    /// </summary>
    internal void MarkCompleted()
    {
        IsCompleted = true;
        Error = null;
    }

    /// <summary>
    /// Marks the task as failed with the specified exception.
    /// </summary>
    /// <param name="error">The exception that caused the failure.</param>
    internal void MarkFailed(Exception error)
    {
        IsCompleted = true;
        Error = error;
    }

    /// <summary>
    /// Sets the task identifier.
    /// </summary>
    /// <param name="taskId">The task identifier to assign.</param>
    internal void SetTaskId(int taskId)
    {
        TaskId = taskId;
    }
}
