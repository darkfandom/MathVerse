namespace MathVerse.Math.DataScience.DatasetManagement;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Defines the schema (column structure) of a dataset.
/// </summary>
public sealed class Schema
{
    /// <summary>
    /// Gets the list of column definitions in this schema.
    /// </summary>
    public List<ColumnDefinition> Columns { get; } = new();

    /// <summary>
    /// Gets the names of all columns in the schema.
    /// </summary>
    public IEnumerable<string> ColumnNames => Columns.Select(c => c.Name);

    /// <summary>
    /// Adds a column to the schema.
    /// </summary>
    /// <param name="name">The column name.</param>
    /// <param name="type">The column data type.</param>
    /// <returns>The schema instance for method chaining.</returns>
    public Schema AddColumn(string name, ColumnType type)
    {
        Columns.Add(new ColumnDefinition(name, type));
        return this;
    }

    /// <summary>
    /// Gets a column definition by name.
    /// </summary>
    /// <param name="name">The column name.</param>
    /// <returns>The column definition.</returns>
    public ColumnDefinition GetColumn(string name)
    {
        var column = Columns.FirstOrDefault(c => c.Name == name);
        if (column == null)
            throw new ArgumentException($"Column '{name}' not found in schema.", nameof(name));
        return column;
    }

    /// <summary>
    /// Determines whether the schema contains a column with the specified name.
    /// </summary>
    /// <param name="name">The column name to check.</param>
    /// <returns>true if the column exists; otherwise, false.</returns>
    public bool HasColumn(string name)
    {
        return Columns.Any(c => c.Name == name);
    }
}