namespace MathVerse.Math.Interop.DataExchange;

using System;
using System.Collections.Generic;
using System.Text;
using MathVerse.Math.DataScience.DatasetManagement;

/// <summary>
/// Converts schemas between different representations.
/// </summary>
public sealed class SchemaConverter
{
    /// <summary>
    /// Converts a schema to a type-name map dictionary.
    /// </summary>
    /// <param name="schema">The schema to convert.</param>
    /// <returns>A dictionary mapping column names to type strings.</returns>
    public Dictionary<string, string> ToTypeMap(Schema schema)
    {
        if (schema is null)
            throw new ArgumentNullException(nameof(schema));

        var map = new Dictionary<string, string>(schema.Columns.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var col in schema.Columns)
            map[col.Name] = col.Type.ToString();
        return map;
    }

    /// <summary>
    /// Creates a schema from a type-name map dictionary.
    /// </summary>
    /// <param name="typeMap">A dictionary mapping column names to type strings.</param>
    /// <returns>A new schema instance.</returns>
    public Schema FromTypeMap(Dictionary<string, string> typeMap)
    {
        if (typeMap is null)
            throw new ArgumentNullException(nameof(typeMap));

        var schema = new Schema();
        foreach (var kvp in typeMap)
        {
            var colType = Enum.TryParse<ColumnType>(kvp.Value, true, out var ct)
                ? ct
                : ColumnType.String;
            schema.AddColumn(kvp.Key, colType);
        }
        return schema;
    }

    /// <summary>
    /// Serializes a schema to a JSON string using manual formatting.
    /// </summary>
    /// <param name="schema">The schema to serialize.</param>
    /// <returns>A JSON string representing the schema.</returns>
    public string ToJson(Schema schema)
    {
        if (schema is null)
            throw new ArgumentNullException(nameof(schema));

        var sb = new StringBuilder();
        sb.Append("[");
        for (int i = 0; i < schema.Columns.Count; i++)
        {
            if (i > 0) sb.Append(",");
            sb.Append("{\"name\":");
            sb.Append(EscapeJson(schema.Columns[i].Name));
            sb.Append(",\"type\":");
            sb.Append(EscapeJson(schema.Columns[i].Type.ToString()));
            sb.Append(",\"nullable\":");
            sb.Append(schema.Columns[i].IsNullable ? "true" : "false");
            sb.Append(",\"description\":");
            sb.Append(EscapeJson(schema.Columns[i].Description ?? string.Empty));
            sb.Append("}");
        }
        sb.Append("]");
        return sb.ToString();
    }

    /// <summary>
    /// Deserializes a schema from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A new schema instance.</returns>
    public Schema FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON string cannot be null or empty.", nameof(json));

        var schema = new Schema();
        int pos = 0;
        SkipWs(json, ref pos);
        if (pos < json.Length && json[pos] == '[') pos++;

        while (pos < json.Length)
        {
            SkipWs(json, ref pos);
            if (pos >= json.Length || json[pos] == ']') break;
            if (json[pos] == ',') pos++;
            SkipWs(json, ref pos);
            if (pos >= json.Length || json[pos] == '{') break;
            pos++;

            string name = string.Empty;
            ColumnType type = ColumnType.String;
            bool nullable = true;
            string description = string.Empty;

            while (pos < json.Length)
            {
                SkipWs(json, ref pos);
                if (pos >= json.Length || json[pos] == '}') break;
                if (json[pos] == ',') pos++;
                SkipWs(json, ref pos);
                if (pos >= json.Length || json[pos] != '"') break;

                string key = ReadStr(json, ref pos);
                SkipWs(json, ref pos);
                if (pos < json.Length && json[pos] == ':') pos++;
                SkipWs(json, ref pos);

                switch (key)
                {
                    case "name":
                        name = ReadStr(json, ref pos);
                        break;
                    case "type":
                        string t = ReadStr(json, ref pos);
                        type = Enum.TryParse<ColumnType>(t, true, out var ct) ? ct : ColumnType.String;
                        break;
                    case "nullable":
                        nullable = ReadBool(json, ref pos);
                        break;
                    case "description":
                        description = ReadStr(json, ref pos);
                        break;
                    default:
                        SkipValue(json, ref pos);
                        break;
                }
            }
            if (pos < json.Length && json[pos] == '}') pos++;

            var col = new ColumnDefinition(name, type)
            {
                IsNullable = nullable,
                Description = description
            };
            schema.Columns.Add(col);
        }
        if (pos < json.Length && json[pos] == ']') pos++;
        return schema;
    }

    private static string EscapeJson(string s)
    {
        var sb = new StringBuilder(s.Length + 2);
        sb.Append('"');
        foreach (char c in s)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default: sb.Append(c); break;
            }
        }
        sb.Append('"');
        return sb.ToString();
    }

    private static void SkipWs(string s, ref int pos)
    {
        while (pos < s.Length && char.IsWhiteSpace(s[pos])) pos++;
    }

    private static string ReadStr(string s, ref int pos)
    {
        if (pos >= s.Length || s[pos] != '"') return string.Empty;
        pos++;
        var sb = new StringBuilder();
        while (pos < s.Length && s[pos] != '"')
        {
            if (s[pos] == '\\' && pos + 1 < s.Length)
            {
                pos++;
                sb.Append(s[pos] switch { 'n' => '\n', 'r' => '\r', 't' => '\t', '\\' => '\\', '"' => '"', _ => s[pos] });
            }
            else
                sb.Append(s[pos]);
            pos++;
        }
        if (pos < s.Length) pos++;
        return sb.ToString();
    }

    private static bool ReadBool(string s, ref int pos)
    {
        SkipWs(s, ref pos);
        if (pos + 3 < s.Length && s[pos] == 't' && s[pos + 1] == 'r' && s[pos + 2] == 'u' && s[pos + 3] == 'e')
        { pos += 4; return true; }
        if (pos + 4 < s.Length && s[pos] == 'f' && s[pos + 1] == 'a' && s[pos + 2] == 'l' && s[pos + 3] == 's' && s[pos + 4] == 'e')
        { pos += 5; return false; }
        return false;
    }

    private static void SkipValue(string s, ref int pos)
    {
        SkipWs(s, ref pos);
        if (pos >= s.Length) return;
        char c = s[pos];
        if (c == '"') { pos++; while (pos < s.Length && s[pos] != '"') { if (s[pos] == '\\') pos++; pos++; } if (pos < s.Length) pos++; return; }
        if (c == '{') { int depth = 1; pos++; while (pos < s.Length && depth > 0) { if (s[pos] == '{') depth++; else if (s[pos] == '}') depth--; pos++; } return; }
        if (c == '[') { int depth = 1; pos++; while (pos < s.Length && depth > 0) { if (s[pos] == '[') depth++; else if (s[pos] == ']') depth--; pos++; } return; }
        while (pos < s.Length && s[pos] != ',' && s[pos] != '}' && s[pos] != ']') pos++;
    }
}
