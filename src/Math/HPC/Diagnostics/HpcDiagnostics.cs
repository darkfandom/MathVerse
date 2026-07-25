namespace MathVerse.Math.HPC.Diagnostics;

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;

/// <summary>
/// Collects all diagnostics during HPC operations.
/// </summary>
public sealed class HpcDiagnostics
{
    private readonly ConcurrentBag<DiagnosticMessage> _messages = new();
    private readonly object _summaryLock = new();
    private DiagnosticSummary? _cachedSummary;
    private long _messageCountAtCache;

    /// <summary>
    /// Reports a diagnostic message.
    /// </summary>
    /// <param name="severity">The severity of the diagnostic.</param>
    /// <param name="code">The diagnostic code (e.g., "HPC001").</param>
    /// <param name="message">The diagnostic message.</param>
    /// <param name="location">Optional source location.</param>
    public void Report(DiagnosticSeverity severity, string code, string message, SourceLocation? location = null)
    {
        var diagnostic = new DiagnosticMessage(severity, code, message, location ?? SourceLocation.None, DateTime.UtcNow);
        _messages.Add(diagnostic);
        _cachedSummary = null;
    }

    /// <summary>
    /// Reports a diagnostic from a <see cref="Diagnostic"/> object.
    /// </summary>
    public void Report(Diagnostic diagnostic)
    {
        Report(diagnostic.Severity, diagnostic.Id, diagnostic.Message, diagnostic.Location);
    }

    /// <summary>
    /// Reports multiple diagnostics from a <see cref="DiagnosticBag"/>.
    /// </summary>
    public void Report(DiagnosticBag bag)
    {
        foreach (var diagnostic in bag)
        {
            Report(diagnostic);
        }
    }

    /// <summary>
    /// Gets all messages, optionally filtered by severity.
    /// </summary>
    /// <param name="filter">Optional severity filter.</param>
    /// <returns>Read-only list of diagnostic messages.</returns>
    public IReadOnlyList<DiagnosticMessage> GetMessages(DiagnosticSeverity? filter = null)
    {
        var messages = _messages.ToArray();
        if (filter.HasValue)
        {
            return messages.Where(m => m.Severity == filter.Value).ToArray();
        }
        return messages;
    }

    /// <summary>
    /// Gets whether any error messages have been reported.
    /// </summary>
    public bool HasErrors => _messages.Any(m => m.Severity == DiagnosticSeverity.Error);

    /// <summary>
    /// Gets whether any warning messages have been reported.
    /// </summary>
    public bool HasWarnings => _messages.Any(m => m.Severity == DiagnosticSeverity.Warning);

    /// <summary>
    /// Gets a summary of all diagnostics.
    /// </summary>
    public DiagnosticSummary GetSummary()
    {
        var currentCount = _messages.Count;
        if (_cachedSummary != null && _messageCountAtCache == currentCount)
        {
            return _cachedSummary;
        }

        lock (_summaryLock)
        {
            if (_cachedSummary != null && _messageCountAtCache == currentCount)
            {
                return _cachedSummary;
            }

            var messages = _messages.ToArray();
            var summary = new DiagnosticSummary(
                TotalCount: messages.Length,
                ErrorCount: messages.Count(m => m.Severity == DiagnosticSeverity.Error),
                WarningCount: messages.Count(m => m.Severity == DiagnosticSeverity.Warning),
                InfoCount: messages.Count(m => m.Severity == DiagnosticSeverity.Info),
                HiddenCount: messages.Count(m => m.Severity == DiagnosticSeverity.Hidden),
                ByCode: messages.GroupBy(m => m.Code).ToImmutableDictionary(g => g.Key, g => g.Count()),
                BySeverity: messages.GroupBy(m => m.Severity).ToImmutableDictionary(g => g.Key, g => g.Count()),
                FirstError: messages.FirstOrDefault(m => m.Severity == DiagnosticSeverity.Error),
                Timestamp: DateTime.UtcNow
            );

            _cachedSummary = summary;
            _messageCountAtCache = currentCount;
            return summary;
        }
    }

    /// <summary>
    /// Clears all diagnostic messages.
    /// </summary>
    public void Clear()
    {
        while (_messages.TryTake(out _)) { }
        _cachedSummary = null;
        _messageCountAtCache = 0;
    }

    /// <summary>
    /// Gets the total number of messages.
    /// </summary>
    public int Count => _messages.Count;
}