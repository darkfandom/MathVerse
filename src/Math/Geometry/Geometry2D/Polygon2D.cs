using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace MathVerse.Math.Geometry.Geometry2D;

/// <summary>Represents a polygon defined by an ordered set of vertices.</summary>
public readonly record struct Polygon2D(ImmutableArray<Point2D> Vertices)
{
    /// <summary>The vertices of the polygon.</summary>
    public ImmutableArray<Point2D> Vertices { get; } = Vertices;

    /// <summary>Gets the number of vertices.</summary>
    public int VertexCount => Vertices.Length;

    /// <summary>Gets the area using the shoelace formula.</summary>
    public double Area
    {
        get
        {
            double area = 0;
            int n = Vertices.Length;
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                area += Vertices[i].X * Vertices[j].Y;
                area -= Vertices[j].X * Vertices[i].Y;
            }
            return System.Math.Abs(area) * 0.5;
        }
    }

    /// <summary>Gets the perimeter of the polygon.</summary>
    public double Perimeter
    {
        get
        {
            double perimeter = 0;
            int n = Vertices.Length;
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                perimeter += Vertices[i].DistanceTo(Vertices[j]);
            }
            return perimeter;
        }
    }

    /// <summary>Gets the centroid of the polygon.</summary>
    public Point2D Centroid
    {
        get
        {
            double cx = 0, cy = 0;
            double signedArea = 0;
            int n = Vertices.Length;
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                double cross = Vertices[i].X * Vertices[j].Y - Vertices[j].X * Vertices[i].Y;
                signedArea += cross;
                cx += (Vertices[i].X + Vertices[j].X) * cross;
                cy += (Vertices[i].Y + Vertices[j].Y) * cross;
            }
            signedArea *= 0.5;
            if (System.Math.Abs(signedArea) < 1e-15) return Vertices.Length > 0 ? Vertices[0] : Point2D.Origin;
            cx /= (6.0 * signedArea);
            cy /= (6.0 * signedArea);
            return new Point2D(cx, cy);
        }
    }

    /// <summary>Determines whether the polygon is convex.</summary>
    public bool IsConvex
    {
        get
        {
            int n = Vertices.Length;
            if (n < 3) return false;
            bool hasPositive = false, hasNegative = false;
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                int k = (j + 1) % n;
                double cross = (Vertices[j].X - Vertices[i].X) * (Vertices[k].Y - Vertices[j].Y)
                             - (Vertices[j].Y - Vertices[i].Y) * (Vertices[k].X - Vertices[j].X);
                if (cross > 0) hasPositive = true;
                if (cross < 0) hasNegative = true;
                if (hasPositive && hasNegative) return false;
            }
            return true;
        }
    }

    /// <summary>Determines whether the polygon is simple (no self-intersections).</summary>
    public bool IsSimple
    {
        get
        {
            int n = Vertices.Length;
            if (n < 3) return false;
            for (int i = 0; i < n; i++)
            {
                int iNext = (i + 1) % n;
                var seg1 = new Segment2D(Vertices[i], Vertices[iNext]);
                for (int j = i + 2; j < n; j++)
                {
                    if (i == 0 && j == n - 1) continue;
                    int jNext = (j + 1) % n;
                    var seg2 = new Segment2D(Vertices[j], Vertices[jNext]);
                    if (seg1.Intersect(seg2).hit) return false;
                }
            }
            return true;
        }
    }

    /// <summary>Determines the winding order of the vertices.</summary>
    /// <returns>The winding order (clockwise or counter-clockwise).</returns>
    public WindingOrder WindingOrder
    {
        get
        {
            double signedArea = 0;
            int n = Vertices.Length;
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                signedArea += Vertices[i].X * Vertices[j].Y;
                signedArea -= Vertices[j].X * Vertices[i].Y;
            }
            return signedArea > 0 ? WindingOrder.CounterClockwise : WindingOrder.Clockwise;
        }
    }

    /// <summary>Determines whether the polygon contains the specified point using ray casting.</summary>
    /// <param name="p">The point to test.</param>
    /// <returns><c>true</c> if the polygon contains the point; otherwise, <c>false</c>.</returns>
    public bool Contains(Point2D p)
    {
        bool inside = false;
        int n = Vertices.Length;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            if ((Vertices[i].Y > p.Y) != (Vertices[j].Y > p.Y) &&
                p.X < (Vertices[j].X - Vertices[i].X) * (p.Y - Vertices[i].Y) / (Vertices[j].Y - Vertices[i].Y) + Vertices[i].X)
                inside = !inside;
        }
        return inside;
    }

    /// <summary>Triangulates the polygon using ear clipping.</summary>
    /// <returns>An immutable array of triangles.</returns>
    public ImmutableArray<Triangle2D> Triangulate()
    {
        var result = ImmutableArray.CreateBuilder<Triangle2D>();
        int n = Vertices.Length;
        if (n < 3) return result.ToImmutable();

        var indices = new List<int>();
        for (int i = 0; i < n; i++) indices.Add(i);

        int remaining = n;
        int failSafe = n * 2;
        int idx = 0;

        while (remaining > 2 && failSafe-- > 0)
        {
            int i0 = indices[idx % remaining];
            int i1 = indices[(idx + 1) % remaining];
            int i2 = indices[(idx + 2) % remaining];

            Point2D a = Vertices[i0], b = Vertices[i1], c = Vertices[i2];
            double cross = (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);

            bool isEar = cross > 1e-10;
            if (isEar)
            {
                for (int k = 0; k < remaining; k++)
                {
                    int ki = indices[k];
                    if (ki == i0 || ki == i1 || ki == i2) continue;
                    Point2D pt = Vertices[ki];
                    if (PointInTriangle(pt, a, b, c))
                    {
                        isEar = false;
                        break;
                    }
                }
            }

            if (isEar)
            {
                result.Add(new Triangle2D(a, b, c));
                indices.RemoveAt((idx + 1) % remaining);
                remaining--;
                idx = 0;
            }
            else
            {
                idx++;
            }
        }

        return result.ToImmutable();
    }

    /// <summary>Computes the axis-aligned bounding box of this polygon.</summary>
    /// <returns>The bounding box enclosing the polygon.</returns>
    public BoundingBox2D ToBoundingBox() => BoundingBox2D.FromPoints(Vertices);

    /// <summary>Gets the edges of the polygon.</summary>
    public IEnumerable<Segment2D> Edges
    {
        get
        {
            int n = Vertices.Length;
            for (int i = 0; i < n; i++)
                yield return new Segment2D(Vertices[i], Vertices[(i + 1) % n]);
        }
    }

    /// <summary>Indexer for vertex access by index.</summary>
    /// <param name="index">The vertex index.</param>
    /// <returns>The vertex at the specified index.</returns>
    public Point2D this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Vertices[index];
    }

    private static bool PointInTriangle(Point2D pt, Point2D a, Point2D b, Point2D c)
    {
        double d1 = (pt.X - b.X) * (a.Y - b.Y) - (a.X - b.X) * (pt.Y - b.Y);
        double d2 = (pt.X - c.X) * (b.Y - c.Y) - (b.X - c.X) * (pt.Y - c.Y);
        double d3 = (pt.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (pt.Y - a.Y);
        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        return !(hasNeg && hasPos);
    }

    /// <summary>Returns a string representation of this polygon.</summary>
    public override string ToString() => $"Polygon2D(vertices={Vertices.Length})";
}
