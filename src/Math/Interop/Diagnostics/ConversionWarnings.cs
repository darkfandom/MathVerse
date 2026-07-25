namespace MathVerse.Math.Interop.Diagnostics;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a warning generated during format conversion.
/// </summary>
public sealed class ConversionWarning
{
    /// <summary>
    /// Gets or sets the warning code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the warning message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source type or format that triggered the warning.
    /// </summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target type or format involved in the conversion.
    /// </summary>
    public string TargetType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the field or property path where the warning occurred.
    /// </summary>
    public string? FieldPath { get; set; }

    /// <summary>
    /// Gets or sets the severity of the warning.
    /// </summary>
    public WarningSeverity Severity { get; set; }

    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Severity of a conversion warning.
/// </summary>
public enum WarningSeverity
{
    /// <summary>Low severity, informational.</summary>
    Low,

    /// <summary>Medium severity, may affect results.</summary>
    Medium,

    /// <summary>High severity, significant data loss possible.</summary>
    High
}
