using System.Collections.Immutable;

namespace MathVerse.Geometry.Tests.Geometry2D;

/// <summary>Tests for Geometry2DOperations static class.</summary>
public class Geometry2DOperationsTests
{
    private const double Precision = 1e-10;

    /// <summary>Distance between two points should be Euclidean distance.</summary>
    [Fact]
    public void Distance_PointToPoint_ShouldBeEuclideanDistance()
    {
        var a = new Point2D(0, 0);
        var b = new Point2D(3, 4);
        Geometry2DOperations.Distance(a, b).Should().BeApproximately(5.0, Precision);
    }

    /// <summary>Distance from line to point should be perpendicular distance.</summary>
    [Fact]
    public void Distance_LineToPoint_ShouldBePerpendicularDistance()
    {
        var line = new Line2D(new Point2D(0, 0), new Point2D(1, 0));
        Geometry2DOperations.Distance(line, new Point2D(5, 3)).Should().BeApproximately(3.0, Precision);
    }

    /// <summary>Distance from segment to point should be minimum distance.</summary>
    [Fact]
    public void Distance_SegmentToPoint_ShouldBeMinimumDistance()
    {
        var seg = new Segment2D(new Point2D(0, 0), new Point2D(4, 0));
        Geometry2DOperations.Distance(seg, new Point2D(2, 3)).Should().BeApproximately(3.0, Precision);
    }

    /// <summary>Distance from segment to point beyond endpoint should clamp.</summary>
    [Fact]
    public void Distance_SegmentToPoint_BeyondEndpoint_ShouldClamp()
    {
        var seg = new Segment2D(new Point2D(0, 0), new Point2D(1, 0));
        Geometry2DOperations.Distance(seg, new Point2D(5, 0)).Should().BeApproximately(4.0, Precision);
    }

    /// <summary>Intersect of two crossing lines should return intersection.</summary>
    [Fact]
    public void Intersect_LineLine_Crossing_ShouldReturnIntersection()
    {
        var l1 = new Line2D(new Point2D(0, 0), new Point2D(1, 1));
        var l2 = new Line2D(new Point2D(1, 0), new Point2D(0, 1));
        var (hit, point) = Geometry2DOperations.Intersect(l1, l2);
        hit.Should().BeTrue();
        point.X.Should().BeApproximately(0.5, Precision);
        point.Y.Should().BeApproximately(0.5, Precision);
    }

    /// <summary>Intersect of parallel lines should not hit.</summary>
    [Fact]
    public void Intersect_LineLine_Parallel_ShouldNotHit()
    {
        var l1 = new Line2D(new Point2D(0, 0), new Point2D(1, 0));
        var l2 = new Line2D(new Point2D(0, 1), new Point2D(1, 1));
        var (hit, _) = Geometry2DOperations.Intersect(l1, l2);
        hit.Should().BeFalse();
    }

