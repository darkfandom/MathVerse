namespace MathVerse.Math.HPC.Diagnostics;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

/// <summary>
/// Summary of diagnostic results.
/// </summary>
public sealed record DiagnosticSummary(
    int TotalCount,
    int ErrorCount,
    int WarningCount,
    int InfoCount,
    int HiddenCount,
    ImmutableDictionary<string, int> ByCode,
    ImmutableDictionary<DiagnosticSeverity, int> BySeverity,
    DiagnosticMessage? FirstError,
    DateTimeOffset Timestamp)
{
    /// <summary>
    /// Gets whether there are any errors.
    /// </summary>
    public bool HasErrors => ErrorCount > 0;

    /// <summary>
    /// Gets whether there are any warnings.
    /// </summary>
    public bool HasWarnings => WarningCount > 0;

    /// <summary>
    /// Gets whether there are any diagnostics at all.
    /// </summary>
    public bool IsClean => ErrorCount == 0 && WarningCount == 0;

    /// <summary>
    /// Creates a summary from a collection of diagnostics.
    /// </summary>
    public static DiagnosticSummary From(IEnumerable<DiagnosticMessage> diagnostics)
    {
        var dict = new Dictionary<string, int>();
        var bySeverity = new Dictionary<DiagnosticSeverity, int>();
        int total = 0, errors = 0, warnings = 0, infos = 0, hidden = 0;
        DiagnosticMessage? firstError = null;

        foreach (var d in diagnostics)
        {
            total++;
            switch (d.Severity)
            {
                case DiagnosticSeverity.Error:
                    errors++;
                    if (firstError == null) firstError = d;
                    break;
                case DiagnosticSeverity.Warning: warnings++; break;
                case DiagnosticSeverity.Info: infos++; break;
                case DiagnosticSeverity.Hidden: hidden++; break;
            }

            dict[d.Code] = dict.GetValueOrDefault(d.Code, 0) + 1;
            bySeverity[d.Severity] = bySeverity.GetValueOrDefault(d.Severity, 0) + 1;
        }

        return new DiagnosticSummary(
            total,
            errors,
            warnings,
            infos,
            hidden,
            dict.ToImmutableDictionary(),
            bySeverity.ToImmutableDictionary(),
            firstError,
            DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Creates an empty summary.
    /// </summary>
    public static DiagnosticSummary Empty => new(0, 0, 0, 0, 0,
        ImmutableDictionary<string, int>.Empty,
        ImmutableDictionary<DiagnosticSeverity, int>.Empty,
        null,
        DateTimeOffset.UtcNow);

    public override string ToString() =>
        HasErrors ? $"Errors: {ErrorCount}, Warnings: {WarningCount}, Info: {InfoCount}"
            : HasWarnings ? $"Warnings: {WarningCount}, Info: {InfoCount}"
            : IsClean ? "No issues" : $"Info: {InfoCount}";
}