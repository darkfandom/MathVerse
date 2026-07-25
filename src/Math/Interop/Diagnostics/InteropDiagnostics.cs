namespace MathVerse.Math.Interop.Diagnostics;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Collects and reports diagnostics for interoperability operations.
/// </summary>
public sealed class InteropDiagnostics
{
    private readonly ConcurrentBag<DiagnosticEntry> _entries = new();
    private readonly ConcurrentBag<ConversionWarning> _warnings = new();

    /// <summary>
    /// Gets the total number of diagnostic entries.
    /// </summary>
    public int EntryCount => _entries.Count;

    /// <summary>
    /// Gets the total number of warnings.
    /// </summary>
    public int WarningCount => _warnings.Count;

    /// <summary>
    /// Records a diagnostic entry.
    /// </summary>
    /// <param name="level">The diagnostic level.</param>
    /// <param name="source">The source component.</param>
    /// <param name="message">The diagnostic message.</param>
    public void Record(DiagnosticLevel level, string source, string message)
    {
        _ = source ?? throw new ArgumentNullException(nameof(source));
        _ = message ?? throw new ArgumentNullException(nameof(message));
        _entries.Add(new DiagnosticEntry(level, source, message, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Records a conversion warning.
    /// </summary>
    /// <param name="warning">The conversion warning.</param>
    public void RecordWarning(ConversionWarning warning)
    {
        _ = warning ?? throw new ArgumentNullException(nameof(warning));
        _warnings.Add(warning);
    }

    /// <summary>
    /// Gets all diagnostic entries.
    /// </summary>
    /// <returns>A collection of diagnostic entries.</returns>
    public IReadOnlyList<DiagnosticEntry> GetAllEntries()
    {
        return _entries.ToArray();
    }

    /// <summary>
    /// Gets all conversion warnings.
    /// </summary>
    /// <returns>A collection of conversion warnings.</returns>
    public IReadOnlyList<ConversionWarning> GetAllWarnings()
    {
        return _warnings.ToArray();
    }

    /// <summary>
    /// Gets entries filtered by level.
    /// </summary>
    /// <param name="level">The diagnostic level to filter by.</param>
    /// <returns>A filtered collection of entries.</returns>
    public IReadOnlyList<DiagnosticEntry> GetEntriesByLevel(DiagnosticLevel level)
    {
        return _entries.Where(e => e.Level == level).ToArray();
    }

    /// <summary>
    /// Clears all diagnostic data.
    /// </summary>
    public void Clear()
    {
        while (_entries.TryTake(out _)) { }
        while (_warnings.TryTake(out _)) { }
    }
}

/// <summary>
/// Represents a single diagnostic entry.
/// </summary>
public sealed class DiagnosticEntry
{
    /// <summary>Gets the diagnostic level.</summary>
    public DiagnosticLevel Level { get; }

    /// <summary>Gets the source component.</summary>
    public string Source { get; }

    /// <summary>Gets the diagnostic message.</summary>
    public string Message { get; }

    /// <summary>Gets the timestamp.</summary>
    public DateTimeOffset Timestamp { get; }

    internal DiagnosticEntry(DiagnosticLevel level, string source, string message, DateTimeOffset timestamp)
    {
        Level = level;
        Source = source;
        Message = message;
        Timestamp = timestamp;
    }
}

/// <summary>
/// Diagnostic severity levels.
/// </summary>
public enum DiagnosticLevel
{
    /// <summary>Informational message.</summary>
    Information,

    /// <summary>Warning message.</summary>
    Warning,

    /// <summary>Error message.</summary>
    Error,

    /// <summary>Verbose debug message.</summary>
    Debug
}
