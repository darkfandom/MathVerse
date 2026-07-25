namespace MathVerse.Math.DataScience.DatasetManagement;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

/// <summary>
/// Represents a typed column in a dataset.
/// </summary>
public sealed class Column
{
    /// <summary>
    /// Gets the column name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the data type of values in this column.
    /// </summary>
    public ColumnType DataType { get; }

    /// <summary>
    /// Gets the immutable array of values in this column.
    /// </summary>
    public ImmutableArray<object?> Values { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Column"/> class.
    /// </summary>
    /// <param name="name">The column name.</param>
    /// <param name="dataType">The data type of the column.</param>
    /// <param name="values">The values in the column.</param>
    public Column(string name, ColumnType dataType, IEnumerable<object?> values)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        DataType = dataType;
        Values = values?.ToImmutableArray() ?? ImmutableArray<object?>.Empty;
    }

    /// <summary>
    /// Gets the value at the specified index as a double.
    /// </summary>
    /// <param name="index">The zero-based index.</param>
    /// <returns>The value as a double, or 0 if conversion fails.</returns>
    public double GetDouble(int index)
    {
        if (index < 0 || index >= Values.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (Values[index] == null) return 0.0;
        if (double.TryParse(Values[index]?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            return result;
        return 0.0;
    }

    /// <summary>
    /// Gets the value at the specified index as a string.
    /// </summary>
    /// <param name="index">The zero-based index.</param>
    /// <returns>The value as a string, or null if the value is null.</returns>
    public string? GetString(int index)
    {
        if (index < 0 || index >= Values.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        return Values[index]?.ToString();
    }

    /// <summary>
    /// Determines whether the value at the specified index is missing (null).
    /// </summary>
    /// <param name="index">The zero-based index.</param>
    /// <returns>true if the value is null; otherwise, false.</returns>
    public bool IsMissing(int index)
    {
        if (index < 0 || index >= Values.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        return Values[index] == null;
    }

    /// <summary>
    /// Gets the count of missing (null) values in the column.
    /// </summary>
    /// <returns>The number of null values.</returns>
    public int MissingCount()
    {
        return Values.Count(v => v == null);
    }

    /// <summary>
    /// Gets the count of distinct non-null values in the column.
    /// </summary>
    /// <returns>The number of distinct values.</returns>
    public int DistinctCount()
    {
        return Values.Where(v => v != null).Select(v => v!.ToString()).Distinct().Count();
    }
}