namespace MathVerse.Math.Geometry.Advanced.PolygonAlgorithms;

/// <summary>
/// Provides polygon offsetting using Minkowski sum techniques.
/// Offsets a polygon outward (positive distance) or inward (negative distance)
/// by computing the parallel curve at the specified offset distance.
/// </summary>
public static class PolygonOffsetter
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Offsets a polygon by the specified distance using Minkowski sum computation.
    /// Positive distances expand the polygon outward; negative distances shrink it inward.
    /// At convex vertices, an arc is generated; at reflex vertices, a miter joint is created.
    /// </summary>
    /// <param name="polygon">The polygon vertices in winding order.</param>
    /// <param name="distance">The offset distance. Positive for outward, negative for inward.</param>
    /// <returns>The offset polygon vertices.</returns>
    public static ImmutableArray<Point2D> Offset(ImmutableArray<Point2D> polygon, double distance)
    {
        int n = polygon.Length;
        if (n < 3 || System.Math.Abs(distance) < Tolerance)
            return polygon;

        bool isCCW = IsCounterClockwise(polygon);
        double sign = isCCW ? 1.0 : -1.0;
        double effectiveDistance = distance * sign;

        var result = ImmutableArray.CreateBuilder<Point2D>();

        for (int i = 0; i < n; i++)
        {
            int prev = (i + n - 1) % n;
            int next = (i + 1) % n;

            Point2D pA = polygon[prev];
            Point2D pB = polygon[i];
            Point2D pC = polygon[next];

            Vector2D edgeIn = new(pB.X - pA.X, pB.Y - pA.Y);
            Vector2D edgeOut = new(pC.X - pB.X, pC.Y - pB.Y);

            double lenIn = edgeIn.Length;
            double lenOut = edgeOut.Length;
            if (lenIn < Tolerance || lenOut < Tolerance) continue;

            Vector2D normalIn = new Vector2D(-edgeIn.Y / lenIn, edgeIn.X / lenIn).Scale(effectiveDistance);
            Vector2D normalOut = new Vector2D(-edgeOut.Y / lenOut, edgeOut.X / lenOut).Scale(effectiveDistance);

            Point2D offsetA = new(pB.X + normalIn.X, pB.Y + normalIn.Y);
            Point2D offsetB = new(pB.X + normalOut.X, pB.Y + normalOut.Y);

            double cross = edgeIn.X * edgeOut.Y - edgeIn.Y * edgeOut.X;
            bool isConvexVertex = cross * sign > 0;

            if (isConvexVertex)
            {
                double angleIn = System.Math.Atan2(-edgeIn.Y, -edgeIn.X);
                double angleOut = System.Math.Atan2(edgeOut.Y, edgeOut.X);

                double startAngle = angleIn;
                double endAngle = angleOut;

                double diff = endAngle - startAngle;
                if (sign > 0)
                {
                    while (diff < 0) diff += 2.0 * System.Math.PI;
                    while (diff > 2.0 * System.Math.PI) diff -= 2.0 * System.Math.PI;
                }
                else
                {
                    while (diff > 0) diff -= 2.0 * System.Math.PI;
                    while (diff < -2.0 * System.Math.PI) diff += 2.0 * System.Math.PI;
                }

                double offsetLen = System.Math.Abs(effectiveDistance);
                int arcSteps = System.Math.Max(1, (int)(System.Math.Abs(diff) / (System.Math.PI / 4.0)));

                result.Add(offsetA);

                for (int s = 1; s < arcSteps; s++)
                {
                    double t = (double)s / arcSteps;
                    double angle = startAngle + diff * t;
                    Point2D arcPoint = new(
                        pB.X + System.Math.Cos(angle) * offsetLen,
                        pB.Y + System.Math.Sin(angle) * offsetLen);
                    result.Add(arcPoint);
                }

                result.Add(offsetB);
            }
            else
            {
                Point2D intersection = ComputeIntersection(
                    pA, pB, pC,
                    effectiveDistance, isCCW);

                if (intersection.DistanceTo(Point2D.Origin) > Tolerance ||
                    pB.DistanceTo(Point2D.Origin) > Tolerance)
                {
                    result.Add(intersection);
                }
            }
        }

        return result.ToImmutable();
    }

    /// <summary>
    /// Computes the miter intersection point for a reflex vertex offset.
    /// The miter point is where the offset lines of the two adjacent edges meet.
    /// </summary>
    /// <param name="pA">The previous vertex.</param>
    /// <param name="pB">The current (reflex) vertex.</param>
    /// <param name="pC">The next vertex.</param>
    /// <param name="distance">The offset distance.</param>
    /// <param name="isCCW">Whether the polygon is counter-clockwise.</param>
    /// <returns>The miter intersection point.</returns>
    internal static Point2D ComputeIntersection(Point2D pA, Point2D pB, Point2D pC, double distance, bool isCCW)
    {
        Vector2D edgeAB = new(pB.X - pA.X, pB.Y - pA.Y);
        Vector2D edgeBC = new(pC.X - pB.X, pC.Y - pB.Y);

        double lenAB = edgeAB.Length;
        double lenBC = edgeBC.Length;

        if (lenAB < Tolerance || lenBC < Tolerance) return pB;

        Vector2D normalAB = new Vector2D(-edgeAB.Y / lenAB, edgeAB.X / lenAB);
        Vector2D normalBC = new Vector2D(-edgeBC.Y / lenBC, edgeBC.X / lenBC);

        Vector2D bisector = new(normalAB.X + normalBC.X, normalAB.Y + normalBC.Y);
        double bisectorLen = bisector.Length;

        if (bisectorLen < Tolerance)
        {
            Vector2D fallbackNormal = isCCW ? normalAB : normalAB.Negate();
            return new Point2D(pB.X + fallbackNormal.X * distance, pB.Y + fallbackNormal.Y * distance);
        }

        double halfAngle = System.Math.Atan2(
            normalAB.X * normalBC.Y - normalAB.Y * normalBC.X,
            normalAB.X * normalBC.X + normalAB.Y * normalBC.Y) * 0.5;

        double miterLen = System.Math.Abs(distance) / System.Math.Cos(halfAngle);
        if (System.Math.Abs(halfAngle) > System.Math.PI / 3.0)
            miterLen = System.Math.Abs(distance) * 2.0;

        bisector = bisector.Scale(1.0 / bisectorLen);
        double sign = isCCW ? 1.0 : -1.0;

        return new Point2D(
            pB.X + bisector.X * miterLen * sign,
            pB.Y + bisector.Y * miterLen * sign);
    }

    private static bool IsCounterClockwise(ImmutableArray<Point2D> polygon)
    {
        double signedArea = 0;
        int n = polygon.Length;
        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            signedArea += polygon[i].X * polygon[j].Y;
            signedArea -= polygon[j].X * polygon[i].Y;
        }
        return signedArea > 0;
    }
}
