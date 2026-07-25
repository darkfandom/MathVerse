namespace MathVerse.Math.DataScience.DatasetManagement;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a single row in a data table.
/// </summary>
public sealed class Row
{
    /// <summary>
    /// Gets or sets the row index.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Gets the dictionary of column names to values for this row.
    /// </summary>
    public Dictionary<string, object?> Values { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Row"/> class.
    /// </summary>
    /// <param name="index">The row index.</param>
    public Row(int index)
    {
        Index = index;
        Values = new Dictionary<string, object?>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Row"/> class with initial values.
    /// </summary>
    /// <param name="index">The row index.</param>
    /// <param name="values">The initial values dictionary.</param>
    public Row(int index, Dictionary<string, object?> values)
    {
        Index = index;
        Values = values ?? throw new ArgumentNullException(nameof(values));
    }

    /// <summary>
    /// Gets the value of the specified column cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The expected type of the value.</typeparam>
    /// <param name="column">The column name.</param>
    /// <returns>The value cast to <typeparamref name="T"/>, or the default value of <typeparamref name="T"/> if not found.</returns>
    public T? GetField<T>(string column)
    {
        if (Values.TryGetValue(column, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }

    /// <summary>
    /// Sets the value of the specified column.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="value">The value to set.</param>
    public void SetField(string column, object? value)
    {
        Values[column] = value;
    }
}