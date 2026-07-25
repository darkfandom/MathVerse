namespace MathVerse.Math.Geometry.Advanced.Boolean;

/// <summary>
/// Computes the union of two polygons using boolean operations.
/// The union is the combined area covered by either polygon.
/// </summary>
public static class PolygonUnion
{
    /// <summary>
    /// Computes the union of two polygons, returning the merged boundary.
    /// The union combines the areas of both polygons, removing any overlapping regions.
    /// </summary>
    /// <param name="polygonA">The first polygon.</param>
    /// <param name="polygonB">The second polygon.</param>
    /// <returns>The union polygon boundary as an immutable array of vertices.</returns>
    public static ImmutableArray<Point2D> Compute(ImmutableArray<Point2D> polygonA, ImmutableArray<Point2D> polygonB)
    {
        var result = BooleanOperation.Execute(polygonA, polygonB, BooleanOperationType.Union);
        return result.ResultPolygon;
    }
}
