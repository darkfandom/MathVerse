namespace MathVerse.Math.Geometry.Advanced.Boolean;

/// <summary>
/// Computes the intersection of two polygons using boolean operations.
/// The intersection is the area where both polygons overlap.
/// </summary>
public static class PolygonIntersection
{
    /// <summary>
    /// Computes the intersection (overlap) of two polygons, returning the shared boundary.
    /// Only the region common to both polygons is included in the result.
    /// </summary>
    /// <param name="polygonA">The first polygon.</param>
    /// <param name="polygonB">The second polygon.</param>
    /// <returns>The intersection polygon boundary as an immutable array of vertices.</returns>
    public static ImmutableArray<Point2D> Compute(ImmutableArray<Point2D> polygonA, ImmutableArray<Point2D> polygonB)
    {
        var result = BooleanOperation.Execute(polygonA, polygonB, BooleanOperationType.Intersection);
        return result.ResultPolygon;
    }
}
