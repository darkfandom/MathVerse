namespace MathVerse.Math.DataScience.Export;

using System;
using System.Text;

using MathVerse.Math.DataScience.Core;

/// <summary>
/// Writes a dataset to XML format.
/// </summary>
public sealed class XmlWriter
{
    /// <summary>
    /// Writes a dataset to an XML string.
    /// </summary>
    /// <param name="dataset">The dataset to write.</param>
    /// <param name="rootElement">The root element name.</param>
    /// <returns>The XML string representation.</returns>
    public string Write(Dataset dataset, string rootElement = "data")
    {
        _ = dataset ?? throw new ArgumentNullException(nameof(dataset));

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.Append('<');
        sb.Append(rootElement);
        sb.Append('>');

        var headers = dataset.Schema.ColumnNames.ToArray();

        foreach (var row in dataset.Rows)
        {
            sb.AppendLine();
            sb.Append("  <row>");

            for (int i = 0; i < headers.Length; i++)
            {
                sb.AppendLine();
                sb.Append("    <");
                sb.Append(EscapeXmlName(headers[i]));
                sb.Append('>');

                if (row.TryGetValue(headers[i], out var val) && val != null)
                {
                    sb.Append(EscapeXmlContent(val.ToString() ?? ""));
                }

                sb.Append("</");
                sb.Append(EscapeXmlName(headers[i]));
                sb.Append('>');
            }

            sb.AppendLine();
            sb.Append("  </row>");
        }

        sb.AppendLine();
        sb.Append("</");
        sb.Append(rootElement);
        sb.Append('>');

        return sb.ToString();
    }

    private static string EscapeXmlContent(string value)
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
                case '\'': sb.Append("&apos;"); break;
                default: sb.Append(c); break;
            }
        }
        return sb.ToString();
    }

    private static string EscapeXmlName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "_";

        var sb = new StringBuilder(name.Length);
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            if (i == 0 && (char.IsDigit(c) || c == '-' || c == '.'))
            {
                sb.Append('_');
            }
            else if (char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.')
            {
                sb.Append(c);
            }
            else
            {
                sb.Append('_');
            }
        }
        return sb.ToString();
    }
}