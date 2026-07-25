using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Collision;

/// <summary>
/// Provides static methods for the Gilbert-Johnson-Keerthi (GJK) collision detection algorithm.
/// </summary>
public static class GJK
{
    private const double Tolerance = 1e-10;
    private const int MaxIterations = 64;

    /// <summary>
    /// Tests whether two convex shapes intersect using the GJK algorithm.
    /// Maintains a simplex of up to 4 vertices (tetrahedron in 3D) and checks
    /// whether the origin is contained within the Minkowski difference.
    /// </summary>
    /// <param name="shapeA">The vertices of the first convex shape.</param>
    /// <param name="shapeB">The vertices of the second convex shape.</param>
    /// <returns>True if the shapes intersect; otherwise, false.</returns>
    public static bool Intersects(ImmutableArray<Point3D> shapeA, ImmutableArray<Point3D> shapeB)
    {
        Vector3D direction = new Vector3D(1, 0, 0);
        Point3D a = MinkowskiSupport(shapeA, shapeB, direction);

        double aLenSq = a.X * a.X + a.Y * a.Y + a.Z * a.Z;
        if (aLenSq < Tolerance)
            return true;

        direction = NegatePoint(a);
        Point3D b = MinkowskiSupport(shapeA, shapeB, direction);

        double bDotDir = b.X * direction.X + b.Y * direction.Y + b.Z * direction.Z;
        if (bDotDir < Tolerance)
            return false;

        Vector3D ab = Sub(b, a);
        Vector3D ao = NegatePoint(a);
        direction = Cross(ab, ao);
        if (Dot(direction, direction) < Tolerance)
        {
            direction = Cross(new Vector3D(ab.Z, ab.Y, ab.X), ab);
            if (Dot(direction, direction) < Tolerance)
                direction = new Vector3D(0, 1, 0);
        }

        for (int iter = 0; iter < MaxIterations; iter++)
        {
            Point3D c = MinkowskiSupport(shapeA, shapeB, direction);

            double cDotDir = c.X * direction.X + c.Y * direction.Y + c.Z * direction.Z;
            if (cDotDir < Tolerance)
                return false;

            Vector3D ao2 = NegatePoint(c);
            Vector3D ac = Sub(a, c);
            Vector3D bc = Sub(b, c);

            direction = Cross(ac, bc);
            if (Dot(direction, direction) < Tolerance)
            {
                direction = Cross(new Vector3D(ac.Z, ac.Y, ac.X), ac);
                if (Dot(direction, direction) < Tolerance)
                    direction = new Vector3D(0, 1, 0);
            }

            if (!SameSide(a, b, c, ao2) || !SameSide(a, c, b, ao2) || !SameSide(b, c, a, ao2))
            {
                a = c;
                direction = NegatePoint(a);
                continue;
            }

            Point3D d = MinkowskiSupport(shapeA, shapeB, direction);

            double dDotDir = d.X * direction.X + d.Y * direction.Y + d.Z * direction.Z;
            if (dDotDir < Tolerance)
                return false;

            double vol = SignedVolume(a, b, c, d);

            if (vol > -Tolerance && vol < Tolerance)
            {
                direction = Cross(Sub(b, a), Sub(c, a));
                if (Dot(direction, direction) < Tolerance)
                    direction = new Vector3D(0, 1, 0);
                continue;
            }

            Vector3D ao3 = NegatePoint(d);
            Point3D origin = new Point3D(0, 0, 0);

            double volA = SignedVolume(b, c, d, origin);
            double volB = SignedVolume(a, c, d, origin);
            double volC = SignedVolume(a, b, d, origin);
            double volD = SignedVolume(a, b, c, origin);

            bool bOutside = volB * vol > 0;
            bool aOutside = volA * vol > 0;
            bool dOutside = volD * vol > 0;
            bool cOutside = volC * vol > 0;

            bool originInTet = bOutside && aOutside && dOutside && cOutside;

            if (originInTet)
                return true;

            if (bOutside && aOutside && dOutside && cOutside)
            {
                direction = Cross(Sub(b, a), Sub(c, a));
                if (Dot(direction, direction) < Tolerance)
                    direction = new Vector3D(0, 1, 0);
                continue;
            }

            if (!bOutside && !cOutside && !dOutside)
            {
                b = a; a = c; c = d;
                direction = Cross(Sub(b, a), Sub(c, a));
                if (Dot(direction, direction) < Tolerance)
                    direction = new Vector3D(0, 1, 0);
                continue;
            }
            if (!aOutside && !cOutside && !dOutside)
            {
                a = b; b = c; c = d;
                direction = Cross(Sub(b, a), Sub(c, a));
                if (Dot(direction, direction) < Tolerance)
                    direction = new Vector3D(0, 1, 0);
                continue;
            }
            if (!bOutside && !aOutside && !dOutside)
            {
                c = d;
                direction = Cross(Sub(b, a), Sub(c, a));
                if (Dot(direction, direction) < Tolerance)
                    direction = new Vector3D(0, 1, 0);
                continue;
            }
            if (!bOutside && !aOutside && !cOutside)
                continue;

            Point3D closestA = a, closestB = b;
            bool foundEdge = false;

            if (!bOutside && !aOutside) { closestA = a; closestB = b; foundEdge = true; }
            else if (!aOutside && !cOutside) { closestA = a; closestB = c; foundEdge = true; }
            else if (!bOutside && !cOutside) { closestA = b; closestB = c; foundEdge = true; }
            else if (!bOutside && !dOutside) { closestA = b; closestB = d; foundEdge = true; }
            else if (!aOutside && !dOutside) { closestA = a; closestB = d; foundEdge = true; }
            else if (!cOutside && !dOutside) { closestA = c; closestB = d; foundEdge = true; }

            if (foundEdge)
            {
                a = closestA; b = closestB;
                direction = Cross(Sub(b, a), NegatePoint(a));
                if (Dot(direction, direction) < Tolerance)
                {
                    Vector3D abDir = Sub(b, a);
                    direction = Cross(new Vector3D(abDir.Z, abDir.Y, abDir.X), abDir);
                    if (Dot(direction, direction) < Tolerance)
                        direction = new Vector3D(0, 1, 0);
                }
                continue;
            }

            a = d;
            direction = NegatePoint(a);
        }

        return true;
    }

