using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Advanced.PolygonAlgorithms;

namespace MathVerse.Math.Geometry.Advanced.Boolean;

/// <summary>
/// Specifies the type of boolean operation to perform on two polygons.
/// </summary>
public enum BooleanOperationType
{
    /// <summary>Union: the combined area of both polygons.</summary>
    Union,

    /// <summary>Difference: the area of polygon A minus polygon B.</summary>
    Difference,

    /// <summary>Intersection: the overlapping area of both polygons.</summary>
    Intersection,

    /// <summary>Exclusive or: the area in either polygon but not in both.</summary>
    Xor
}

/// <summary>
/// Represents the result of a polygon boolean operation.
/// Contains the outer boundary polygon and any holes formed by the operation.
/// </summary>
/// <param name="ResultPolygon">The outer boundary of the result polygon.</param>
/// <param name="Holes">Any holes (interior voids) within the result polygon.</param>
public readonly record struct BooleanResult(
    ImmutableArray<Point2D> ResultPolygon,
    ImmutableArray<ImmutableArray<Point2D>> Holes);

/// <summary>
/// Provides general polygon boolean operations (union, difference, intersection, XOR)
/// using Weiler-Atherton intersection followed by polygon assembly per operation type.
/// </summary>
public static class BooleanOperation
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Executes a boolean operation on two polygons.
    /// The operation type determines how the overlapping and non-overlapping regions are combined.
    /// </summary>
    /// <param name="polygonA">The first polygon.</param>
    /// <param name="polygonB">The second polygon.</param>
    /// <param name="operation">The boolean operation to perform.</param>
    /// <returns>A <see cref="BooleanResult"/> containing the resulting polygon and any holes.</returns>
    public static BooleanResult Execute(ImmutableArray<Point2D> polygonA, ImmutableArray<Point2D> polygonB, BooleanOperationType operation)
    {
        if (polygonA.Length < 3 || polygonB.Length < 3)
            return new BooleanResult(ImmutableArray<Point2D>.Empty, ImmutableArray<ImmutableArray<Point2D>>.Empty);

        bool aInB = IsPointInPolygon(polygonA[0], polygonB);
        bool bInA = IsPointInPolygon(polygonB[0], polygonA);

        if (aInB && AllPointsInB(polygonA, polygonB))
        {
            return operation switch
            {
                BooleanOperationType.Union => new BooleanResult(polygonB, ImmutableArray<ImmutableArray<Point2D>>.Empty),
                BooleanOperationType.Intersection => new BooleanResult(polygonA, ImmutableArray<ImmutableArray<Point2D>>.Empty),
                BooleanOperationType.Difference => new BooleanResult(ImmutableArray<Point2D>.Empty, ImmutableArray<ImmutableArray<Point2D>>.Empty),
                BooleanOperationType.Xor => new BooleanResult(ImmutableArray<Point2D>.Empty, ImmutableArray<ImmutableArray<Point2D>>.Empty),
                _ => new BooleanResult(ImmutableArray<Point2D>.Empty, ImmutableArray<ImmutableArray<Point2D>>.Empty)
            };
        }

        if (bInA && IsPointInPolygon(polygonB[0], polygonA))
        {
            return operation switch
            {
                BooleanOperationType.Union => new BooleanResult(polygonA, ImmutableArray<ImmutableArray<Point2D>>.Empty),
                BooleanOperationType.Intersection => new BooleanResult(polygonB, ImmutableArray<ImmutableArray<Point2D>>.Empty),
                BooleanOperationType.Difference => new BooleanResult(polygonA, ImmutableArray.Create(polygonB)),
                BooleanOperationType.Xor => new BooleanResult(polygonA, ImmutableArray.Create(polygonB)),
                _ => new BooleanResult(ImmutableArray<Point2D>.Empty, ImmutableArray<ImmutableArray<Point2D>>.Empty)
            };
        }

        if (!PolygonsOverlap(polygonA, polygonB))
        {
            return operation switch
            {
                BooleanOperationType.Union => new BooleanResult(CombinePolygons(polygonA, polygonB), ImmutableArray<ImmutableArray<Point2D>>.Empty),
                BooleanOperationType.Intersection => new BooleanResult(ImmutableArray<Point2D>.Empty, ImmutableArray<ImmutableArray<Point2D>>.Empty),
                BooleanOperationType.Difference => new BooleanResult(polygonA, ImmutableArray<ImmutableArray<Point2D>>.Empty),
                BooleanOperationType.Xor => new BooleanResult(CombinePolygons(polygonA, polygonB), ImmutableArray<ImmutableArray<Point2D>>.Empty),
                _ => new BooleanResult(ImmutableArray<Point2D>.Empty, ImmutableArray<ImmutableArray<Point2D>>.Empty)
            };
        }

        var intersection = PolygonClipper.WeilerAtherton(polygonA, polygonB);

        if (intersection.Length < 3)
        {
            return operation switch
            {
                BooleanOperationType.Union => new BooleanResult(CombinePolygons(polygonA, polygonB), ImmutableArray<ImmutableArray<Point2D>>.Empty),
                BooleanOperationType.Intersection => new BooleanResult(ImmutableArray<Point2D>.Empty, ImmutableArray<ImmutableArray<Point2D>>.Empty),
                BooleanOperationType.Difference => new BooleanResult(polygonA, ImmutableArray<ImmutableArray<Point2D>>.Empty),
                BooleanOperationType.Xor => new BooleanResult(CombinePolygons(polygonA, polygonB), ImmutableArray<ImmutableArray<Point2D>>.Empty),
                _ => new BooleanResult(ImmutableArray<Point2D>.Empty, ImmutableArray<ImmutableArray<Point2D>>.Empty)
            };
        }

        return operation switch
        {
            BooleanOperationType.Union => ComputeUnion(polygonA, polygonB, intersection),
            BooleanOperationType.Intersection => new BooleanResult(intersection, ImmutableArray<ImmutableArray<Point2D>>.Empty),
            BooleanOperationType.Difference => ComputeDifference(polygonA, polygonB, intersection),
            BooleanOperationType.Xor => ComputeXor(polygonA, polygonB, intersection),
            _ => new BooleanResult(ImmutableArray<Point2D>.Empty, ImmutableArray<ImmutableArray<Point2D>>.Empty)
        };
    }

    /// <summary>
    /// Determines whether a point lies inside a polygon using the ray casting algorithm.
    /// Casts a horizontal ray from the point and counts edge crossings.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <param name="polygon">The polygon to test against.</param>
    /// <returns><c>true</c> if the point is inside the polygon; otherwise, <c>false</c>.</returns>
    public static bool IsPointInPolygon(Point2D point, ImmutableArray<Point2D> polygon)
    {
        bool inside = false;
        int n = polygon.Length;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            if ((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y) &&
                point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X)
                inside = !inside;
        }
        return inside;
    }

    private static bool AllPointsInB(ImmutableArray<Point2D> a, ImmutableArray<Point2D> b)
    {
        for (int i = 0; i < a.Length; i++)
        {
            if (!IsPointInPolygon(a[i], b)) return false;
        }
        return true;
    }

    private static bool PolygonsOverlap(ImmutableArray<Point2D> a, ImmutableArray<Point2D> b)
    {
        var boxA = BoundingBox2D.FromPoints(a);
        var boxB = BoundingBox2D.FromPoints(b);
        return boxA.Intersects(boxB);
    }

    /// <summary>
    /// Combines two non-overlapping polygons into a single vertex array.
    /// </summary>
    /// <remarks>
    /// TODO: This is a placeholder that returns an empty result. Full boolean operations
    /// require Weiler-Atherton boundary tracing to produce correct polygon topology
    /// (union boundary, holes, etc.). Concatenating vertex sets does not produce valid polygons.
    /// </remarks>
    private static ImmutableArray<Point2D> CombinePolygons(ImmutableArray<Point2D> a, ImmutableArray<Point2D> b)
    {
        return ImmutableArray<Point2D>.Empty;
    }

    private static BooleanResult ComputeUnion(ImmutableArray<Point2D> a, ImmutableArray<Point2D> b, ImmutableArray<Point2D> intersection)
    {
        var boundary = MergePolygonsSubtractOverlap(a, b, intersection);
        return new BooleanResult(boundary, ImmutableArray<ImmutableArray<Point2D>>.Empty);
    }

    private static BooleanResult ComputeDifference(ImmutableArray<Point2D> a, ImmutableArray<Point2D> b, ImmutableArray<Point2D> intersection)
    {
        var boundary = SubtractOverlap(a, intersection);
        return new BooleanResult(boundary, ImmutableArray<ImmutableArray<Point2D>>.Empty);
    }

    private static BooleanResult ComputeXor(ImmutableArray<Point2D> a, ImmutableArray<Point2D> b, ImmutableArray<Point2D> intersection)
    {
        var aMinusOverlap = SubtractOverlap(a, intersection);
        var bMinusOverlap = SubtractOverlap(b, intersection);
        var holes = new List<ImmutableArray<Point2D>>();

        if (aMinusOverlap.Length >= 3)
            holes.Add(aMinusOverlap);
        if (bMinusOverlap.Length >= 3)
            holes.Add(bMinusOverlap);

        return new BooleanResult(intersection, holes.ToImmutableArray());
    }

    /// <summary>
    /// Merges two overlapping polygons by subtracting the overlap region.
    /// </summary>
    /// <remarks>
    /// TODO: This is a placeholder that returns an empty result. Computing a valid union
    /// boundary requires full Weiler-Atherton boundary tracing with proper edge rewinding
    /// at intersection points. Subtracting vertex sets from polygon interiors does not
    /// produce topologically correct output.
    /// </remarks>
    private static ImmutableArray<Point2D> MergePolygonsSubtractOverlap(ImmutableArray<Point2D> a, ImmutableArray<Point2D> b, ImmutableArray<Point2D> overlap)
    {
        return ImmutableArray<Point2D>.Empty;
    }

    /// <summary>
    /// Subtracts the overlap region from a polygon.
    /// </summary>
    /// <remarks>
    /// TODO: This is a placeholder that returns an empty result. Proper polygon difference
    /// requires tracing the boundary of A outside of B using Weiler-Atherton intersection
    /// points. Removing vertices inside the overlap does not produce a valid polygon.
    /// </remarks>
    private static ImmutableArray<Point2D> SubtractOverlap(ImmutableArray<Point2D> polygon, ImmutableArray<Point2D> overlap)
    {
        return ImmutableArray<Point2D>.Empty;
    }
}
