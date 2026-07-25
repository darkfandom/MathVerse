using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Collision;

/// <summary>
/// Represents the result of the Separating Axis Theorem (SAT) test.
/// </summary>
/// <param name="Colliding">Whether the shapes are colliding.</param>
/// <param name="Normal">The collision normal direction.</param>
/// <param name="Depth">The penetration depth.</param>
public readonly record struct SATResult(bool Colliding, Vector3D Normal, double Depth);

/// <summary>
/// Provides static methods for collision detection using the Separating Axis Theorem (SAT).
/// </summary>
public static class SAT
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Tests two convex polyhedra for intersection using SAT.
    /// </summary>
    /// <param name="shapeA">The vertices of the first convex shape.</param>
    /// <param name="shapeB">The vertices of the second convex shape.</param>
    /// <returns>An <see cref="SATResult"/> indicating collision status and penetration info.</returns>
    public static SATResult Test(ImmutableArray<Point3D> shapeA, ImmutableArray<Point3D> shapeB)
    {
        double minOverlap = double.MaxValue;
        Vector3D minAxis = default;

        Vector3D[] axesA = ComputeFaceNormals(shapeA);
        Vector3D[] axesB = ComputeFaceNormals(shapeB);

        Vector3D[] allAxes = new Vector3D[axesA.Length + axesB.Length];
        axesA.CopyTo(allAxes, 0);
        axesB.CopyTo(allAxes, axesA.Length);

        for (int i = 0; i < allAxes.Length; i++)
        {
            Vector3D axis = allAxes[i];
            double axisLen = System.Math.Sqrt(axis.X * axis.X + axis.Y * axis.Y + axis.Z * axis.Z);

            if (axisLen < Tolerance)
            {
                continue;
            }

            axis = new Vector3D(axis.X / axisLen, axis.Y / axisLen, axis.Z / axisLen);

            (double minA, double maxA) = ProjectShape(shapeA, axis);
            (double minB, double maxB) = ProjectShape(shapeB, axis);

            double overlap = System.Math.Min(maxA, maxB) - System.Math.Max(minA, minB);

            if (overlap < Tolerance)
            {
                return new SATResult(false, default, 0);
            }

            if (overlap < minOverlap)
            {
                minOverlap = overlap;
                minAxis = axis;
            }
        }

        Vector3D d = new Vector3D(
            shapeA[0].X - shapeB[0].X,
            shapeA[0].Y - shapeB[0].Y,
            shapeA[0].Z - shapeB[0].Z
        );

        if (d.X * minAxis.X + d.Y * minAxis.Y + d.Z * minAxis.Z < 0)
        {
            minAxis = new Vector3D(-minAxis.X, -minAxis.Y, -minAxis.Z);
        }

        return new SATResult(true, minAxis, minOverlap);
    }

    /// <summary>
    /// Tests two axis-aligned bounding boxes for intersection using SAT.
    /// </summary>
    /// <param name="a">The first bounding box.</param>
    /// <param name="b">The second bounding box.</param>
    /// <returns>An <see cref="SATResult"/> indicating collision status and penetration info.</returns>
    public static SATResult TestAABBAABB(BoundingBox3D a, BoundingBox3D b)
    {
        Vector3D aCenter = new Vector3D(
            (a.Min.X + a.Max.X) * 0.5,
            (a.Min.Y + a.Max.Y) * 0.5,
            (a.Min.Z + a.Max.Z) * 0.5
        );

        Vector3D bCenter = new Vector3D(
            (b.Min.X + b.Max.X) * 0.5,
            (b.Min.Y + b.Max.Y) * 0.5,
            (b.Min.Z + b.Max.Z) * 0.5
        );

        Vector3D d = new Vector3D(
            bCenter.X - aCenter.X,
            bCenter.Y - aCenter.Y,
            bCenter.Z - aCenter.Z
        );

        Vector3D aHalf = new Vector3D(
            (a.Max.X - a.Min.X) * 0.5,
            (a.Max.Y - a.Min.Y) * 0.5,
            (a.Max.Z - a.Min.Z) * 0.5
        );

        Vector3D bHalf = new Vector3D(
            (b.Max.X - b.Min.X) * 0.5,
            (b.Max.Y - b.Min.Y) * 0.5,
            (b.Max.Z - b.Min.Z) * 0.5
        );

        double overlapX = aHalf.X + bHalf.X - System.Math.Abs(d.X);
        if (overlapX < Tolerance)
        {
            return new SATResult(false, default, 0);
        }

        double overlapY = aHalf.Y + bHalf.Y - System.Math.Abs(d.Y);
        if (overlapY < Tolerance)
        {
            return new SATResult(false, default, 0);
        }

        double overlapZ = aHalf.Z + bHalf.Z - System.Math.Abs(d.Z);
        if (overlapZ < Tolerance)
        {
            return new SATResult(false, default, 0);
        }

        Vector3D normal;
        double depth;

        if (overlapX <= overlapY && overlapX <= overlapZ)
        {
            normal = new Vector3D(d.X < 0 ? -1 : 1, 0, 0);
            depth = overlapX;
        }
        else if (overlapY <= overlapX && overlapY <= overlapZ)
        {
            normal = new Vector3D(0, d.Y < 0 ? -1 : 1, 0);
            depth = overlapY;
        }
        else
        {
            normal = new Vector3D(0, 0, d.Z < 0 ? -1 : 1);
            depth = overlapZ;
        }

        return new SATResult(true, normal, depth);
    }

    private static Vector3D[] ComputeFaceNormals(ImmutableArray<Point3D> shape)
    {
        var normals = new List<Vector3D>();

        for (int i = 0; i < shape.Length - 2; i++)
        {
            for (int j = i + 1; j < shape.Length - 1; j++)
            {
                for (int k = j + 1; k < shape.Length; k++)
                {
                    Vector3D v1 = new Vector3D(shape[j].X - shape[i].X, shape[j].Y - shape[i].Y, shape[j].Z - shape[i].Z);
                    Vector3D v2 = new Vector3D(shape[k].X - shape[i].X, shape[k].Y - shape[i].Y, shape[k].Z - shape[i].Z);

                    Vector3D n = new Vector3D(
                        v1.Y * v2.Z - v1.Z * v2.Y,
                        v1.Z * v2.X - v1.X * v2.Z,
                        v1.X * v2.Y - v1.Y * v2.X
                    );

                    double len = System.Math.Sqrt(n.X * n.X + n.Y * n.Y + n.Z * n.Z);

                    if (len > Tolerance)
                    {
                        normals.Add(new Vector3D(n.X / len, n.Y / len, n.Z / len));
                    }
                }
            }
        }

        return normals.ToArray();
    }

    private static (double min, double max) ProjectShape(ImmutableArray<Point3D> shape, Vector3D axis)
    {
        double min = shape[0].X * axis.X + shape[0].Y * axis.Y + shape[0].Z * axis.Z;
        double max = min;

        for (int i = 1; i < shape.Length; i++)
        {
            double proj = shape[i].X * axis.X + shape[i].Y * axis.Y + shape[i].Z * axis.Z;

            if (proj < min) min = proj;
            if (proj > max) max = proj;
        }

        return (min, max);
    }
}