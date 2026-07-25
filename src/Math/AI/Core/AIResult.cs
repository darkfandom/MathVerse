namespace MathVerse.Math.AI.Core;

using System.Collections.Immutable;

/// <summary>Result of an AI operation including success status, metrics, and output data.</summary>
public sealed class AIResult
{
    /// <summary>Whether the operation completed successfully.</summary>
    public bool Success { get; init; }

    /// <summary>Human-readable description of the result.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Output values produced by the operation, or <c>null</c> when none.</summary>
    public double[]? OutputValues { get; init; }

    /// <summary>Final loss value reported by the operation.</summary>
    public double LossValue { get; init; }

    /// <summary>Number of training epochs that were executed.</summary>
    public int EpochsExecuted { get; init; }

    /// <summary>Total wall-clock time consumed by the operation.</summary>
    public TimeSpan ElapsedTime { get; init; }

    /// <summary>Key-value metrics collected during the operation.</summary>
    public ImmutableDictionary<string, double> Metrics { get; init; } = ImmutableDictionary<string, double>.Empty;

    /// <summary>Diagnostic messages generated during the operation.</summary>
    public List<string> Diagnostics { get; init; } = [];

    /// <summary>Creates a successful result.</summary>
    /// <param name="output">Optional output values.</param>
    /// <param name="message">Human-readable success message.</param>
    /// <returns>A new <see cref="AIResult"/> with <see cref="Success"/> set to <c>true</c>.</returns>
    public static AIResult Ok(double[]? output = null, string message = "Success") =>
        new()
        {
            Success = true,
            OutputValues = output,
            Message = message,
        };

    /// <summary>Creates a successful result with full detail.</summary>
    /// <param name="output">Output values.</param>
    /// <param name="lossValue">Final loss value.</param>
    /// <param name="epochsExecuted">Epochs executed.</param>
    /// <param name="elapsedTime">Wall-clock time.</param>
    /// <param name="metrics">Collected metrics.</param>
    /// <param name="diagnostics">Diagnostic messages.</param>
    /// <param name="message">Human-readable message.</param>
    /// <returns>A new successful <see cref="AIResult"/>.</returns>
    public static AIResult Ok(
        double[] output,
        double lossValue,
        int epochsExecuted,
        TimeSpan elapsedTime,
        ImmutableDictionary<string, double>? metrics = null,
        List<string>? diagnostics = null,
        string message = "Success") =>
        new()
        {
            Success = true,
            OutputValues = output,
            LossValue = lossValue,
            EpochsExecuted = epochsExecuted,
            ElapsedTime = elapsedTime,
            Metrics = metrics ?? ImmutableDictionary<string, double>.Empty,
            Diagnostics = diagnostics ?? [],
            Message = message,
        };

    /// <summary>Creates a failure result.</summary>
    /// <param name="message">Human-readable error description.</param>
    /// <returns>A new <see cref="AIResult"/> with <see cref="Success"/> set to <c>false</c>.</returns>
    public static AIResult Fail(string message) =>
        new()
        {
            Success = false,
            Message = message,
        };

    /// <summary>Creates a failure result with diagnostics.</summary>
    /// <param name="message">Human-readable error description.</param>
    /// <param name="diagnostics">Diagnostic messages produced before the failure.</param>
    /// <returns>A new failing <see cref="AIResult"/>.</returns>
    public static AIResult Fail(string message, List<string> diagnostics) =>
        new()
        {
            Success = false,
            Message = message,
            Diagnostics = diagnostics,
        };
}
