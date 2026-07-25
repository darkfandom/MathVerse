namespace MathVerse.Math.DataScience.Export;

using System;
using System.Text;

using MathVerse.Math.DataScience.Core;

/// <summary>
/// Writes a dataset to an HTML table string.
/// </summary>
public sealed class HtmlWriter
{
    /// <summary>
    /// Writes a dataset to an HTML table string.
    /// </summary>
    /// <param name="dataset">The dataset to write.</param>
    /// <returns>The HTML table string representation.</returns>
    public string Write(Dataset dataset)
    {
        _ = dataset ?? throw new ArgumentNullException(nameof(dataset));

        var sb = new StringBuilder();
        sb.AppendLine("<table border=\"1\" cellpadding=\"4\" cellspacing=\"0\">");

        var headers = dataset.Schema.ColumnNames.ToArray();

        sb.AppendLine("  <thead>");
        sb.Append("    <tr>");
        for (int i = 0; i < headers.Length; i++)
        {
            sb.Append("<th>");
            sb.Append(EscapeHtml(headers[i]));
            sb.Append("</th>");
        }
        sb.AppendLine("</tr>");
        sb.AppendLine("  </thead>");

        sb.AppendLine("  <tbody>");
        foreach (var row in dataset.Rows)
        {
            sb.Append("    <tr>");
            for (int i = 0; i < headers.Length; i++)
            {
                sb.Append("<td>");
                if (row.TryGetValue(headers[i], out var val) && val != null)
                {
                    sb.Append(EscapeHtml(val.ToString() ?? ""));
                }
                sb.Append("</td>");
            }
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("  </tbody>");

        sb.AppendLine("</table>");
        return sb.ToString();
    }

    private static string EscapeHtml(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (char c in value)
        {
            switch (c)
            {
                case '&': sb.Append("&amp;"); break;
                case '<': sb.Append("&lt;"); break;
                case '>': sb.Append("&gt;"); break;
                case '"': sb.Append("&quot;"); break;
                case '\'': sb.Append("&#39;"); break;
                default: sb.Append(c); break;
            }
        }
        return sb.ToString();
    }
}