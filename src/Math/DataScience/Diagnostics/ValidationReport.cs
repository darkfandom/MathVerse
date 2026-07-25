namespace MathVerse.Math.DataScience.Diagnostics;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents the result of validating a dataset against a set of rules.
/// </summary>
public sealed class ValidationReport
{
    /// <summary>
    /// Gets or sets a value indicating whether all validation checks passed.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the list of validation errors that prevent the data from being valid.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of validation warnings that do not prevent validity.
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of validation checks that passed.
    /// </summary>
    public List<string> PassedChecks { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of validation checks that failed.
    /// </summary>
    public List<string> FailedChecks { get; set; } = new();

    /// <summary>
    /// Gets the total number of checks performed.
    /// </summary>
    public int TotalChecks => PassedChecks.Count + FailedChecks.Count;

    /// <summary>
    /// Gets the pass rate as a fraction (0-1).
    /// </summary>
    public double PassRate => TotalChecks > 0 ? (double)PassedChecks.Count / TotalChecks : 0.0;

    /// <summary>
    /// Gets or sets the name of the dataset that was validated.
    /// </summary>
    public string DatasetName { get; set; } = string.Empty;

    /// <summary>
    /// Creates a new empty <see cref="ValidationReport"/> instance.
    /// </summary>
    /// <param name="datasetName">The name of the dataset.</param>
    /// <returns>A new validation report.</returns>
    public static ValidationReport Create(string datasetName)
    {
        return new ValidationReport { DatasetName = datasetName, IsValid = true };
    }
}
