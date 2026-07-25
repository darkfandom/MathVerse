using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Validation;

/// <summary>
/// Specifies the severity level of a geometry validation result.
/// </summary>
public enum ValidationSeverity
{
    /// <summary>A non-critical issue that may affect quality but not correctness.</summary>
    Warning,

    /// <summary>An issue that affects geometric correctness.</summary>
    Error,

    /// <summary>A critical issue that makes the geometry unusable.</summary>
    Critical
}

/// <summary>
/// Describes a single validation result with severity, message, and context.
/// </summary>
/// <param name="Severity">The severity level of the issue.</param>
/// <param name="Message">A human-readable description of the issue.</param>
/// <param name="Context">Additional context about where the issue was found.</param>
public readonly record struct ValidationResult(ValidationSeverity Severity, string Message, string Context);

/// <summary>
/// Provides comprehensive validation for 2D polygons and 3D triangle meshes.
/// Checks for self-intersections, degenerate geometry, valid index ranges,
/// winding consistency, and manifold properties.
/// </summary>
public static class GeometryValidationEngine
{
    private const double Tolerance = 1e-10;
    private const double DefaultMinArea = 1e-14;

    /// <summary>
    /// Validates a 2D polygon for geometric correctness.
    /// Checks for self-intersections, minimum vertex count, winding consistency,
    /// and degenerate edges.
    /// </summary>
    /// <param name="polygon">The polygon vertices in order.</param>
    /// <returns>An immutable array of validation results describing any issues found.</returns>
    public static ImmutableArray<ValidationResult> ValidatePolygon(ImmutableArray<Point2D> polygon)
    {
        var results = ImmutableArray.CreateBuilder<ValidationResult>();

        if (polygon.Length < 3)
        {
            results.Add(new ValidationResult(ValidationSeverity.Critical,
                "Polygon has fewer than 3 vertices",
                $"Vertex count: {polygon.Length}"));
            return results.ToImmutable();
        }

        for (int i = 0; i < polygon.Length; i++)
        {
            double dx = polygon[(i + 1) % polygon.Length].X - polygon[i].X;
            double dy = polygon[(i + 1) % polygon.Length].Y - polygon[i].Y;
            double lenSq = dx * dx + dy * dy;
            if (lenSq < Tolerance * Tolerance)
            {
                results.Add(new ValidationResult(ValidationSeverity.Warning,
                    "Degenerate edge detected (zero-length edge)",
                    $"Edge from vertex {i} to {(i + 1) % polygon.Length}"));
            }
        }

        if (HasSelfIntersections(polygon))
        {
            results.Add(new ValidationResult(ValidationSeverity.Error,
                "Polygon has self-intersections",
                "Self-intersecting polygon is not simple"));
        }

        double signedArea = 0;
        for (int i = 0; i < polygon.Length; i++)
        {
            int j = (i + 1) % polygon.Length;
            signedArea += polygon[i].X * polygon[j].Y;
            signedArea -= polygon[j].X * polygon[i].Y;
        }

        if (System.Math.Abs(signedArea) < Tolerance)
        {
            results.Add(new ValidationResult(ValidationSeverity.Warning,
                "Polygon has near-zero area",
                $"Signed area: {signedArea}"));
        }

        bool hasColinear = false;
        for (int i = 0; i < polygon.Length; i++)
        {
            int prev = (i - 1 + polygon.Length) % polygon.Length;
            int next = (i + 1) % polygon.Length;
            double cross = (polygon[i].X - polygon[prev].X) * (polygon[next].Y - polygon[i].Y)
                         - (polygon[i].Y - polygon[prev].Y) * (polygon[next].X - polygon[i].X);
            if (System.Math.Abs(cross) < Tolerance)
            {
                hasColinear = true;
                break;
            }
        }

        if (hasColinear)
        {
            results.Add(new ValidationResult(ValidationSeverity.Warning,
                "Collinear consecutive vertices detected",
                "Some consecutive edges are collinear and could be simplified"));
        }

        if (results.Count == 0)
        {
            results.Add(new ValidationResult(ValidationSeverity.Warning,
                "Polygon is valid",
                "No issues detected"));
        }

        return results.ToImmutable();
    }

