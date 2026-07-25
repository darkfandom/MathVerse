namespace MathVerse.Math.DataScience.DatasetManagement;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using MathVerse.Math.DataScience.Core;

/// <summary>
/// Result of validating a dataset.
/// </summary>
public sealed class ValidationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the dataset is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the list of validation errors.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of validation warnings.
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Creates a new validation result.
    /// </summary>
    /// <param name="isValid">Whether the dataset is valid.</param>
    /// <returns>A new validation result.</returns>
    public static ValidationResult Create(bool isValid)
    {
        return new ValidationResult { IsValid = isValid };
    }
}

/// <summary>
/// Validates datasets against various quality checks.
/// </summary>
public sealed class DatasetValidator
{
    /// <summary>
    /// Validates a dataset and returns a validation result.
    /// </summary>
    /// <param name="dataset">The dataset to validate.</param>
    /// <returns>The validation result.</returns>
    public ValidationResult Validate(Dataset dataset)
    {
        _ = dataset ?? throw new ArgumentNullException(nameof(dataset));

        var result = ValidationResult.Create(true);
        CheckMissingValues(dataset, result);
        CheckDuplicates(dataset, result);
        CheckTypes(dataset, result);
        CheckRanges(dataset, result);
        return result;
    }

    /// <summary>
    /// Checks for missing values in the dataset.
    /// </summary>
    /// <param name="dataset">The dataset to check.</param>
    /// <param name="result">The validation result to update.</param>
    public void CheckMissingValues(Dataset dataset, ValidationResult result)
    {
        foreach (var col in dataset.Schema.Columns)
        {
            int missingCount = dataset.Rows.Count(r =>
                !r.TryGetValue(col.Name, out var v) || v == null);

            if (missingCount > 0)
            {
                double missingPct = missingCount * 100.0 / System.Math.Max(1, dataset.Count);
                result.Warnings.Add($"Column '{col.Name}' has {missingCount} missing values ({missingPct:F1}%)");
            }
        }
    }

    /// <summary>
    /// Checks for duplicate rows in the dataset.
    /// </summary>
    /// <param name="dataset">The dataset to check.</param>
    /// <param name="result">The validation result to update.</param>
    public void CheckDuplicates(Dataset dataset, ValidationResult result)
    {
        var seen = new HashSet<string>();
        int duplicateCount = 0;
        foreach (var row in dataset.Rows)
        {
            string key = string.Join("|", row.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value?.ToString() ?? ""));
            if (!seen.Add(key))
            {
                duplicateCount++;
            }
        }

        if (duplicateCount > 0)
        {
            result.Warnings.Add($"Dataset contains {duplicateCount} duplicate rows");
        }
    }

    /// <summary>
    /// Checks data types in the dataset.
    /// </summary>
    /// <param name="dataset">The dataset to check.</param>
    /// <param name="result">The validation result to update.</param>
    public void CheckTypes(Dataset dataset, ValidationResult result)
    {
        foreach (var col in dataset.Schema.Columns)
        {
            if (col.Type == ColumnType.Double)
            {
                foreach (var row in dataset.Rows)
                {
                    if (row.TryGetValue(col.Name, out var v) && v != null &&
                        !double.TryParse(v?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    {
                        result.Errors.Add($"Column '{col.Name}' contains non-numeric value: {v}");
                        result.IsValid = false;
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Checks value ranges in the dataset.
    /// </summary>
    /// <param name="dataset">The dataset to check.</param>
    /// <param name="result">The validation result to update.</param>
    public void CheckRanges(Dataset dataset, ValidationResult result)
    {
        foreach (var col in dataset.Schema.Columns)
        {
            if (col.Type == ColumnType.Double)
            {
                var values = dataset.GetColumn(col.Name)
                    .Where(v => v != null && double.TryParse(v?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    .Select(v => double.Parse(v!.ToString()!, NumberStyles.Any, CultureInfo.InvariantCulture))
                    .ToList();

                if (values.Count > 0)
                {
                    double min = values.Min();
                    double max = values.Max();
                    if (System.Math.Abs(min - max) < 1e-10)
                    {
                        result.Warnings.Add($"Column '{col.Name}' has constant value: {min}");
                    }
                }
            }
        }
    }
}