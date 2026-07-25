namespace MathVerse.Math.DataScience.Core;

using System.Collections.Generic;

/// <summary>
/// Result of a dataset analysis operation.
/// </summary>
public sealed class AnalysisResult
{
    /// <summary>
    /// Gets or sets the name of the dataset analyzed.
    /// </summary>
    public string DatasetName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of rows in the dataset.
    /// </summary>
    public int RowCount { get; set; }

    /// <summary>
    /// Gets or sets the number of columns in the dataset.
    /// </summary>
    public int ColumnCount { get; set; }

    /// <summary>
    /// Gets or sets the column statistics.
    /// </summary>
    public Dictionary<string, ColumnStatistics> ColumnStatistics { get; set; } = new();

    /// <summary>
    /// Gets or sets the overall data quality score (0-100).
    /// </summary>
    public double QualityScore { get; set; }

    /// <summary>
    /// Gets or sets the list of detected issues.
    /// </summary>
    public List<string> Issues { get; set; } = new();

    /// <summary>
    /// Creates a new <see cref="AnalysisResult"/> instance.
    /// </summary>
    /// <param name="datasetName">The dataset name.</param>
    /// <param name="rowCount">The row count.</param>
    /// <param name="columnCount">The column count.</param>
    /// <returns>A new analysis result.</returns>
    public static AnalysisResult Create(string datasetName, int rowCount, int columnCount)
    {
        return new AnalysisResult
        {
            DatasetName = datasetName,
            RowCount = rowCount,
            ColumnCount = columnCount
        };
    }
}