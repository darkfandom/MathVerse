namespace MathVerse.Math.DataScience.Core;

using System.Collections.Generic;

/// <summary>
/// Contains metadata information about a dataset.
/// </summary>
public sealed class DatasetMetadata
{
    /// <summary>
    /// Gets or sets the name of the dataset.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the dataset.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source of the dataset.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTimeOffset Modified { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the number of rows in the dataset.
    /// </summary>
    public int RowCount { get; set; }

    /// <summary>
    /// Gets or sets the number of columns in the dataset.
    /// </summary>
    public int ColumnCount { get; set; }

    /// <summary>
    /// Gets or sets the tags associated with the dataset.
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Gets or sets the schema information for the dataset.
    /// </summary>
    public Dictionary<string, string> Schema { get; set; } = new();
}