namespace MathVerse.Math.Interop.GeometryExchange;

using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Represents a node in a glTF scene hierarchy.
/// </summary>
public sealed class GLTFNode
{
    /// <summary>Gets or sets the node name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the translation (x, y, z).</summary>
    public double[] Translation { get; set; } = new double[] { 0.0, 0.0, 0.0 };

    /// <summary>Gets or sets the rotation quaternion (x, y, z, w).</summary>
    public double[] Rotation { get; set; } = new double[] { 0.0, 0.0, 0.0, 1.0 };

    /// <summary>Gets or sets the scale (x, y, z).</summary>
    public double[] Scale { get; set; } = new double[] { 1.0, 1.0, 1.0 };

    /// <summary>Gets or sets the index of the mesh referenced by this node, or -1 if none.</summary>
    public int MeshIndex { get; set; } = -1;

    /// <summary>
    /// Gets the list of child node indices.
    /// </summary>
    public List<int> Children { get; } = new();
}

/// <summary>
/// Represents a mesh in glTF format.
/// </summary>
public sealed class GLTFMesh
{
    /// <summary>Gets or sets the mesh name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets the list of vertex positions (x, y, z triples).</summary>
    public List<double> Vertices { get; } = new();

    /// <summary>Gets the list of vertex indices.</summary>
    public List<int> Indices { get; } = new();

    /// <summary>Gets the list of vertex normals (x, y, z triples).</summary>
    public List<double> Normals { get; } = new();
}

/// <summary>
/// Represents a complete glTF scene.
/// </summary>
public sealed class GLTFScene
{
    /// <summary>Gets or sets the scene name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets the list of nodes in the scene.</summary>
    public List<GLTFNode> Nodes { get; } = new();

    /// <summary>Gets the list of meshes in the scene.</summary>
    public List<GLTFMesh> Meshes { get; } = new();

    /// <summary>Gets or sets the list of root node indices for the scene.</summary>
    public List<int> RootNodeIndices { get; } = new();
}

/// <summary>
/// Interface for reading glTF format geometry files.
/// </summary>
public interface IGLTFReader
{
    /// <summary>
    /// Reads a glTF scene from a stream.
    /// </summary>
    /// <param name="stream">The stream containing glTF JSON data.</param>
    /// <returns>The parsed glTF scene.</returns>
    GLTFScene Read(System.IO.Stream stream);

    /// <summary>
    /// Reads a glTF scene from a JSON string.
    /// </summary>
    /// <param name="json">The glTF JSON content.</param>
    /// <returns>The parsed glTF scene.</returns>
    GLTFScene Read(string json);
}

/// <summary>
/// Interface for writing glTF format geometry files.
/// </summary>
public interface IGLTFWriter
{
    /// <summary>
    /// Writes a glTF scene as a JSON string.
    /// </summary>
    /// <param name="scene">The scene to write.</param>
    /// <returns>The glTF JSON string.</returns>
    string Write(GLTFScene scene);

    /// <summary>
    /// Writes a glTF scene to a stream.
    /// </summary>
    /// <param name="stream">The output stream.</param>
    /// <param name="scene">The scene to write.</param>
    void Write(System.IO.Stream stream, GLTFScene scene);
}

/// <summary>
/// In-memory glTF scene implementation with reader/writer support.
/// </summary>
public sealed class GLTFFile : IGLTFReader, IGLTFWriter
{
    private static readonly System.Globalization.CultureInfo Inv = System.Globalization.CultureInfo.InvariantCulture;

