namespace MathVerse.Math.Compiler.Diagnostics;

using System;

/// <summary>Represents a single diagnostic message produced during compilation.</summary>
public sealed class DiagnosticMessage
{
    /// <summary>The severity level of this message.</summary>
    public DiagnosticSeverity Severity { get; }
    /// <summary>The human-readable message text.</summary>
    public string Message { get; }
    /// <summary>The source line number (1-based), or null if not applicable.</summary>
    public int? Line { get; }
    /// <summary>The timestamp when this message was created.</summary>
    public DateTime Timestamp { get; }

    /// <summary>Initializes a new instance of the <see cref="DiagnosticMessage"/> class.</summary>
    public DiagnosticMessage(DiagnosticSeverity severity, string message, int? line = null)
    {
        Severity = severity;
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Line = line;
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>Returns a formatted string representation of this diagnostic.</summary>
    public override string ToString()
    {
        var prefix = Severity switch
        {
            DiagnosticSeverity.Error => "error",
            DiagnosticSeverity.Warning => "warning",
            DiagnosticSeverity.Info => "info",
            DiagnosticSeverity.Hidden => "hidden",
            _ => "unknown"
        };
        var location = Line.HasValue ? $" (line {Line.Value})" : "";
        return $"{prefix}: {Message}{location}";
    }
}