    private static Vector3D NegatePoint(Point3D p) => new Vector3D(-p.X, -p.Y, -p.Z);

    private static Point3D ToPoint(Vector3D v) => new Point3D(v.X, v.Y, v.Z);

    private static Vector3D Sub(Point3D a, Point3D b) => new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    private static double Dot(Vector3D a, Vector3D b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    private static Vector3D Cross(Vector3D a, Vector3D b) => new Vector3D(
        a.Y * b.Z - a.Z * b.Y,
        a.Z * b.X - a.X * b.Z,
        a.X * b.Y - a.Y * b.X);

    private static double SignedVolume(Point3D a, Point3D b, Point3D c, Point3D o)
    {
        double adx = a.X - o.X, ady = a.Y - o.Y, adz = a.Z - o.Z;
        double bdx = b.X - o.X, bdy = b.Y - o.Y, bdz = b.Z - o.Z;
        double cdx = c.X - o.X, cdy = c.Y - o.Y, cdz = c.Z - o.Z;
        return adx * (bdy * cdz - bdz * cdy)
             - ady * (bdx * cdz - bdz * cdx)
             + adz * (bdx * cdy - bdy * cdx);
    }

    private static bool SameSide(Point3D p1, Point3D p2, Point3D a, Vector3D o)
    {
        Vector3D e1 = Sub(p2, p1);
        Vector3D e2 = Sub(a, p1);
        Vector3D e3 = new Vector3D(o.X - p1.X, o.Y - p1.Y, o.Z - p1.Z);
        Vector3D n1 = Cross(e1, e2);
        Vector3D n2 = Cross(e1, e3);
        return Dot(n1, n2) >= -Tolerance;
    }

    /// <summary>
    /// Computes the minimum distance between two convex shapes using GJK.
    /// </summary>
    /// <param name="shapeA">The vertices of the first convex shape.</param>
    /// <param name="shapeB">The vertices of the second convex shape.</param>
    /// <returns>The minimum distance between the two shapes.</returns>
    public static double Distance(ImmutableArray<Point3D> shapeA, ImmutableArray<Point3D> shapeB)
    {
        Vector3D direction = new Vector3D(1, 0, 0);
        Point3D simplex = MinkowskiSupport(shapeA, shapeB, direction);

        double bestDist = simplex.X * simplex.X + simplex.Y * simplex.Y + simplex.Z * simplex.Z;
        direction = new Vector3D(-simplex.X, -simplex.Y, -simplex.Z);

        for (int i = 0; i < MaxIterations; i++)
        {
            Point3D a = MinkowskiSupport(shapeA, shapeB, direction);

            double aLenSq = a.X * a.X + a.Y * a.Y + a.Z * a.Z;

            if (aLenSq < Tolerance)
                return 0;

            double dot = a.X * direction.X + a.Y * direction.Y + a.Z * direction.Z;

            if (dot * dot < Tolerance * bestDist)
                return System.Math.Sqrt(bestDist);

            bestDist = System.Math.Min(bestDist, aLenSq);

            direction = new Vector3D(-a.X, -a.Y, -a.Z);
        }

        return System.Math.Sqrt(bestDist);
    }

    /// <summary>
    /// Computes the support point of a shape in a given direction.
    /// </summary>
    /// <param name="shape">The vertices of the convex shape.</param>
    /// <param name="direction">The search direction.</param>
    /// <returns>The point on the shape with the maximum projection along the direction.</returns>
    internal static Point3D Support(ImmutableArray<Point3D> shape, Vector3D direction)
    {
        double maxDot = double.MinValue;
        Point3D bestPoint = shape[0];

        for (int i = 0; i < shape.Length; i++)
        {
            double dot = shape[i].X * direction.X + shape[i].Y * direction.Y + shape[i].Z * direction.Z;

            if (dot > maxDot)
            {
                maxDot = dot;
                bestPoint = shape[i];
            }
        }

        return bestPoint;
    }

    /// <summary>
    /// Computes the support point of the Minkowski difference of two shapes.
    /// </summary>
    /// <param name="a">The vertices of the first shape.</param>
    /// <param name="b">The vertices of the second shape.</param>
    /// <param name="direction">The search direction.</param>
    /// <returns>The support point of A-B in the given direction.</returns>
    private static Point3D MinkowskiSupport(ImmutableArray<Point3D> a, ImmutableArray<Point3D> b, Vector3D direction)
    {
        Point3D supA = Support(a, direction);
        Vector3D negDir = new Vector3D(-direction.X, -direction.Y, -direction.Z);
        Point3D supB = Support(b, negDir);

        return new Point3D(
            supA.X - supB.X,
            supA.Y - supB.Y,
            supA.Z - supB.Z
        );
    }
}
