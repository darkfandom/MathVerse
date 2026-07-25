namespace MathVerse.Math.Interop.VisualizationExchange;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

/// <summary>
/// Represents a single animation keyframe with a time value and property values.
/// </summary>
public sealed class AnimationKeyframe
{
    /// <summary>Gets or sets the keyframe time in seconds.</summary>
    public double Time { get; set; }

    /// <summary>Gets the dictionary of property names to their values at this keyframe.</summary>
    public Dictionary<string, double> Properties { get; } = new();
}

/// <summary>
/// Represents a complete animation sequence.
/// </summary>
public sealed class AnimationSequence
{
    /// <summary>Gets or sets the animation name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the total duration in seconds.</summary>
    public double Duration { get; set; }

    /// <summary>
    /// Gets the ordered list of animation keyframes.
    /// </summary>
    public List<AnimationKeyframe> Keyframes { get; } = new();
}

/// <summary>
/// Serializes and deserializes animation data in JSON format.
/// </summary>
public sealed class AnimationSerializer
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>
    /// Serializes an animation sequence to a JSON string.
    /// </summary>
    /// <param name="animation">The animation sequence to serialize.</param>
    /// <returns>A JSON string representing the animation.</returns>
    public string Serialize(AnimationSequence animation)
    {
        if (animation is null)
            throw new ArgumentNullException(nameof(animation));

        var sb = new StringBuilder();
        sb.Append("{\"name\":");
        sb.Append(Esc(animation.Name));
        sb.Append(",\"duration\":");
        sb.Append(animation.Duration.ToString(Inv));
        sb.Append(",\"keyframes\":[");

        for (int i = 0; i < animation.Keyframes.Count; i++)
        {
            if (i > 0) sb.Append(',');
            var kf = animation.Keyframes[i];
            sb.Append("{\"time\":");
            sb.Append(kf.Time.ToString(Inv));
            sb.Append(",\"properties\":{");

            bool first = true;
            foreach (var kvp in kf.Properties)
            {
                if (!first) sb.Append(',');
                first = false;
                sb.Append(Esc(kvp.Key));
                sb.Append(':');
                sb.Append(kvp.Value.ToString(Inv));
            }

            sb.Append("}}");
        }

        sb.Append("]}");
        return sb.ToString();
    }

    /// <summary>
    /// Deserializes an animation sequence from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <returns>The deserialized animation sequence.</returns>
    public AnimationSequence Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON cannot be null or empty.", nameof(json));

        var anim = new AnimationSequence();
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
                case "name":
                    anim.Name = ReadStr(json, ref pos);
                    break;
                case "duration":
                    anim.Duration = ReadDbl(json, ref pos);
                    break;
                case "keyframes":
                    ParseKeyframes(json, ref pos, anim);
                    break;
                default:
                    SkipVal(json, ref pos);
                    break;
            }
        }

        return anim;
    }

    private static void ParseKeyframes(string json, ref int pos, AnimationSequence anim)
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

            var kf = new AnimationKeyframe();
            while (pos < json.Length)
            {
                SkipWs(json, ref pos);
                if (pos >= json.Length || json[pos] == '}') break;
                if (json[pos] == ',') pos++;
                SkipWs(json, ref pos);
                if (pos >= json.Length || json[pos] != '"') break;

                string k = ReadStr(json, ref pos);
                SkipWs(json, ref pos);
                Expect(json, ref pos, ':');
                SkipWs(json, ref pos);

                if (k == "time")
                {
                    kf.Time = ReadDbl(json, ref pos);
                }
                else if (k == "properties")
                {
                    if (pos < json.Length && json[pos] == '{')
                    {
                        pos++;
                        while (pos < json.Length)
                        {
                            SkipWs(json, ref pos);
                            if (pos >= json.Length || json[pos] == '}') break;
                            if (json[pos] == ',') pos++;
                            SkipWs(json, ref pos);
                            if (pos >= json.Length || json[pos] != '"') break;
                            string pk = ReadStr(json, ref pos);
                            SkipWs(json, ref pos);
                            Expect(json, ref pos, ':');
                            SkipWs(json, ref pos);
                            double pv = ReadDbl(json, ref pos);
                            kf.Properties[pk] = pv;
                        }
                        if (pos < json.Length && json[pos] == '}') pos++;
                    }
                }
                else
                {
                    SkipVal(json, ref pos);
                }
            }
            if (pos < json.Length && json[pos] == '}') pos++;
            anim.Keyframes.Add(kf);
        }
        if (pos < json.Length && json[pos] == ']') pos++;
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
