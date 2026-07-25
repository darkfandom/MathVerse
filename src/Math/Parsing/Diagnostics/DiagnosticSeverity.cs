namespace MathVerse.Math.Parsing.Diagnostics;

/// <summary>
/// Categorizes the severity of a diagnostic message.
/// </summary>
public enum DiagnosticSeverity
{
    /// <summary>An informational message.</summary>
    Info,

    /// <summary>A warning that does not prevent compilation.</summary>
    Warning,

    /// <summary>An error that prevents successful parsing.</summary>
    Error
}
