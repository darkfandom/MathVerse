namespace MathVerse.Math.DataScience.Export;

using System;
using System.Text;
using System.Text.Json;

using MathVerse.Math.DataScience.Core;

/// <summary>
/// Writes a dataset to JSON format (array of objects).
/// </summary>
public sealed class JsonWriter
{
    /// <summary>
    /// Writes a dataset to a JSON string.
    /// </summary>
    /// <param name="dataset">The dataset to write.</param>
    /// <returns>The JSON string representation.</returns>
    public string Write(Dataset dataset)
    {
        _ = dataset ?? throw new ArgumentNullException(nameof(dataset));

        var sb = new StringBuilder();
        sb.Append('[');

        var headers = dataset.Schema.ColumnNames.ToArray();

        for (int i = 0; i < dataset.Rows.Count; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append('{');

            var row = dataset.Rows[i];
            bool first = true;

            for (int j = 0; j < headers.Length; j++)
            {
                if (!first) sb.Append(',');
                first = false;

                sb.Append('"');
                sb.Append(EscapeJsonString(headers[j]));
                sb.Append("\":");

                if (row.TryGetValue(headers[j], out var val) && val != null)
                {
                    sb.Append(ConvertToJsonValue(val));
                }
                else
                {
                    sb.Append("null");
                }
            }

            sb.Append('}');
        }

        sb.Append(']');
        return sb.ToString();
    }

    private static string ConvertToJsonValue(object value)
    {
        return value switch
        {
            string str => "\"" + EscapeJsonString(str) + "\"",
            bool b => b ? "true" : "false",
            int i => i.ToString(),
            long l => l.ToString(),
            float f => f.ToString("R"),
            double d => d.ToString("R"),
            decimal m => m.ToString("R"),
            _ => "\"" + EscapeJsonString(value.ToString() ?? "") + "\""
        };
    }

    private static string EscapeJsonString(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (char c in value)
        {
            switch (c)
            {
                case '\\': sb.Append("\\\\"); break;
                case '"': sb.Append("\\\""); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                default:
                    if (c < 0x20)
                    {
                        sb.Append("\\u");
                        sb.Append(((int)c).ToString("x4"));
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;
            }
        }
        return sb.ToString();
    }
}