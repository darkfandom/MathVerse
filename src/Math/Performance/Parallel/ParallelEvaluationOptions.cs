namespace MathVerse.Math.Performance.Parallel;

/// <summary>
/// Configuration options for parallel expression evaluation.
/// </summary>
public sealed record ParallelEvaluationOptions
{
    /// <summary>
    /// Gets the default set of parallel evaluation options.
    /// </summary>
    public static ParallelEvaluationOptions Default { get; } = new();

    /// <summary>
    /// Initializes a new instance of <see cref="ParallelEvaluationOptions"/>.
    /// </summary>
    public ParallelEvaluationOptions()
        : this(Environment.ProcessorCount, false, CancellationToken.None, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ParallelEvaluationOptions"/> with the specified values.
    /// </summary>
    /// <param name="maxDegreeOfParallelism">Maximum number of concurrent tasks.</param>
    /// <param name="deterministic">Whether results should be produced in deterministic order.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <param name="timeout">Optional timeout for the entire operation.</param>
    public ParallelEvaluationOptions(
        int maxDegreeOfParallelism,
        bool deterministic,
        CancellationToken cancellationToken,
        TimeSpan? timeout)
    {
        MaxDegreeOfParallelism = maxDegreeOfParallelism > 0
            ? maxDegreeOfParallelism
            : throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism), maxDegreeOfParallelism, "Must be greater than zero.");
        Deterministic = deterministic;
        CancellationToken = cancellationToken;
        Timeout = timeout;
    }

    /// <summary>
    /// Gets the maximum number of tasks to execute concurrently.
    /// </summary>
    public int MaxDegreeOfParallelism { get; init; }

    /// <summary>
    /// Gets whether results should be produced in deterministic (input) order.
    /// </summary>
    public bool Deterministic { get; init; }

    /// <summary>
    /// Gets the cancellation token for the operation.
    /// </summary>
    public CancellationToken CancellationToken { get; init; }

    /// <summary>
    /// Gets the optional timeout for the entire parallel operation.
    /// </summary>
    public TimeSpan? Timeout { get; init; }
}
