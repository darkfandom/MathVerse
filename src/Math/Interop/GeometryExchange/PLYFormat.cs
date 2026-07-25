namespace MathVerse.Math.Interop.GeometryExchange;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

/// <summary>
/// Represents a vertex in PLY format.
/// </summary>
public sealed class PLYVertex
{
    /// <summary>Gets or sets the X coordinate.</summary>
    public double X { get; set; }

    /// <summary>Gets or sets the Y coordinate.</summary>
    public double Y { get; set; }

    /// <summary>Gets or sets the Z coordinate.</summary>
    public double Z { get; set; }

    /// <summary>Gets or sets the X component of the normal.</summary>
    public double Nx { get; set; }

    /// <summary>Gets or sets the Y component of the normal.</summary>
    public double Ny { get; set; }

    /// <summary>Gets or sets the Z component of the normal.</summary>
    public double Nz { get; set; }
}

/// <summary>
/// Represents a face in PLY format.
/// </summary>
public sealed class PLYFace
{
    /// <summary>
    /// Gets the list of vertex indices for this face.
    /// </summary>
    public List<int> VertexIndices { get; } = new();
}

/// <summary>
/// Represents a complete PLY mesh.
/// </summary>
public sealed class PLYMesh
{
    /// <summary>Gets or sets the format (ascii or binary_little_endian).</summary>
    public string Format { get; set; } = "ascii";

    /// <summary>Gets the list of vertices.</summary>
    public List<PLYVertex> Vertices { get; } = new();

    /// <summary>Gets the list of faces.</summary>
    public List<PLYFace> Faces { get; } = new();
}

