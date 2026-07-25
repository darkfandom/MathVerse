namespace MathVerse.Math.Interop.GeometryExchange;

using System;
using System.Collections.Generic;

/// <summary>
/// Interface for reading STEP format geometry files.
/// </summary>
public interface ISTEPReader
{
    /// <summary>
    /// Reads a STEP file from the provided stream.
    /// </summary>
    /// <param name="stream">The stream containing STEP data.</param>
    /// <returns>A <see cref="STEPFile"/> containing the parsed shapes.</returns>
    STEPFile Read(System.IO.Stream stream);

    /// <summary>
    /// Reads a STEP file from the provided string content.
    /// </summary>
    /// <param name="content">The STEP file content string.</param>
    /// <returns>A <see cref="STEPFile"/> containing the parsed shapes.</returns>
    STEPFile Read(string content);
}

/// <summary>
/// Interface for writing STEP format geometry files.
/// </summary>
public interface ISTEPWriter
{
    /// <summary>
    /// Writes a STEP file to a string.
    /// </summary>
    /// <param name="stepFile">The STEP file to write.</param>
    /// <returns>A string containing the STEP format data.</returns>
    string Write(STEPFile stepFile);

    /// <summary>
    /// Writes a STEP file to a stream.
    /// </summary>
    /// <param name="stream">The output stream.</param>
    /// <param name="stepFile">The STEP file to write.</param>
    void Write(System.IO.Stream stream, STEPFile stepFile);
}

/// <summary>
/// In-memory representation of a STEP file storing shapes as a dictionary of properties.
/// </summary>
public sealed class STEPFile
{
    /// <summary>
    /// Gets or sets the file description header.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the implementation level.
    /// </summary>
    public string ImplementationLevel { get; set; } = "2;1";

    /// <summary>
    /// Gets or sets the originating system.
    /// </summary>
    public string OriginatingSystem { get; set; } = "MathVerse STEP Writer";

    /// <summary>
    /// Gets the dictionary of shapes stored by entity ID.
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> Shapes { get; } = new();

    /// <summary>
    /// Gets the list of entity ID keys in file order.
    /// </summary>
    public List<string> EntityOrder { get; } = new();

    /// <summary>
    /// Gets or sets the STEP file identifier.
    /// </summary>
    public int FileIdentifier { get; set; } = 1;

    /// <summary>
    /// Adds a shape entity to the STEP file.
    /// </summary>
    /// <param name="entityId">The entity identifier (e.g., '#1').</param>
    /// <param name="entityType">The STEP entity type name.</param>
    /// <param name="properties">The entity properties.</param>
    public void AddShape(string entityId, string entityType, Dictionary<string, string> properties)
    {
        if (entityId is null) throw new ArgumentNullException(nameof(entityId));
        if (entityType is null) throw new ArgumentNullException(nameof(entityType));
        if (properties is null) throw new ArgumentNullException(nameof(properties));

        var data = new Dictionary<string, string>(properties, StringComparer.OrdinalIgnoreCase)
        {
            ["_type"] = entityType
        };
        Shapes[entityId] = data;
        EntityOrder.Add(entityId);
    }
}

