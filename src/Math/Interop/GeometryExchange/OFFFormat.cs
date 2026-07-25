namespace MathVerse.Math.Interop.GeometryExchange;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

/// <summary>
/// Represents a vertex in OFF format.
/// </summary>
public sealed class OFFVertex
{
    /// <summary>Gets or sets the X coordinate.</summary>
    public double X { get; set; }

    /// <summary>Gets or sets the Y coordinate.</summary>
    public double Y { get; set; }

    /// <summary>Gets or sets the Z coordinate.</summary>
    public double Z { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OFFVertex"/> class.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="z">The Z coordinate.</param>
    public OFFVertex(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}

/// <summary>
/// Represents a face in OFF format.
/// </summary>
public sealed class OFFFace
{
    /// <summary>
    /// Gets the list of vertex indices for this face.
    /// </summary>
    public List<int> VertexIndices { get; } = new();
}

/// <summary>
/// Represents a complete OFF mesh.
/// </summary>
public sealed class OFFMesh
{
    /// <summary>Gets or sets the mesh name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets the list of vertices.</summary>
    public List<OFFVertex> Vertices { get; } = new();

    /// <summary>Gets the list of faces.</summary>
    public List<OFFFace> Faces { get; } = new();
}

/// <summary>
/// Reads Object File Format (OFF) data.
/// </summary>
public sealed class OFFReader
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>
    /// Reads an OFF mesh from a stream.
    /// </summary>
    /// <param name="stream">The stream containing OFF data.</param>
    /// <returns>The parsed OFF mesh.</returns>
    public OFFMesh Read(Stream stream)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        var mesh = new OFFMesh();

        string? header = reader.ReadLine();
        while (header != null && (header.Trim().StartsWith("#") || string.IsNullOrWhiteSpace(header)))
            header = reader.ReadLine();

        if (header is null || !header.Trim().StartsWith("OFF", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Invalid OFF header.");

        string? countsLine = reader.ReadLine();
        while (countsLine != null && (countsLine.Trim().StartsWith("#") || string.IsNullOrWhiteSpace(countsLine)))
            countsLine = reader.ReadLine();

        if (countsLine is null)
            throw new InvalidDataException("Missing OFF vertex/face counts.");

        var countParts = countsLine.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        int vertexCount = countParts.Length >= 1 && int.TryParse(countParts[0], out int vc) ? vc : 0;
        int faceCount = countParts.Length >= 2 && int.TryParse(countParts[1], out int fc) ? fc : 0;
        _ = countParts.Length >= 3 && int.TryParse(countParts[2], out int _ec) ? _ec : 0;

        for (int i = 0; i < vertexCount; i++)
        {
            string? line = reader.ReadLine();
            while (line != null && line.Trim().StartsWith("#"))
                line = reader.ReadLine();
            if (line is null) break;

            var parts = line.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3
                && double.TryParse(parts[0], NumberStyles.Float, Inv, out double x)
                && double.TryParse(parts[1], NumberStyles.Float, Inv, out double y)
                && double.TryParse(parts[2], NumberStyles.Float, Inv, out double z))
            {
                mesh.Vertices.Add(new OFFVertex(x, y, z));
            }
        }

        for (int i = 0; i < faceCount; i++)
        {
            string? line = reader.ReadLine();
            while (line != null && line.Trim().StartsWith("#"))
                line = reader.ReadLine();
            if (line is null) break;

            var parts = line.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 1 && int.TryParse(parts[0], out int n))
            {
                var face = new OFFFace();
                for (int j = 1; j <= n && j < parts.Length; j++)
                {
                    if (int.TryParse(parts[j], out int idx))
                        face.VertexIndices.Add(idx);
                }
                if (face.VertexIndices.Count >= 3)
                    mesh.Faces.Add(face);
            }
        }

        return mesh;
    }
}

/// <summary>
/// Writes Object File Format (OFF) data.
/// </summary>
public sealed class OFFWriter
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>
    /// Writes an OFF mesh to a string.
    /// </summary>
    /// <param name="mesh">The mesh to write.</param>
    /// <returns>The OFF format string.</returns>
    public string Write(OFFMesh mesh)
    {
        if (mesh is null)
            throw new ArgumentNullException(nameof(mesh));

        var sb = new StringBuilder();
        sb.AppendLine("OFF");
        sb.AppendLine($"{mesh.Vertices.Count} {mesh.Faces.Count} 0");

        foreach (var v in mesh.Vertices)
            sb.AppendLine($"{v.X.ToString(Inv)} {v.Y.ToString(Inv)} {v.Z.ToString(Inv)}");

        foreach (var f in mesh.Faces)
        {
            sb.Append(f.VertexIndices.Count);
            foreach (int idx in f.VertexIndices)
                sb.Append($" {idx}");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
