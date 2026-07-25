using System.Globalization;
using System.Text;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;
using MathVerse.Math.Geometry.Meshes;

namespace MathVerse.Math.Geometry.Utilities;

/// <summary>Formats geometric objects as strings in various formats.</summary>
public static class GeometryFormatter
{
    /// <summary>Formats a 2D point as "(x, y)".</summary>
    public static string Format(Point2D p, string fmt = "F6") =>
        $"({p.X.ToString(fmt, CultureInfo.InvariantCulture)}, {p.Y.ToString(fmt, CultureInfo.InvariantCulture)})";

    /// <summary>Formats a 3D point as "(x, y, z)".</summary>
    public static string Format(Point3D p, string fmt = "F6") =>
        $"({p.X.ToString(fmt, CultureInfo.InvariantCulture)}, {p.Y.ToString(fmt, CultureInfo.InvariantCulture)}, {p.Z.ToString(fmt, CultureInfo.InvariantCulture)})";

    /// <summary>Formats a 3D vector as "(x, y, z)".</summary>
    public static string Format(Vector3D v, string fmt = "F6") =>
        $"({v.X.ToString(fmt, CultureInfo.InvariantCulture)}, {v.Y.ToString(fmt, CultureInfo.InvariantCulture)}, {v.Z.ToString(fmt, CultureInfo.InvariantCulture)})";

    /// <summary>Formats a triangle as "A -> B -> C".</summary>
    public static string Format(Triangle3D t, string fmt = "F6") =>
        $"{Format(t.A, fmt)} -> {Format(t.B, fmt)} -> {Format(t.C, fmt)}";

    /// <summary>Formats a mesh as a summary string.</summary>
    public static string Format(TriangleMesh mesh) =>
        $"TriangleMesh(vertices={mesh.VertexCount}, triangles={mesh.TriangleCount})";

    /// <summary>Formats points as a comma-separated list.</summary>
    public static string FormatPoints(IReadOnlyList<Point2D> points, string fmt = "F6")
    {
        if (points.Count == 0) return "[]";
        StringBuilder sb = new("[");
        for (int i = 0; i < points.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(Format(points[i], fmt));
        }
        sb.Append(']');
        return sb.ToString();
    }

    /// <summary>Formats 3D points as a comma-separated list.</summary>
    public static string FormatPoints(IReadOnlyList<Point3D> points, string fmt = "F6")
    {
        if (points.Count == 0) return "[]";
        StringBuilder sb = new("[");
        for (int i = 0; i < points.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(Format(points[i], fmt));
        }
        sb.Append(']');
        return sb.ToString();
    }

    /// <summary>Formats a polygon as a vertex list.</summary>
    public static string Format(Polygon2D polygon, string fmt = "F6") =>
        $"Polygon2D({FormatPoints(polygon.Vertices, fmt)})";

    /// <summary>Formats a polyline as a vertex list.</summary>
    public static string Format(Polyline2D polyline, string fmt = "F6") =>
        $"Polyline2D({FormatPoints(polyline.Vertices, fmt)})";

    /// <summary>Formats a bounding box.</summary>
    public static string Format(BoundingBox3D box, string fmt = "F6") =>
        $"AABB({Format(box.Min, fmt)} .. {Format(box.Max, fmt)})";

    /// <summary>Formats a sphere.</summary>
    public static string Format(Sphere3D sphere, string fmt = "F6") =>
        $"Sphere({Format(sphere.Center, fmt)}, r={sphere.Radius.ToString(fmt, CultureInfo.InvariantCulture)})";

    /// <summary>Formats points in WKT (Well-Known Text) format.</summary>
    public static string ToWKT(IReadOnlyList<Point2D> points)
    {
        StringBuilder sb = new("POLYGON (");
        for (int i = 0; i < points.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(points[i].X.ToString("F6", CultureInfo.InvariantCulture));
            sb.Append(' ');
            sb.Append(points[i].Y.ToString("F6", CultureInfo.InvariantCulture));
        }
        if (points.Count > 0) { sb.Append(", "); sb.Append(points[0].X.ToString("F6", CultureInfo.InvariantCulture)); sb.Append(' '); sb.Append(points[0].Y.ToString("F6", CultureInfo.InvariantCulture)); }
        sb.Append(')');
        return sb.ToString();
    }

    /// <summary>Formats a 3D mesh as OBJ format.</summary>
    public static string ToOBJ(TriangleMesh mesh)
    {
        StringBuilder sb = new();
        for (int i = 0; i < mesh.VertexCount; i++)
            sb.AppendLine($"v {mesh.Vertices[i].Position.X.ToString("F6", CultureInfo.InvariantCulture)} {mesh.Vertices[i].Position.Y.ToString("F6", CultureInfo.InvariantCulture)} {mesh.Vertices[i].Position.Z.ToString("F6", CultureInfo.InvariantCulture)}");
        for (int i = 0; i < mesh.TriangleCount; i++)
            sb.AppendLine($"f {mesh.Faces[i].V0 + 1} {mesh.Faces[i].V1 + 1} {mesh.Faces[i].V2 + 1}");
        return sb.ToString();
    }
}
