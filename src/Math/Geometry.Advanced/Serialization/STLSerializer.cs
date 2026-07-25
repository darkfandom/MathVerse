using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Text;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Serialization;

/// <summary>
/// Provides serialization and deserialization of 3D triangle mesh geometry to and from the STL (STereoLithography) format.
/// Supports both ASCII and binary STL encoding. The ASCII format stores human-readable facet data
/// while the binary format uses a compact 50-byte-per-triangle layout.
/// </summary>
public static class STLSerializer
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Serializes a set of 3D vertices and triangle indices into the ASCII STL text format.
    /// Each triangle facet includes a normal vector computed from the cross product of its edge vectors,
    /// followed by three vertex coordinates in an outer loop block.
    /// </summary>
    /// <param name="vertices">The vertex positions to serialize.</param>
    /// <param name="indices">The triangle index buffer, where every three consecutive indices define a face.</param>
    /// <returns>A string containing the complete ASCII STL-formatted content.</returns>
    public static string Serialize(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices)
    {
        var sb = new StringBuilder();
        sb.AppendLine("solid MathVerse");

        int faceCount = indices.Length / 3;
        for (int i = 0; i < faceCount; i++)
        {
            int i0 = indices[i * 3];
            int i1 = indices[i * 3 + 1];
            int i2 = indices[i * 3 + 2];

            Point3D p0 = vertices[i0];
            Point3D p1 = vertices[i1];
            Point3D p2 = vertices[i2];

            Vector3D edge1 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D edge2 = new Vector3D(p2.X - p0.X, p2.Y - p0.Y, p2.Z - p0.Z);
            Vector3D normal = edge1.Cross(edge2);
            double len = normal.Length;
            if (len > Tolerance)
            {
                double invLen = 1.0 / len;
                normal = new Vector3D(normal.X * invLen, normal.Y * invLen, normal.Z * invLen);
            }

            sb.AppendFormat(CultureInfo.InvariantCulture, "  facet normal {0:G} {1:G} {2:G}", normal.X, normal.Y, normal.Z);
            sb.AppendLine();
            sb.AppendLine("    outer loop");
            sb.AppendFormat(CultureInfo.InvariantCulture, "      vertex {0:G} {1:G} {2:G}", p0.X, p0.Y, p0.Z);
            sb.AppendLine();
            sb.AppendFormat(CultureInfo.InvariantCulture, "      vertex {0:G} {1:G} {2:G}", p1.X, p1.Y, p1.Z);
            sb.AppendLine();
            sb.AppendFormat(CultureInfo.InvariantCulture, "      vertex {0:G} {1:G} {2:G}", p2.X, p2.Y, p2.Z);
            sb.AppendLine();
            sb.AppendLine("    endloop");
            sb.AppendLine("  endfacet");
        }

        sb.AppendLine("endsolid MathVerse");
        return sb.ToString();
    }

    /// <summary>
    /// Serializes a set of 3D vertices and triangle indices into the binary STL format.
    /// The binary layout consists of an 80-byte header, a 4-byte uint32 triangle count,
    /// followed by 50 bytes per triangle: 12 bytes for the normal vector, 36 bytes for
    /// three vertices (12 bytes each), and a 2-byte attribute byte count.
    /// </summary>
    /// <param name="vertices">The vertex positions to serialize.</param>
    /// <param name="indices">The triangle index buffer, where every three consecutive indices define a face.</param>
    /// <returns>A byte array containing the complete binary STL content.</returns>
    public static byte[] SerializeBinary(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices)
    {
        int faceCount = indices.Length / 3;
        int totalSize = 80 + 4 + (faceCount * 50);
        byte[] buffer = new byte[totalSize];

        Encoding.ASCII.GetBytes("MathVerse Binary STL").CopyTo(buffer, 0);

        BitConverter.GetBytes((uint)faceCount).CopyTo(buffer, 80);

        int offset = 84;
        for (int i = 0; i < faceCount; i++)
        {
            int i0 = indices[i * 3];
            int i1 = indices[i * 3 + 1];
            int i2 = indices[i * 3 + 2];

            Point3D p0 = vertices[i0];
            Point3D p1 = vertices[i1];
            Point3D p2 = vertices[i2];

            Vector3D edge1 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D edge2 = new Vector3D(p2.X - p0.X, p2.Y - p0.Y, p2.Z - p0.Z);
            Vector3D normal = edge1.Cross(edge2);
            double len = normal.Length;
            if (len > Tolerance)
            {
                double invLen = 1.0 / len;
                normal = new Vector3D(normal.X * invLen, normal.Y * invLen, normal.Z * invLen);
            }

            WriteFloat(buffer, offset, (float)normal.X); offset += 4;
            WriteFloat(buffer, offset, (float)normal.Y); offset += 4;
            WriteFloat(buffer, offset, (float)normal.Z); offset += 4;

            WriteFloat(buffer, offset, (float)p0.X); offset += 4;
            WriteFloat(buffer, offset, (float)p0.Y); offset += 4;
            WriteFloat(buffer, offset, (float)p0.Z); offset += 4;

            WriteFloat(buffer, offset, (float)p1.X); offset += 4;
            WriteFloat(buffer, offset, (float)p1.Y); offset += 4;
            WriteFloat(buffer, offset, (float)p1.Z); offset += 4;

            WriteFloat(buffer, offset, (float)p2.X); offset += 4;
            WriteFloat(buffer, offset, (float)p2.Y); offset += 4;
            WriteFloat(buffer, offset, (float)p2.Z); offset += 4;

            BitConverter.GetBytes((ushort)0).CopyTo(buffer, offset); offset += 2;
        }

        return buffer;
    }

    /// <summary>
    /// Parses ASCII STL text content and extracts the vertex positions and triangle indices.
    /// Vertices are deduplicated so that shared vertices across faces reference the same index.
    /// The parser expects the standard solid/endsolid, facet normal, outer loop/endloop, and vertex blocks.
    /// </summary>
    /// <param name="content">The ASCII STL-formatted text content to parse.</param>
    /// <returns>
    /// A tuple containing the unique vertex positions and the triangle index buffer.
    /// The indices are arranged in groups of three, each defining one triangle face.
    /// </returns>
    /// <exception cref="FormatException">Thrown when the STL content contains invalid numeric data or malformed structure.</exception>
    public static (ImmutableArray<Point3D> Vertices, ImmutableArray<int> Indices) Deserialize(string content)
    {
        string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var vertices = ImmutableArray.CreateBuilder<Point3D>();
        var indices = ImmutableArray.CreateBuilder<int>();
        var vertexMap = new Dictionary<(double, double, double), int>();

        int vertexIndex = 0;
        Point3D currentVertex = Point3D.Origin;
        int vertexCount = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0) continue;

            if (line.StartsWith("vertex", StringComparison.Ordinal))
            {
                string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 4)
                {
                    double x = double.Parse(parts[1], CultureInfo.InvariantCulture);
                    double y = double.Parse(parts[2], CultureInfo.InvariantCulture);
                    double z = double.Parse(parts[3], CultureInfo.InvariantCulture);
                    currentVertex = new Point3D(x, y, z);

                    var key = (x, y, z);
                    if (!vertexMap.TryGetValue(key, out int idx))
                    {
                        idx = vertexIndex++;
                        vertexMap[key] = idx;
                        vertices.Add(currentVertex);
                    }
                    indices.Add(idx);
                    vertexCount++;
                }
            }
        }

        return (vertices.ToImmutable(), indices.ToImmutable());
    }

    private static void WriteFloat(byte[] buffer, int offset, float value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        bytes.CopyTo(buffer, offset);
    }
}
