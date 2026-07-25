namespace MathVerse.Math.Interop.DataExchange;

using System;
using System.Collections.Generic;
using System.Text;
using MathVerse.Math.Interop.Core;
using MathVerse.Math.DataScience.Core;
using MathVerse.Math.DataScience.DatasetManagement;

/// <summary>
/// Serializes and deserializes datasets to and from multiple formats.
/// </summary>
public sealed class DatasetSerializer
{
    private const int MagicNumber = 0x4D564453;
    private const int Version = 1;

    /// <summary>
    /// Serializes a dataset to a byte array in the specified format.
    /// </summary>
    /// <param name="ds">The dataset to serialize.</param>
    /// <param name="format">The target format identifier (binary, csv, json).</param>
    /// <returns>An <see cref="InteropResult{T}"/> containing the serialized bytes or an error.</returns>
    public InteropResult<byte[]> SerializeToBytes(Dataset ds, string format)
    {
        if (ds is null)
            return InteropResult<byte[]>.Failure("Dataset cannot be null.");
        if (string.IsNullOrWhiteSpace(format))
            return InteropResult<byte[]>.Failure("Format cannot be null or empty.");

        try
        {
            byte[] result = format.ToLowerInvariant() switch
            {
                "binary" => SerializeBinary(ds),
                "csv" => Encoding.UTF8.GetBytes(SerializeToCsvString(ds)),
                "json" => Encoding.UTF8.GetBytes(SerializeToJsonString(ds)),
                _ => throw new NotSupportedException($"Format '{format}' is not supported.")
            };
            return InteropResult<byte[]>.Success(result);
        }
        catch (Exception ex)
        {
            return InteropResult<byte[]>.Failure($"Serialization to '{format}' failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deserializes a dataset from a byte array in the specified format.
    /// </summary>
    /// <param name="data">The byte array containing serialized data.</param>
    /// <param name="format">The source format identifier (binary, csv, json).</param>
    /// <returns>An <see cref="InteropResult{T}"/> containing the deserialized dataset or an error.</returns>
    public InteropResult<Dataset> DeserializeFromBytes(byte[] data, string format)
    {
        if (data is null || data.Length == 0)
            return InteropResult<Dataset>.Failure("Data cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(format))
            return InteropResult<Dataset>.Failure("Format cannot be null or empty.");

        try
        {
            Dataset result = format.ToLowerInvariant() switch
            {
                "binary" => DeserializeBinary(data),
                "csv" => DeserializeCsvString(Encoding.UTF8.GetString(data)),
                "json" => DeserializeJsonString(Encoding.UTF8.GetString(data)),
                _ => throw new NotSupportedException($"Format '{format}' is not supported.")
            };
            return InteropResult<Dataset>.Success(result);
        }
        catch (Exception ex)
        {
            return InteropResult<Dataset>.Failure($"Deserialization from '{format}' failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Serializes a dataset to a JSON string using manual formatting.
    /// </summary>
    /// <param name="ds">The dataset to serialize.</param>
    /// <returns>An <see cref="InteropResult{T}"/> containing the JSON string or an error.</returns>
    public InteropResult<string> SerializeToJson(Dataset ds)
    {
        if (ds is null)
            return InteropResult<string>.Failure("Dataset cannot be null.");

        try
        {
            return InteropResult<string>.Success(SerializeToJsonString(ds));
        }
        catch (Exception ex)
        {
            return InteropResult<string>.Failure($"JSON serialization failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deserializes a dataset from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An <see cref="InteropResult{T}"/> containing the deserialized dataset or an error.</returns>
    public InteropResult<Dataset> DeserializeFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return InteropResult<Dataset>.Failure("JSON string cannot be null or empty.");

        try
        {
            return InteropResult<Dataset>.Success(DeserializeJsonString(json));
        }
        catch (Exception ex)
        {
            return InteropResult<Dataset>.Failure($"JSON deserialization failed: {ex.Message}", ex);
        }
    }

    private static string SerializeToJsonString(Dataset ds)
    {
        var sb = new StringBuilder();
        sb.Append("{\"name\":");
        sb.Append(EscapeJsonString(ds.Name));
        sb.Append(",\"schema\":[");
        for (int i = 0; i < ds.Schema.Columns.Count; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append("{\"name\":");
            sb.Append(EscapeJsonString(ds.Schema.Columns[i].Name));
            sb.Append(",\"type\":");
            sb.Append(EscapeJsonString(ds.Schema.Columns[i].Type.ToString()));
            sb.Append('}');
        }
        sb.Append("],\"rows\":[");
        for (int i = 0; i < ds.Rows.Count; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append('{');
            bool first = true;
            foreach (var kvp in ds.Rows[i])
            {
                if (!first) sb.Append(',');
                first = false;
                sb.Append(EscapeJsonString(kvp.Key));
                sb.Append(':');
                sb.Append(FormatJsonValue(kvp.Value));
            }
            sb.Append('}');
        }
        sb.Append("]}");
        return sb.ToString();
    }

    private static Dataset DeserializeJsonString(string json)
    {
        var ds = new Dataset();
        int pos = 0;
        SkipWhitespace(json, ref pos);
        Expect(json, ref pos, '{');

        SkipWhitespace(json, ref pos);
        if (MatchKey(json, ref pos, "name"))
        {
            SkipWhitespace(json, ref pos);
            ds.Name = ReadJsonString(json, ref pos);
        }

        SkipComma(json, ref pos);
        SkipWhitespace(json, ref pos);
        if (MatchKey(json, ref pos, "schema"))
        {
            SkipWhitespace(json, ref pos);
            Expect(json, ref pos, '[');
            while (pos < json.Length)
            {
                SkipWhitespace(json, ref pos);
                if (pos >= json.Length || json[pos] == ']') break;
                if (json[pos] == ',') pos++;
                SkipWhitespace(json, ref pos);
                if (pos >= json.Length || json[pos] == '{') break;
                Expect(json, ref pos, '{');
                string colName = string.Empty;
                ColumnType colType = ColumnType.String;
                SkipWhitespace(json, ref pos);
                if (MatchKey(json, ref pos, "name"))
                {
                    SkipWhitespace(json, ref pos);
                    colName = ReadJsonString(json, ref pos);
                }
                SkipComma(json, ref pos);
                SkipWhitespace(json, ref pos);
                if (MatchKey(json, ref pos, "type"))
                {
                    SkipWhitespace(json, ref pos);
                    string typeStr = ReadJsonString(json, ref pos);
                    colType = Enum.TryParse<ColumnType>(typeStr, true, out var ct) ? ct : ColumnType.String;
                }
                SkipWhitespace(json, ref pos);
                if (pos < json.Length && json[pos] == '}') pos++;
                ds.Schema.AddColumn(colName, colType);
            }
            if (pos < json.Length && json[pos] == ']') pos++;
        }

        SkipComma(json, ref pos);
        SkipWhitespace(json, ref pos);
        if (MatchKey(json, ref pos, "rows"))
        {
            SkipWhitespace(json, ref pos);
            Expect(json, ref pos, '[');
            while (pos < json.Length)
            {
                SkipWhitespace(json, ref pos);
                if (pos >= json.Length || json[pos] == ']') break;
                if (json[pos] == ',') pos++;
                SkipWhitespace(json, ref pos);
                if (pos >= json.Length || json[pos] == '{') break;
                Expect(json, ref pos, '{');
                var row = new Dictionary<string, object?>();
                while (pos < json.Length)
                {
                    SkipWhitespace(json, ref pos);
                    if (pos >= json.Length || json[pos] == '}') break;
                    if (json[pos] == ',') pos++;
                    SkipWhitespace(json, ref pos);
                    if (pos >= json.Length || json[pos] != '"') break;
                    string key = ReadJsonString(json, ref pos);
                    SkipWhitespace(json, ref pos);
                    Expect(json, ref pos, ':');
                    SkipWhitespace(json, ref pos);
                    row[key] = ReadJsonValue(json, ref pos);
                }
                if (pos < json.Length && json[pos] == '}') pos++;
                ds.Rows.Add(row);
            }
            if (pos < json.Length && json[pos] == ']') pos++;
        }

        SkipWhitespace(json, ref pos);
        if (pos < json.Length && json[pos] == '}') pos++;
        return ds;
    }

    private static string SerializeToCsvString(Dataset ds)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < ds.Schema.Columns.Count; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append(CsvEscapeField(ds.Schema.Columns[i].Name));
        }
        sb.AppendLine();
        foreach (var row in ds.Rows)
        {
            for (int i = 0; i < ds.Schema.Columns.Count; i++)
            {
                if (i > 0) sb.Append(',');
                if (row.TryGetValue(ds.Schema.Columns[i].Name, out var val) && val is not null)
                    sb.Append(CsvEscapeField(FormatCsvValue(val)));
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private static Dataset DeserializeCsvString(string csv)
    {
        var ds = new Dataset();
        var lines = csv.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        if (lines.Length == 0) return ds;

        var headers = ParseCsvLine(lines[0]);
        foreach (var header in headers)
            ds.Schema.AddColumn(header.Trim(), ColumnType.String);

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var fields = ParseCsvLine(lines[i]);
            var row = new Dictionary<string, object?>();
            for (int j = 0; j < headers.Length && j < fields.Length; j++)
                row[headers[j].Trim()] = fields[j].Trim();
            ds.Rows.Add(row);
        }
        return ds;
    }

    private static byte[] SerializeBinary(Dataset ds)
    {
        using var ms = new System.IO.MemoryStream();
        using var bw = new System.IO.BinaryWriter(ms, Encoding.UTF8, true);

        bw.Write(MagicNumber);
        bw.Write(Version);
        bw.Write(ds.Name ?? string.Empty);

        bw.Write(ds.Schema.Columns.Count);
        foreach (var col in ds.Schema.Columns)
        {
            bw.Write(col.Name);
            bw.Write((int)col.Type);
        }

        bw.Write(ds.Rows.Count);
        foreach (var row in ds.Rows)
        {
            bw.Write(row.Count);
            foreach (var kvp in row)
            {
                bw.Write(kvp.Key);
                WriteBinaryValue(bw, kvp.Value);
            }
        }
        return ms.ToArray();
    }

    private static Dataset DeserializeBinary(byte[] data)
    {
        using var ms = new System.IO.MemoryStream(data);
        using var br = new System.IO.BinaryReader(ms, Encoding.UTF8, true);

        int magic = br.ReadInt32();
        if (magic != MagicNumber)
            throw new InvalidOperationException("Invalid binary format magic number.");

        _ = br.ReadInt32();
        var ds = new Dataset { Name = br.ReadString() };

        int colCount = br.ReadInt32();
        for (int i = 0; i < colCount; i++)
        {
            string colName = br.ReadString();
            var colType = (ColumnType)br.ReadInt32();
            ds.Schema.AddColumn(colName, colType);
        }

        int rowCount = br.ReadInt32();
        for (int i = 0; i < rowCount; i++)
        {
            var row = new Dictionary<string, object?>();
            int fieldCount = br.ReadInt32();
            for (int j = 0; j < fieldCount; j++)
            {
                string key = br.ReadString();
                row[key] = ReadBinaryValue(br);
            }
            ds.Rows.Add(row);
        }
        return ds;
    }

    private static void WriteBinaryValue(System.IO.BinaryWriter bw, object? value)
    {
        switch (value)
        {
            case null:
                bw.Write((byte)0);
                break;
            case double d:
                bw.Write((byte)1);
                bw.Write(d);
                break;
            case int n:
                bw.Write((byte)2);
                bw.Write(n);
                break;
            case bool b:
                bw.Write((byte)3);
                bw.Write(b);
                break;
            case string s:
                bw.Write((byte)4);
                bw.Write(s);
                break;
            default:
                bw.Write((byte)5);
                bw.Write(value.ToString() ?? string.Empty);
                break;
        }
    }

    private static object? ReadBinaryValue(System.IO.BinaryReader br)
    {
        byte tag = br.ReadByte();
        return tag switch
        {
            0 => null,
            1 => br.ReadDouble(),
            2 => br.ReadInt32(),
            3 => br.ReadBoolean(),
            4 => br.ReadString(),
            5 => br.ReadString(),
            _ => null
        };
    }

    private static string FormatJsonValue(object? value)
    {
        return value switch
        {
            null => "null",
            bool b => b ? "true" : "false",
            int n => n.ToString(System.Globalization.CultureInfo.InvariantCulture),
            double d => d.ToString(System.Globalization.CultureInfo.InvariantCulture),
            string s => EscapeJsonString(s),
            _ => EscapeJsonString(value.ToString() ?? string.Empty)
        };
    }

    private static string EscapeJsonString(string s)
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

    private static string FormatCsvValue(object value)
    {
        return value switch
        {
            double d => d.ToString(System.Globalization.CultureInfo.InvariantCulture),
            int n => n.ToString(System.Globalization.CultureInfo.InvariantCulture),
            bool b => b.ToString(),
            _ => value.ToString() ?? string.Empty
        };
    }

    private static string CsvEscapeField(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
            return "\"" + field.Replace("\"", "\"\"") + "\"";
        return field;
    }

    private static string[] ParseCsvLine(string line)
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
                        inQuotes = false;
                }
                else
                    current.Append(c);
            }
            else
            {
                if (c == '"')
                    inQuotes = true;
                else if (c == ',')
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                    current.Append(c);
            }
        }
        result.Add(current.ToString());
        return result.ToArray();
    }

    private static void SkipWhitespace(string s, ref int pos)
    {
        while (pos < s.Length && char.IsWhiteSpace(s[pos])) pos++;
    }

    private static void SkipComma(string s, ref int pos)
    {
        SkipWhitespace(s, ref pos);
        if (pos < s.Length && s[pos] == ',') pos++;
    }

    private static void Expect(string s, ref int pos, char expected)
    {
        SkipWhitespace(s, ref pos);
        if (pos >= s.Length || s[pos] != expected)
            throw new FormatException($"Expected '{expected}' at position {pos}.");
        pos++;
    }

    private static bool MatchKey(string s, ref int pos, string key)
    {
        SkipWhitespace(s, ref pos);
        if (pos >= s.Length || s[pos] != '"') return false;
        int start = pos + 1;
        pos++;
        while (pos < s.Length && s[pos] != '"') pos++;
        if (pos >= s.Length) return false;
        string candidate = s.Substring(start, pos - start);
        pos++;
        if (candidate != key) return false;
        SkipWhitespace(s, ref pos);
        if (pos < s.Length && s[pos] == ':') pos++;
        return true;
    }

    private static string ReadJsonString(string s, ref int pos)
    {
        Expect(s, ref pos, '"');
        var sb = new StringBuilder();
        while (pos < s.Length && s[pos] != '"')
        {
            if (s[pos] == '\\' && pos + 1 < s.Length)
            {
                pos++;
                sb.Append(s[pos] switch
                {
                    'n' => '\n',
                    'r' => '\r',
                    't' => '\t',
                    '\\' => '\\',
                    '"' => '"',
                    _ => s[pos]
                });
            }
            else
                sb.Append(s[pos]);
            pos++;
        }
        if (pos < s.Length) pos++;
        return sb.ToString();
    }

    private static object? ReadJsonValue(string s, ref int pos)
    {
        if (pos >= s.Length) return null;
        char c = s[pos];
        if (c == '"') return ReadJsonString(s, ref pos);
        if (c == 'n') { pos += 4; return null; }
        if (c == 't') { pos += 4; return true; }
        if (c == 'f') { pos += 5; return false; }
        if (c == '{')
        {
            pos++;
            var dict = new Dictionary<string, object?>();
            while (pos < s.Length)
            {
                SkipWhitespace(s, ref pos);
                if (pos >= s.Length || s[pos] == '}') break;
                if (s[pos] == ',') pos++;
                SkipWhitespace(s, ref pos);
                if (pos >= s.Length || s[pos] != '"') break;
                string key = ReadJsonString(s, ref pos);
                SkipWhitespace(s, ref pos);
                Expect(s, ref pos, ':');
                SkipWhitespace(s, ref pos);
                dict[key] = ReadJsonValue(s, ref pos);
            }
            if (pos < s.Length && s[pos] == '}') pos++;
            return dict;
        }
        if (c == '[')
        {
            pos++;
            var list = new List<object?>();
            while (pos < s.Length)
            {
                SkipWhitespace(s, ref pos);
                if (pos >= s.Length || s[pos] == ']') break;
                if (s[pos] == ',') pos++;
                SkipWhitespace(s, ref pos);
                list.Add(ReadJsonValue(s, ref pos));
            }
            if (pos < s.Length && s[pos] == ']') pos++;
            return list;
        }
        int numStart = pos;
        if (c == '-' || c == '+' || char.IsDigit(c))
        {
            pos++;
            while (pos < s.Length && (char.IsDigit(s[pos]) || s[pos] == '.' || s[pos] == 'e' || s[pos] == 'E' || s[pos] == '+' || s[pos] == '-'))
                pos++;
            string numStr = s.Substring(numStart, pos - numStart);
            if (numStr.Contains('.') || numStr.Contains('e') || numStr.Contains('E'))
            {
                if (double.TryParse(numStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double d))
                    return d;
            }
            if (int.TryParse(numStr, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int n))
                return n;
            if (double.TryParse(numStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double dn))
                return dn;
        }
        return null;
    }
}
