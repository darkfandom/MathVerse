namespace MathVerse.Math.Interop.VisualizationExchange;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

/// <summary>
/// Serializes and deserializes scene graphs in JSON and binary formats.
/// </summary>
public sealed class SceneSerializer
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>
    /// Serializes a scene to a JSON string.
    /// </summary>
    /// <param name="scene">The scene to serialize.</param>
    /// <returns>A JSON string representing the scene.</returns>
    public string SerializeJSON(Scene scene)
    {
        if (scene is null)
            throw new ArgumentNullException(nameof(scene));

        var sb = new StringBuilder();
        sb.Append("{\"width\":");
        sb.Append(scene.Width.ToString(Inv));
        sb.Append(",\"height\":");
        sb.Append(scene.Height.ToString(Inv));
        sb.Append(",\"background\":");
        sb.Append(Esc(scene.BackgroundColor));
        sb.Append(",\"elements\":[");

        for (int i = 0; i < scene.Elements.Count; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append(SerializeElement(scene.Elements[i]));
        }

        sb.Append("]}");
        return sb.ToString();
    }

    /// <summary>
    /// Deserializes a scene from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <returns>The deserialized scene.</returns>
    public Scene DeserializeJSON(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON cannot be null or empty.", nameof(json));

        var scene = new Scene();
        int pos = 0;
        SkipWs(json, ref pos);
        Expect(json, ref pos, '{');

        while (pos < json.Length)
        {
            SkipWs(json, ref pos);
            if (pos >= json.Length || json[pos] == '}') break;
            if (json[pos] == ',') pos++;
            SkipWs(json, ref pos);
            if (pos >= json.Length || json[pos] != '"') break;

            string key = ReadStr(json, ref pos);
            SkipWs(json, ref pos);
            Expect(json, ref pos, ':');
            SkipWs(json, ref pos);

            switch (key)
            {
                case "width":
                    scene.Width = ReadDbl(json, ref pos);
                    break;
                case "height":
                    scene.Height = ReadDbl(json, ref pos);
                    break;
                case "background":
                    scene.BackgroundColor = ReadStr(json, ref pos);
                    break;
                case "elements":
                    ParseElements(json, ref pos, scene);
                    break;
                default:
                    SkipVal(json, ref pos);
                    break;
            }
        }

        return scene;
    }

    /// <summary>
    /// Serializes a scene to a binary byte array.
    /// </summary>
    /// <param name="scene">The scene to serialize.</param>
    /// <returns>A byte array containing the binary scene data.</returns>
    public byte[] SerializeBinary(Scene scene)
    {
        if (scene is null)
            throw new ArgumentNullException(nameof(scene));

        using var ms = new System.IO.MemoryStream();
        using var bw = new System.IO.BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);

        bw.Write(scene.Width);
        bw.Write(scene.Height);
        bw.Write(scene.BackgroundColor ?? string.Empty);
        bw.Write(scene.Elements.Count);

        foreach (var elem in scene.Elements)
            WriteElement(bw, elem);

        return ms.ToArray();
    }

    /// <summary>
    /// Deserializes a scene from a binary byte array.
    /// </summary>
    /// <param name="data">The binary data.</param>
    /// <returns>The deserialized scene.</returns>
    public Scene DeserializeBinary(byte[] data)
    {
        if (data is null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty.", nameof(data));

        using var ms = new System.IO.MemoryStream(data);
        using var br = new System.IO.BinaryReader(ms, Encoding.UTF8, leaveOpen: true);

        var scene = new Scene
        {
            Width = br.ReadDouble(),
            Height = br.ReadDouble(),
            BackgroundColor = br.ReadString()
        };

        int count = br.ReadInt32();
        for (int i = 0; i < count; i++)
            scene.Elements.Add(ReadElement(br));

        return scene;
    }

    private static string SerializeElement(SceneElement elem)
    {
        var sb = new StringBuilder();
        string type = elem switch
        {
            CircleElement => "circle",
            LineElement => "line",
            TextElement => "text",
            PathElement => "path",
            _ => "unknown"
        };

        sb.Append("{\"type\":").Append(Esc(type));
        sb.Append(",\"fill\":").Append(Esc(elem.FillColor));
        sb.Append(",\"stroke\":").Append(Esc(elem.StrokeColor));
        sb.Append(",\"strokeWidth\":").Append(elem.StrokeWidth.ToString(Inv));
        sb.Append(",\"opacity\":").Append(elem.Opacity.ToString(Inv));

        switch (elem)
        {
            case CircleElement c:
                sb.Append(",\"cx\":").Append(c.CX.ToString(Inv));
                sb.Append(",\"cy\":").Append(c.CY.ToString(Inv));
                sb.Append(",\"r\":").Append(c.Radius.ToString(Inv));
                break;
            case LineElement l:
                sb.Append(",\"x1\":").Append(l.X1.ToString(Inv));
                sb.Append(",\"y1\":").Append(l.Y1.ToString(Inv));
                sb.Append(",\"x2\":").Append(l.X2.ToString(Inv));
                sb.Append(",\"y2\":").Append(l.Y2.ToString(Inv));
                break;
            case TextElement t:
                sb.Append(",\"x\":").Append(t.X.ToString(Inv));
                sb.Append(",\"y\":").Append(t.Y.ToString(Inv));
                sb.Append(",\"text\":").Append(Esc(t.Text));
                sb.Append(",\"fontSize\":").Append(t.FontSize.ToString(Inv));
                break;
            case PathElement p:
                sb.Append(",\"data\":").Append(Esc(p.PathData));
                break;
        }

        sb.Append('}');
        return sb.ToString();
    }

    private static void ParseElements(string json, ref int pos, Scene scene)
    {
        if (pos >= json.Length || json[pos] != '[') return;
        pos++;

        while (pos < json.Length)
        {
            SkipWs(json, ref pos);
            if (pos >= json.Length || json[pos] == ']') break;
            if (json[pos] == ',') pos++;
            SkipWs(json, ref pos);
            if (pos >= json.Length || json[pos] != '{') break;
            pos++;

            string type = string.Empty;
            string fill = "#000000", stroke = "#000000";
            double sw = 1, op = 1;
            double cx = 0, cy = 0, r = 0, x1 = 0, y1 = 0, x2 = 0, y2 = 0;
            double tx = 0, ty = 0, fs = 14;
            string text = string.Empty, pathData = string.Empty;

            while (pos < json.Length)
            {
                SkipWs(json, ref pos);
                if (pos >= json.Length || json[pos] == '}') break;
                if (json[pos] == ',') pos++;
                SkipWs(json, ref pos);
                if (pos >= json.Length || json[pos] != '"') break;
                string key = ReadStr(json, ref pos);
                SkipWs(json, ref pos);
                Expect(json, ref pos, ':');
                SkipWs(json, ref pos);

                switch (key)
                {
                    case "type": type = ReadStr(json, ref pos); break;
                    case "fill": fill = ReadStr(json, ref pos); break;
                    case "stroke": stroke = ReadStr(json, ref pos); break;
                    case "strokeWidth": sw = ReadDbl(json, ref pos); break;
                    case "opacity": op = ReadDbl(json, ref pos); break;
                    case "cx": cx = ReadDbl(json, ref pos); break;
                    case "cy": cy = ReadDbl(json, ref pos); break;
                    case "r": r = ReadDbl(json, ref pos); break;
                    case "x1": x1 = ReadDbl(json, ref pos); break;
                    case "y1": y1 = ReadDbl(json, ref pos); break;
                    case "x2": x2 = ReadDbl(json, ref pos); break;
                    case "y2": y2 = ReadDbl(json, ref pos); break;
                    case "x": tx = ReadDbl(json, ref pos); break;
                    case "y": ty = ReadDbl(json, ref pos); break;
                    case "text": text = ReadStr(json, ref pos); break;
                    case "fontSize": fs = ReadDbl(json, ref pos); break;
                    case "data": pathData = ReadStr(json, ref pos); break;
                    default: SkipVal(json, ref pos); break;
                }
            }
            if (pos < json.Length && json[pos] == '}') pos++;

            SceneElement? elem = type switch
            {
                "circle" => new CircleElement { CX = cx, CY = cy, Radius = r, FillColor = fill, StrokeColor = stroke, StrokeWidth = sw, Opacity = op },
                "line" => new LineElement { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, StrokeColor = stroke, StrokeWidth = sw, Opacity = op },
                "text" => new TextElement { X = tx, Y = ty, Text = text, FontSize = fs, FillColor = fill, Opacity = op },
                "path" => new PathElement { PathData = pathData, FillColor = fill, StrokeColor = stroke, StrokeWidth = sw, Opacity = op },
                _ => null
            };
            if (elem is not null)
                scene.Elements.Add(elem);
        }
        if (pos < json.Length && json[pos] == ']') pos++;
    }

    private static void WriteElement(System.IO.BinaryWriter bw, SceneElement elem)
    {
        byte typeTag = elem switch
        {
            CircleElement => 1,
            LineElement => 2,
            TextElement => 3,
            PathElement => 4,
            _ => 0
        };

        bw.Write(typeTag);
        bw.Write(elem.FillColor ?? string.Empty);
        bw.Write(elem.StrokeColor ?? string.Empty);
        bw.Write(elem.StrokeWidth);
        bw.Write(elem.Opacity);

        switch (elem)
        {
            case CircleElement c:
                bw.Write(c.CX);
                bw.Write(c.CY);
                bw.Write(c.Radius);
                break;
            case LineElement l:
                bw.Write(l.X1);
                bw.Write(l.Y1);
                bw.Write(l.X2);
                bw.Write(l.Y2);
                break;
            case TextElement t:
                bw.Write(t.X);
                bw.Write(t.Y);
                bw.Write(t.Text ?? string.Empty);
                bw.Write(t.FontSize);
                bw.Write(t.FontFamily ?? string.Empty);
                bw.Write(t.TextAnchor ?? string.Empty);
                break;
            case PathElement p:
                bw.Write(p.PathData ?? string.Empty);
                break;
        }
    }

    private static SceneElement ReadElement(System.IO.BinaryReader br)
    {
        byte typeTag = br.ReadByte();
        string fill = br.ReadString();
        string stroke = br.ReadString();
        double sw = br.ReadDouble();
        double op = br.ReadDouble();

        return typeTag switch
        {
            1 => new CircleElement
            {
                FillColor = fill, StrokeColor = stroke, StrokeWidth = sw, Opacity = op,
                CX = br.ReadDouble(), CY = br.ReadDouble(), Radius = br.ReadDouble()
            },
            2 => new LineElement
            {
                StrokeColor = stroke, StrokeWidth = sw, Opacity = op,
                X1 = br.ReadDouble(), Y1 = br.ReadDouble(), X2 = br.ReadDouble(), Y2 = br.ReadDouble()
            },
            3 => new TextElement
            {
                FillColor = fill, Opacity = op,
                X = br.ReadDouble(), Y = br.ReadDouble(), Text = br.ReadString(),
                FontSize = br.ReadDouble(), FontFamily = br.ReadString(), TextAnchor = br.ReadString()
            },
            4 => new PathElement
            {
                FillColor = fill, StrokeColor = stroke, StrokeWidth = sw, Opacity = op,
                PathData = br.ReadString()
            },
            _ => throw new InvalidOperationException($"Unknown element type tag: {typeTag}")
        };
    }

    private static string Esc(string s)
    {
        var sb = new StringBuilder("\"");
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

    private static string ReadStr(string s, ref int pos)
    {
        if (pos >= s.Length || s[pos] != '"') return string.Empty;
        pos++;
        var sb = new StringBuilder();
        while (pos < s.Length && s[pos] != '"')
        {
            if (s[pos] == '\\' && pos + 1 < s.Length) { pos++; sb.Append(s[pos] switch { 'n' => '\n', 'r' => '\r', 't' => '\t', '\\' => '\\', '"' => '"', _ => s[pos] }); }
            else sb.Append(s[pos]);
            pos++;
        }
        if (pos < s.Length) pos++;
        return sb.ToString();
    }

    private static double ReadDbl(string s, ref int pos)
    {
        SkipWs(s, ref pos);
        int start = pos;
        while (pos < s.Length && (char.IsDigit(s[pos]) || s[pos] == '.' || s[pos] == '-' || s[pos] == '+' || s[pos] == 'e' || s[pos] == 'E'))
            pos++;
        if (double.TryParse(s.Substring(start, pos - start), NumberStyles.Float, Inv, out double val))
            return val;
        return 0;
    }

    private static void SkipWs(string s, ref int pos) { while (pos < s.Length && char.IsWhiteSpace(s[pos])) pos++; }
    private static void Expect(string s, ref int pos, char c) { SkipWs(s, ref pos); if (pos < s.Length && s[pos] == c) pos++; }

    private static void SkipVal(string s, ref int pos)
    {
        SkipWs(s, ref pos);
        if (pos >= s.Length) return;
        char c = s[pos];
        if (c == '"') { pos++; while (pos < s.Length && s[pos] != '"') { if (s[pos] == '\\') pos++; pos++; } if (pos < s.Length) pos++; return; }
        if (c is '{' or '[')
        {
            char close = c == '{' ? '}' : ']';
            int depth = 1; pos++;
            while (pos < s.Length && depth > 0) { if (s[pos] == c) depth++; else if (s[pos] == close) depth--; pos++; }
            return;
        }
        while (pos < s.Length && s[pos] != ',' && s[pos] != '}' && s[pos] != ']') pos++;
    }
}