/// <summary>
/// Default STEP format reader with in-memory shape storage.
/// </summary>
public sealed class DefaultSTEPReader : ISTEPReader
{
    /// <inheritdoc/>
    public STEPFile Read(System.IO.Stream stream)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        using var reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        return Read(reader.ReadToEnd());
    }

    /// <inheritdoc/>
    public STEPFile Read(string content)
    {
        if (content is null)
            throw new ArgumentNullException(nameof(content));

        var stepFile = new STEPFile();
        var lines = content.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        bool headerSection = true;

        foreach (var rawLine in lines)
        {
            string line = rawLine.TrimEnd(';').Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith('*'))
                continue;

            if (line.Equals("END-ISO-10303-21", StringComparison.OrdinalIgnoreCase))
                break;

            if (line.Equals("ENDSEC", StringComparison.OrdinalIgnoreCase))
            {
                headerSection = false;
                continue;
            }

            if (headerSection && line.StartsWith("FILE_DESCRIPTION", StringComparison.OrdinalIgnoreCase))
            {
                int start = line.IndexOf('(');
                int end = line.LastIndexOf(')');
                if (start >= 0 && end > start)
                    stepFile.Description = line.Substring(start + 1, end - start - 1).Trim('"');
            }
            else if (headerSection && line.StartsWith("FILE_NAME", StringComparison.OrdinalIgnoreCase))
            {
                // skip file name parsing for now
            }
            else if (headerSection && line.StartsWith("FILE_SCHEMA", StringComparison.OrdinalIgnoreCase))
            {
                // skip schema parsing for now
            }
            else if (!headerSection && line.StartsWith("#"))
            {
                int eqIdx = line.IndexOf('=');
                if (eqIdx < 0) continue;

                string entityId = line.Substring(0, eqIdx).Trim();
                string rest = line.Substring(eqIdx + 1).Trim();
                int parenIdx = rest.IndexOf('(');
                string entityType = parenIdx >= 0 ? rest.Substring(0, parenIdx).Trim() : rest.Trim();

                var props = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                props["_type"] = entityType;

                if (parenIdx >= 0)
                {
                    int lastParen = rest.LastIndexOf(')');
                    if (lastParen > parenIdx)
                    {
                        string argsStr = rest.Substring(parenIdx + 1, lastParen - parenIdx - 1);
                        var args = ParseArguments(argsStr);
                        for (int i = 0; i < args.Length; i++)
                            props[$"arg{i}"] = args[i];
                    }
                }

                stepFile.Shapes[entityId] = props;
                stepFile.EntityOrder.Add(entityId);
            }
        }

        return stepFile;
    }

    private static string[] ParseArguments(string argsStr)
    {
        var result = new List<string>();
        int depth = 0;
        var current = new System.Text.StringBuilder();
        bool inString = false;

        foreach (char c in argsStr)
        {
            if (c == '\'' && !inString) { inString = true; current.Append(c); continue; }
            if (c == '\'' && inString) { inString = false; current.Append(c); continue; }
            if (inString) { current.Append(c); continue; }

            if (c == '(') { depth++; current.Append(c); }
            else if (c == ')') { depth--; current.Append(c); }
            else if (c == ',' && depth == 0)
            {
                result.Add(current.ToString().Trim());
                current.Clear();
            }
            else
                current.Append(c);
        }

        if (current.Length > 0)
            result.Add(current.ToString().Trim());

        return result.ToArray();
    }
}

/// <summary>
/// Default STEP format writer from in-memory shape storage.
/// </summary>
public sealed class DefaultSTEPWriter : ISTEPWriter
{
    /// <inheritdoc/>
    public string Write(STEPFile stepFile)
    {
        if (stepFile is null)
            throw new ArgumentNullException(nameof(stepFile));

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("ISO-10303-21;");
        sb.AppendLine("HEADER;");
        sb.AppendLine($"FILE_DESCRIPTION(('{EscapeStep(stepFile.Description)}'),'{stepFile.ImplementationLevel}');");
        sb.AppendLine($"FILE_NAME('model.step','1',('{EscapeStep(stepFile.OriginatingSystem)}'),(''),'','');");
        sb.AppendLine("FILE_SCHEMA(('AUTOMOTIVE_DESIGN'));");
        sb.AppendLine("ENDSEC;");
        sb.AppendLine("DATA;");

        foreach (string entityId in stepFile.EntityOrder)
        {
            if (!stepFile.Shapes.TryGetValue(entityId, out var shape)) continue;
            string type = shape.TryGetValue("_type", out string? t) ? t : "UNKNOWN";
            sb.Append($"{entityId}={type}(");

            var args = new List<string>();
            int argIdx = 0;
            while (shape.TryGetValue($"arg{argIdx}", out string? val))
            {
                args.Add(val);
                argIdx++;
            }
            sb.Append(string.Join(",", args));
            sb.AppendLine(");");
        }

        sb.AppendLine("ENDSEC;");
        sb.AppendLine("END-ISO-10303-21;");
        return sb.ToString();
    }

    /// <inheritdoc/>
    public void Write(System.IO.Stream stream, STEPFile stepFile)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        string content = Write(stepFile);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static string EscapeStep(string s)
    {
        return s?.Replace("'", "''") ?? string.Empty;
    }
}
