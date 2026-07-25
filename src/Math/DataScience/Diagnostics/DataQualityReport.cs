namespace MathVerse.Math.DataScience.Diagnostics;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a quality report for a dataset, including scores and detected issues.
/// </summary>
public sealed class DataQualityReport
{
    /// <summary>
    /// Gets or sets the overall quality score (0-100).
    /// </summary>
    public double OverallScore { get; set; }

    /// <summary>
    /// Gets or sets the completeness score (0-100), measuring how many cells are non-null.
    /// </summary>
    public double CompletenessScore { get; set; }

    /// <summary>
    /// Gets or sets the consistency score (0-100), measuring data type consistency.
    /// </summary>
    public double ConsistencyScore { get; set; }

    /// <summary>
    /// Gets or sets the accuracy score (0-100), measuring data within expected ranges.
    /// </summary>
    public double AccuracyScore { get; set; }

    /// <summary>
    /// Gets or sets the timeliness score (0-100), measuring data freshness if timestamps are present.
    /// </summary>
    public double TimelinessScore { get; set; }

    /// <summary>
    /// Gets or sets the list of detected data quality issues.
    /// </summary>
    public List<string> Issues { get; set; } = new();

    /// <summary>
    /// Gets or sets the total row count.
    /// </summary>
    public int RowCount { get; set; }

    /// <summary>
    /// Gets or sets the total column count.
    /// </summary>
    public int ColumnCount { get; set; }

    /// <summary>
    /// Gets or sets the per-column completeness (fraction of non-null values).
    /// </summary>
    public Dictionary<string, double> ColumnCompleteness { get; set; } = new();

    /// <summary>
    /// Gets or sets the name of the dataset that was analyzed.
    /// </summary>
    public string DatasetName { get; set; } = string.Empty;

    /// <summary>
    /// Creates a new <see cref="DataQualityReport"/> instance.
    /// </summary>
    /// <param name="datasetName">The name of the dataset.</param>
    /// <returns>A new empty quality report.</returns>
    public static DataQualityReport Create(string datasetName)
    {
        return new DataQualityReport { DatasetName = datasetName };
    }
}
