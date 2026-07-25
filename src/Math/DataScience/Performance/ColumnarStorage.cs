namespace MathVerse.Math.DataScience.Performance;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Provides column-oriented storage for fast columnar access and analytics.
/// </summary>
public sealed class ColumnarStorage
{
    private readonly Dictionary<string, double[]> _columns = new();
    private int _rowCount;

    /// <summary>
    /// Gets the number of rows stored.
    /// </summary>
    public int RowCount => _rowCount;

    /// <summary>
    /// Gets the names of all stored columns.
    /// </summary>
    public IReadOnlyCollection<string> ColumnNames => _columns.Keys;

    /// <summary>
    /// Gets the number of columns stored.
    /// </summary>
    public int ColumnCount => _columns.Count;

    /// <summary>
    /// Stores a column of values with the specified name.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <param name="values">The column values. If this is the first column, it sets the row count.</param>
    public void Store(string column, double[] values)
    {
        if (string.IsNullOrEmpty(column)) throw new ArgumentException("Column name cannot be null or empty.", nameof(column));
        if (values is null) throw new ArgumentNullException(nameof(values));

        if (_columns.Count == 0)
        {
            _rowCount = values.Length;
        }
        else if (values.Length != _rowCount)
        {
            throw new ArgumentException(
                $"Array length ({values.Length}) must match existing row count ({_rowCount}).",
                nameof(values));
        }

        _columns[column] = values;
    }

    /// <summary>
    /// Gets a read-only span of values for the specified column.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <returns>A read-only span of the column's values.</returns>
    public ReadOnlySpan<double> GetColumn(string column)
    {
        if (!_columns.TryGetValue(column, out double[]? values))
            throw new KeyNotFoundException($"Column '{column}' not found.");

        return values;
    }

    /// <summary>
    /// Gets the underlying array for the specified column.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <returns>The array of column values.</returns>
    public double[] GetColumnArray(string column)
    {
        if (!_columns.TryGetValue(column, out double[]? values))
            throw new KeyNotFoundException($"Column '{column}' not found.");

        return values;
    }

    /// <summary>
    /// Determines whether the specified column exists.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <returns>true if the column exists; otherwise, false.</returns>
    public bool HasColumn(string column)
    {
        return _columns.ContainsKey(column);
    }

    /// <summary>
    /// Gets the sum of all values in the specified column.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <returns>The sum of all values.</returns>
    public double ColumnSum(string column)
    {
        ReadOnlySpan<double> values = GetColumn(column);
        double sum = 0.0;
        for (int i = 0; i < values.Length; i++)
        {
            sum += values[i];
        }
        return sum;
    }

    /// <summary>
    /// Gets the mean of all values in the specified column.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <returns>The arithmetic mean.</returns>
    public double ColumnMean(string column)
    {
        ReadOnlySpan<double> values = GetColumn(column);
        if (values.Length == 0) return 0.0;
        return ColumnSum(column) / values.Length;
    }

    /// <summary>
    /// Gets the variance of all values in the specified column.
    /// </summary>
    /// <param name="column">The column name.</param>
    /// <returns>The population variance.</returns>
    public double ColumnVariance(string column)
    {
        ReadOnlySpan<double> values = GetColumn(column);
        if (values.Length < 2) return 0.0;

        double mean = ColumnMean(column);
        double m2 = 0.0;
        for (int i = 0; i < values.Length; i++)
        {
            double diff = values[i] - mean;
            m2 += diff * diff;
        }
        return m2 / values.Length;
    }
}
