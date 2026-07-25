namespace MathVerse.Math.Interop.Diagnostics;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Report on compatibility between source and target formats.
/// </summary>
public sealed class CompatibilityReport
{
    /// <summary>
    /// Gets or sets the source format identifier.
    /// </summary>
    public string SourceFormat { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target format identifier.
    /// </summary>
    public string TargetFormat { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the overall compatibility score (0.0 to 1.0).
    /// </summary>
    public double CompatibilityScore { get; set; }

    /// <summary>
    /// Gets or sets the list of compatible features.
    /// </summary>
    public List<string> CompatibleFeatures { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of incompatible features.
    /// </summary>
    public List<string> IncompatibleFeatures { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of compatibility warnings.
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Gets a value indicating whether the conversion is fully compatible.
    /// </summary>
    public bool IsFullyCompatible => IncompatibleFeatures.Count == 0;

    /// <summary>
    /// Generates a human-readable summary.
    /// </summary>
    /// <returns>A summary string.</returns>
    public string GetSummary()
    {
        return $"Compatibility between {SourceFormat} and {TargetFormat}: {CompatibilityScore:P0} " +
               $"({CompatibleFeatures.Count} compatible, {IncompatibleFeatures.Count} incompatible, {Warnings.Count} warnings)";
    }
}
