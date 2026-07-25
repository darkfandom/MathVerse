using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Serialization;

/// <summary>
/// Provides serialization and deserialization of 3D geometry to and from the Wavefront OBJ format.
/// Supports vertex positions (v lines) and face definitions (f lines with 1-based indices).
/// </summary>
public static class OBJSerializer
{
    private const string LineEnding = "\n";

    /// <summary>
    /// Serializes a set of 3D vertices and triangle indices into the Wavefront OBJ text format.
    /// Each vertex is written as a "v x y z" line and each triangle face as an "f i1 i2 i3" line
    /// with 1-based indices as per the OBJ specification.
    /// </summary>
    /// <param name="vertices">The vertex positions to serialize.</param>
    /// <param name="indices">The triangle index buffer, where every three consecutive indices define a face.</param>
    /// <returns>A string containing the complete OBJ-formatted content.</returns>
    public static string Serialize(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Wavefront OBJ file");
        sb.AppendLine($"# Vertices: {vertices.Length}");
        sb.AppendLine($"# Faces: {indices.Length / 3}");
        sb.AppendLine();

        for (int i = 0; i < vertices.Length; i++)
        {
            Point3D v = vertices[i];
            sb.AppendFormat(CultureInfo.InvariantCulture, "v {0:G} {1:G} {2:G}", v.X, v.Y, v.Z);
            sb.AppendLine();
        }

        sb.AppendLine();

        int faceCount = indices.Length / 3;
        for (int i = 0; i < faceCount; i++)
        {
            int i0 = indices[i * 3] + 1;
            int i1 = indices[i * 3 + 1] + 1;
            int i2 = indices[i * 3 + 2] + 1;
            sb.AppendFormat(CultureInfo.InvariantCulture, "f {0} {1} {2}", i0, i1, i2);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Parses Wavefront OBJ text content and extracts the vertex positions and face indices.
    /// Lines starting with 'v' define vertices and lines starting with 'f' define faces using 1-based indices.
    /// Non-geometry lines (comments, groups, etc.) are ignored.
    /// </summary>
    /// <param name="content">The OBJ-formatted text content to parse.</param>
    /// <returns>
    /// A tuple containing the parsed vertex positions and the triangle index buffer.
    /// Face indices are converted from 1-based (OBJ convention) to 0-based.
    /// </returns>
    /// <exception cref="FormatException">Thrown when a vertex or face line contains invalid numeric data.</exception>
    public static (ImmutableArray<Point3D> Vertices, ImmutableArray<int> Indices) Deserialize(string content)
    {
        var vertices = ImmutableArray.CreateBuilder<Point3D>();
        var indices = ImmutableArray.CreateBuilder<int>();

        string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0 || line[0] == '#')
                continue;

            string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                continue;

            if (parts[0] == "v" && parts.Length >= 4)
            {
                double x = double.Parse(parts[1], CultureInfo.InvariantCulture);
                double y = double.Parse(parts[2], CultureInfo.InvariantCulture);
                double z = double.Parse(parts[3], CultureInfo.InvariantCulture);
                vertices.Add(new Point3D(x, y, z));
            }
            else if (parts[0] == "f")
            {
                var faceIndices = new List<int>();
                for (int j = 1; j < parts.Length; j++)
                {
                    string token = parts[j];
                    int slashIdx = token.IndexOf('/');
                    string indexStr = slashIdx >= 0 ? token.Substring(0, slashIdx) : token;
                    int idx = int.Parse(indexStr, CultureInfo.InvariantCulture) - 1;
                    faceIndices.Add(idx);
                }

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
