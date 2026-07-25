using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Validation;

/// <summary>
/// Describes a degenerate geometric element found during validation.
/// </summary>
/// <param name="Type">The type of degenerate element (e.g., "Triangle", "Edge", "Vertex").</param>
/// <param name="Index">The index of the degenerate element.</param>
/// <param name="Description">A human-readable description of the degeneracy.</param>
public readonly record struct DegenerateElement(string Type, int Index, string Description);

/// <summary>
/// Detects degenerate geometric elements in 2D polygons and 3D meshes.
/// Identifies zero-area triangles, zero-length edges, and collinear vertices.
/// </summary>
public static class DegenerateGeometryDetector
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Finds all triangles in a mesh with area below the specified threshold.
    /// </summary>
    /// <param name="vertices">The mesh vertex positions.</param>
    /// <param name="indices">The triangle index buffer (groups of 3).</param>
    /// <param name="minArea">The minimum acceptable triangle area.</param>
    /// <returns>An immutable array of degenerate elements describing triangles below the area threshold.</returns>
    public static ImmutableArray<DegenerateElement> FindDegenerateTriangles(
        ImmutableArray<Point3D> vertices, ImmutableArray<int> indices, double minArea)
    {
        var results = ImmutableArray.CreateBuilder<DegenerateElement>();
        int triCount = indices.Length / 3;

        for (int t = 0; t < triCount; t++)
        {
            int i0 = indices[t * 3];
            int i1 = indices[t * 3 + 1];
            int i2 = indices[t * 3 + 2];

            if (i0 >= vertices.Length || i1 >= vertices.Length || i2 >= vertices.Length)
            {
                results.Add(new DegenerateElement("Triangle", t,
                    $"Invalid indices: ({i0}, {i1}, {i2})"));
                continue;
            }

            if (i0 == i1 || i1 == i2 || i0 == i2)
            {
                results.Add(new DegenerateElement("Triangle", t,
                    $"Degenerate: repeated vertex index ({i0}, {i1}, {i2})"));
                continue;
            }

            double area = ComputeTriangleArea(vertices[i0], vertices[i1], vertices[i2]);
            if (area < minArea)
            {
                results.Add(new DegenerateElement("Triangle", t,
                    $"Area {area:G6} below threshold {minArea:G6}"));
            }
        }

        return results.ToImmutable();
    }

    /// <summary>
    /// Finds all edges in a 2D polygon shorter than the specified threshold length.
    /// </summary>
    /// <param name="polygon">The polygon vertices in order.</param>
    /// <param name="minLength">The minimum acceptable edge length.</param>
    /// <returns>An immutable array of degenerate elements describing edges below the length threshold.</returns>
    public static ImmutableArray<DegenerateElement> FindDegenerateEdges(
        ImmutableArray<Point2D> polygon, double minLength)
    {
        var results = ImmutableArray.CreateBuilder<DegenerateElement>();
        int n = polygon.Length;

        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            double dx = polygon[j].X - polygon[i].X;
            double dy = polygon[j].Y - polygon[i].Y;
            double length = System.Math.Sqrt(dx * dx + dy * dy);

            if (length < minLength)
            {
                results.Add(new DegenerateElement("Edge", i,
                    $"Length {length:G6} below threshold {minLength:G6} (vertex {i} to {j})"));
            }
        }

        return results.ToImmutable();
    }

    /// <summary>
    /// Finds consecutive collinear vertices in a 2D polygon that could be merged
    /// without changing the polygon shape. Three consecutive vertices are collinear
    /// if the cross product of the edge vectors is within tolerance.
    /// </summary>
    /// <param name="polygon">The polygon vertices in order.</param>
    /// <param name="tolerance">The cross-product tolerance for collinearity testing.</param>
    /// <returns>An immutable array of degenerate elements describing collinear vertex triples.</returns>
    public static ImmutableArray<DegenerateElement> FindColinearVertices(
        ImmutableArray<Point2D> polygon, double tolerance)
    {
        var results = ImmutableArray.CreateBuilder<DegenerateElement>();
        int n = polygon.Length;
        if (n < 3) return results.ToImmutable();

        for (int i = 0; i < n; i++)
        {
            int prev = (i - 1 + n) % n;
            int next = (i + 1) % n;

            double cross = (polygon[i].X - polygon[prev].X) * (polygon[next].Y - polygon[i].Y)
                         - (polygon[i].Y - polygon[prev].Y) * (polygon[next].X - polygon[i].X);

            if (System.Math.Abs(cross) < tolerance)
            {
                results.Add(new DegenerateElement("Vertex", i,
                    $"Vertex {i} is collinear with vertices {prev} and {next} (cross={cross:G6})"));
            }
        }

        return results.ToImmutable();
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
}
