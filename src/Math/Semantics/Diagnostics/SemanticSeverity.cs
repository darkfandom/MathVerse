namespace MathVerse.Math.Semantics.Diagnostics;

/// <summary>Severity of a semantic diagnostic.</summary>
public enum SemanticSeverity
{
    /// <summary>An informational note.</summary>
    Info,
    /// <summary>A non-fatal warning.</summary>
    Warning,
    /// <summary>A fatal error that prevents further analysis.</summary>
    Error,
}
