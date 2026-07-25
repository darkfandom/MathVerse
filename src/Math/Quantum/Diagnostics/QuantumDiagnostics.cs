using System.Collections.Concurrent;

namespace MathVerse.Math.Quantum.Diagnostics;

/// <summary>
/// Severity levels for diagnostic entries.
/// </summary>
public enum QuantumDiagnosticLevel
{
    /// <summary>Trace-level diagnostic information.</summary>
    Trace,

    /// <summary>Debug-level diagnostic information.</summary>
    Debug,

    /// <summary>Informational diagnostic messages.</summary>
    Info,

    /// <summary>Warning-level diagnostic messages.</summary>
    Warning,

    /// <summary>Error-level diagnostic messages.</summary>
    Error,

    /// <summary>Critical error diagnostic messages.</summary>
    Critical
}

/// <summary>
/// Represents a single diagnostic entry with level, source, message, and timestamp.
/// </summary>
/// <param name="Level">The diagnostic severity level.</param>
/// <param name="Source">The source component that generated the entry.</param>
    /// <param name="Message">The diagnostic message.</param>
    /// <param name="Timestamp">The timestamp when the entry was recorded.</param>
public sealed record QuantumDiagnosticEntry(
    QuantumDiagnosticLevel Level,
    string Source,
    string Message,
    DateTimeOffset Timestamp);

/// <summary>
/// Collects and manages diagnostic entries from quantum operations in a thread-safe manner.
/// </summary>
public sealed class QuantumDiagnostics
{
    private readonly ConcurrentBag<QuantumDiagnosticEntry> _entries;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuantumDiagnostics"/> class.
    /// </summary>
    public QuantumDiagnostics()
    {
        _entries = new ConcurrentBag<QuantumDiagnosticEntry>();
    }

    /// <summary>
    /// Records a diagnostic entry with the specified level, source, and message.
    /// </summary>
    /// <param name="level">The diagnostic severity level.</param>
    /// <param name="source">The source component.</param>
    /// <param name="message">The diagnostic message.</param>
    public void Record(QuantumDiagnosticLevel level, string source, string message)
    {
        var entry = new QuantumDiagnosticEntry(
            level,
            source ?? throw new ArgumentNullException(nameof(source)),
            message ?? throw new ArgumentNullException(nameof(message)),
            DateTimeOffset.UtcNow);
        _entries.Add(entry);
    }

    /// <summary>
    /// Gets all recorded diagnostic entries.
    /// </summary>
    /// <returns>A read-only list of all diagnostic entries.</returns>
    public IReadOnlyList<QuantumDiagnosticEntry> GetAllEntries()
    {
        return _entries.ToArray();
    }

    /// <summary>
    /// Gets diagnostic entries filtered by the specified level.
    /// </summary>
    /// <param name="level">The diagnostic level to filter by.</param>
    /// <returns>A read-only list of matching diagnostic entries.</returns>
    public IReadOnlyList<QuantumDiagnosticEntry> GetByLevel(QuantumDiagnosticLevel level)
    {
        return _entries.Where(e => e.Level == level).ToArray();
    }

    /// <summary>
    /// Clears all recorded diagnostic entries.
    /// </summary>
    public void Clear()
    {
        while (_entries.TryTake(out _)) { }
    }
}
