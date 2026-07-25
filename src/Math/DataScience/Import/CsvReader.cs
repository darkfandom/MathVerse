namespace MathVerse.Math.DataScience.Import;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using MathVerse.Math.DataScience.Core;
using MathVerse.Math.DataScience.DatasetManagement;

/// <summary>
/// Parses CSV content into a dataset.
/// </summary>
public sealed class CsvReader
{
    /// <summary>
    /// Reads CSV content and returns a dataset.
    /// </summary>
    /// <param name="content">The CSV content string.</param>
    /// <param name="delimiter">The delimiter character.</param>
    /// <returns>A dataset containing the parsed data.</returns>
    public Dataset Read(string content, char delimiter = ',')
    {
        _ = content ?? throw new ArgumentNullException(nameof(content));

        var lines = content.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        var nonEmptyLines = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();

        if (nonEmptyLines.Length == 0)
        {
            return new Dataset { Name = "empty" };
        }

        var headers = ParseCsvLine(nonEmptyLines[0], delimiter);
        var schema = new Schema();
        var rows = new List<Dictionary<string, object?>>();

        foreach (var header in headers)
        {
            schema.AddColumn(header.Trim(), ColumnType.String);
        }

        for (int i = 1; i < nonEmptyLines.Length; i++)
        {
            var values = ParseCsvLine(nonEmptyLines[i], delimiter);
            var row = new Dictionary<string, object?>();

            for (int j = 0; j < headers.Count; j++)
            {
                string cellValue = j < values.Count ? values[j].Trim() : "";
                object? parsedValue = ParseValue(cellValue);
                row[headers[j].Trim()] = parsedValue;
            }

            rows.Add(row);
        }

        DetectColumnTypes(schema, rows);

        var ds = new Dataset
        {
            Name = "imported_csv",
            Metadata = new DatasetMetadata
            {
                Name = "imported_csv",
                RowCount = rows.Count,
                ColumnCount = headers.Count,
                Created = DateTimeOffset.UtcNow,
                Modified = DateTimeOffset.UtcNow
            },
            Schema = schema
        };
        ds.Rows.AddRange(rows);
        return ds;
    }

    /// <summary>
    /// Parses a single CSV line, handling quoted fields and escaped delimiters.
    /// </summary>
    /// <param name="line">The CSV line to parse.</param>
    /// <param name="delimiter">The delimiter character.</param>
    /// <returns>A list of field values.</returns>
    public static List<string> ParseCsvLine(string line, char delimiter)
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

    private static object? ParseValue(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Equals("null", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("NA", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("-", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (bool.TryParse(value, out bool boolVal))
        {
            return boolVal;
        }

        if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out int intVal))
        {
            return intVal;
        }

        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleVal))
        {
            return doubleVal;
        }

        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            return value;
        }

        return value;
    }

    private static void DetectColumnTypes(Schema schema, List<Dictionary<string, object?>> rows)
    {
        foreach (var colDef in schema.Columns.ToList())
        {
            int doubleCount = 0;
            int intCount = 0;
            int boolCount = 0;
            int dateCount = 0;
            int totalNonNull = 0;

            foreach (var row in rows)
            {
                if (row.TryGetValue(colDef.Name, out var val) && val != null)
                {
                    totalNonNull++;
                    if (val is double) doubleCount++;
                    else if (val is int) intCount++;
                    else if (val is bool) boolCount++;
                    else if (val is string str)
                    {
                        if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out _)) doubleCount++;
                        else if (int.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out _)) intCount++;
                        else if (bool.TryParse(str, out _)) boolCount++;
                        else if (DateTimeOffset.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)) dateCount++;
                    }
                }
            }

            if (totalNonNull > 0)
            {
                if (doubleCount == totalNonNull)
                    colDef.Type = ColumnType.Double;
                else if (intCount == totalNonNull)
                    colDef.Type = ColumnType.Int;
                else if (boolCount == totalNonNull)
                    colDef.Type = ColumnType.Bool;
                else if (dateCount == totalNonNull)
                    colDef.Type = ColumnType.DateTime;
            }
        }
    }
}