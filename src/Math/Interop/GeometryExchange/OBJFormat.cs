namespace MathVerse.Math.Interop.GeometryExchange;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

/// <summary>
/// Represents a vertex in a Wavefront OBJ mesh.
/// </summary>
/// <param name="X">The X coordinate.</param>
/// <param name="Y">The Y coordinate.</param>
/// <param name="Z">The Z coordinate.</param>
public sealed record OBJVertex(double X, double Y, double Z);

/// <summary>
/// Represents a face in a Wavefront OBJ mesh.
/// </summary>
public sealed class OBJFace
{
    /// <summary>
    /// Gets the list of 1-based vertex indices for this face.
    /// </summary>
    public List<int> VertexIndices { get; } = new();
}

/// <summary>
/// Represents a complete Wavefront OBJ mesh.
/// </summary>
public sealed class OBJMesh
{
    /// <summary>
    /// Gets or sets the name of the mesh.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets the list of vertices in the mesh.
    /// </summary>
    public List<OBJVertex> Vertices { get; } = new();

    /// <summary>
    /// Gets the list of faces in the mesh.
    /// </summary>
    public List<OBJFace> Faces { get; } = new();
}

/// <summary>
/// Reads Wavefront OBJ format data.
/// </summary>
public sealed class OBJReader
{
    /// <summary>
    /// Reads an OBJ mesh from a stream.
    /// </summary>
    /// <param name="stream">The stream containing OBJ data.</param>
    /// <returns>The parsed OBJ mesh.</returns>
    public OBJMesh Read(Stream stream)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        return Read(reader.ReadToEnd());
    }

    /// <summary>
    /// Reads an OBJ mesh from a string.
    /// </summary>
    /// <param name="content">The OBJ format string content.</param>
    /// <returns>The parsed OBJ mesh.</returns>
    public OBJMesh Read(string content)
    {
        if (content is null)
            throw new ArgumentNullException(nameof(content));

        var mesh = new OBJMesh();
        var lines = content.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

        foreach (var rawLine in lines)
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith('#'))
                continue;

            string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            switch (parts[0])
            {
                case "o":
                    if (parts.Length > 1)
                        mesh.Name = parts[1];
                    break;

                case "v":
                    if (parts.Length >= 4
                        && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double x)
                        && double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double y)
                        && double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double z))
                    {
                        mesh.Vertices.Add(new OBJVertex(x, y, z));
                    }
                    break;

                case "f":
                    var face = new OBJFace();
                    for (int i = 1; i < parts.Length; i++)
                    {
                        string idxStr = parts[i].Split('/')[0];
                        if (int.TryParse(idxStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int idx))
                            face.VertexIndices.Add(idx);
                    }
                    if (face.VertexIndices.Count >= 3)
                        mesh.Faces.Add(face);
                    break;
            }
        }

        return mesh;
    }
}

/// <summary>
/// Writes Wavefront OBJ format data.
/// </summary>
public sealed class OBJWriter
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>
    /// Writes an OBJ mesh to a string.
    /// </summary>
    /// <param name="mesh">The mesh to write.</param>
    /// <returns>The OBJ format string.</returns>
    public string Write(OBJMesh mesh)
    {
        if (mesh is null)
            throw new ArgumentNullException(nameof(mesh));

        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(mesh.Name))
            sb.AppendLine($"o {mesh.Name}");

        foreach (var v in mesh.Vertices)
            sb.AppendLine($"v {v.X.ToString(Inv)} {v.Y.ToString(Inv)} {v.Z.ToString(Inv)}");

        foreach (var f in mesh.Faces)
        {
            sb.Append("f");
            foreach (int idx in f.VertexIndices)
                sb.Append($" {idx}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Writes an OBJ mesh to a stream.
    /// </summary>
    /// <param name="stream">The output stream.</param>
    /// <param name="mesh">The mesh to write.</param>
    public void Write(Stream stream, OBJMesh mesh)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));
        if (mesh is null)
            throw new ArgumentNullException(nameof(mesh));

        string content = Write(mesh);
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        stream.Write(bytes, 0, bytes.Length);
    }
}
