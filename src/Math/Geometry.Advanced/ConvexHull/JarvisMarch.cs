using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.ConvexHull;

/// <summary>
/// Computes the convex hull of a set of 2D or 3D points using the Jarvis march (gift wrapping) algorithm.
/// </summary>
public static class JarvisMarch
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Computes the convex hull of a set of 2D points using the Jarvis march (gift wrapping) algorithm.
    /// </summary>
    /// <param name="points">The input point set.</param>
    /// <returns>
    /// An immutable array of points forming the convex hull in counter-clockwise order.
    /// Returns an empty array if fewer than 3 non-collinear points exist.
    /// </returns>
    public static ImmutableArray<Point2D> Compute(ImmutableArray<Point2D> points)
    {
        if (points.Length <= 2)
            return ImmutableArray.CreateRange(points);

        int leftIdx = 0;
        for (int i = 1; i < points.Length; i++)
        {
            if (points[i].X < points[leftIdx].X ||
                (System.Math.Abs(points[i].X - points[leftIdx].X) < Tolerance && points[i].Y < points[leftIdx].Y))
                leftIdx = i;
        }

        var hull = ImmutableArray.CreateBuilder<Point2D>();
        int p = leftIdx;

        do
        {
            hull.Add(points[p]);
            int q = (p + 1) % points.Length;

            for (int r = 0; r < points.Length; r++)
            {
                double cross = (points[q].X - points[p].X) * (points[r].Y - points[p].Y)
                             - (points[q].Y - points[p].Y) * (points[r].X - points[p].X);

                if (cross < -Tolerance)
                {
                    q = r;
                }
                else if (System.Math.Abs(cross) < Tolerance)
                {
                    double distPR = points[p].DistanceSquaredTo(points[r]);
                    double distPQ = points[p].DistanceSquaredTo(points[q]);
                    if (distPR > distPQ)
                        q = r;
                }
            }

            p = q;
        }
        while (p != leftIdx);

        return hull.ToImmutable();
    }

    /// <summary>
    /// Computes the convex hull of a set of 3D points using a plane-based gift wrapping approach.
    /// </summary>
    /// <param name="points">The input 3D point set.</param>
    /// <returns>
    /// An immutable array of 3D points forming the convex hull.
    /// Returns the input directly if fewer than 4 non-coplanar points exist.
    /// </returns>
    public static ImmutableArray<Point3D> Compute3D(ImmutableArray<Point3D> points)
    {
        if (points.Length <= 3)
            return ImmutableArray.CreateRange(points);

        int i0 = FindBottommost(points);
        int i1 = FindRightmostFromPoint(points, i0);
        int i2 = FindBestTriangle(points, i0, i1);
        Plane3D face = new Plane3D(points[i0], points[i1], points[i2]);

        var visitedFaces = new HashSet<(int, int, int)>();
        var hullTriangles = new List<(int, int, int)>();
        var edgeQueue = new Queue<(int, int, int)>();

        edgeQueue.Enqueue(ValidateOrder(points, i0, i1, i2));
        visitedFaces.Add(edgeQueue.Peek());

        while (edgeQueue.Count > 0)
        {
            (int a, int b, int c) = edgeQueue.Dequeue();
            Plane3D triPlane = new Plane3D(points[a], points[b], points[c]);
            int opp = FindOpposite(points, triPlane, a, b, c);

            if (opp < 0)
                continue;

            (int ab, int bc, int ca) = ValidateOrder(points, b, c, opp);
            (int ba, int ac, int cb) = ValidateOrder(points, c, a, opp);

            if (visitedFaces.Add((ab, bc, ca)))
            {
                edgeQueue.Enqueue((ab, bc, ca));
                hullTriangles.Add((ab, bc, ca));
            }

            if (visitedFaces.Add((ba, ac, cb)))
            {
                edgeQueue.Enqueue((ba, ac, cb));
                hullTriangles.Add((ba, ac, cb));
            }
        }

        if (hullTriangles.Count == 0)
        {
            hullTriangles.Add(ValidateOrder(points, i0, i1, i2));
        }

        var seen = new HashSet<int>();
        var result = ImmutableArray.CreateBuilder<Point3D>();
        for (int i = 0; i < hullTriangles.Count; i++)
        {
            (int a, int b, int c) = hullTriangles[i];
            if (seen.Add(a)) result.Add(points[a]);
            if (seen.Add(b)) result.Add(points[b]);
            if (seen.Add(c)) result.Add(points[c]);
        }

        return result.ToImmutable();
    }

    private static int FindBottommost(ImmutableArray<Point3D> points)
    {
        int idx = 0;
        for (int i = 1; i < points.Length; i++)
        {
            if (points[i].Y < points[idx].Y ||
                (System.Math.Abs(points[i].Y - points[idx].Y) < Tolerance && points[i].X < points[idx].X))
                idx = i;
        }
        return idx;
    }

    private static int FindRightmostFromPoint(ImmutableArray<Point3D> points, int from)
    {
        int best = -1;
        double bestAngle = double.MaxValue;
        for (int i = 0; i < points.Length; i++)
        {
            if (i == from) continue;
            double angle = System.Math.Atan2(points[i].Z - points[from].Z, points[i].X - points[from].X);
            if (angle < bestAngle - Tolerance || (System.Math.Abs(angle - bestAngle) < Tolerance && best >= 0 && points[i].DistanceSquaredTo(points[from]) > points[best].DistanceSquaredTo(points[from])))
            {
                bestAngle = angle;
                best = i;
            }
        }
        return best >= 0 ? best : (from + 1) % points.Length;
    }

    private static int FindBestTriangle(ImmutableArray<Point3D> points, int i0, int i1)
    {
        int best = -1;
        double bestArea = -1;
        Vector3D refDir = new Vector3D(points[i1].X - points[i0].X, points[i1].Y - points[i0].Y, points[i1].Z - points[i0].Z);
        for (int i = 0; i < points.Length; i++)
        {
            if (i == i0 || i == i1) continue;
            Vector3D v = new Vector3D(points[i].X - points[i0].X, points[i].Y - points[i0].Y, points[i].Z - points[i0].Z);
            Vector3D cross = refDir.Cross(v);
            double area = cross.Length;
            if (area > bestArea + Tolerance)
            {
                bestArea = area;
                best = i;
            }
        }
        return best >= 0 ? best : ((i1 + 1) % points.Length);
    }

    private static int FindOpposite(ImmutableArray<Point3D> points, Plane3D plane, int a, int b, int c)
    {
        int best = -1;
        double bestDist = Tolerance;

        for (int i = 0; i < points.Length; i++)
        {
            if (i == a || i == b || i == c) continue;
            double dist = plane.SignedDistance(points[i]);
            if (dist > bestDist)
            {
                bestDist = dist;
                best = i;
            }
        }
        return best;
    }

    private static (int, int, int) ValidateOrder(ImmutableArray<Point3D> points, int a, int b, int c)
    {
        Vector3D ab = new Vector3D(points[b].X - points[a].X, points[b].Y - points[a].Y, points[b].Z - points[a].Z);
        Vector3D ac = new Vector3D(points[c].X - points[a].X, points[c].Y - points[a].Y, points[c].Z - points[a].Z);
        Vector3D cross = ab.Cross(ac);

        double cx = 0, cy = 0, cz = 0;
        for (int i = 0; i < points.Length; i++)
        {
            cx += points[i].X;
            cy += points[i].Y;
            cz += points[i].Z;
        }
        double inv = 1.0 / points.Length;
        Vector3D toCentroid = new Vector3D(cx * inv - points[a].X, cy * inv - points[a].Y, cz * inv - points[a].Z);

        if (cross.Dot(toCentroid) < 0)
            return (a, c, b);
        return (a, b, c);
    }

    private readonly struct Plane3D
    {
        public readonly double A, B, C, D;

        public Plane3D(Point3D p0, Point3D p1, Point3D p2)
        {
            Vector3D v1 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D v2 = new Vector3D(p2.X - p0.X, p2.Y - p0.Y, p2.Z - p0.Z);
            Vector3D n = v1.Cross(v2);
            A = n.X;
            B = n.Y;
            C = n.Z;
            D = -(A * p0.X + B * p0.Y + C * p0.Z);
        }

        public double SignedDistance(Point3D p) =>
            A * p.X + B * p.Y + C * p.Z + D;
    }
}