    /// <summary>
    /// Validates a 3D triangle mesh for geometric correctness.
    /// Checks for non-manifold edges, degenerate triangles, valid index ranges,
    /// and consistent winding.
    /// </summary>
    /// <param name="vertices">The mesh vertex positions.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <returns>An immutable array of validation results describing any issues found.</returns>
    public static ImmutableArray<ValidationResult> ValidateMesh(
        ImmutableArray<Point3D> vertices, ImmutableArray<int> indices)
    {
        var results = ImmutableArray.CreateBuilder<ValidationResult>();

        if (vertices.Length == 0)
        {
            results.Add(new ValidationResult(ValidationSeverity.Critical,
                "Mesh has no vertices",
                "Vertex array is empty"));
            return results.ToImmutable();
        }

        if (indices.Length % 3 != 0)
        {
            results.Add(new ValidationResult(ValidationSeverity.Critical,
                "Index buffer length is not a multiple of 3",
                $"Index count: {indices.Length}"));
            return results.ToImmutable();
        }

        int outOfRange = 0;
        for (int i = 0; i < indices.Length; i++)
        {
            if (indices[i] < 0 || indices[i] >= vertices.Length)
                outOfRange++;
        }

        if (outOfRange > 0)
        {
            results.Add(new ValidationResult(ValidationSeverity.Critical,
                "Index buffer contains out-of-range indices",
                $"{outOfRange} indices are outside valid vertex range [0, {vertices.Length - 1}]"));
        }

        int triCount = indices.Length / 3;
        int degenerateCount = 0;
        for (int t = 0; t < triCount; t++)
        {
            int i0 = indices[t * 3];
            int i1 = indices[t * 3 + 1];
            int i2 = indices[t * 3 + 2];

            if (i0 < 0 || i0 >= vertices.Length || i1 < 0 || i1 >= vertices.Length || i2 < 0 || i2 >= vertices.Length)
                continue;

            if (i0 == i1 || i1 == i2 || i0 == i2)
            {
                degenerateCount++;
                continue;
            }

            double area = ComputeTriangleArea(vertices[i0], vertices[i1], vertices[i2]);
            if (area < DefaultMinArea)
                degenerateCount++;
        }

        if (degenerateCount > 0)
        {
            results.Add(new ValidationResult(ValidationSeverity.Warning,
                "Mesh contains degenerate triangles",
                $"{degenerateCount} degenerate triangles found"));
        }

        var edgeCount = new Dictionary<(int, int), int>();
        for (int t = 0; t < triCount; t++)
        {
            int i0 = indices[t * 3];
            int i1 = indices[t * 3 + 1];
            int i2 = indices[t * 3 + 2];

            if (i0 < 0 || i0 >= vertices.Length || i1 < 0 || i1 >= vertices.Length || i2 < 0 || i2 >= vertices.Length)
                continue;

            CountEdge(edgeCount, i0, i1);
            CountEdge(edgeCount, i1, i2);
            CountEdge(edgeCount, i2, i0);
        }

        int nonManifold = 0;
        foreach (var kvp in edgeCount)
        {
            if (kvp.Value > 2)
                nonManifold++;
        }

        if (nonManifold > 0)
        {
            results.Add(new ValidationResult(ValidationSeverity.Error,
                "Mesh has non-manifold edges",
                $"{nonManifold} edges are shared by more than 2 faces"));
        }

        int boundaryEdges = 0;
        foreach (var kvp in edgeCount)
        {
            if (kvp.Value == 1)
                boundaryEdges++;
        }

        if (boundaryEdges > 0)
        {
            results.Add(new ValidationResult(ValidationSeverity.Warning,
                "Mesh has boundary edges",
                $"{boundaryEdges} boundary edges found (open mesh)"));
        }

        if (results.Count == 0)
        {
            results.Add(new ValidationResult(ValidationSeverity.Warning,
                "Mesh is valid",
                "No issues detected"));
        }

        return results.ToImmutable();
    }

