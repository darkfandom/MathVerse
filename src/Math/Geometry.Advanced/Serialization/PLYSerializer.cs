using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Serialization;

/// <summary>
/// Provides serialization and deserialization of 3D triangle mesh geometry to and from the PLY (Polygon File Format).
/// The PLY format stores geometric data in a header-defined structure with vertex and face element descriptions,
/// followed by ASCII-encoded data records.
/// </summary>
public static class PLYSerializer
{
    /// <summary>
    /// Serializes a set of 3D vertices and triangle indices into the ASCII PLY format.
    /// The output includes a header defining vertex elements (with x, y, z properties) and
    /// face elements (with vertex index list), followed by the corresponding ASCII data.
    /// </summary>
    /// <param name="vertices">The vertex positions to serialize.</param>
    /// <param name="indices">The triangle index buffer, where every three consecutive indices define a face.</param>
    /// <returns>A string containing the complete ASCII PLY-formatted content.</returns>
    public static string Serialize(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices)
    {
        int faceCount = indices.Length / 3;
        var sb = new StringBuilder();

        sb.AppendLine("ply");
        sb.AppendLine("format ascii 1.0");
        sb.AppendFormat("element vertex {0}", vertices.Length);
        sb.AppendLine();
        sb.AppendLine("property float x");
        sb.AppendLine("property float y");
        sb.AppendLine("property float z");
        sb.AppendFormat("element face {0}", faceCount);
        sb.AppendLine();
        sb.AppendLine("property list uchar int vertex_indices");
        sb.AppendLine("end_header");

        for (int i = 0; i < vertices.Length; i++)
        {
            Point3D v = vertices[i];
            sb.AppendFormat(CultureInfo.InvariantCulture, "{0:G} {1:G} {2:G}", v.X, v.Y, v.Z);
            sb.AppendLine();
        }

        for (int i = 0; i < faceCount; i++)
        {
            sb.AppendFormat("3 {0} {1} {2}",
                indices[i * 3], indices[i * 3 + 1], indices[i * 3 + 2]);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Parses ASCII PLY text content and extracts the vertex positions and triangle indices.
    /// The parser reads the header to determine vertex and face counts, then parses the
    /// corresponding data records. Faces with more than three vertices are triangulated via fan decomposition.
    /// </summary>
    /// <param name="content">The ASCII PLY-formatted text content to parse.</param>
    /// <returns>
    /// A tuple containing the parsed vertex positions and the triangle index buffer.
    /// </returns>
    /// <exception cref="FormatException">Thrown when the PLY content contains invalid data or malformed headers.</exception>
    public static (ImmutableArray<Point3D> Vertices, ImmutableArray<int> Indices) Deserialize(string content)
    {
        string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var vertices = ImmutableArray.CreateBuilder<Point3D>();
        var indices = ImmutableArray.CreateBuilder<int>();

        int vertexCount = 0;
        int faceCount = 0;
        int headerEnd = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line == "end_header")
            {
                headerEnd = i + 1;
                break;
            }

            if (line.StartsWith("element vertex", StringComparison.Ordinal))
            {
                string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                    vertexCount = int.Parse(parts[2], CultureInfo.InvariantCulture);
            }
            else if (line.StartsWith("element face", StringComparison.Ordinal))
            {
                string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                    faceCount = int.Parse(parts[2], CultureInfo.InvariantCulture);
            }
        }

        int dataIndex = headerEnd;
        for (int i = 0; i < vertexCount && dataIndex < lines.Length; i++)
        {
            string[] parts = lines[dataIndex].Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            dataIndex++;
            if (parts.Length >= 3)
            {
                double x = double.Parse(parts[0], CultureInfo.InvariantCulture);
                double y = double.Parse(parts[1], CultureInfo.InvariantCulture);
                double z = double.Parse(parts[2], CultureInfo.InvariantCulture);
                vertices.Add(new Point3D(x, y, z));
            }
        }

        for (int i = 0; i < faceCount && dataIndex < lines.Length; i++)
        {
            string[] parts = lines[dataIndex].Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            dataIndex++;
            if (parts.Length >= 4)
            {
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
            }
        }

        return (vertices.ToImmutable(), indices.ToImmutable());
    }
}
