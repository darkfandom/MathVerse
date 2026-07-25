namespace MathVerse.Math.DataScience.DatasetManagement;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using MathVerse.Math.DataScience.Core;

/// <summary>
/// Fluent builder for constructing datasets.
/// </summary>
public sealed class DatasetBuilder
{
    private string _name = string.Empty;
    private readonly Schema _schema = new();
    private readonly List<Dictionary<string, object?>> _rows = new();

    /// <summary>
    /// Sets the name of the dataset.
    /// </summary>
    /// <param name="name">The dataset name.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DatasetBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Adds a column definition to the schema.
    /// </summary>
    /// <param name="name">The column name.</param>
    /// <param name="type">The column data type.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DatasetBuilder WithColumn(string name, ColumnType type)
    {
        _schema.AddColumn(name, type);
        return this;
    }

    /// <summary>
    /// Adds a row of values to the dataset.
    /// </summary>
    /// <param name="values">The column name-value pairs for the row.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DatasetBuilder WithRow(Dictionary<string, object?> values)
    {
        _rows.Add(values);
        return this;
    }

    /// <summary>
    /// Populates the dataset from CSV content.
    /// </summary>
    /// <param name="csv">The CSV content.</param>
    /// <param name="delimiter">The delimiter character.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DatasetBuilder WithCsv(string csv, char delimiter = ',')
    {
        var lines = csv.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToArray();

        if (lines.Length == 0) return this;

        var headers = ParseCsvLine(lines[0], delimiter);

        foreach (var header in headers)
        {
            _schema.AddColumn(header, ColumnType.String);
        }

        for (int i = 1; i < lines.Length; i++)
        {
            var values = ParseCsvLine(lines[i], delimiter);
            var row = new Dictionary<string, object?>();
            for (int j = 0; j < headers.Count; j++)
            {
                row[headers[j]] = j < values.Count ? values[j] : null;
            }
            _rows.Add(row);
        }

        return this;
    }

    /// <summary>
    /// Builds and returns the dataset.
    /// </summary>
    /// <returns>The constructed dataset.</returns>
    public Dataset Build()
    {
        var dataset = new Dataset
        {
            Name = _name,
            Metadata = new DatasetMetadata
            {
                Name = _name,
                RowCount = _rows.Count,
                ColumnCount = _schema.Columns.Count,
                Created = DateTimeOffset.UtcNow,
                Modified = DateTimeOffset.UtcNow
            },
            Schema = _schema
        };
        dataset.Rows.AddRange(_rows);
        return dataset;
    }

    private static List<string> ParseCsvLine(string line, char delimiter)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == delimiter)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
        }

        result.Add(current.ToString());
        return result;
    }
}