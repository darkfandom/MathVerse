namespace MathVerse.Math.HPC.Diagnostics;

/// <summary>
/// Represents the severity level of a diagnostic.
/// </summary>
public enum DiagnosticSeverity : byte
{
    /// <summary>
    /// Hidden diagnostic, not shown by default.
    /// </summary>
    Hidden = 0,

    /// <summary>
    /// Informational message.
    /// </summary>
    Info = 1,

    /// <summary>
    /// Warning indicating a potential issue.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Error indicating a definite problem.
    /// </summary>
    Error = 3
}