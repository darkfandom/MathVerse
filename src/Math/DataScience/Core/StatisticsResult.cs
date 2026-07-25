namespace MathVerse.Math.DataScience.Core;

using System.Collections.Generic;

/// <summary>
/// Result of computing statistics on a dataset.
/// </summary>
public sealed class StatisticsResult
{
    /// <summary>
    /// Gets or sets the per-column statistics.
    /// </summary>
    public Dictionary<string, ColumnStatistics> Columns { get; set; } = new();

    /// <summary>
    /// Gets or sets the total row count.
    /// </summary>
    public int RowCount { get; set; }

    /// <summary>
    /// Gets or sets the total column count.
    /// </summary>
    public int ColumnCount { get; set; }

    /// <summary>
    /// Creates a new <see cref="StatisticsResult"/> instance.
    /// </summary>
    /// <param name="columns">The column statistics dictionary.</param>
    /// <param name="rowCount">The total row count.</param>
    /// <param name="columnCount">The total column count.</param>
    /// <returns>A new statistics result.</returns>
    public static StatisticsResult Create(Dictionary<string, ColumnStatistics> columns, int rowCount, int columnCount)
    {
        return new StatisticsResult
        {
            Columns = columns,
            RowCount = rowCount,
            ColumnCount = columnCount
        };
    }
}