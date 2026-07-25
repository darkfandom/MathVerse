using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Serialization;

/// <summary>
/// Provides serialization and deserialization of 3D geometry to and from the Object File Format (OFF).
/// The OFF format is a simple text-based representation storing vertex coordinates followed by
/// face definitions with vertex index lists.
/// </summary>
public static class OFFSerializer
{
    /// <summary>
    /// Serializes a set of 3D vertices and triangle indices into the OFF text format.
    /// The output begins with an "OFF" header, followed by a line specifying vertex count, edge count,
    /// and face count, then vertex coordinate lines, and finally face index lines.
    /// Edge count is computed as the number of unique edges in the triangle mesh.
    /// </summary>
    /// <param name="vertices">The vertex positions to serialize.</param>
    /// <param name="indices">The triangle index buffer, where every three consecutive indices define a face.</param>
    /// <returns>A string containing the complete OFF-formatted content.</returns>
    public static string Serialize(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices)
    {
        var sb = new StringBuilder();
        int faceCount = indices.Length / 3;
        int edgeCount = ComputeEdgeCount(indices);

        sb.AppendLine("OFF");
        sb.AppendFormat(CultureInfo.InvariantCulture, "{0} {1} {2}", vertices.Length, edgeCount, faceCount);
        sb.AppendLine();

        for (int i = 0; i < vertices.Length; i++)
        {
            Point3D v = vertices[i];
            sb.AppendFormat(CultureInfo.InvariantCulture, "{0:G} {1:G} {2:G}", v.X, v.Y, v.Z);
            sb.AppendLine();
        }

        for (int i = 0; i < faceCount; i++)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, "3 {0} {1} {2}",
                indices[i * 3], indices[i * 3 + 1], indices[i * 3 + 2]);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Parses OFF text content and extracts the vertex positions and triangle indices.
    /// The parser expects the "OFF" header, a line with vertex/edge/face counts,
    /// followed by vertex coordinate lines and face definition lines.
    /// Faces with more than three vertices are triangulated using a fan decomposition.
    /// </summary>
    /// <param name="content">The OFF-formatted text content to parse.</param>
    /// <returns>
    /// A tuple containing the parsed vertex positions and the triangle index buffer.
    /// Non-triangle faces are decomposed into triangle fans.
    /// </returns>
    /// <exception cref="FormatException">Thrown when the OFF content contains invalid numeric data or is missing required headers.</exception>
    public static (ImmutableArray<Point3D> Vertices, ImmutableArray<int> Indices) Deserialize(string content)
    {
        string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var vertices = ImmutableArray.CreateBuilder<Point3D>();
        var indices = ImmutableArray.CreateBuilder<int>();

        int lineIndex = 0;

        while (lineIndex < lines.Length && lines[lineIndex].Trim().StartsWith("#", StringComparison.Ordinal))
            lineIndex++;

        if (lineIndex < lines.Length && lines[lineIndex].Trim() == "OFF")
            lineIndex++;

        while (lineIndex < lines.Length && lines[lineIndex].Trim().StartsWith("#", StringComparison.Ordinal))
            lineIndex++;

        if (lineIndex >= lines.Length)
            return (ImmutableArray<Point3D>.Empty, ImmutableArray<int>.Empty);

        string[] counts = lines[lineIndex].Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        lineIndex++;

        int vertexCount = int.Parse(counts[0], CultureInfo.InvariantCulture);
        int faceCount = counts.Length >= 3 ? int.Parse(counts[2], CultureInfo.InvariantCulture) : 0;

        for (int i = 0; i < vertexCount && lineIndex < lines.Length; i++)
        {
            string[] parts = lines[lineIndex].Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            lineIndex++;
            if (parts.Length >= 3)
            {
                double x = double.Parse(parts[0], CultureInfo.InvariantCulture);
                double y = double.Parse(parts[1], CultureInfo.InvariantCulture);
                double z = double.Parse(parts[2], CultureInfo.InvariantCulture);
                vertices.Add(new Point3D(x, y, z));
            }
        }

        int facesRead = 0;
        while (facesRead < faceCount && lineIndex < lines.Length)
        {
            string line = lines[lineIndex].Trim();
            lineIndex++;
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                continue;

            string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
                continue;

            int polyCount = int.Parse(parts[0], CultureInfo.InvariantCulture);
            var faceIndices = new List<int>();
            for (int j = 1; j <= polyCount && j < parts.Length; j++)
                faceIndices.Add(int.Parse(parts[j], CultureInfo.InvariantCulture));

            for (int j = 1; j < faceIndices.Count - 1; j++)
            {
                indices.Add(faceIndices[0]);
                indices.Add(faceIndices[j]);
                indices.Add(faceIndices[j + 1]);
            }

            facesRead++;
        }

        return (vertices.ToImmutable(), indices.ToImmutable());
    }

    private static int ComputeEdgeCount(ImmutableArray<int> indices)
    {
        var edges = new HashSet<(int, int)>();
        int faceCount = indices.Length / 3;
        for (int i = 0; i < faceCount; i++)
        {
            int a = indices[i * 3];
            int b = indices[i * 3 + 1];
            int c = indices[i * 3 + 2];
            edges.Add(MinMax(a, b));
            edges.Add(MinMax(b, c));
            edges.Add(MinMax(c, a));
        }
        return edges.Count;
    }

    private static (int, int) MinMax(int a, int b)
    {
        return a < b ? (a, b) : (b, a);
    }
}
