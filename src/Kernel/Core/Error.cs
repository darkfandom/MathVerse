namespace MathVerse.Core;

/// <summary>
/// Represents an error that occurred during an operation.
/// </summary>
public record Error : IEquatable<Error>
{
    /// <summary>Creates a new Error.</summary>
    public Error(string code, string message, ErrorKind kind, Error? inner = null)
    {
        Code = code;
        Message = message;
        Kind = kind;
        Inner = inner;
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>Gets the error code.</summary>
    public string Code { get; }

    /// <summary>Gets the error message.</summary>
    public string Message { get; }

    /// <summary>Gets the error kind.</summary>
    public ErrorKind Kind { get; }

    /// <summary>Gets the inner error, if any.</summary>
    public Error? Inner { get; }

    /// <summary>Gets when the error occurred.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <inheritdoc/>
    public virtual bool Equals(Error? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Code == other.Code && Message == other.Message && Kind == other.Kind;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Code, Message, Kind);

    /// <summary>Creates a validation error.</summary>
    public static Error Validation(string code, string message) =>
        new(code, message, ErrorKind.Validation);

    /// <summary>Creates a not found error.</summary>
    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorKind.NotFound);

    /// <summary>Creates a conflict error.</summary>
    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorKind.Conflict);

    /// <summary>Creates an internal error.</summary>
    public static Error Internal(string code, string message) =>
        new(code, message, ErrorKind.Internal);
}

/// <summary>
/// Represents the kind of error that occurred.
/// </summary>
public enum ErrorKind
{
    /// <summary>A validation error.</summary>
    Validation,

    /// <summary>A not found error.</summary>
    NotFound,

    /// <summary>A conflict error.</summary>
    Conflict,

    /// <summary>An unauthorized error.</summary>
    Unauthorized,

    /// <summary>A forbidden error.</summary>
    Forbidden,

    /// <summary>A timeout error.</summary>
    Timeout,

    /// <summary>A canceled operation.</summary>
    Canceled,

    /// <summary>An external error.</summary>
    External,

    /// <summary>An internal error.</summary>
    Internal,

    /// <summary>An unknown error.</summary>
    Unknown
}
