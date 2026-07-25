namespace MathVerse.Math.Quantum.Core;

/// <summary>
/// Represents the outcome of a quantum operation without a return value.
/// </summary>
public sealed class QuantumResult
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the error message if the operation failed.
    /// </summary>
    public string? Error { get; }

    private QuantumResult(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful <see cref="QuantumResult"/>.</returns>
    public static QuantumResult Success()
    {
        return new QuantumResult(true, null);
    }

    /// <summary>
    /// Creates a failure result with the specified error message.
    /// </summary>
    /// <param name="error">The error description.</param>
    /// <returns>A failed <see cref="QuantumResult"/>.</returns>
    public static QuantumResult Failure(string error)
    {
        return new QuantumResult(false, error ?? throw new ArgumentNullException(nameof(error)));
    }
}

/// <summary>
/// Represents the outcome of a quantum operation that returns a value.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
public sealed class QuantumResult<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the result value if the operation succeeded.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Gets the error message if the operation failed.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Gets the exception that caused the failure, if any.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets the duration of the operation.
    /// </summary>
    public TimeSpan Duration { get; }

    private QuantumResult(T? value, bool isSuccess, string? error, Exception? exception, TimeSpan duration)
    {
        Value = value;
        IsSuccess = isSuccess;
        Error = error;
        Exception = exception;
        Duration = duration;
    }

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The result value.</param>
    /// <returns>A successful <see cref="QuantumResult{T}"/>.</returns>
    public static QuantumResult<T> Success(T value)
    {
        return new QuantumResult<T>(value, true, null, null, TimeSpan.Zero);
    }

    /// <summary>
    /// Creates a successful result with the specified value and duration.
    /// </summary>
    /// <param name="value">The result value.</param>
    /// <param name="duration">The operation duration.</param>
    /// <returns>A successful <see cref="QuantumResult{T}"/>.</returns>
    public static QuantumResult<T> Success(T value, TimeSpan duration)
    {
        return new QuantumResult<T>(value, true, null, null, duration);
    }

    /// <summary>
    /// Creates a failure result with the specified error message.
    /// </summary>
    /// <param name="error">The error description.</param>
    /// <returns>A failed <see cref="QuantumResult{T}"/>.</returns>
    public static QuantumResult<T> Failure(string error)
    {
        return new QuantumResult<T>(default, false, error ?? throw new ArgumentNullException(nameof(error)), null, TimeSpan.Zero);
    }

    /// <summary>
    /// Creates a failure result with the specified error message and exception.
    /// </summary>
    /// <param name="error">The error description.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <returns>A failed <see cref="QuantumResult{T}"/>.</returns>
    public static QuantumResult<T> Failure(string error, Exception? exception)
    {
        return new QuantumResult<T>(default, false, error ?? throw new ArgumentNullException(nameof(error)), exception, TimeSpan.Zero);
    }
}
