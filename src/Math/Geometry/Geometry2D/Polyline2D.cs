using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace MathVerse.Math.Geometry.Geometry2D;

/// <summary>Represents an open polyline defined by an ordered sequence of 2D vertices.</summary>
public readonly record struct Polyline2D(ImmutableArray<Point2D> Vertices)
{
    /// <summary>The vertices of the polyline.</summary>
    public ImmutableArray<Point2D> Vertices { get; } = Vertices;

    /// <summary>Gets the number of vertices.</summary>
    public int VertexCount => Vertices.Length;

    /// <summary>Gets the number of segments.</summary>
    public int SegmentCount => System.Math.Max(0, Vertices.Length - 1);

    /// <summary>Gets the total length of the polyline.</summary>
    public double Length
    {
        get
        {
            double len = 0;
            for (int i = 0; i < Vertices.Length - 1; i++)
                len += Vertices[i].DistanceTo(Vertices[i + 1]);
            return len;
        }
    }

    /// <summary>Gets the centroid of the polyline.</summary>
    public Point2D Centroid
    {
        get
        {
            if (Vertices.Length == 0) return Point2D.Origin;
            double cx = 0, cy = 0;
            for (int i = 0; i < Vertices.Length; i++)
            {
                cx += Vertices[i].X;
                cy += Vertices[i].Y;
            }
            return new Point2D(cx / Vertices.Length, cy / Vertices.Length);
        }
    }

    /// <summary>Gets the start point.</summary>
    public Point2D Start => Vertices.Length > 0 ? Vertices[0] : Point2D.Origin;

    /// <summary>Gets the end point.</summary>
    public Point2D End => Vertices.Length > 0 ? Vertices[^1] : Point2D.Origin;

    /// <summary>Computes the axis-aligned bounding box.</summary>
    public BoundingBox2D ToBoundingBox() => BoundingBox2D.FromPoints(Vertices);

    /// <summary>Returns a closed polygon by connecting the last vertex to the first.</summary>
    public Polygon2D ToPolygon() => new(Vertices);

    /// <summary>Indexer for vertex access.</summary>
    public Point2D this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Vertices[index];
    }

    /// <summary>Gets the segments of the polyline.</summary>
    public IEnumerable<Segment2D> Segments
    {
        get
        {
            for (int i = 0; i < Vertices.Length - 1; i++)
                yield return new Segment2D(Vertices[i], Vertices[i + 1]);
        }
    }

    /// <summary>Reverses the vertex order.</summary>
    public Polyline2D Reverse() => new(Vertices.Reverse().ToImmutableArray());

    /// <summary>Simplifies the polyline using the Ramer-Douglas-Peucker algorithm.</summary>
    /// <param name="tolerance">Maximum perpendicular distance tolerance.</param>
    /// <returns>A simplified polyline.</returns>
    public Polyline2D Simplify(double tolerance)
    {
        if (Vertices.Length <= 2) return this;
        List<Point2D> simplified = new();
        SimplifyRDP(Vertices, 0, Vertices.Length - 1, tolerance, simplified);
        return new Polyline2D(simplified.ToImmutableArray());
    }

    /// <summary>Finds the closest point on the polyline to a given point.</summary>
    public Point2D ClosestPoint(Point2D p)
    {
        if (Vertices.Length == 0) return Point2D.Origin;
        if (Vertices.Length == 1) return Vertices[0];

        Point2D best = Vertices[0];
        double bestDist = p.DistanceSquaredTo(best);

        for (int i = 0; i < Vertices.Length - 1; i++)
        {
            var seg = new Segment2D(Vertices[i], Vertices[i + 1]);
            Point2D cp = seg.ClosestPoint(p);
            double d = p.DistanceSquaredTo(cp);
            if (d < bestDist)
            {
                bestDist = d;
                best = cp;
            }
        }
        return best;
    }

    /// <summary>Returns a string representation.</summary>
    public override string ToString() => $"Polyline2D(vertices={Vertices.Length}, length={Length:F4})";

    private static void SimplifyRDP(ImmutableArray<Point2D> pts, int start, int end, double tol, List<Point2D> result)
    {
        if (end - start < 2) { result.Add(pts[start]); return; }

        double maxDist = 0;
        int maxIdx = start;
        Point2D a = pts[start], b = pts[end];

        for (int i = start + 1; i < end; i++)
        {
            double d = PointLineDistance(pts[i], a, b);
            if (d > maxDist) { maxDist = d; maxIdx = i; }
        }

        if (maxDist > tol * tol)
        {
            SimplifyRDP(pts, start, maxIdx, tol, result);
            SimplifyRDP(pts, maxIdx, end, tol, result);
        }
        else
        {
            result.Add(pts[start]);
        }
    }

    private static double PointLineDistance(Point2D p, Point2D a, Point2D b)
    {
        double dx = b.X - a.X, dy = b.Y - a.Y;
        double lenSq = dx * dx + dy * dy;
        if (lenSq < 1e-30) return (p.X - a.X) * (p.X - a.X) + (p.Y - a.Y) * (p.Y - a.Y);
        double t = System.Math.Max(0, System.Math.Min(1, ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lenSq));
        double px = a.X + t * dx, py = a.Y + t * dy;
        return (p.X - px) * (p.X - px) + (p.Y - py) * (p.Y - py);
    }
}
