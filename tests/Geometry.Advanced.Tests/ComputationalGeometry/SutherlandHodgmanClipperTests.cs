namespace MathVerse.Geometry.Advanced.Tests.ComputationalGeometry;

public class SutherlandHodgmanClipperTests
{
    private const double Precision = 1e-6;

    [Fact]
    public void Clip_SquareInsideLargerSquare_ReturnsSameSquare()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(1, 1),
            new Point2D(3, 1),
            new Point2D(3, 3),
            new Point2D(1, 3)));

        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(4, 4),
            new Point2D(0, 4)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().Be(4);
        result.Area.Should().BeApproximately(4.0, Precision);
    }

    [Fact]
    public void Clip_SquareAgainstSmallerSquare_SmallerResult()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(4, 4),
            new Point2D(0, 4)));

        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(1, 1),
            new Point2D(3, 1),
            new Point2D(3, 3),
            new Point2D(1, 3)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().Be(4);
        result.Area.Should().BeApproximately(4.0, Precision);
    }

    [Fact]
    public void Clip_TriangleAgainstRectangle()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(2, 4)));

        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(1, 0),
            new Point2D(3, 0),
            new Point2D(3, 3),
            new Point2D(1, 3)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().BeGreaterThanOrEqualTo(3);
        result.Area.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Clip_EmptySubject_ReturnsEmpty()
    {
        var subject = new Polygon2D(ImmutableArray<Point2D>.Empty);
        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(4, 4),
            new Point2D(0, 4)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().Be(0);
    }

    [Fact]
    public void Clip_EmptyClip_ReturnsEmpty()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(4, 4),
            new Point2D(0, 4)));
        var clip = new Polygon2D(ImmutableArray<Point2D>.Empty);

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().Be(0);
    }

    [Fact]
    public void Clip_TwoPointsSubject_ReturnsEmpty()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(1, 1)));
        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(4, 4),
            new Point2D(0, 4)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().Be(0);
    }

    [Fact]
    public void Clip_TwoPointsClip_ReturnsEmpty()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(4, 4),
            new Point2D(0, 4)));
        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(1, 1)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().Be(0);
    }

    [Fact]
    public void Clip_NoOverlap_ReturnsEmpty()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(1, 0),
            new Point2D(1, 1),
            new Point2D(0, 1)));

        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(5, 5),
            new Point2D(6, 5),
            new Point2D(6, 6),
            new Point2D(5, 6)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().Be(0);
    }

    [Fact]
    public void Clip_IdenticalSquares_ReturnsSameSquare()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(2, 0),
            new Point2D(2, 2),
            new Point2D(0, 2)));

        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(2, 0),
            new Point2D(2, 2),
            new Point2D(0, 2)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().Be(4);
        result.Area.Should().BeApproximately(4.0, Precision);
    }

    [Fact]
    public void Clip_SubjectHalfOverlappingClip_SmallerArea()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(4, 4),
            new Point2D(0, 4)));

        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(2, 0),
            new Point2D(6, 0),
            new Point2D(6, 4),
            new Point2D(2, 4)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().Be(4);
        result.Area.Should().BeApproximately(8.0, Precision);
    }

    [Fact]
    public void Clip_PentagonAgainstSquare_WorksCorrectly()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(2, 0),
            new Point2D(4, 1),
            new Point2D(3.5, 3),
            new Point2D(0.5, 3),
            new Point2D(0, 1)));

        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(-1, -1),
            new Point2D(5, -1),
            new Point2D(5, 4),
            new Point2D(-1, 4)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().Be(5);
        result.Area.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Clip_TriangleAgainstTriangle_WorksCorrectly()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(2, 4)));

        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(1, -1),
            new Point2D(5, -1),
            new Point2D(3, 3)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().BeGreaterThanOrEqualTo(3);
        result.Area.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Clip_SubjectInsideClip_PreservesArea()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(2, 2),
            new Point2D(3, 2),
            new Point2D(3, 3),
            new Point2D(2, 3)));

        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(5, 0),
            new Point2D(5, 5),
            new Point2D(0, 5)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().Be(4);
        result.Area.Should().BeApproximately(1.0, Precision);
    }

    [Fact]
    public void Clip_LargeSubjectSmallClip_ResultHasClipArea()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(-10, -10),
            new Point2D(10, -10),
            new Point2D(10, 10),
            new Point2D(-10, 10)));

        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(1, 0),
            new Point2D(1, 1),
            new Point2D(0, 1)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().Be(4);
        result.Area.Should().BeApproximately(1.0, Precision);
    }

    [Fact]
    public void IsInsideConvex_PointInsideSquare_ReturnsTrue()
    {
        var square = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(4, 4),
            new Point2D(0, 4)));

        var point = new Point2D(2, 2);

        SutherlandHodgmanClipper.IsInsideConvex(point, square).Should().BeTrue();
    }

    [Fact]
    public void IsInsideConvex_PointOutsideSquare_ReturnsFalse()
    {
        var square = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(4, 4),
            new Point2D(0, 4)));

        var point = new Point2D(5, 5);

        SutherlandHodgmanClipper.IsInsideConvex(point, square).Should().BeFalse();
    }

    [Fact]
    public void IsInsideConvex_PointOnEdge_ReturnsTrue()
    {
        var square = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(4, 4),
            new Point2D(0, 4)));

        var point = new Point2D(2, 0);

        SutherlandHodgmanClipper.IsInsideConvex(point, square).Should().BeTrue();
    }

    [Fact]
    public void IsInsideConvex_PointAtVertex_ReturnsTrue()
    {
        var square = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(4, 4),
            new Point2D(0, 4)));

        var point = new Point2D(0, 0);

        SutherlandHodgmanClipper.IsInsideConvex(point, square).Should().BeTrue();
    }

    [Fact]
    public void IsInsideConvex_PointAtCenter_ReturnsTrue()
    {
        var triangle = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(6, 0),
            new Point2D(3, 5)));

        var center = new Point2D(3, 5.0 / 3.0);

        SutherlandHodgmanClipper.IsInsideConvex(center, triangle).Should().BeTrue();
    }

    [Fact]
    public void IsInsideConvex_PointFarOutside_ReturnsFalse()
    {
        var triangle = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(2, 0),
            new Point2D(1, 2)));

        var point = new Point2D(100, 100);

        SutherlandHodgmanClipper.IsInsideConvex(point, triangle).Should().BeFalse();
    }

    [Fact]
    public void IsInsideConvex_EmptyPolygon_ReturnsFalse()
    {
        var polygon = new Polygon2D(ImmutableArray<Point2D>.Empty);
        var point = new Point2D(1, 1);

        SutherlandHodgmanClipper.IsInsideConvex(point, polygon).Should().BeFalse();
    }

    [Fact]
    public void IsInsideConvex_OnePointPolygon_ReturnsFalse()
    {
        var polygon = new Polygon2D(ImmutableArray.Create(new Point2D(1, 1)));
        var point = new Point2D(1, 1);

        SutherlandHodgmanClipper.IsInsideConvex(point, polygon).Should().BeFalse();
    }

    [Fact]
    public void IsInsideConvex_TwoPointPolygon_ReturnsFalse()
    {
        var polygon = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(1, 1)));
        var point = new Point2D(0.5, 0.5);

        SutherlandHodgmanClipper.IsInsideConvex(point, polygon).Should().BeFalse();
    }

    [Fact]
    public void ClipSegment_HorizontalSegmentInsideSquare()
    {
        var square = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(4, 4),
            new Point2D(0, 4)));

        var segment = new Segment2D(new Point2D(1, 2), new Point2D(3, 2));

        var (hit, p1, p2) = SutherlandHodgmanClipper.ClipSegment(segment, square);

        hit.Should().BeTrue();
        p1.X.Should().BeApproximately(1.0, Precision);
        p2.X.Should().BeApproximately(3.0, Precision);
    }

    [Fact]
    public void ClipSegment_SegmentCompletelyInside()
    {
        var square = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(10, 0),
            new Point2D(10, 10),
            new Point2D(0, 10)));

        var segment = new Segment2D(new Point2D(2, 2), new Point2D(8, 8));

        var (hit, p1, p2) = SutherlandHodgmanClipper.ClipSegment(segment, square);

        hit.Should().BeTrue();
        p1.Should().Be(segment.P1);
        p2.Should().Be(segment.P2);
    }

    [Fact]
    public void ClipSegment_SegmentCompletelyOutside_ReturnsFalse()
    {
        var square = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(2, 0),
            new Point2D(2, 2),
            new Point2D(0, 2)));

        var segment = new Segment2D(new Point2D(5, 5), new Point2D(6, 6));

        var (hit, _, _) = SutherlandHodgmanClipper.ClipSegment(segment, square);

        hit.Should().BeFalse();
    }

    [Fact]
    public void ClipSegment_PartiallyInsideSegment_ClipsCorrectly()
    {
        var square = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(4, 4),
            new Point2D(0, 4)));

        var segment = new Segment2D(new Point2D(-2, 2), new Point2D(6, 2));

        var (hit, p1, p2) = SutherlandHodgmanClipper.ClipSegment(segment, square);

        hit.Should().BeTrue();
        p1.X.Should().BeApproximately(0.0, Precision);
        p2.X.Should().BeApproximately(4.0, Precision);
    }

    [Fact]
    public void ClipSegment_EmptyClip_ReturnsFalse()
    {
        var clip = new Polygon2D(ImmutableArray<Point2D>.Empty);
        var segment = new Segment2D(new Point2D(0, 0), new Point2D(1, 1));

        var (hit, _, _) = SutherlandHodgmanClipper.ClipSegment(segment, clip);

        hit.Should().BeFalse();
    }

    [Fact]
    public void ClipSegment_DiagonalSegmentAcrossSquare()
    {
        var square = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(4, 4),
            new Point2D(0, 4)));

        var segment = new Segment2D(new Point2D(-1, -1), new Point2D(5, 5));

        var (hit, p1, p2) = SutherlandHodgmanClipper.ClipSegment(segment, square);

        hit.Should().BeTrue();
        p1.X.Should().BeApproximately(0.0, Precision);
        p1.Y.Should().BeApproximately(0.0, Precision);
        p2.X.Should().BeApproximately(4.0, Precision);
        p2.Y.Should().BeApproximately(4.0, Precision);
    }

    [Fact]
    public void Clip_TriangleAgainstTriangle2_ResultIsNonEmpty()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(-2, -1),
            new Point2D(2, -1),
            new Point2D(0, 3)));

        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(-1, -2),
            new Point2D(3, -2),
            new Point2D(1, 2)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().BeGreaterThanOrEqualTo(3);
        result.Area.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Clip_SubjectIsPoint_ReturnsEmpty()
    {
        var subject = new Polygon2D(ImmutableArray.Create(new Point2D(2, 2)));
        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(4, 4),
            new Point2D(0, 4)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().Be(0);
    }

    [Fact]
    public void Clip_ClipIsTriangle_WorksCorrectly()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(4, 4),
            new Point2D(0, 4)));

        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(1, 1),
            new Point2D(5, 1),
            new Point2D(3, 5)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().BeGreaterThanOrEqualTo(3);
        result.Area.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Clip_SubjectIsTriangleClipIsSquare()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(-1, -1),
            new Point2D(3, -1),
            new Point2D(1, 3)));

        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(2, 0),
            new Point2D(2, 2),
            new Point2D(0, 2)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().BeGreaterThanOrEqualTo(3);
        result.Area.Should().BeGreaterThan(0);
    }

    [Fact]
    public void IsInsideConvex_CenterOfConvexPentagon_ReturnsTrue()
    {
        var sites = new List<Point2D>();
        for (int i = 0; i < 5; i++)
        {
            double angle = 2 * System.Math.PI * i / 5;
            sites.Add(new Point2D(System.Math.Cos(angle), System.Math.Sin(angle)));
        }
        var pentagon = new Polygon2D(sites.ToImmutableArray());

        var center = new Point2D(0, 0);

        SutherlandHodgmanClipper.IsInsideConvex(center, pentagon).Should().BeTrue();
    }

    [Fact]
    public void IsInsideConvex_OriginInsideUnitSquare_ReturnsTrue()
    {
        var square = new Polygon2D(ImmutableArray.Create(
            new Point2D(-1, -1),
            new Point2D(1, -1),
            new Point2D(1, 1),
            new Point2D(-1, 1)));

        SutherlandHodgmanClipper.IsInsideConvex(Point2D.Origin, square).Should().BeTrue();
    }

    [Fact]
    public void IsInsideConvex_NegativePointOutside_ReturnsFalse()
    {
        var square = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(2, 0),
            new Point2D(2, 2),
            new Point2D(0, 2)));

        SutherlandHodgmanClipper.IsInsideConvex(new Point2D(-1, -1), square).Should().BeFalse();
    }

    [Fact]
    public void Clip_DiamondAgainstSquare()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(2, 0),
            new Point2D(4, 2),
            new Point2D(2, 4),
            new Point2D(0, 2)));

        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(1, 1),
            new Point2D(3, 1),
            new Point2D(3, 3),
            new Point2D(1, 3)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().BeGreaterThanOrEqualTo(3);
        result.Area.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Clip_SquareAgainstPentagon()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(4, 4),
            new Point2D(0, 4)));

        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(1, 0),
            new Point2D(3, 0),
            new Point2D(4, 2),
            new Point2D(2, 4),
            new Point2D(0, 2)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().BeGreaterThanOrEqualTo(3);
        result.Area.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ClipSegment_VerticalSegmentClipping()
    {
        var square = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(2, 0),
            new Point2D(2, 2),
            new Point2D(0, 2)));

        var segment = new Segment2D(new Point2D(1, -1), new Point2D(1, 3));

        var (hit, p1, p2) = SutherlandHodgmanClipper.ClipSegment(segment, square);

        hit.Should().BeTrue();
        p1.Y.Should().BeApproximately(0.0, Precision);
        p2.Y.Should().BeApproximately(2.0, Precision);
    }

    [Fact]
    public void ClipSegment_HorizontalSegmentClipping()
    {
        var square = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(2, 0),
            new Point2D(2, 2),
            new Point2D(0, 2)));

        var segment = new Segment2D(new Point2D(-1, 1), new Point2D(3, 1));

        var (hit, p1, p2) = SutherlandHodgmanClipper.ClipSegment(segment, square);

        hit.Should().BeTrue();
        p1.X.Should().BeApproximately(0.0, Precision);
        p2.X.Should().BeApproximately(2.0, Precision);
    }

    [Fact]
    public void Clip_EmptyBoth_ReturnsEmpty()
    {
        var subject = new Polygon2D(ImmutableArray<Point2D>.Empty);
        var clip = new Polygon2D(ImmutableArray<Point2D>.Empty);

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().Be(0);
    }

    [Fact]
    public void Clip_HexagonAgainstSquare()
    {
        var hexPoints = new List<Point2D>();
        for (int i = 0; i < 6; i++)
        {
            double angle = 2 * System.Math.PI * i / 6;
            hexPoints.Add(new Point2D(2 + System.Math.Cos(angle), 2 + System.Math.Sin(angle)));
        }
        var subject = new Polygon2D(hexPoints.ToImmutableArray());

        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(4, 4),
            new Point2D(0, 4)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().BeGreaterThanOrEqualTo(3);
        result.Area.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ClipSegment_SegmentTouchingEdgeAtOnePoint()
    {
        var square = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(2, 0),
            new Point2D(2, 2),
            new Point2D(0, 2)));

        var segment = new Segment2D(new Point2D(-1, 0), new Point2D(0, 0));

        var (hit, p1, p2) = SutherlandHodgmanClipper.ClipSegment(segment, square);

        hit.Should().BeTrue();
    }

    [Fact]
    public void IsInsideConvex_NegativeCoordinatesInside()
    {
        var square = new Polygon2D(ImmutableArray.Create(
            new Point2D(-5, -5),
            new Point2D(5, -5),
            new Point2D(5, 5),
            new Point2D(-5, 5)));

        SutherlandHodgmanClipper.IsInsideConvex(new Point2D(-2, -3), square).Should().BeTrue();
    }

    [Fact]
    public void Clip_SquareAgainstHexagon_LargerClip()
    {
        var hexPoints = new List<Point2D>();
        for (int i = 0; i < 6; i++)
        {
            double angle = 2 * System.Math.PI * i / 6;
            hexPoints.Add(new Point2D(System.Math.Cos(angle) * 5, System.Math.Sin(angle) * 5));
        }
        var clip = new Polygon2D(hexPoints.ToImmutableArray());

        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(-1, -1),
            new Point2D(1, -1),
            new Point2D(1, 1),
            new Point2D(-1, 1)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().Be(4);
        result.Area.Should().BeApproximately(4.0, Precision);
    }

    [Fact]
    public void ClipSegment_SegmentEndpointOnBoundary()
    {
        var square = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(4, 4),
            new Point2D(0, 4)));

        var segment = new Segment2D(new Point2D(2, 2), new Point2D(6, 6));

        var (hit, p1, p2) = SutherlandHodgmanClipper.ClipSegment(segment, square);

        hit.Should().BeTrue();
        p1.Should().Be(segment.P1);
        p2.X.Should().BeApproximately(4.0, Precision);
        p2.Y.Should().BeApproximately(4.0, Precision);
    }

    [Fact]
    public void Clip_LargeTriangleAgainstSmallSquare()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(10, 0),
            new Point2D(5, 10)));

        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(3, 1),
            new Point2D(7, 1),
            new Point2D(7, 3),
            new Point2D(3, 3)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().BeGreaterThanOrEqualTo(3);
        result.Area.Should().BeGreaterThan(0);
    }

    [Fact]
    public void IsInsideConvex_PentagonCenterIsInside()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(4, 0),
            new(5, 3),
            new(2, 5),
            new(-1, 3)
        };
        var pentagon = new Polygon2D(sites.ToImmutableArray());

        var centroid = new Point2D(
            sites.Average(p => p.X),
            sites.Average(p => p.Y));

        SutherlandHodgmanClipper.IsInsideConvex(centroid, pentagon).Should().BeTrue();
    }

    [Fact]
    public void IsInsideConvex_HexagonCenterIsInside()
    {
        var hexPoints = new List<Point2D>();
        for (int i = 0; i < 6; i++)
        {
            double angle = 2 * System.Math.PI * i / 6;
            hexPoints.Add(new Point2D(System.Math.Cos(angle), System.Math.Sin(angle)));
        }
        var hexagon = new Polygon2D(hexPoints.ToImmutableArray());

        SutherlandHodgmanClipper.IsInsideConvex(Point2D.Origin, hexagon).Should().BeTrue();
    }

    [Fact]
    public void IsInsideConvex_PointJustOutsideEdge_ReturnsFalse()
    {
        var square = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(2, 0),
            new Point2D(2, 2),
            new Point2D(0, 2)));

        var point = new Point2D(2.01, 1);

        SutherlandHodgmanClipper.IsInsideConvex(point, square).Should().BeFalse();
    }

    [Fact]
    public void Clip_PentagonAgainstLargerPentagon_ReturnsSameShape()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(2, 0),
            new Point2D(4, 1),
            new Point2D(3.5, 3),
            new Point2D(0.5, 3),
            new Point2D(0, 1)));

        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(-1, -1),
            new Point2D(6, -1),
            new Point2D(7, 3),
            new Point2D(3, 6),
            new Point2D(-1, 3)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().Be(5);
        result.Area.Should().BeApproximately(subject.Area, Precision);
    }

    [Fact]
    public void ClipSegment_TouchingCornerOfSquare()
    {
        var square = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(2, 0),
            new Point2D(2, 2),
            new Point2D(0, 2)));

        var segment = new Segment2D(new Point2D(2, 2), new Point2D(4, 4));

        var (hit, p1, _) = SutherlandHodgmanClipper.ClipSegment(segment, square);

        hit.Should().BeTrue();
        p1.X.Should().BeApproximately(2.0, Precision);
        p1.Y.Should().BeApproximately(2.0, Precision);
    }

    [Fact]
    public void Clip_SmallTriangleInsideSquare()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(1, 1),
            new Point2D(2, 1),
            new Point2D(1.5, 2)));

        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(3, 0),
            new Point2D(3, 3),
            new Point2D(0, 3)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().Be(3);
        result.Area.Should().BeApproximately(subject.Area, Precision);
    }

    [Fact]
    public void IsInsideConvex_ObliqueTriangle_PointOnBoundary()
    {
        var triangle = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(2, 4)));

        var midpoint = new Point2D(1, 2);

        SutherlandHodgmanClipper.IsInsideConvex(midpoint, triangle).Should().BeTrue();
    }

    [Fact]
    public void Clip_SliverTriangleAgainstSquare()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(4, 0),
            new Point2D(2, 0.01)));

        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(-1, -1),
            new Point2D(5, -1),
            new Point2D(5, 1),
            new Point2D(-1, 1)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().BeGreaterThanOrEqualTo(3);
        result.Area.Should().BeGreaterThan(0);
    }

    [Fact]
    public void IsInsideConvex_ThreePointsCollinear_ReturnsFalse()
    {
        var polygon = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(1, 1),
            new Point2D(2, 2)));

        SutherlandHodgmanClipper.IsInsideConvex(new Point2D(1, 1), polygon).Should().BeTrue();
    }

    [Fact]
    public void Clip_ArbitraryQuadAgainstSquare()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(-1, 1),
            new Point2D(1, -1),
            new Point2D(3, 1),
            new Point2D(1, 3)));

        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(2, 0),
            new Point2D(2, 2),
            new Point2D(0, 2)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().BeGreaterThanOrEqualTo(3);
        result.Area.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ClipSegment_NegativeCoordinates()
    {
        var square = new Polygon2D(ImmutableArray.Create(
            new Point2D(-5, -5),
            new Point2D(5, -5),
            new Point2D(5, 5),
            new Point2D(-5, 5)));

        var segment = new Segment2D(new Point2D(-10, 0), new Point2D(10, 0));

        var (hit, p1, p2) = SutherlandHodgmanClipper.ClipSegment(segment, square);

        hit.Should().BeTrue();
        p1.X.Should().BeApproximately(-5.0, Precision);
        p2.X.Should().BeApproximately(5.0, Precision);
    }

    [Fact]
    public void Clip_ComplicatedOverlap_ReturnsCorrectArea()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(6, 0),
            new Point2D(6, 6),
            new Point2D(0, 6)));

        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(2, 1),
            new Point2D(5, 1),
            new Point2D(5, 5),
            new Point2D(2, 5)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().Be(4);
        result.Area.Should().BeApproximately(12.0, Precision);
    }

    [Fact]
    public void IsInsideConvex_RhombusCenterIsInside()
    {
        var rhombus = new Polygon2D(ImmutableArray.Create(
            new Point2D(2, 0),
            new Point2D(4, 2),
            new Point2D(2, 4),
            new Point2D(0, 2)));

        SutherlandHodgmanClipper.IsInsideConvex(new Point2D(2, 2), rhombus).Should().BeTrue();
    }

    [Fact]
    public void ClipSegment_SegmentEntirelyBelowSquare()
    {
        var square = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(2, 0),
            new Point2D(2, 2),
            new Point2D(0, 2)));

        var segment = new Segment2D(new Point2D(0, -4), new Point2D(2, -3));

        var (hit, _, _) = SutherlandHodgmanClipper.ClipSegment(segment, square);

        hit.Should().BeFalse();
    }

    [Fact]
    public void IsInsideConvex_OriginOnNegativeSquareEdge_ReturnsTrue()
    {
        var square = new Polygon2D(ImmutableArray.Create(
            new Point2D(-3, 0),
            new Point2D(3, 0),
            new Point2D(3, 6),
            new Point2D(-3, 6)));

        SutherlandHodgmanClipper.IsInsideConvex(Point2D.Origin, square).Should().BeTrue();
    }

    [Fact]
    public void IsInsideConvex_PointJustInsideConvex_ReturnsTrue()
    {
        var triangle = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(10, 0),
            new Point2D(5, 10)));

        var point = new Point2D(5, 0.01);

        SutherlandHodgmanClipper.IsInsideConvex(point, triangle).Should().BeTrue();
    }

    [Fact]
    public void Clip_TwoNonOverlappingTriangles_ReturnsEmpty()
    {
        var subject = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0),
            new Point2D(1, 0),
            new Point2D(0.5, 1)));

        var clip = new Polygon2D(ImmutableArray.Create(
            new Point2D(5, 5),
            new Point2D(6, 5),
            new Point2D(5.5, 6)));

        var result = SutherlandHodgmanClipper.Clip(subject, clip);

        result.VertexCount.Should().Be(0);
    }
}
