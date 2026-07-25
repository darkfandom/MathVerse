using System;
using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Serialization;

/// <summary>
/// Provides a unified dispatch interface for serializing and deserializing 3D geometry
/// data across all supported file formats. Routes operations to the appropriate format-specific
/// serializer based on the requested <see cref="GeometryFormatType"/>.
/// </summary>
public static class SerializationRegistry
{
    /// <summary>
    /// Serializes 3D vertex and index data into the specified geometry file format.
    /// Delegates to the appropriate format-specific serializer based on the format type.
    /// </summary>
    /// <param name="vertices">The vertex positions to serialize.</param>
    /// <param name="indices">The triangle index buffer, where every three consecutive indices define a face.</param>
    /// <param name="format">The target serialization format.</param>
    /// <returns>A string containing the serialized geometry data in the requested format.</returns>
    /// <exception cref="NotSupportedException">Thrown when the specified format type is not supported.</exception>
    public static string Serialize(ImmutableArray<Point3D> vertices, ImmutableArray<int> indices, GeometryFormatType format)
    {
        return format switch
        {
            GeometryFormatType.OBJ => OBJSerializer.Serialize(vertices, indices),
            GeometryFormatType.STL => STLSerializer.Serialize(vertices, indices),
            GeometryFormatType.OFF => OFFSerializer.Serialize(vertices, indices),
            GeometryFormatType.PLY => PLYSerializer.Serialize(vertices, indices),
            _ => throw new NotSupportedException($"Format '{format}' is not supported for 3D vertex/index serialization.")
        };
    }

    /// <summary>
    /// Deserializes 3D geometry data from the specified format, returning vertices and triangle indices.
    /// Automatically detects the format from the content structure if possible, otherwise uses the
    /// explicitly specified format type.
    /// </summary>
    /// <param name="content">The serialized geometry data to parse.</param>
    /// <param name="format">The format of the input data.</param>
    /// <returns>
    /// A tuple containing the parsed vertex positions and the triangle index buffer.
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown when the specified format type is not supported.</exception>
    /// <exception cref="FormatException">Thrown when the content cannot be parsed in the specified format.</exception>
    public static (ImmutableArray<Point3D> Vertices, ImmutableArray<int> Indices) Deserialize(string content, GeometryFormatType format)
    {
        GeometryFormatType detected = format == GeometryFormatType.OBJ ? DetectFormat(content) : format;

        return detected switch
        {
            GeometryFormatType.OBJ => OBJSerializer.Deserialize(content),
            GeometryFormatType.STL => STLSerializer.Deserialize(content),
            GeometryFormatType.OFF => OFFSerializer.Deserialize(content),
            GeometryFormatType.PLY => PLYSerializer.Deserialize(content),
            _ => throw new NotSupportedException($"Format '{detected}' is not supported for deserialization.")
        };
    }

    private static GeometryFormatType DetectFormat(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return GeometryFormatType.OBJ;

        string trimmed = content.TrimStart();

        if (trimmed.StartsWith("ply", StringComparison.OrdinalIgnoreCase))
            return GeometryFormatType.PLY;

        if (trimmed.StartsWith("solid", StringComparison.OrdinalIgnoreCase) &&
            trimmed.IndexOf("facet", StringComparison.OrdinalIgnoreCase) >= 0)
            return GeometryFormatType.STL;

        if (trimmed.StartsWith("OFF", StringComparison.Ordinal))
            return GeometryFormatType.OFF;

        return GeometryFormatType.OBJ;
    }
}
