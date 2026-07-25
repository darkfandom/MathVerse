namespace MathVerse.Math.DataScience.StreamingAnalytics;

using System;
using System.Collections.Generic;

/// <summary>
/// A streaming dataset that accumulates rows and supports windowed access.
/// </summary>
public sealed class StreamingDataset
{
    private readonly List<Dictionary<string, object?>> _rows = new();

    /// <summary>
    /// Occurs when a new row is added to the streaming dataset.
    /// </summary>
    public event Action<Dictionary<string, object?>>? OnDataAdded;

    /// <summary>
    /// Gets the total number of rows that have been added.
    /// </summary>
    public int Count => _rows.Count;

    /// <summary>
    /// Adds a row to the streaming dataset and raises the <see cref="OnDataAdded"/> event.
    /// </summary>
    /// <param name="row">The row data as a dictionary of column names to values.</param>
    public void AddRow(Dictionary<string, object?> row)
    {
        if (row is null) throw new ArgumentNullException(nameof(row));

        _rows.Add(row);
        OnDataAdded?.Invoke(row);
    }

    /// <summary>
    /// Gets the most recently added rows.
    /// </summary>
    /// <param name="count">The maximum number of rows to return.</param>
    /// <returns>A list containing the latest rows, in chronological order.</returns>
    public List<Dictionary<string, object?>> GetLatest(int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be non-negative.");

        int startIndex = System.Math.Max(0, _rows.Count - count);
        int resultCount = _rows.Count - startIndex;

        List<Dictionary<string, object?>> result = new(resultCount);
        for (int i = startIndex; i < _rows.Count; i++)
        {
            result.Add(_rows[i]);
        }
        return result;
    }

    /// <summary>
    /// Gets a window of rows starting from the specified index.
    /// </summary>
    /// <param name="startIndex">The zero-based starting index.</param>
    /// <param name="size">The maximum number of rows in the window.</param>
    /// <returns>A list containing the rows in the specified window.</returns>
    public List<Dictionary<string, object?>> GetWindow(int startIndex, int size)
    {
        if (startIndex < 0) throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "Must be non-negative.");
        if (size < 0) throw new ArgumentOutOfRangeException(nameof(size), size, "Must be non-negative.");

        int end = System.Math.Min(startIndex + size, _rows.Count);
        List<Dictionary<string, object?>> result = new();
        for (int i = startIndex; i < end; i++)
        {
            result.Add(_rows[i]);
        }
        return result;
    }

    /// <summary>
    /// Gets the row at the specified index.
    /// </summary>
    /// <param name="index">The zero-based row index.</param>
    /// <returns>The row at the specified index.</returns>
    public Dictionary<string, object?> GetRow(int index)
    {
        if (index < 0 || index >= _rows.Count)
            throw new ArgumentOutOfRangeException(nameof(index), index, "Index is out of range.");
        return _rows[index];
    }
}
