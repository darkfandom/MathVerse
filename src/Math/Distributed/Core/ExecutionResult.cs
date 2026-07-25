namespace MathVerse.Math.Distributed.Core;

using System.Collections.Immutable;

/// <summary>Result of a distributed execution operation.</summary>
public sealed class ExecutionResult
{
    /// <summary>Whether the execution completed successfully.</summary>
    public bool Success { get; init; }

    /// <summary>A message describing the result.</summary>
    public string Message { get; init; } = "";

    /// <summary>Output values produced by the execution, if any.</summary>
    public double[]? OutputValues { get; init; }

    /// <summary>Wall-clock time elapsed during execution.</summary>
    public TimeSpan ElapsedTime { get; init; }

    /// <summary>Number of tasks executed.</summary>
    public int TasksExecuted { get; init; }

    /// <summary>Number of tasks that ran in parallel.</summary>
    public int ParallelTasksExecuted { get; init; }

    /// <summary>Execution mode used: Sequential, Parallel, Distributed, SIMD, or GPU.</summary>
    public string ExecutionMode { get; init; } = "";

    /// <summary>Key-value metrics collected during execution.</summary>
    public ImmutableDictionary<string, double> Metrics { get; init; } = ImmutableDictionary<string, double>.Empty;

    /// <summary>The exception that caused failure, if any.</summary>
    public Exception? Error { get; init; }

    /// <summary>Creates a successful result with the given output.</summary>
    /// <param name="output">The output values.</param>
    /// <param name="message">A descriptive message.</param>
    /// <returns>A successful ExecutionResult.</returns>
    public static ExecutionResult Ok(double[] output, string message = "Success") =>
        new()
        {
            Success = true,
            OutputValues = output,
            Message = message
        };

    /// <summary>Creates a failed result with the given message and optional exception.</summary>
    /// <param name="message">A descriptive error message.</param>
    /// <param name="ex">The exception that caused the failure.</param>
    /// <returns>A failed ExecutionResult.</returns>
    public static ExecutionResult Fail(string message, Exception? ex = null) =>
        new()
        {
            Success = false,
            Message = message,
            Error = ex
        };
}
