namespace MathVerse.Math.DataScience.Import;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

using MathVerse.Math.DataScience.Core;
using MathVerse.Math.DataScience.DatasetManagement;

/// <summary>
/// Reads JSON content (array of objects) into a dataset.
/// </summary>
public sealed class JsonReader
{
    /// <summary>
    /// Reads JSON content and returns a dataset.
    /// </summary>
    /// <param name="json">The JSON content string (array of objects).</param>
    /// <returns>A dataset containing the parsed data.</returns>
    public Dataset Read(string json)
    {
        _ = json ?? throw new ArgumentNullException(nameof(json));

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Array)
        {
            throw new FormatException("JSON root must be an array of objects.");
        }

        var columnNames = new HashSet<string>();
        var allRows = new List<Dictionary<string, object?>>();

        foreach (var element in root.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var row = new Dictionary<string, object?>();
            foreach (var prop in element.EnumerateObject())
            {
                columnNames.Add(prop.Name);
                row[prop.Name] = ConvertJsonValue(prop.Value);
            }
            allRows.Add(row);
        }

        var schema = new Schema();
        foreach (var colName in columnNames)
        {
            var detectedType = DetectColumnType(allRows, colName);
            schema.AddColumn(colName, detectedType);
        }

        var ds = new Dataset
        {
            Name = "imported_json",
            Metadata = new DatasetMetadata
            {
                Name = "imported_json",
                RowCount = allRows.Count,
                ColumnCount = columnNames.Count,
                Created = DateTimeOffset.UtcNow,
                Modified = DateTimeOffset.UtcNow
            },
            Schema = schema
        };
        ds.Rows.AddRange(allRows);
        return ds;
    }

    private static object? ConvertJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out long longVal) ? (object)longVal : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Undefined => null,
            _ => element.ToString()
        };
    }

    private static ColumnType DetectColumnType(List<Dictionary<string, object?>> rows, string columnName)
    {
        int doubleCount = 0;
        int intCount = 0;
        int boolCount = 0;
        int totalNonNull = 0;

        foreach (var row in rows)
        {
            if (row.TryGetValue(columnName, out var val) && val != null)
            {
                totalNonNull++;
                if (val is double) doubleCount++;
                else if (val is long) intCount++;
                else if (val is bool) boolCount++;
                else if (val is string str)
                {
                    if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out _)) doubleCount++;
                    else if (int.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out _)) intCount++;
                    else if (bool.TryParse(str, out _)) boolCount++;
                }
            }
        }

        if (totalNonNull == 0) return ColumnType.String;
        if (doubleCount == totalNonNull) return ColumnType.Double;
        if (intCount == totalNonNull) return ColumnType.Int;
        if (boolCount == totalNonNull) return ColumnType.Bool;
        return ColumnType.String;
    }
}