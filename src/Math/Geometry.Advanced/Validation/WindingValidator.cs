using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;

namespace MathVerse.Math.Geometry.Advanced.Validation;

/// <summary>
/// Provides validation and correction of polygon winding order.
/// </summary>
public static class WindingValidator
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Determines the winding order of a 2D polygon using the signed area (shoelace formula).
    /// </summary>
    /// <param name="polygon">The polygon vertices in order.</param>
    /// <returns>The winding order: <see cref="WindingOrder.CounterClockwise"/> if the signed area is positive, otherwise <see cref="WindingOrder.Clockwise"/>.</returns>
    public static WindingOrder DetermineWindingOrder(ImmutableArray<Point2D> polygon)
    {
        double signedArea = 0;
        int n = polygon.Length;

        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            signedArea += polygon[i].X * polygon[j].Y;
            signedArea -= polygon[j].X * polygon[i].Y;
        }

        return signedArea > 0 ? WindingOrder.CounterClockwise : WindingOrder.Clockwise;
    }

    /// <summary>
    /// Checks whether all edges of the polygon have consistent winding direction.
    /// Verifies that the cross product of consecutive edge vectors maintains the same sign.
    /// </summary>
    /// <param name="polygon">The polygon vertices in order.</param>
    /// <returns><c>true</c> if all edges have consistent winding; otherwise, <c>false</c>.</returns>
    public static bool IsConsistentlyWound(ImmutableArray<Point2D> polygon)
    {
        int n = polygon.Length;
        if (n < 3) return true;

        bool hasPositive = false;
        bool hasNegative = false;

        for (int i = 0; i < n; i++)
        {
            int prev = (i - 1 + n) % n;
            int next = (i + 1) % n;

            double cross = (polygon[i].X - polygon[prev].X) * (polygon[next].Y - polygon[i].Y)
                         - (polygon[i].Y - polygon[prev].Y) * (polygon[next].X - polygon[i].X);

            if (cross > Tolerance) hasPositive = true;
            if (cross < -Tolerance) hasNegative = true;

            if (hasPositive && hasNegative)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Ensures the polygon has consistent counter-clockwise winding by reversing
    /// the vertex order if necessary. Modifies the list in place.
    /// </summary>
    /// <param name="polygon">The polygon vertices to check and potentially reverse.</param>
    public static void EnsureConsistentWinding(List<Point2D> polygon)
    {
        if (polygon.Count < 3) return;

        double signedArea = 0;
        int n = polygon.Count;

        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            signedArea += polygon[i].X * polygon[j].Y;
            signedArea -= polygon[j].X * polygon[i].Y;
        }

        if (signedArea < 0)
        {
            polygon.Reverse();
        }
    }
}
