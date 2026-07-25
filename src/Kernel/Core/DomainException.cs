namespace MathVerse.Core;

/// <summary>
/// Represents an exception thrown when a domain rule is violated.
/// </summary>
public sealed class DomainException : Exception
{
    /// <summary>Initializes a new domain exception.</summary>
    public DomainException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    /// <summary>Initializes a new domain exception with an inner exception.</summary>
    public DomainException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }

    /// <summary>Gets the error code.</summary>
    public string Code { get; }

    /// <summary>Converts to an Error.</summary>
    public Error ToError() => new(Code, Message, ErrorKind.Internal);
}
