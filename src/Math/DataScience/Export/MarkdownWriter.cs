namespace MathVerse.Math.DataScience.Export;

using System;
using System.Linq;
using System.Text;

using MathVerse.Math.DataScience.Core;

/// <summary>
/// Writes a dataset to Markdown table format.
/// </summary>
public sealed class MarkdownWriter
{
    /// <summary>
    /// Writes a dataset to a Markdown table string.
    /// </summary>
    /// <param name="dataset">The dataset to write.</param>
    /// <returns>The Markdown table string representation.</returns>
    public string Write(Dataset dataset)
    {
        _ = dataset ?? throw new ArgumentNullException(nameof(dataset));

        var sb = new StringBuilder();
        var headers = dataset.Schema.ColumnNames.ToArray();

        int[] widths = new int[headers.Length];
        for (int i = 0; i < headers.Length; i++)
        {
            widths[i] = headers[i].Length;
        }

        foreach (var row in dataset.Rows)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                if (row.TryGetValue(headers[i], out var val) && val != null)
                {
                    int len = (val.ToString() ?? "").Length;
                    if (len > widths[i]) widths[i] = len;
                }
            }
        }

        for (int i = 0; i < headers.Length; i++)
        {
            if (i > 0) sb.Append(" | ");
            sb.Append(headers[i].PadRight(widths[i]));
        }
        sb.AppendLine();

        for (int i = 0; i < headers.Length; i++)
        {
            if (i > 0) sb.Append(" | ");
            sb.Append(new string('-', widths[i]));
        }
        sb.AppendLine();

        foreach (var row in dataset.Rows)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                if (i > 0) sb.Append(" | ");
                string cellValue = "";
                if (row.TryGetValue(headers[i], out var val) && val != null)
                {
                    cellValue = val.ToString() ?? "";
                }
                sb.Append(cellValue.PadRight(widths[i]));
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}