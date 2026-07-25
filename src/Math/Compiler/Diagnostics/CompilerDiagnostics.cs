namespace MathVerse.Math.Compiler.Diagnostics;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

/// <summary>Thread-safe collector of compilation warnings, errors, and informational messages.</summary>
public sealed class CompilerDiagnostics
{
    private readonly ConcurrentBag<DiagnosticMessage> _messages = new();

    /// <summary>Reports a diagnostic message.</summary>
    /// <param name="severity">The severity of the message.</param>
    /// <param name="message">The message text.</param>
    /// <param name="line">Optional source line number.</param>
    public void Report(DiagnosticSeverity severity, string message, int? line = null)
    {
        if (message is null) throw new ArgumentNullException(nameof(message));
        _messages.Add(new DiagnosticMessage(severity, message, line));
    }

    /// <summary>Returns all diagnostic messages collected so far.</summary>
    public IReadOnlyList<DiagnosticMessage> GetMessages()
    {
        return _messages.ToList();
    }

    /// <summary>Returns only error-level diagnostic messages.</summary>
    public IReadOnlyList<DiagnosticMessage> GetErrors()
    {
        return _messages.Where(m => m.Severity == DiagnosticSeverity.Error).ToList();
    }

    /// <summary>Returns only warning-level diagnostic messages.</summary>
    public IReadOnlyList<DiagnosticMessage> GetWarnings()
    {
        return _messages.Where(m => m.Severity == DiagnosticSeverity.Warning).ToList();
    }

    /// <summary>Gets whether any error-level messages have been reported.</summary>
    public bool HasErrors => _messages.Any(m => m.Severity == DiagnosticSeverity.Error);

    /// <summary>Gets whether any warning-level messages have been reported.</summary>
    public bool HasWarnings => _messages.Any(m => m.Severity == DiagnosticSeverity.Warning);

    /// <summary>Gets the total number of messages collected.</summary>
    public int Count => _messages.Count;

    /// <summary>Clears all diagnostic messages.</summary>
    public void Clear()
    {
        while (_messages.TryTake(out _)) { }
    }
}
