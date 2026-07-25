namespace MathVerse.Math.Semantics.Diagnostics;

/// <summary>
/// Thread-safe collection of semantic diagnostics.
/// </summary>
public sealed class SemanticDiagnosticBag
{
    private readonly List<SemanticDiagnostic> _diagnostics = [];
    private readonly object _lock = new();

    /// <summary>Gets all collected diagnostics.</summary>
    public IReadOnlyList<SemanticDiagnostic> All
    {
        get { lock (_lock) return [.. _diagnostics]; }
    }

    /// <summary>Gets diagnostics at a specific severity.</summary>
    public IReadOnlyList<SemanticDiagnostic> GetBySeverity(SemanticSeverity severity)
    {
        lock (_lock) return _diagnostics.Where(d => d.Severity == severity).ToList();
    }

    /// <summary>Gets whether there are any errors.</summary>
    public bool HasErrors
    {
        get { lock (_lock) return _diagnostics.Any(d => d.Severity == SemanticSeverity.Error); }
    }

    /// <summary>Gets whether there are any warnings.</summary>
    public bool HasWarnings
    {
        get { lock (_lock) return _diagnostics.Any(d => d.Severity == SemanticSeverity.Warning); }
    }

    /// <summary>Gets the count of all diagnostics.</summary>
    public int Count
    {
        get { lock (_lock) return _diagnostics.Count; }
    }

    /// <summary>Reports a diagnostic.</summary>
    public void Report(SemanticDiagnostic diagnostic)
    {
        lock (_lock) _diagnostics.Add(diagnostic);
    }

    /// <summary>Reports a diagnostic from components.</summary>
    public void Report(SemanticDiagnosticCode code, string message,
        SemanticSeverity severity, string? location = null)
    {
        Report(new SemanticDiagnostic(code, message, severity, location));
    }

    /// <summary>Reports an error.</summary>
    public void ReportError(SemanticDiagnosticCode code, string message, string? location = null)
        => Report(code, message, SemanticSeverity.Error, location);

    /// <summary>Reports a warning.</summary>
    public void ReportWarning(SemanticDiagnosticCode code, string message, string? location = null)
        => Report(code, message, SemanticSeverity.Warning, location);

    /// <summary>Reports an info message.</summary>
    public void ReportInfo(SemanticDiagnosticCode code, string message, string? location = null)
        => Report(code, message, SemanticSeverity.Info, location);

    /// <summary>Merges another bag's diagnostics into this one.</summary>
    public void Merge(SemanticDiagnosticBag other)
    {
        lock (_lock)
        {
            lock (other._lock)
            {
                _diagnostics.AddRange(other._diagnostics);
            }
        }
    }

    /// <summary>Clears all diagnostics.</summary>
    public void Clear()
    {
        lock (_lock) _diagnostics.Clear();
    }
}
