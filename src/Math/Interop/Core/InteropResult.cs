namespace MathVerse.Math.Interop.Core;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents the result of an interoperability operation.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
public sealed class InteropResult<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the result value, or default if the operation failed.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Gets the error message, or null if the operation succeeded.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Gets the exception, or null if the operation succeeded.
    /// </summary>
    public Exception? Error { get; }

    /// <summary>
    /// Gets the diagnostics produced during the operation.
    /// </summary>
    public IReadOnlyList<string> Diagnostics { get; }

    /// <summary>
    /// Gets the operation duration.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// Gets the timestamp when the operation completed.
    /// </summary>
    public DateTimeOffset CompletedAt { get; }

    private InteropResult(T? value, bool isSuccess, string? errorMessage, Exception? error,
        IReadOnlyList<string> diagnostics, TimeSpan duration)
    {
        Value = value;
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Error = error;
        Diagnostics = diagnostics;
        Duration = duration;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <param name="value">The result value.</param>
    /// <param name="diagnostics">Optional diagnostics.</param>
    /// <param name="duration">The operation duration.</param>
    /// <returns>A successful InteropResult.</returns>
    public static InteropResult<T> Success(T value, IReadOnlyList<string>? diagnostics = null, TimeSpan duration = default)
    {
        return new InteropResult<T>(value, true, null, null, diagnostics ?? Array.Empty<string>(), duration);
    }

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="error">The exception.</param>
    /// <param name="diagnostics">Optional diagnostics.</param>
    /// <returns>A failed InteropResult.</returns>
    public static InteropResult<T> Failure(string errorMessage, Exception? error = null, IReadOnlyList<string>? diagnostics = null)
    {
        return new InteropResult<T>(default, false, errorMessage, error, diagnostics ?? Array.Empty<string>(), TimeSpan.Zero);
    }
}

/// <summary>
/// Non-generic result type for operations that do not return a value.
/// </summary>
public sealed class InteropResult
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the error message, or null if the operation succeeded.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Gets the diagnostics produced during the operation.
    /// </summary>
    public IReadOnlyList<string> Diagnostics { get; }

    /// <summary>
    /// Gets the operation duration.
    /// </summary>
    public TimeSpan Duration { get; }

    private InteropResult(bool isSuccess, string? errorMessage, IReadOnlyList<string> diagnostics, TimeSpan duration)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Diagnostics = diagnostics;
        Duration = duration;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful InteropResult.</returns>
    public static InteropResult Success()
    {
        return new InteropResult(true, null, Array.Empty<string>(), TimeSpan.Zero);
    }

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>A failed InteropResult.</returns>
    public static InteropResult Failure(string errorMessage)
    {
        return new InteropResult(false, errorMessage, Array.Empty<string>(), TimeSpan.Zero);
    }
}
