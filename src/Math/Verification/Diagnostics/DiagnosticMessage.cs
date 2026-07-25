namespace MathVerse.Math.Verification.Diagnostics;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

/// <summary>
/// Represents a diagnostic message with severity, location, and optional fixes.
/// </summary>
public sealed record DiagnosticMessage(
    string Id,
    string Message,
    DiagnosticSeverity Severity,
    string? FilePath = null,
    int Line = 0,
    int Column = 0,
    int EndLine = 0,
    int EndColumn = 0,
    ImmutableArray<DiagnosticFix>? Fixes = null,
    ImmutableDictionary<string, string?>? Properties = null)
{
    public ImmutableArray<DiagnosticFix> FixesValue => Fixes ?? ImmutableArray<DiagnosticFix>.Empty;
    public ImmutableDictionary<string, string?> PropertiesValue => Properties ?? ImmutableDictionary<string, string?>.Empty;

    /// <summary>
    /// Gets the location span of this diagnostic.
    /// </summary>
    public DiagnosticLocation Location => new(FilePath, Line, Column, EndLine, EndColumn);

    /// <summary>
    /// Gets whether this diagnostic has an associated location.
    /// </summary>
    public bool HasLocation => !string.IsNullOrEmpty(FilePath) && Line > 0;

    /// <summary>
    /// Creates an info diagnostic.
    /// </summary>
    public static DiagnosticMessage Info(string id, string message, string? filePath = null, int line = 0, int column = 0) =>
        new(id, message, DiagnosticSeverity.Info, filePath, line, column);

    /// <summary>
    /// Creates a warning diagnostic.
    /// </summary>
    public static DiagnosticMessage Warning(string id, string message, string? filePath = null, int line = 0, int column = 0) =>
        new(id, message, DiagnosticSeverity.Warning, filePath, line, column);

    /// <summary>
    /// Creates an error diagnostic.
    /// </summary>
    public static DiagnosticMessage Error(string id, string message, string? filePath = null, int line = 0, int column = 0) =>
        new(id, message, DiagnosticSeverity.Error, filePath, line, column);

    /// <summary>
    /// Creates a hidden diagnostic.
    /// </summary>
    public static DiagnosticMessage Hidden(string id, string message, string? filePath = null, int line = 0, int column = 0) =>
        new(id, message, DiagnosticSeverity.Hidden, filePath, line, column);

    /// <summary>
    /// Creates a diagnostic with a fix.
    /// </summary>
    public static DiagnosticMessage WithFix(string id, string message, DiagnosticFix fix, string? filePath = null, int line = 0, int column = 0) =>
        new(id, message, DiagnosticSeverity.Error, filePath, line, column, Fixes: ImmutableArray.Create(fix));

    /// <summary>
    /// Creates a diagnostic with properties.
    /// </summary>
    public DiagnosticMessage WithProperties(ImmutableDictionary<string, string?> properties) =>
        this with { Properties = properties };

    /// <summary>
    /// Creates a diagnostic with additional properties.
    /// </summary>
    public DiagnosticMessage WithProperty(string key, string? value) =>
        this with { Properties = PropertiesValue.Add(key, value) };
}

/// <summary>
/// Represents a source code location.
/// </summary>
public readonly record struct DiagnosticLocation(
    string? FilePath,
    int Line,
    int Column,
    int EndLine,
    int EndColumn)
{
    public bool HasLocation => !string.IsNullOrEmpty(FilePath) && Line > 0;

    public override string ToString() =>
        HasLocation ? $"{FilePath}({Line},{Column})" : "<unknown>";
}

/// <summary>
/// Represents a code fix for a diagnostic.
/// </summary>
public sealed record DiagnosticFix(
    string Title,
    string Description,
    ImmutableArray<TextEdit> Edits)
{
    public static DiagnosticFix Create(string title, string description, params TextEdit[] edits) =>
        new(title, description, edits.ToImmutableArray());
}

/// <summary>
/// Represents a text edit for a diagnostic fix.
/// </summary>
public readonly record struct TextEdit(
    string FilePath,
    int StartLine,
    int StartColumn,
    int EndLine,
    int EndColumn,
    string NewText);