namespace MathVerse.Math.HPC.Diagnostics;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

/// <summary>
/// Represents a diagnostic message with severity, location, and optional properties.
/// </summary>
public sealed record DiagnosticMessage(
    DiagnosticSeverity Severity,
    string Code,
    string Message,
    SourceLocation? Location = null,
    DateTimeOffset Timestamp = default,
    ImmutableDictionary<string, string?>? Properties = null)
{
    public DiagnosticMessage(
        DiagnosticSeverity severity,
        string code,
        string message,
        SourceLocation? location = null)
        : this(severity, code, message, location, DateTimeOffset.UtcNow, ImmutableDictionary<string, string?>.Empty)
    {
    }

    /// <summary>
    /// Creates an error diagnostic.
    /// </summary>
    public static DiagnosticMessage Error(string code, string message, SourceLocation? location = null) =>
        new(DiagnosticSeverity.Error, code, message, location);

    /// <summary>
    /// Creates a warning diagnostic.
    /// </summary>
    public static DiagnosticMessage Warning(string code, string message, SourceLocation? location = null) =>
        new(DiagnosticSeverity.Warning, code, message, location);

    /// <summary>
    /// Creates an info diagnostic.
    /// </summary>
    public static DiagnosticMessage Info(string code, string message, SourceLocation? location = null) =>
        new(DiagnosticSeverity.Info, code, message, location);

    /// <summary>
    /// Creates a hidden diagnostic.
    /// </summary>
    public static DiagnosticMessage Hidden(string code, string message, SourceLocation? location = null) =>
        new(DiagnosticSeverity.Hidden, code, message, location);

    /// <summary>
    /// Creates a diagnostic with properties.
    /// </summary>
    public DiagnosticMessage WithProperties(ImmutableDictionary<string, string?> properties) =>
        this with { Properties = properties };

    /// <summary>
    /// Creates a diagnostic with an additional property.
    /// </summary>
    public DiagnosticMessage WithProperty(string key, string? value) =>
        this with { Properties = (Properties ?? ImmutableDictionary<string, string?>.Empty).Add(key, value) };
}