    /// <summary>Intersect of crossing segments should return intersection.</summary>
    [Fact]
    public void Intersect_SegmentSegment_Crossing_ShouldReturnIntersection()
    {
        var s1 = new Segment2D(new Point2D(0, 0), new Point2D(2, 2));
        var s2 = new Segment2D(new Point2D(2, 0), new Point2D(0, 2));
        var (hit, point) = Geometry2DOperations.Intersect(s1, s2);
        hit.Should().BeTrue();
        point.X.Should().BeApproximately(1.0, Precision);
        point.Y.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>Intersect of non-overlapping segments should not hit.</summary>
    [Fact]
    public void Intersect_SegmentSegment_NonOverlapping_ShouldNotHit()
    {
        var s1 = new Segment2D(new Point2D(0, 0), new Point2D(1, 0));
        var s2 = new Segment2D(new Point2D(0, 1), new Point2D(1, 1));
        var (hit, _) = Geometry2DOperations.Intersect(s1, s2);
        hit.Should().BeFalse();
    }

    /// <summary>Intersect of overlapping circles should give two points.</summary>
    [Fact]
    public void Intersect_CircleCircle_Overlapping_ShouldGiveTwoPoints()
    {
        var c1 = new Circle2D(new Point2D(0, 0), 5);
        var c2 = new Circle2D(new Point2D(5, 0), 5);
        var (hit, points) = Geometry2DOperations.Intersect(c1, c2);
        hit.Should().BeTrue();
        points.Length.Should().Be(2);
    }

    /// <summary>Intersect of separate circles should not hit.</summary>
    [Fact]
    public void Intersect_CircleCircle_Separate_ShouldNotHit()
    {
        var c1 = new Circle2D(new Point2D(0, 0), 2);
        var c2 = new Circle2D(new Point2D(100, 0), 2);
        var (hit, _) = Geometry2DOperations.Intersect(c1, c2);
        hit.Should().BeFalse();
    }

    /// <summary>Intersect of line with circle should give two points.</summary>
    [Fact]
    public void Intersect_LineCircle_Crossing_ShouldGiveTwoPoints()
    {
        var line = new Line2D(new Point2D(-10, 0), new Point2D(10, 0));
        var circle = new Circle2D(new Point2D(0, 0), 5);
        var (hit, points) = Geometry2DOperations.Intersect(line, circle);
        hit.Should().BeTrue();
        points.Length.Should().Be(2);
    }

    /// <summary>Project point onto line should return closest point.</summary>
    [Fact]
    public void Project_PointOntoLine_ShouldReturnClosestPoint()
    {
        var line = new Line2D(new Point2D(0, 0), new Point2D(1, 0));
        Point2D projected = Geometry2DOperations.Project(new Point2D(5, 3), line);
        projected.X.Should().BeApproximately(5.0, Precision);
        projected.Y.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Project point onto segment should clamp to endpoints.</summary>
    [Fact]
    public void Project_PointOntoSegment_ShouldClampToEndpoints()
    {
        var seg = new Segment2D(new Point2D(0, 0), new Point2D(1, 0));
        Point2D projected = Geometry2DOperations.Project(new Point2D(5, 0), seg);
        projected.X.Should().BeApproximately(1.0, Precision);
        projected.Y.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Area of right triangle should be 0.5 * base * height.</summary>
    [Fact]
    public void Area_Triangle_ShouldBeCorrect()
    {
        var tri = new Triangle2D(new Point2D(0, 0), new Point2D(3, 0), new Point2D(0, 4));
        Geometry2DOperations.Area(tri).Should().BeApproximately(6.0, Precision);
    }

    /// <summary>Area of square polygon should be side squared.</summary>
    [Fact]
    public void Area_Polygon_ShouldBeSideSquared()
    {
        var poly = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(4, 0), new Point2D(4, 4), new Point2D(0, 4)));
        Geometry2DOperations.Area(poly).Should().BeApproximately(16.0, Precision);
    }

    /// <summary>Perimeter of polygon should be sum of edge lengths.</summary>
    [Fact]
    public void Perimeter_Polygon_ShouldBeSumOfEdges()
    {
        var poly = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(3, 0), new Point2D(3, 4), new Point2D(0, 4)));
        Geometry2DOperations.Perimeter(poly).Should().BeApproximately(14.0, Precision);
    }

    /// <summary>Centroid of polygon should be at center.</summary>
    [Fact]
    public void Centroid_Polygon_ShouldBeAtCenter()
    {
        var poly = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(4, 0), new Point2D(4, 4), new Point2D(0, 4)));
        Point2D c = Geometry2DOperations.Centroid(poly);
        c.X.Should().BeApproximately(2.0, Precision);
        c.Y.Should().BeApproximately(2.0, Precision);
    }

    /// <summary>ConvexHull of points in a square should produce the four corners.</summary>
    [Fact]
    public void ConvexHull_SquarePoints_ShouldProduceFourCorners()
    {
        var points = new List<Point2D>
        {
            new(0, 0), new(4, 0), new(4, 4), new(0, 4),
            new(2, 2), new(1, 1), new(3, 3)
        };
        Polygon2D hull = Geometry2DOperations.ConvexHull(points);
        hull.VertexCount.Should().Be(4);
    }

    /// <summary>ConvexHull should contain all input points.</summary>
    [Fact]
    public void ConvexHull_ShouldContainAllInputPoints()
    {
        var points = new List<Point2D>
        {
            new(0, 0), new(5, 0), new(5, 5), new(0, 5),
            new(2, 2), new(3, 1)
        };
        Polygon2D hull = Geometry2DOperations.ConvexHull(points);
        hull.VertexCount.Should().Be(4);
        hull.Contains(new Point2D(2.5, 2.5)).Should().BeTrue();
    }

    /// <summary>ConvexHull of collinear points should return endpoints.</summary>
    [Fact]
    public void ConvexHull_CollinearPoints_ShouldReturnEndpoints()
    {
        var points = new List<Point2D>
        {
            new(0, 0), new(1, 1), new(2, 2), new(3, 3)
        };
        Polygon2D hull = Geometry2DOperations.ConvexHull(points);
        hull.VertexCount.Should().Be(2);
    }

    /// <summary>ConvexHull of single point should return that point.</summary>
    [Fact]
    public void ConvexHull_SinglePoint_ShouldReturnSinglePoint()
    {
        var points = new List<Point2D> { new(3, 4) };
        Polygon2D hull = Geometry2DOperations.ConvexHull(points);
        hull.VertexCount.Should().Be(1);
    }
}
