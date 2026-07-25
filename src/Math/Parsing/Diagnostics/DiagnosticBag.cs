namespace MathVerse.Math.Parsing.Diagnostics;

/// <summary>
/// Accumulates diagnostics during lexing and parsing. Thread-safe.
/// </summary>
public sealed class DiagnosticBag
{
    private readonly List<Diagnostic> _diagnostics = [];
    private readonly object _lock = new();

    /// <summary>Gets the number of diagnostics.</summary>
    public int Count
    {
        get { lock (_lock) { return _diagnostics.Count; } }
    }

    /// <summary>Gets whether any errors exist.</summary>
    public bool HasErrors => GetBySeverity(DiagnosticSeverity.Error).Length > 0;

    /// <summary>Adds a diagnostic.</summary>
    public void Add(Diagnostic diagnostic)
    {
        lock (_lock)
        {
            _diagnostics.Add(diagnostic);
        }
    }

    /// <summary>Adds a diagnostic with the specified parameters.</summary>
    public void Add(string code, string message, DiagnosticSeverity severity, int line, int column, int length, string? suggestedFix = null)
    {
        Add(new Diagnostic(code, message, severity, line, column, length, suggestedFix));
    }

    /// <summary>Adds an error diagnostic.</summary>
    public void AddError(string code, string message, int line, int column, int length, string? suggestedFix = null)
    {
        Add(code, message, DiagnosticSeverity.Error, line, column, length, suggestedFix);
    }

    /// <summary>Adds a warning diagnostic.</summary>
    public void AddWarning(string code, string message, int line, int column, int length, string? suggestedFix = null)
    {
        Add(code, message, DiagnosticSeverity.Warning, line, column, length, suggestedFix);
    }

    /// <summary>Adds an info diagnostic.</summary>
    public void AddInfo(string code, string message, int line, int column, int length, string? suggestedFix = null)
    {
        Add(code, message, DiagnosticSeverity.Info, line, column, length, suggestedFix);
    }

    /// <summary>Returns all diagnostics.</summary>
    public Diagnostic[] GetAll()
    {
        lock (_lock)
        {
            return [.. _diagnostics];
        }
    }

    /// <summary>Returns diagnostics filtered by severity.</summary>
    public Diagnostic[] GetBySeverity(DiagnosticSeverity severity)
    {
        lock (_lock)
        {
            var result = new List<Diagnostic>();
            foreach (var d in _diagnostics)
            {
                if (d.Severity == severity)
                    result.Add(d);
            }
            return [.. result];
        }
    }

    /// <summary>Returns only error diagnostics.</summary>
    public Diagnostic[] GetErrors() => GetBySeverity(DiagnosticSeverity.Error);

    /// <summary>Returns only warning diagnostics.</summary>
    public Diagnostic[] GetWarnings() => GetBySeverity(DiagnosticSeverity.Warning);

    /// <summary>Merges another DiagnosticBag into this one.</summary>
    public void Merge(DiagnosticBag other)
    {
        var all = other.GetAll();
        lock (_lock)
        {
            _diagnostics.AddRange(all);
        }
    }

    /// <summary>Clears all diagnostics.</summary>
    public void Clear()
    {
        lock (_lock)
        {
            _diagnostics.Clear();
        }
    }
}
