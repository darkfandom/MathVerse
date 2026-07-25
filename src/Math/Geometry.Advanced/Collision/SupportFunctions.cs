using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Collision;

/// <summary>
/// Provides static support point functions for convex shapes used in GJK and EPA algorithms.
/// </summary>
public static class SupportFunctions
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Computes the support point of a convex shape in a given direction.
    /// </summary>
    /// <param name="shape">The vertices of the convex shape.</param>
    /// <param name="direction">The search direction.</param>
    /// <returns>The point on the shape with the maximum dot product along the direction.</returns>
    public static Point3D Support(ImmutableArray<Point3D> shape, Vector3D direction)
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
    /// Computes the support point of the Minkowski difference of two convex shapes.
    /// </summary>
    /// <param name="a">The vertices of the first shape.</param>
    /// <param name="b">The vertices of the second shape.</param>
    /// <param name="direction">The search direction.</param>
    /// <returns>The support point of A-B in the given direction.</returns>
    public static Point3D MinkowskiSupport(ImmutableArray<Point3D> a, ImmutableArray<Point3D> b, Vector3D direction)
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