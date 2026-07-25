namespace MathVerse.Math.DataScience.Export;

using System;
using System.Text;

using MathVerse.Math.DataScience.Core;

/// <summary>
/// Writes a dataset to CSV format.
/// </summary>
public sealed class CsvWriter
{
    /// <summary>
    /// Writes a dataset to a CSV string.
    /// </summary>
    /// <param name="dataset">The dataset to write.</param>
    /// <param name="delimiter">The delimiter character.</param>
    /// <returns>The CSV string representation.</returns>
    public string Write(Dataset dataset, char delimiter = ',')
    {
        _ = dataset ?? throw new ArgumentNullException(nameof(dataset));

        var sb = new StringBuilder();
        var headers = dataset.Schema.ColumnNames.ToArray();

        sb.AppendLine(string.Join(delimiter.ToString(), headers.Select(EscapeCsvField)));

        foreach (var row in dataset.Rows)
        {
            var values = new string[headers.Length];
            for (int i = 0; i < headers.Length; i++)
            {
                if (row.TryGetValue(headers[i], out var val))
                {
                    values[i] = EscapeCsvValue(val);
                }
                else
                {
                    values[i] = "";
                }
            }
            sb.AppendLine(string.Join(delimiter.ToString(), values));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Escapes a CSV field value for safe inclusion in CSV output.
    /// </summary>
    /// <param name="value">The value to escape.</param>
    /// <returns>The escaped string.</returns>
    public static string EscapeCsvField(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }

    private static string EscapeCsvValue(object? value)
    {
        if (value == null) return "";
        return EscapeCsvField(value.ToString() ?? "");
    }
}