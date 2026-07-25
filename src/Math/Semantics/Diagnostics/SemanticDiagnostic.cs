namespace MathVerse.Math.Semantics.Diagnostics;

/// <summary>
/// Represents a single semantic diagnostic message.
/// </summary>
/// <param name="Code">The diagnostic code.</param>
/// <param name="Message">The human-readable message.</param>
/// <param name="Severity">The severity level.</param>
/// <param name="Location">Optional location info (e.g., token position).</param>
public sealed record SemanticDiagnostic(
    SemanticDiagnosticCode Code,
    string Message,
    SemanticSeverity Severity,
    string? Location = null)
{
    /// <inheritdoc/>
    public override string ToString() =>
        Location is not null
            ? $"[{Severity}] {Code}: {Message} ({Location})"
            : $"[{Severity}] {Code}: {Message}";
}
