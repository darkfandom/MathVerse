namespace MathVerse.Math.DataScience.Import;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

using MathVerse.Math.DataScience.Core;
using MathVerse.Math.DataScience.DatasetManagement;

/// <summary>
/// Reads XML content into a dataset.
/// </summary>
public sealed class XmlReader
{
    /// <summary>
    /// Reads XML content and returns a dataset.
    /// </summary>
    /// <param name="xml">The XML content string.</param>
    /// <param name="rowElement">The name of the element representing each row.</param>
    /// <returns>A dataset containing the parsed data.</returns>
    public Dataset Read(string xml, string rowElement = "row")
    {
        _ = xml ?? throw new ArgumentNullException(nameof(xml));

        var doc = new XmlDocument();
        doc.LoadXml(xml);

        var rows = new List<Dictionary<string, object?>>();
        var columnNames = new HashSet<string>();

        var rowNodes = doc.SelectNodes($"//{rowElement}") ?? doc.DocumentElement?.ChildNodes;
        if (rowNodes == null || rowNodes.Count == 0)
        {
            return new Dataset { Name = "empty_xml" };
        }

        foreach (XmlNode rowNode in rowNodes)
        {
            if (rowNode.Name != rowElement) continue;

            var row = new Dictionary<string, object?>();
            foreach (XmlNode child in rowNode.ChildNodes)
            {
                columnNames.Add(child.Name);
                row[child.Name] = ParseXmlValue(child.InnerText);
            }
            rows.Add(row);
        }

        var schema = new Schema();
        foreach (var colName in columnNames)
        {
            var detectedType = DetectColumnType(rows, colName);
            schema.AddColumn(colName, detectedType);
        }

        var ds = new Dataset
        {
            Name = "imported_xml",
            Metadata = new DatasetMetadata
            {
                Name = "imported_xml",
                RowCount = rows.Count,
                ColumnCount = columnNames.Count,
                Created = DateTimeOffset.UtcNow,
                Modified = DateTimeOffset.UtcNow
            },
            Schema = schema
        };
        ds.Rows.AddRange(rows);
        return ds;
    }

    private static object? ParseXmlValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();

        if (trimmed.Equals("null", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("NA", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("N/A", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (bool.TryParse(trimmed, out bool boolVal))
        {
            return boolVal;
        }

        if (int.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out int intVal))
        {
            return intVal;
        }

        if (double.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleVal))
        {
            return doubleVal;
        }

        return trimmed;
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
                else if (val is int) intCount++;
                else if (val is bool) boolCount++;
            }
        }

        if (totalNonNull == 0) return ColumnType.String;
        if (doubleCount == totalNonNull) return ColumnType.Double;
        if (intCount == totalNonNull) return ColumnType.Int;
        if (boolCount == totalNonNull) return ColumnType.Bool;
        return ColumnType.String;
    }
}