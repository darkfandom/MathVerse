namespace MathVerse.Math.Interop.Diagnostics;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Aggregated diagnostic report for interop operations.
/// </summary>
public sealed class DiagnosticReport
{
    /// <summary>
    /// Gets or sets the report title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the report was generated.
    /// </summary>
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the serialization diagnostics.
    /// </summary>
    public SerializationDiagnostics Serialization { get; set; } = new();

    /// <summary>
    /// Gets or sets the version diagnostics.
    /// </summary>
    public VersionDiagnostics Version { get; set; } = new();

    /// <summary>
    /// Gets or sets the conversion warnings.
    /// </summary>
    public List<ConversionWarning> ConversionWarnings { get; set; } = new();

    /// <summary>
    /// Gets or sets the compatibility reports.
    /// </summary>
    public List<CompatibilityReport> CompatibilityReports { get; set; } = new();

    /// <summary>
    /// Gets or sets the general diagnostic entries.
    /// </summary>
    public List<DiagnosticEntry> Entries { get; set; } = new();

    /// <summary>
    /// Gets the total number of issues (warnings + errors).
    /// </summary>
    public int TotalIssueCount => ConversionWarnings.Count + Entries.Count(e => e.Level == DiagnosticLevel.Error);

    /// <summary>
    /// Gets a value indicating whether the report contains any errors.
    /// </summary>
    public bool HasErrors => Entries.Any(e => e.Level == DiagnosticLevel.Error);

    /// <summary>
    /// Generates a human-readable summary of the report.
    /// </summary>
    /// <returns>A summary string.</returns>
    public string GetSummary()
    {
        var parts = new List<string>
        {
            $"Report: {Title}",
            $"Generated: {GeneratedAt:O}",
            $"Total Issues: {TotalIssueCount}",
            $"Has Errors: {HasErrors}",
            $"Conversion Warnings: {ConversionWarnings.Count}",
            $"Compatibility Reports: {CompatibilityReports.Count}"
        };
        return string.Join(Environment.NewLine, parts);
    }
}
