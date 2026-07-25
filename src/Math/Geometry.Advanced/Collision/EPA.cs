using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Collision;

/// <summary>
/// Represents the result of the Expanding Polytope Algorithm (EPA).
/// </summary>
/// <param name="Normal">The collision normal direction.</param>
/// <param name="Distance">The penetration depth.</param>
public readonly record struct EPAResult(Vector3D Normal, double Distance);

/// <summary>
/// Represents a simplex used as input to the EPA algorithm.
/// </summary>
/// <param name="Vertices">The vertices of the simplex.</param>
public readonly record struct Simplex(ImmutableArray<Point3D> Vertices);

/// <summary>
/// Provides the Expanding Polytope Algorithm (EPA) for computing penetration depth and normal.
/// </summary>
public static class EPA
{
    private const double Tolerance = 1e-10;
    private const int MaxIterations = 64;

    /// <summary>
    /// Computes the penetration normal and depth between two intersecting convex shapes.
    /// </summary>
    /// <param name="shapeA">The vertices of the first convex shape.</param>
    /// <param name="shapeB">The vertices of the second convex shape.</param>
    /// <param name="simplex">The simplex from GJK indicating intersection.</param>
    /// <returns>An <see cref="EPAResult"/> if convergence is achieved; otherwise, null.</returns>
    public static EPAResult? Compute(ImmutableArray<Point3D> shapeA, ImmutableArray<Point3D> shapeB, Simplex simplex)
    {
        var polytope = simplex.Vertices.ToBuilder();
        var faces = new List<(int a, int b, int c)>();

        if (polytope.Count >= 4)
        {
            faces.Add((0, 1, 2));
            faces.Add((0, 2, 3));
            faces.Add((0, 3, 1));
            faces.Add((1, 3, 2));
        }
        else
        {
            return null;
        }

        var faceNormals = new List<Vector3D>();
        var faceDistances = new List<double>();

        for (int i = 0; i < faces.Count; i++)
        {
            (int a, int b, int c) = faces[i];
            Vector3D normal = ComputeFaceNormal(polytope[a], polytope[b], polytope[c]);
            double dist = normal.X * polytope[a].X + normal.Y * polytope[a].Y + normal.Z * polytope[a].Z;

            faceNormals.Add(normal);
            faceDistances.Add(dist);
        }

        int closestFace = 0;
        for (int i = 1; i < faceDistances.Count; i++)
        {
            if (faceDistances[i] < faceDistances[closestFace])
            {
                closestFace = i;
            }
        }

        Vector3D bestNormal = faceNormals[closestFace];
        double bestDist = faceDistances[closestFace];

        for (int iter = 0; iter < MaxIterations; iter++)
        {
            Vector3D supportDir = bestNormal;
            Point3D aSup = SupportShape(shapeA, supportDir);
            Point3D bSup = SupportShape(shapeB, new Vector3D(-supportDir.X, -supportDir.Y, -supportDir.Z));
            Point3D newPoint = new Point3D(aSup.X - bSup.X, aSup.Y - bSup.Y, aSup.Z - bSup.Z);

            double newDist = newPoint.X * bestNormal.X + newPoint.Y * bestNormal.Y + newPoint.Z * bestNormal.Z;

            if (newDist - bestDist < Tolerance)
            {
                break;
            }

            polytope.Add(newPoint);
            int newIdx = polytope.Count - 1;

            var newFaces = new List<(int a, int b, int c)>();
            var newNormals = new List<Vector3D>();
            var newDistances = new List<double>();

            for (int i = 0; i < faces.Count; i++)
            {
                (int a, int b, int c) = faces[i];
                Vector3D n = faceNormals[i];

                Vector3D toNew = new Vector3D(
                    polytope[newIdx].X - polytope[a].X,
                    polytope[newIdx].Y - polytope[a].Y,
                    polytope[newIdx].Z - polytope[a].Z
                );

                if (toNew.X * n.X + toNew.Y * n.Y + toNew.Z * n.Z > 0)
                {
                    newFaces.Add((a, b, newIdx));
                    newFaces.Add((b, c, newIdx));
                    newFaces.Add((c, a, newIdx));
                }
                else
                {
                    newFaces.Add((a, b, c));
                }
            }

            faces = newFaces;
            faceNormals.Clear();
            faceDistances.Clear();

            for (int i = 0; i < faces.Count; i++)
            {
                (int a, int b, int c) = faces[i];
                Vector3D n = ComputeFaceNormal(polytope[a], polytope[b], polytope[c]);
                double d = n.X * polytope[a].X + n.Y * polytope[a].Y + n.Z * polytope[a].Z;

                faceNormals.Add(n);
                faceDistances.Add(d);
            }

            closestFace = 0;
            for (int i = 1; i < faceDistances.Count; i++)
            {
                if (faceDistances[i] < faceDistances[closestFace])
                {
                    closestFace = i;
                }
            }

            bestNormal = faceNormals[closestFace];
            bestDist = faceDistances[closestFace];
        }

        double normalLen = System.Math.Sqrt(bestNormal.X * bestNormal.X + bestNormal.Y * bestNormal.Y + bestNormal.Z * bestNormal.Z);

        if (normalLen < Tolerance)
        {
            return null;
        }

        return new EPAResult(
            new Vector3D(bestNormal.X / normalLen, bestNormal.Y / normalLen, bestNormal.Z / normalLen),
            bestDist
        );
    }

    private static Vector3D ComputeFaceNormal(Point3D a, Point3D b, Point3D c)
    {
        Vector3D ab = new Vector3D(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
        Vector3D ac = new Vector3D(c.X - a.X, c.Y - a.Y, c.Z - a.Z);

        Vector3D n = new Vector3D(
            ab.Y * ac.Z - ab.Z * ac.Y,
            ab.Z * ac.X - ab.X * ac.Z,
            ab.X * ac.Y - ab.Y * ac.X
        );

        double len = System.Math.Sqrt(n.X * n.X + n.Y * n.Y + n.Z * n.Z);

        if (len < Tolerance)
        {
            return default;
        }

        return new Vector3D(n.X / len, n.Y / len, n.Z / len);
    }

    private static Point3D SupportShape(ImmutableArray<Point3D> shape, Vector3D direction)
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
}