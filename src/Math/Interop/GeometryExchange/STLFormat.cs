namespace MathVerse.Math.Interop.GeometryExchange;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

/// <summary>
/// Represents a 3D vertex in STL format.
/// </summary>
public sealed class STLVertex
{
    /// <summary>Gets or sets the X coordinate.</summary>
    public double X { get; set; }

    /// <summary>Gets or sets the Y coordinate.</summary>
    public double Y { get; set; }

    /// <summary>Gets or sets the Z coordinate.</summary>
    public double Z { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="STLVertex"/> class.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="z">The Z coordinate.</param>
    public STLVertex(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}

/// <summary>
/// Represents a triangular face in STL format with a normal and three vertices.
/// </summary>
public sealed class STLTriangle
{
    /// <summary>Gets or sets the surface normal.</summary>
    public STLVertex Normal { get; set; }

    /// <summary>Gets or sets the first vertex.</summary>
    public STLVertex V1 { get; set; }

    /// <summary>Gets or sets the second vertex.</summary>
    public STLVertex V2 { get; set; }

    /// <summary>Gets or sets the third vertex.</summary>
    public STLVertex V3 { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="STLTriangle"/> class.
    /// </summary>
    /// <param name="normal">The surface normal.</param>
    /// <param name="v1">The first vertex.</param>
    /// <param name="v2">The second vertex.</param>
    /// <param name="v3">The third vertex.</param>
    public STLTriangle(STLVertex normal, STLVertex v1, STLVertex v2, STLVertex v3)
    {
        Normal = normal;
        V1 = v1;
        V2 = v2;
        V3 = v3;
    }
}

/// <summary>
/// Represents a complete STL mesh.
/// </summary>
public sealed class STLMesh
{
    /// <summary>Gets or sets the mesh name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets the list of triangular faces.</summary>
    public List<STLTriangle> Triangles { get; } = new();
}

/// <summary>
/// Reads STL format (binary and ASCII) data.
/// </summary>
public sealed class STLReader
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>
    /// Reads an STL mesh from a stream, auto-detecting binary vs ASCII.
    /// </summary>
    /// <param name="stream">The stream containing STL data.</param>
    /// <returns>The parsed STL mesh.</returns>
    public STLMesh Read(Stream stream)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        if (!stream.CanSeek)
        {
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Position = 0;
            return ReadFromStream(ms);
        }

        long startPos = stream.Position;
        byte[] header = new byte[80];
        if (stream.Read(header, 0, 80) < 80)
            throw new InvalidDataException("Stream too short for STL header.");

        stream.Position = startPos;
        return ReadFromStream(stream);
    }

    private static STLMesh ReadFromStream(Stream stream)
    {
        byte[] header = new byte[80];
        if (stream.Read(header, 0, 80) < 80)
            throw new InvalidDataException("Invalid STL data.");

        string headerStr = Encoding.ASCII.GetString(header).TrimEnd('\0');
        bool isBinary = !headerStr.StartsWith("solid", StringComparison.OrdinalIgnoreCase);

        if (!isBinary)
        {
            stream.Position = 0;
            return ReadAscii(stream);
        }

        return ReadBinary(stream);
    }

    private static STLMesh ReadBinary(Stream stream)
    {
        var mesh = new STLMesh { Name = "STLBinary" };
        byte[] header = new byte[80];
        stream.Read(header, 0, 80);

        byte[] countBytes = new byte[4];
        stream.Read(countBytes, 0, 4);
        int triangleCount = BitConverter.ToInt32(countBytes, 0);

        using var br = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        for (int i = 0; i < triangleCount; i++)
        {
            var normal = new STLVertex(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            var v1 = new STLVertex(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            var v2 = new STLVertex(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            var v3 = new STLVertex(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            _ = br.ReadUInt16();
            mesh.Triangles.Add(new STLTriangle(normal, v1, v2, v3));
        }

        return mesh;
    }

    private static STLMesh ReadAscii(Stream stream)
    {
        var mesh = new STLMesh { Name = "STLAscii" };
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

        STLVertex? normal = null;
        STLVertex? v1 = null;
        STLVertex? v2 = null;
        int vertexIndex = 0;

        while (!reader.EndOfStream)
        {
            string? line = reader.ReadLine();
            if (line is null) break;
            line = line.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            if (line.StartsWith("solid", StringComparison.OrdinalIgnoreCase))
            {
                mesh.Name = line.Length > 6 ? line.Substring(6).Trim() : "STLAscii";
            }
            else if (line.StartsWith("endsolid", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }
            else if (line.StartsWith("facet normal", StringComparison.OrdinalIgnoreCase))
            {
                var nums = ParseDoubles(line.Substring(12).Trim());
                if (nums.Length >= 3)
                    normal = new STLVertex(nums[0], nums[1], nums[2]);
            }
            else if (line.StartsWith("vertex", StringComparison.OrdinalIgnoreCase))
            {
                var nums = ParseDoubles(line.Substring(6).Trim());
                if (nums.Length >= 3)
                {
                    var vertex = new STLVertex(nums[0], nums[1], nums[2]);
                    switch (vertexIndex % 3)
                    {
                        case 0: v1 = vertex; break;
                        case 1: v2 = vertex; break;
                        case 2:
                            if (normal is not null && v1 is not null && v2 is not null)
                                mesh.Triangles.Add(new STLTriangle(normal, v1, v2, vertex));
                            break;
                    }
                    vertexIndex++;
                }
            }
            else if (line.StartsWith("endloop", StringComparison.OrdinalIgnoreCase))
            {
            }
        }

        return mesh;
    }

    private static double[] ParseDoubles(string s)
    {
        var parts = s.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var result = new List<double>();
        foreach (var p in parts)
        {
            if (double.TryParse(p, NumberStyles.Float, Inv, out double d))
                result.Add(d);
        }
        return result.ToArray();
    }
}

/// <summary>
/// Writes STL format (binary and ASCII) data.
/// </summary>
public sealed class STLWriter
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>
    /// Writes an STL mesh as binary format.
    /// </summary>
    /// <param name="mesh">The mesh to write.</param>
    /// <returns>A byte array containing binary STL data.</returns>
    public byte[] WriteBinary(STLMesh mesh)
    {
        if (mesh is null)
            throw new ArgumentNullException(nameof(mesh));

        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);

        byte[] header = new byte[80];
        Encoding.ASCII.GetBytes("STL Binary - MathVerse").CopyTo(header, 0);
        bw.Write(header);
        bw.Write(mesh.Triangles.Count);

        foreach (var tri in mesh.Triangles)
        {
            bw.Write((float)tri.Normal.X);
            bw.Write((float)tri.Normal.Y);
            bw.Write((float)tri.Normal.Z);
            bw.Write((float)tri.V1.X);
            bw.Write((float)tri.V1.Y);
            bw.Write((float)tri.V1.Z);
            bw.Write((float)tri.V2.X);
            bw.Write((float)tri.V2.Y);
            bw.Write((float)tri.V2.Z);
            bw.Write((float)tri.V3.X);
            bw.Write((float)tri.V3.Y);
            bw.Write((float)tri.V3.Z);
            bw.Write((ushort)0);
        }

        return ms.ToArray();
    }

    /// <summary>
    /// Writes an STL mesh as ASCII format.
    /// </summary>
    /// <param name="mesh">The mesh to write.</param>
    /// <returns>A string containing ASCII STL data.</returns>
    public string WriteASCII(STLMesh mesh)
    {
        if (mesh is null)
            throw new ArgumentNullException(nameof(mesh));

        var sb = new StringBuilder();
        string name = string.IsNullOrEmpty(mesh.Name) ? "STLMesh" : mesh.Name;
        sb.AppendLine($"solid {name}");

        foreach (var tri in mesh.Triangles)
        {
            sb.AppendLine($"  facet normal {tri.Normal.X.ToString(Inv)} {tri.Normal.Y.ToString(Inv)} {tri.Normal.Z.ToString(Inv)}");
            sb.AppendLine("    outer loop");
            sb.AppendLine($"      vertex {tri.V1.X.ToString(Inv)} {tri.V1.Y.ToString(Inv)} {tri.V1.Z.ToString(Inv)}");
            sb.AppendLine($"      vertex {tri.V2.X.ToString(Inv)} {tri.V2.Y.ToString(Inv)} {tri.V2.Z.ToString(Inv)}");
            sb.AppendLine($"      vertex {tri.V3.X.ToString(Inv)} {tri.V3.Y.ToString(Inv)} {tri.V3.Z.ToString(Inv)}");
            sb.AppendLine("    endloop");
            sb.AppendLine("  endfacet");
        }

        sb.AppendLine($"endsolid {name}");
        return sb.ToString();
    }
}
