namespace MathVerse.Math.Verification.Diagnostics;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

/// <summary>
/// Main diagnostics engine for verification results.
/// </summary>
public sealed class VerificationDiagnostics
{
    private readonly List<DiagnosticMessage> _diagnostics = new();
    private readonly DiagnosticOptions _options;

    public VerificationDiagnostics(DiagnosticOptions? options = null)
    {
        _options = options ?? DiagnosticOptions.Default;
    }

    public IReadOnlyList<DiagnosticMessage> Diagnostics => _diagnostics;
    public DiagnosticSummary Summary => DiagnosticSummary.From(_diagnostics);

    /// <summary>
    /// Adds a diagnostic message.
    /// </summary>
    public void Add(DiagnosticMessage diagnostic)
    {
        if (_options.ShouldReport(diagnostic.Severity))
            _diagnostics.Add(diagnostic);
    }

    /// <summary>
    /// Adds an info diagnostic.
    /// </summary>
    public void Info(string id, string message, string? file = null, int line = 0, int column = 0) =>
        Add(DiagnosticMessage.Info(id, message, file, line, column));

    /// <summary>
    /// Adds a warning diagnostic.
    /// </summary>
    public void Warning(string id, string message, string? file = null, int line = 0, int column = 0) =>
        Add(DiagnosticMessage.Warning(id, message, file, line, column));

    /// <summary>
    /// Adds an error diagnostic.
    /// </summary>
    public void Error(string id, string message, string? file = null, int line = 0, int column = 0) =>
        Add(DiagnosticMessage.Error(id, message, file, line, column));

    /// <summary>
    /// Adds a hidden diagnostic.
    /// </summary>
    public void Hidden(string id, string message, string? file = null, int line = 0, int column = 0) =>
        Add(DiagnosticMessage.Hidden(id, message, file, line, column));

    /// <summary>
    /// Merges another diagnostics collection into this one.
    /// </summary>
    public void Merge(VerificationDiagnostics other)
    {
        _diagnostics.AddRange(other._diagnostics);
    }

    /// <summary>
    /// Clears all diagnostics.
    /// </summary>
    public void Clear() => _diagnostics.Clear();

    /// <summary>
    /// Gets diagnostics filtered by severity.
    /// </summary>
    public IEnumerable<DiagnosticMessage> GetBySeverity(DiagnosticSeverity severity) =>
        _diagnostics.Where(d => d.Severity == severity);

    /// <summary>
    /// Gets diagnostics filtered by category.
    /// </summary>
    public IEnumerable<DiagnosticMessage> GetByCategory(string category) =>
        _diagnostics.Where(d => d.PropertiesValue.GetValueOrDefault("Category") == category);

    /// <summary>
    /// Creates an analysis report from the diagnostics.
    /// </summary>
    public AnalysisReport ToReport(string target, TimeSpan analysisTime) =>
        new(target, _diagnostics.ToImmutableArray(), Summary, analysisTime);
}

/// <summary>
/// Options for diagnostic reporting.
/// </summary>
public sealed record DiagnosticOptions(
    DiagnosticSeverity MinimumSeverity = DiagnosticSeverity.Info,
    bool IncludeHidden = false,
    ImmutableHashSet<string>? SuppressedIds = null,
    int MaxDiagnostics = 1000)
{
    public static DiagnosticOptions Default => new();

    public bool ShouldReport(DiagnosticSeverity severity) =>
        severity >= MinimumSeverity && (IncludeHidden || severity != DiagnosticSeverity.Hidden) &&
        (SuppressedIds == null || !SuppressedIds.Contains("ALL"));

    public DiagnosticOptions WithMinimumSeverity(DiagnosticSeverity severity) => this with { MinimumSeverity = severity };
    public DiagnosticOptions WithSuppressed(params string[] ids) => this with { SuppressedIds = (SuppressedIds ?? ImmutableHashSet<string>.Empty).Union(ids) };
    public DiagnosticOptions WithMaxDiagnostics(int max) => this with { MaxDiagnostics = max };
}

/// <summary>
/// Comprehensive analysis report.
/// </summary>
public sealed record AnalysisReport(
    string TargetName,
    ImmutableArray<DiagnosticMessage> Diagnostics,
    DiagnosticSummary Summary,
    TimeSpan AnalysisTime,
    PerformanceMetrics? Performance = null,
    ProofSummary? Proof = null,
    ReliabilitySummary? Reliability = null)
{
    public bool HasErrors => Summary.HasErrors;
    public bool HasWarnings => Summary.HasWarnings;
    public bool IsClean => !HasErrors && !HasWarnings;
    public DateTime Timestamp => DateTime.UtcNow;
    public int TotalDiagnostics => Diagnostics.Length;

    public override string ToString() =>
        $"{TargetName}: {Summary} (Time: {AnalysisTime.TotalMilliseconds:F1}ms)";
}