/// <summary>
/// Reads PLY format (ASCII and binary) data.
/// </summary>
public sealed class PLYReader
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>
    /// Reads a PLY mesh from a stream.
    /// </summary>
    /// <param name="stream">The stream containing PLY data.</param>
    /// <returns>The parsed PLY mesh.</returns>
    public PLYMesh Read(Stream stream)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        string? formatLine = reader.ReadLine();
        if (formatLine is null || !formatLine.Trim().Equals("ply", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Invalid PLY header.");

        var mesh = new PLYMesh();
        int vertexCount = 0;
        int faceCount = 0;
        string format = "ascii";

        while (!reader.EndOfStream)
        {
            string? line = reader.ReadLine();
            if (line is null) break;
            line = line.Trim();
            if (line == "end_header") break;

            if (line.StartsWith("format", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = line.Split(' ');
                if (parts.Length >= 2)
                    format = parts[1].ToLowerInvariant();
                mesh.Format = format;
            }
            else if (line.StartsWith("element vertex", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = line.Split(' ');
                if (parts.Length >= 3)
                    int.TryParse(parts[2], out vertexCount);
            }
            else if (line.StartsWith("element face", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = line.Split(' ');
                if (parts.Length >= 3)
                    int.TryParse(parts[2], out faceCount);
            }
        }

        if (format == "ascii")
            ReadAscii(reader, mesh, vertexCount, faceCount);
        else
            ReadBinary(stream, mesh, vertexCount, faceCount, format.Contains("big"));

        return mesh;
    }

    private static void ReadAscii(StreamReader reader, PLYMesh mesh, int vertexCount, int faceCount)
    {
        for (int i = 0; i < vertexCount && !reader.EndOfStream; i++)
        {
            string? line = reader.ReadLine();
            if (line is null) break;
            var parts = line.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3)
            {
                var v = new PLYVertex();
                double.TryParse(parts[0], NumberStyles.Float, Inv, out double x);
                double.TryParse(parts[1], NumberStyles.Float, Inv, out double y);
                double.TryParse(parts[2], NumberStyles.Float, Inv, out double z);
                v.X = x; v.Y = y; v.Z = z;
                if (parts.Length >= 6)
                {
                    double.TryParse(parts[3], NumberStyles.Float, Inv, out double nx);
                    double.TryParse(parts[4], NumberStyles.Float, Inv, out double ny);
                    double.TryParse(parts[5], NumberStyles.Float, Inv, out double nz);
                    v.Nx = nx; v.Ny = ny; v.Nz = nz;
                }
                mesh.Vertices.Add(v);
            }
        }

        for (int i = 0; i < faceCount && !reader.EndOfStream; i++)
        {
            string? line = reader.ReadLine();
            if (line is null) break;
            var parts = line.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 1 && int.TryParse(parts[0], out int count))
            {
                var face = new PLYFace();
                for (int j = 1; j <= count && j < parts.Length; j++)
                {
                    if (int.TryParse(parts[j], out int idx))
                        face.VertexIndices.Add(idx);
                }
                if (face.VertexIndices.Count >= 3)
                    mesh.Faces.Add(face);
            }
        }
    }

    private static void ReadBinary(Stream stream, PLYMesh mesh, int vertexCount, int faceCount, bool bigEndian)
    {
        using var br = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        for (int i = 0; i < vertexCount; i++)
        {
            var v = new PLYVertex
            {
                X = ReadFloat(br, bigEndian),
                Y = ReadFloat(br, bigEndian),
                Z = ReadFloat(br, bigEndian),
                Nx = ReadFloat(br, bigEndian),
                Ny = ReadFloat(br, bigEndian),
                Nz = ReadFloat(br, bigEndian)
            };
            mesh.Vertices.Add(v);
        }

        for (int i = 0; i < faceCount; i++)
        {
            int count = br.ReadByte();
            var face = new PLYFace();
            for (int j = 0; j < count; j++)
                face.VertexIndices.Add(ReadInt32(br, bigEndian));
            if (face.VertexIndices.Count >= 3)
                mesh.Faces.Add(face);
        }
    }

    private static float ReadFloat(BinaryReader br, bool bigEndian)
    {
        byte[] bytes = br.ReadBytes(4);
        if (bigEndian) Array.Reverse(bytes);
        return BitConverter.ToSingle(bytes, 0);
    }

    private static int ReadInt32(BinaryReader br, bool bigEndian)
    {
        byte[] bytes = br.ReadBytes(4);
        if (bigEndian) Array.Reverse(bytes);
        return BitConverter.ToInt32(bytes, 0);
    }
}

/// <summary>
/// Writes PLY format (ASCII and binary) data.
/// </summary>
public sealed class PLYWriter
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>
    /// Writes a PLY mesh in ASCII format.
    /// </summary>
    /// <param name="mesh">The mesh to write.</param>
    /// <returns>A string containing ASCII PLY data.</returns>
    public string WriteASCII(PLYMesh mesh)
    {
        if (mesh is null)
            throw new ArgumentNullException(nameof(mesh));

        var sb = new StringBuilder();
        sb.AppendLine("ply");
        sb.AppendLine("format ascii 1.0");
        sb.AppendLine($"element vertex {mesh.Vertices.Count}");
        sb.AppendLine("property double x");
        sb.AppendLine("property double y");
        sb.AppendLine("property double z");
        sb.AppendLine("property double nx");
        sb.AppendLine("property double ny");
        sb.AppendLine("property double nz");
        sb.AppendLine($"element face {mesh.Faces.Count}");
        sb.AppendLine("property list uchar int vertex_indices");
        sb.AppendLine("end_header");

        foreach (var v in mesh.Vertices)
            sb.AppendLine($"{v.X.ToString(Inv)} {v.Y.ToString(Inv)} {v.Z.ToString(Inv)} {v.Nx.ToString(Inv)} {v.Ny.ToString(Inv)} {v.Nz.ToString(Inv)}");

        foreach (var f in mesh.Faces)
        {
            sb.Append(f.VertexIndices.Count);
            foreach (int idx in f.VertexIndices)
                sb.Append($" {idx}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Writes a PLY mesh in binary format.
    /// </summary>
    /// <param name="mesh">The mesh to write.</param>
    /// <returns>A byte array containing binary PLY data.</returns>
    public byte[] WriteBinary(PLYMesh mesh)
    {
        if (mesh is null)
            throw new ArgumentNullException(nameof(mesh));

        string header = WriteASCII(mesh);
        int headerEnd = header.IndexOf("end_header", StringComparison.Ordinal) + "end_header".Length + 1;

        using var ms = new MemoryStream();
        byte[] headerBytes = Encoding.UTF8.GetBytes(header.Substring(0, headerEnd));
        ms.Write(headerBytes, 0, headerBytes.Length);

        using var bw = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);
        foreach (var v in mesh.Vertices)
        {
            bw.Write((float)v.X);
            bw.Write((float)v.Y);
            bw.Write((float)v.Z);
            bw.Write((float)v.Nx);
            bw.Write((float)v.Ny);
            bw.Write((float)v.Nz);
        }

        foreach (var f in mesh.Faces)
        {
            bw.Write((byte)f.VertexIndices.Count);
            foreach (int idx in f.VertexIndices)
                bw.Write(idx);
        }

        return ms.ToArray();
    }
}
