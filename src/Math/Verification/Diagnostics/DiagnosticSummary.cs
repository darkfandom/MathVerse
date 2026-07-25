namespace MathVerse.Math.Verification.Diagnostics;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

/// <summary>
/// Summary of diagnostic results.
/// </summary>
public sealed record DiagnosticSummary(
    int TotalCount,
    int ErrorCount,
    int WarningCount,
    int InfoCount,
    int HiddenCount,
    ImmutableDictionary<string, int> ByCategory,
    ImmutableDictionary<string, int> ById)
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
    public bool HasAny => TotalCount > 0;

    /// <summary>
    /// Creates a summary from a collection of diagnostics.
    /// </summary>
    public static DiagnosticSummary From(IEnumerable<DiagnosticMessage> diagnostics)
    {
        var dict = new Dictionary<string, int>();
        var byId = new Dictionary<string, int>();
        int total = 0, errors = 0, warnings = 0, infos = 0, hidden = 0;

        foreach (var d in diagnostics)
        {
            total++;
            switch (d.Severity)
            {
                case DiagnosticSeverity.Error: errors++; break;
                case DiagnosticSeverity.Warning: warnings++; break;
                case DiagnosticSeverity.Info: infos++; break;
                case DiagnosticSeverity.Hidden: hidden++; break;
            }

            var category = d.PropertiesValue.TryGetValue("Category", out var cat) ? cat ?? "General" : "General";
            dict[category] = dict.GetValueOrDefault(category, 0) + 1;
            byId[d.Id] = byId.GetValueOrDefault(d.Id, 0) + 1;
        }

        return new DiagnosticSummary(
            total,
            errors,
            warnings,
            infos,
            hidden,
            dict.ToImmutableDictionary(),
            byId.ToImmutableDictionary());
    }

    /// <summary>
    /// Creates an empty summary.
    /// </summary>
    public static DiagnosticSummary Empty => new(0, 0, 0, 0, 0, ImmutableDictionary<string, int>.Empty, ImmutableDictionary<string, int>.Empty);

    public override string ToString() =>
        HasErrors ? $"Errors: {ErrorCount}, Warnings: {WarningCount}, Info: {InfoCount}"
            : HasWarnings ? $"Warnings: {WarningCount}, Info: {InfoCount}"
            : HasAny ? $"Info: {InfoCount}" : "No issues";
}

/// <summary>
/// Summary of proof diagnostics.
/// </summary>
public sealed record ProofSummary(
    int TotalSteps,
    int VerifiedSteps,
    int FailedSteps,
    int SkippedSteps,
    TimeSpan VerificationTime,
    ImmutableDictionary<string, int> TacticCounts,
    ImmutableDictionary<string, TimeSpan> TacticTimes,
    ImmutableArray<ProofStepSummary> StepSummaries)
{
    public bool AllVerified => FailedSteps == 0 && SkippedSteps == 0;
    public double VerificationRate => TotalSteps > 0 ? (double)VerifiedSteps / TotalSteps : 1.0;
    public TimeSpan AverageStepTime => TotalSteps > 0 ? TimeSpan.FromTicks(VerificationTime.Ticks / TotalSteps) : TimeSpan.Zero;

    public static ProofSummary Empty => new(0, 0, 0, 0, TimeSpan.Zero, ImmutableDictionary<string, int>.Empty, ImmutableDictionary<string, TimeSpan>.Empty, ImmutableArray<ProofStepSummary>.Empty);
}

/// <summary>
/// Summary of a single proof step.
/// </summary>
public sealed record ProofStepSummary(
    int StepNumber,
    string Tactic,
    string Goal,
    bool Verified,
    TimeSpan Duration,
    string? ErrorMessage = null);

/// <summary>
/// Summary of reliability analysis.
/// </summary>
public sealed record ReliabilitySummary(
    double CorrectnessProbability,
    double TerminationProbability,
    double MemorySafetyProbability,
    double OverallReliability,
    ImmutableArray<ReliabilityIssue> Issues,
    TimeSpan AnalysisTime)
{
    public bool IsReliable => OverallReliability >= 0.95 && Issues.All(i => i.Severity != DiagnosticSeverity.Error);

    public static ReliabilitySummary Unknown => new(0, 0, 0, 0, ImmutableArray<ReliabilityIssue>.Empty, TimeSpan.Zero);
}

/// <summary>
/// Represents a reliability issue found during analysis.
/// </summary>
public sealed record ReliabilityIssue(
    string Id,
    string Description,
    DiagnosticSeverity Severity,
    string? Location = null,
    string? Suggestion = null);