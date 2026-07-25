namespace MathVerse.Math.Parsing.Diagnostics;

/// <summary>
/// Represents a single diagnostic message produced during lexing or parsing.
/// </summary>
public sealed record Diagnostic
{
    /// <summary>Initializes a diagnostic.</summary>
    public Diagnostic(
        string code,
        string message,
        DiagnosticSeverity severity,
        int line,
        int column,
        int length,
        string? suggestedFix = null)
    {
        Code = code;
        Message = message;
        Severity = severity;
        Line = line;
        Column = column;
        Length = length;
        SuggestedFix = suggestedFix;
    }

    /// <summary>Gets the diagnostic code (e.g., "MV0001").</summary>
    public string Code { get; }

    /// <summary>Gets the human-readable message.</summary>
    public string Message { get; }

    /// <summary>Gets the severity.</summary>
    public DiagnosticSeverity Severity { get; }

    /// <summary>Gets the 1-based line number.</summary>
    public int Line { get; }

    /// <summary>Gets the 1-based column number.</summary>
    public int Column { get; }

    /// <summary>Gets the length of the offending span in characters.</summary>
    public int Length { get; }

    /// <summary>Gets an optional suggested fix.</summary>
    public string? SuggestedFix { get; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"[{Code}] ({Severity}) Line {Line}, Col {Column}: {Message}";
}