    /// <inheritdoc/>
    public GLTFScene Read(System.IO.Stream stream)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        using var reader = new System.IO.StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        return Read(reader.ReadToEnd());
    }

    /// <inheritdoc/>
    public GLTFScene Read(string json)
    {
        if (json is null)
            throw new ArgumentNullException(nameof(json));

        var scene = new GLTFScene();
        int pos = 0;
        SkipWs(json, ref pos);
        if (pos >= json.Length || json[pos] != '{')
            throw new FormatException("Invalid glTF JSON: expected '{'.");
        pos++;

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
                case "nodes":
                    ParseNodesArray(json, ref pos, scene);
                    break;
                case "meshes":
                    ParseMeshesArray(json, ref pos, scene);
                    break;
                default:
                    SkipValue(json, ref pos);
                    break;
            }
        }

        return scene;
    }

    /// <inheritdoc/>
    public string Write(GLTFScene scene)
    {
        if (scene is null)
            throw new ArgumentNullException(nameof(scene));

        var sb = new StringBuilder();
        sb.Append('{');

        sb.Append("\"meshes\":[");
        for (int i = 0; i < scene.Meshes.Count; i++)
        {
            if (i > 0) sb.Append(',');
            var m = scene.Meshes[i];
            sb.Append('{');
            sb.Append("\"name\":").Append(EscStr(m.Name));
            sb.Append(",\"vertices\":[");
            for (int j = 0; j < m.Vertices.Count; j++)
            {
                if (j > 0) sb.Append(',');
                sb.Append(m.Vertices[j].ToString(Inv));
            }
            sb.Append("],\"indices\":[");
            for (int j = 0; j < m.Indices.Count; j++)
            {
                if (j > 0) sb.Append(',');
                sb.Append(m.Indices[j]);
            }
            sb.Append("],\"normals\":[");
            for (int j = 0; j < m.Normals.Count; j++)
            {
                if (j > 0) sb.Append(',');
                sb.Append(m.Normals[j].ToString(Inv));
            }
            sb.Append("]}");
        }
        sb.Append("],");

        sb.Append("\"nodes\":[");
        for (int i = 0; i < scene.Nodes.Count; i++)
        {
            if (i > 0) sb.Append(',');
            var n = scene.Nodes[i];
            sb.Append('{');
            sb.Append("\"name\":").Append(EscStr(n.Name));
            sb.Append(",\"translation\":").Append(WriteArray(n.Translation));
            sb.Append(",\"rotation\":").Append(WriteArray(n.Rotation));
            sb.Append(",\"scale\":").Append(WriteArray(n.Scale));
            if (n.MeshIndex >= 0)
                sb.Append(",\"mesh\":").Append(n.MeshIndex);
            sb.Append(",\"children\":[");
            for (int j = 0; j < n.Children.Count; j++)
            {
                if (j > 0) sb.Append(',');
                sb.Append(n.Children[j]);
            }
            sb.Append("]}");
        }
        sb.Append("]}");

        return sb.ToString();
    }

    /// <inheritdoc/>
    public void Write(System.IO.Stream stream, GLTFScene scene)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        string json = Write(scene);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static void ParseNodesArray(string json, ref int pos, GLTFScene scene)
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

            var node = new GLTFNode();
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
                    case "name": node.Name = ReadStr(json, ref pos); break;
                    case "translation": node.Translation = ReadDoubleArray(json, ref pos); break;
                    case "rotation": node.Rotation = ReadDoubleArray(json, ref pos); break;
                    case "scale": node.Scale = ReadDoubleArray(json, ref pos); break;
                    case "mesh": node.MeshIndex = ReadInt(json, ref pos); break;
                    case "children": node.Children.AddRange(ReadIntArray(json, ref pos)); break;
                    default: SkipValue(json, ref pos); break;
                }
            }
            if (pos < json.Length && json[pos] == '}') pos++;
            scene.Nodes.Add(node);
        }
        if (pos < json.Length && json[pos] == ']') pos++;
    }

    private static void ParseMeshesArray(string json, ref int pos, GLTFScene scene)
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

            var mesh = new GLTFMesh();
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
                    case "name": mesh.Name = ReadStr(json, ref pos); break;
                    case "vertices": mesh.Vertices.AddRange(ReadDoubleArray(json, ref pos)); break;
                    case "indices": mesh.Indices.AddRange(ReadIntArray(json, ref pos)); break;
                    case "normals": mesh.Normals.AddRange(ReadDoubleArray(json, ref pos)); break;
                    default: SkipValue(json, ref pos); break;
                }
            }
            if (pos < json.Length && json[pos] == '}') pos++;
            scene.Meshes.Add(mesh);
        }
        if (pos < json.Length && json[pos] == ']') pos++;
    }

    private static double[] ReadDoubleArray(string json, ref int pos)
    {
        if (pos >= json.Length || json[pos] != '[') return Array.Empty<double>();
        pos++;
        var list = new List<double>();
        while (pos < json.Length)
        {
            SkipWs(json, ref pos);
            if (pos >= json.Length || json[pos] == ']') break;
            if (json[pos] == ',') pos++;
            SkipWs(json, ref pos);
            int start = pos;
            while (pos < json.Length && json[pos] != ',' && json[pos] != ']' && !char.IsWhiteSpace(json[pos])) pos++;
            string num = json.Substring(start, pos - start);
            if (double.TryParse(num, System.Globalization.NumberStyles.Float, Inv, out double d))
                list.Add(d);
        }
        if (pos < json.Length && json[pos] == ']') pos++;
        return list.ToArray();
    }

    private static int[] ReadIntArray(string json, ref int pos)
    {
        if (pos >= json.Length || json[pos] != '[') return Array.Empty<int>();
        pos++;
        var list = new List<int>();
        while (pos < json.Length)
        {
            SkipWs(json, ref pos);
            if (pos >= json.Length || json[pos] == ']') break;
            if (json[pos] == ',') pos++;
            SkipWs(json, ref pos);
            int start = pos;
            while (pos < json.Length && json[pos] != ',' && json[pos] != ']' && !char.IsWhiteSpace(json[pos])) pos++;
            string num = json.Substring(start, pos - start);
            if (int.TryParse(num, System.Globalization.NumberStyles.Integer, Inv, out int n))
                list.Add(n);
        }
        if (pos < json.Length && json[pos] == ']') pos++;
        return list.ToArray();
    }

    private static int ReadInt(string json, ref int pos)
    {
        SkipWs(json, ref pos);
        int start = pos;
        while (pos < json.Length && char.IsDigit(json[pos])) pos++;
        if (int.TryParse(json.Substring(start, pos - start), out int n))
            return n;
        return -1;
    }

    private static string WriteArray(double[] arr)
    {
        var sb = new StringBuilder("[");
        for (int i = 0; i < arr.Length; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append(arr[i].ToString(Inv));
        }
        sb.Append("]");
        return sb.ToString();
    }

    private static string EscStr(string s)
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

    private static string ReadStr(string json, ref int pos)
    {
        if (pos >= json.Length || json[pos] != '"') return string.Empty;
        pos++;
        var sb = new StringBuilder();
        while (pos < json.Length && json[pos] != '"')
        {
            if (json[pos] == '\\' && pos + 1 < json.Length)
            {
                pos++;
                sb.Append(json[pos] switch { 'n' => '\n', 'r' => '\r', 't' => '\t', '\\' => '\\', '"' => '"', _ => json[pos] });
            }
            else
                sb.Append(json[pos]);
            pos++;
        }
        if (pos < json.Length) pos++;
        return sb.ToString();
    }

    private static void SkipWs(string s, ref int pos)
    {
        while (pos < s.Length && char.IsWhiteSpace(s[pos])) pos++;
    }

    private static void SkipValue(string s, ref int pos)
    {
        SkipWs(s, ref pos);
        if (pos >= s.Length) return;
        char c = s[pos];
        if (c == '"') { pos++; while (pos < s.Length && s[pos] != '"') { if (s[pos] == '\\') pos++; pos++; } if (pos < s.Length) pos++; return; }
        if (c == '{' || c == '[')
        {
            char close = c == '{' ? '}' : ']';
            int depth = 1; pos++;
            while (pos < s.Length && depth > 0) { if (s[pos] == c) depth++; else if (s[pos] == close) depth--; pos++; }
            return;
        }
        while (pos < s.Length && s[pos] != ',' && s[pos] != '}' && s[pos] != ']') pos++;
    }
}
