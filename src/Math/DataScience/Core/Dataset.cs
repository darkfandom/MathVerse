namespace MathVerse.Math.DataScience.Core;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Core dataset type representing a collection of rows with named columns.
/// </summary>
public sealed class Dataset
{
    /// <summary>
    /// Gets the unique identifier for this dataset.
    /// </summary>
    public Guid DatasetId { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the name of the dataset.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metadata for this dataset.
    /// </summary>
    public DatasetMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the schema defining column names and types.
    /// </summary>
    public DatasetManagement.Schema Schema { get; set; } = new();

    /// <summary>
    /// Gets the list of rows, where each row is a dictionary mapping column names to values.
    /// </summary>
    public List<Dictionary<string, object?>> Rows { get; } = new();

    /// <summary>
    /// Gets the number of rows in the dataset.
    /// </summary>
    public int Count => Rows.Count;

    /// <summary>
    /// Gets the values of a specific column across all rows.
    /// </summary>
    /// <param name="name">The column name.</param>
    /// <returns>A list of values in the specified column.</returns>
    public List<object?> GetColumn(string name)
    {
        return Rows.Select(r => r.TryGetValue(name, out var value) ? value : null).ToList();
    }

    /// <summary>
    /// Gets a specific row by index.
    /// </summary>
    /// <param name="index">The zero-based row index.</param>
    /// <returns>The row at the specified index.</returns>
    public Dictionary<string, object?> GetRow(int index)
    {
        return Rows[index];
    }

    /// <summary>
    /// Filters the dataset using a predicate function.
    /// </summary>
    /// <param name="predicate">The filter predicate applied to each row.</param>
    /// <returns>A new dataset containing only the matching rows.</returns>
    public Dataset Filter(Func<Dictionary<string, object?>, bool> predicate)
    {
        var result = new Dataset
        {
            Name = Name,
            Metadata = Metadata,
            Schema = Schema
        };
        result.Rows.AddRange(Rows.Where(predicate));
        return result;
    }

    /// <summary>
    /// Selects specific columns from the dataset.
    /// </summary>
    /// <param name="columns">The column names to include.</param>
    /// <returns>A new dataset containing only the specified columns.</returns>
    public Dataset Select(string[] columns)
    {
        var columnSet = new HashSet<string>(columns);
        var result = new Dataset
        {
            Name = Name,
            Metadata = new DatasetMetadata
            {
                Name = Metadata.Name,
                Description = Metadata.Description,
                Source = Metadata.Source,
                Created = Metadata.Created,
                Modified = DateTimeOffset.UtcNow,
                ColumnCount = columns.Length,
                Tags = new System.Collections.Generic.List<string>(Metadata.Tags)
            }
        };

        foreach (var col in columns)
        {
            if (Schema.HasColumn(col))
            {
                result.Schema.AddColumn(col, Schema.GetColumn(col).Type);
            }
        }

        foreach (var row in Rows)
        {
            var newRow = new Dictionary<string, object?>();
            foreach (var key in columnSet)
            {
                if (row.TryGetValue(key, out var value))
                {
                    newRow[key] = value;
                }
            }
            result.Rows.Add(newRow);
        }

        return result;
    }

    /// <summary>
    /// Returns the first n rows of the dataset.
    /// </summary>
    /// <param name="n">The number of rows to return.</param>
    /// <returns>A new dataset containing the first n rows.</returns>
    public Dataset Head(int n)
    {
        var result = new Dataset
        {
            Name = Name,
            Metadata = Metadata,
            Schema = Schema
        };
        result.Rows.AddRange(Rows.Take(n));
        return result;
    }

    /// <summary>
    /// Returns the last n rows of the dataset.
    /// </summary>
    /// <param name="n">The number of rows to return.</param>
    /// <returns>A new dataset containing the last n rows.</returns>
    public Dataset Tail(int n)
    {
        var result = new Dataset
        {
            Name = Name,
            Metadata = Metadata,
            Schema = Schema
        };
        result.Rows.AddRange(Rows.Skip(System.Math.Max(0, Rows.Count - n)));
        return result;
    }
}