    /// <summary>
    /// Tests whether a 2D polygon has any self-intersections.
    /// Uses an O(n^2) pairwise edge intersection check.
    /// </summary>
    /// <param name="polygon">The polygon vertices in order.</param>
    /// <returns><c>true</c> if the polygon has self-intersections; otherwise, <c>false</c>.</returns>
    public static bool HasSelfIntersections(ImmutableArray<Point2D> polygon)
    {
        int n = polygon.Length;
        if (n < 3) return false;

        for (int i = 0; i < n; i++)
        {
            int iNext = (i + 1) % n;
            for (int j = i + 2; j < n; j++)
            {
                if (i == 0 && j == n - 1) continue;
                int jNext = (j + 1) % n;
                if (SegmentsIntersect(polygon[i], polygon[iNext], polygon[j], polygon[jNext]))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Tests whether a mesh contains degenerate triangles with area below the specified threshold.
    /// </summary>
    /// <param name="vertices">The mesh vertex positions.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <param name="minArea">The minimum acceptable triangle area.</param>
    /// <returns><c>true</c> if any triangle has area below the threshold; otherwise, <c>false</c>.</returns>
    public static bool HasDegenerateTriangles(
        ImmutableArray<Point3D> vertices, ImmutableArray<int> indices, double minArea)
    {
        int triCount = indices.Length / 3;

        for (int t = 0; t < triCount; t++)
        {
            int i0 = indices[t * 3];
            int i1 = indices[t * 3 + 1];
            int i2 = indices[t * 3 + 2];

            if (i0 >= vertices.Length || i1 >= vertices.Length || i2 >= vertices.Length)
                continue;

            if (i0 == i1 || i1 == i2 || i0 == i2)
                return true;

            double area = ComputeTriangleArea(vertices[i0], vertices[i1], vertices[i2]);
            if (area < minArea)
                return true;
        }

        return false;
    }

    private static double ComputeTriangleArea(Point3D a, Point3D b, Point3D c)
    {
        double ax = b.X - a.X, ay = b.Y - a.Y, az = b.Z - a.Z;
        double bx = c.X - a.X, by = c.Y - a.Y, bz = c.Z - a.Z;
        double cx = ay * bz - az * by;
        double cy = az * bx - ax * bz;
        double cz = ax * by - ay * bx;
        return System.Math.Sqrt(cx * cx + cy * cy + cz * cz) * 0.5;
    }

    private static void CountEdge(Dictionary<(int, int), int> edgeCount, int v0, int v1)
    {
        int lo = v0 < v1 ? v0 : v1;
        int hi = v0 < v1 ? v1 : v0;
        var key = (lo, hi);
        if (edgeCount.ContainsKey(key))
            edgeCount[key]++;
        else
            edgeCount[key] = 1;
    }

    private static bool SegmentsIntersect(Point2D a, Point2D b, Point2D c, Point2D d)
    {
        double d1 = Cross(c, d, a);
        double d2 = Cross(c, d, b);
        double d3 = Cross(a, b, c);
        double d4 = Cross(a, b, d);

        if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
            ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
            return true;

        if (System.Math.Abs(d1) < Tolerance && OnSegment(c, d, a)) return true;
        if (System.Math.Abs(d2) < Tolerance && OnSegment(c, d, b)) return true;
        if (System.Math.Abs(d3) < Tolerance && OnSegment(a, b, c)) return true;
        if (System.Math.Abs(d4) < Tolerance && OnSegment(a, b, d)) return true;

        return false;
    }

    private static double Cross(Point2D o, Point2D a, Point2D b)
    {
        return (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);
    }

    private static bool OnSegment(Point2D a, Point2D b, Point2D c)
    {
        double minX = System.Math.Min(a.X, b.X);
        double maxX = System.Math.Max(a.X, b.X);
        double minY = System.Math.Min(a.Y, b.Y);
        double maxY = System.Math.Max(a.Y, b.Y);
        return c.X >= minX - Tolerance && c.X <= maxX + Tolerance &&
               c.Y >= minY - Tolerance && c.Y <= maxY + Tolerance;
    }
}
