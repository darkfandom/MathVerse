namespace MathVerse.Math.Geometry.Advanced.Boolean;

/// <summary>
/// Computes the difference of two polygons (A minus B) using boolean operations.
/// The difference is the area of polygon A that does not overlap with polygon B.
/// </summary>
public static class PolygonDifference
{
    /// <summary>
    /// Computes the difference of polygon A minus polygon B, returning the remaining boundary.
    /// Removes the area of polygon B from polygon A.
    /// </summary>
    /// <param name="polygonA">The polygon to subtract from.</param>
    /// <param name="polygonB">The polygon to subtract.</param>
    /// <returns>The difference polygon boundary as an immutable array of vertices.</returns>
    public static ImmutableArray<Point2D> Compute(ImmutableArray<Point2D> polygonA, ImmutableArray<Point2D> polygonB)
    {
        var result = BooleanOperation.Execute(polygonA, polygonB, BooleanOperationType.Difference);
        return result.ResultPolygon;
    }
}
