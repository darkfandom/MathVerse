namespace MathVerse.Math.Geometry.Advanced.Serialization;

/// <summary>
/// Defines the supported geometry file formats for serialization and deserialization.
/// </summary>
public enum GeometryFormatType
{
    /// <summary>Wavefront OBJ format. Text-based 3D model format supporting vertices and faces.</summary>
    OBJ,

    /// <summary>Stereolithography STL format. Used for 3D printing and CAD, available in ASCII and binary variants.</summary>
    STL,

    /// <summary>Object File Format. A simple text-based format for storing 3D geometry as vertices and polygonal faces.</summary>
    OFF,

    /// <summary>Polygon File Format (Stanford). Supports vertex and face elements with optional per-vertex properties.</summary>
    PLY,

    /// <summary>Scalable Vector Graphics. XML-based format for 2D vector graphics, used for polygon and path rendering.</summary>
    SVG,

    /// <summary>Well-Known Text. OGC standard text representation for geometric objects such as points, lines, and polygons.</summary>
    WKT,

    /// <summary>GeoJSON. A JSON-based format for encoding geographic data structures including points, lines, and polygons.</summary>
    GeoJSON
}
