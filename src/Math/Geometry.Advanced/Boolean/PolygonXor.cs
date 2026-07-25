namespace MathVerse.Math.Geometry.Advanced.Boolean;

/// <summary>
/// Computes the exclusive or (symmetric difference) of two polygons using boolean operations.
/// The XOR is the area in either polygon but not in their intersection.
/// </summary>
public static class PolygonXor
{
    /// <summary>
    /// Computes the exclusive or (symmetric difference) of two polygons.
    /// Returns the regions that are in exactly one of the two polygons, excluding any overlap.
    /// </summary>
    /// <param name="polygonA">The first polygon.</param>
    /// <param name="polygonB">The second polygon.</param>
    /// <returns>The XOR polygon boundary as an immutable array of vertices.</returns>
    public static ImmutableArray<Point2D> Compute(ImmutableArray<Point2D> polygonA, ImmutableArray<Point2D> polygonB)
    {
        var result = BooleanOperation.Execute(polygonA, polygonB, BooleanOperationType.Xor);
        return result.ResultPolygon;
    }
}
