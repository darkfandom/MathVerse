namespace MathVerse.Math.Compiler.Diagnostics;

/// <summary>Defines the severity levels for diagnostic messages.</summary>
public enum DiagnosticSeverity
{
    /// <summary>An error that prevents successful compilation.</summary>
    Error,
    /// <summary>A warning that does not prevent compilation but indicates potential issues.</summary>
    Warning,
    /// <summary>An informational message.</summary>
    Info,
    /// <summary>A hidden diagnostic message (used for internal tracking).</summary>
    Hidden
}
