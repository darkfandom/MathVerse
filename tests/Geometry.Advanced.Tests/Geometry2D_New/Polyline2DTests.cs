namespace MathVerse.Geometry.Advanced.Tests.Geometry2D_New;

public class Polyline2DTests
{
    private const double Tolerance = 1e-6;

    [Fact]
    public void Length_SimpleSegment()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(3, 4)));

        polyline.Length.Should().BeApproximately(5.0, Tolerance);
    }

    [Fact]
    public void Length_MultipleSegments()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 0), new Point2D(1, 1)));

        polyline.Length.Should().BeApproximately(2.0, Tolerance);
    }

    [Fact]
    public void Length_TrianglePerimeter()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(3, 0), new Point2D(0, 4)));

        polyline.Length.Should().BeApproximately(8.0, Tolerance);
    }

    [Fact]
    public void Length_SinglePoint_IsZero()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(new Point2D(1, 1)));

        polyline.Length.Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void Centroid_TwoPoints()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(4, 6)));

        polyline.Centroid.X.Should().BeApproximately(2.0, Tolerance);
        polyline.Centroid.Y.Should().BeApproximately(3.0, Tolerance);
    }

    [Fact]
    public void Centroid_ThreePoints()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(3, 0), new Point2D(0, 3)));

        polyline.Centroid.X.Should().BeApproximately(1.0, Tolerance);
        polyline.Centroid.Y.Should().BeApproximately(1.0, Tolerance);
    }

    [Fact]
    public void Centroid_SinglePoint()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(new Point2D(5, 5)));

        polyline.Centroid.Should().Be(new Point2D(5, 5));
    }

    [Fact]
    public void Start_ReturnsFirstVertex()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(1, 2), new Point2D(3, 4)));

        polyline.Start.Should().Be(new Point2D(1, 2));
    }

    [Fact]
    public void End_ReturnsLastVertex()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(1, 2), new Point2D(3, 4)));

        polyline.End.Should().Be(new Point2D(3, 4));
    }

    [Fact]
    public void End_DifferentFromStart_WhenMultipleVertices()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 1), new Point2D(2, 2)));

        polyline.Start.Should().NotBe(polyline.End);
    }

    [Fact]
    public void SegmentCount_TwoVertices_OneSegment()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 1)));

        polyline.SegmentCount.Should().Be(1);
    }

    [Fact]
    public void SegmentCount_FourVertices_ThreeSegments()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 0),
            new Point2D(1, 1), new Point2D(0, 1)));

        polyline.SegmentCount.Should().Be(3);
    }

    [Fact]
    public void SegmentCount_SingleVertex_Zero()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(new Point2D(0, 0)));

        polyline.SegmentCount.Should().Be(0);
    }

    [Fact]
    public void ToBoundingBox_EnclosesAllVertices()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(3, 4), new Point2D(-1, 2)));

        var box = polyline.ToBoundingBox();

        box.Contains(new Point2D(0, 0)).Should().BeTrue();
        box.Contains(new Point2D(3, 4)).Should().BeTrue();
        box.Contains(new Point2D(-1, 2)).Should().BeTrue();
    }

    [Fact]
    public void ToBoundingBox_MinMax()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(1, 2), new Point2D(5, 8)));

        var box = polyline.ToBoundingBox();

        box.Min.Should().Be(new Point2D(1, 2));
        box.Max.Should().Be(new Point2D(5, 8));
    }

    [Fact]
    public void ToPolygon_ClosesTheLoop()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 0), new Point2D(0, 1)));

        var polygon = polyline.ToPolygon();

        polygon.VertexCount.Should().Be(3);
    }

    [Fact]
    public void Reverse_ChangesVertexOrder()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 1), new Point2D(2, 2)));

        var reversed = polyline.Reverse();

        reversed[0].Should().Be(new Point2D(2, 2));
        reversed.Vertices[reversed.Vertices.Length - 1].Should().Be(new Point2D(0, 0));
    }

    [Fact]
    public void Reverse_PreservesLength()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(3, 4), new Point2D(6, 0)));

        var reversed = polyline.Reverse();

        reversed.Length.Should().BeApproximately(polyline.Length, Tolerance);
    }

    [Fact]
    public void Reverse_DoubleReverse_EqualsOriginal()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 2), new Point2D(3, 4)));

        var doubleReversed = polyline.Reverse().Reverse();

        doubleReversed[0].Should().Be(polyline[0]);
        doubleReversed[1].Should().Be(polyline[1]);
        doubleReversed[2].Should().Be(polyline[2]);
    }

    [Fact]
    public void Simplify_CollinearPoints_Simplifies()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 0), new Point2D(2, 0), new Point2D(3, 0)));

        var simplified = polyline.Simplify(0.1);

        simplified.VertexCount.Should().BeLessOrEqualTo(3);
    }

    [Fact]
    public void Simplify_TooFewPoints_ReturnsSame()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 1)));

        var simplified = polyline.Simplify(0.1);

        simplified.VertexCount.Should().Be(2);
    }

    [Fact]
    public void Simplify_ZeroTolerance_PreservesAll()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 0.1), new Point2D(2, 0)));

        var simplified = polyline.Simplify(0.0);

        simplified.VertexCount.Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public void ClosestPoint_OnVertex()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 0), new Point2D(2, 0)));

        var closest = polyline.ClosestPoint(new Point2D(1, 0));

        closest.X.Should().BeApproximately(1.0, Tolerance);
        closest.Y.Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void ClosestPoint_MidSegment()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(4, 0)));

        var closest = polyline.ClosestPoint(new Point2D(2, 10));

        closest.X.Should().BeApproximately(2.0, Tolerance);
        closest.Y.Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void ClosestPoint_BeyondEnd()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 0)));

        var closest = polyline.ClosestPoint(new Point2D(5, 0));

        closest.X.Should().BeApproximately(1.0, Tolerance);
    }

    [Fact]
    public void Segments_CorrectCount()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 0), new Point2D(2, 0)));

        polyline.Segments.Count().Should().Be(2);
    }

    [Fact]
    public void Segments_ContainCorrectEndpoints()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 0), new Point2D(2, 0)));

        var segments = polyline.Segments.ToList();

        segments[0].P1.Should().Be(new Point2D(0, 0));
        segments[0].P2.Should().Be(new Point2D(1, 0));
        segments[1].P1.Should().Be(new Point2D(1, 0));
        segments[1].P2.Should().Be(new Point2D(2, 0));
    }

    [Fact]
    public void Indexer_FirstVertex()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(1, 2), new Point2D(3, 4)));

        polyline[0].Should().Be(new Point2D(1, 2));
    }

    [Fact]
    public void Indexer_LastVertex()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(1, 2), new Point2D(3, 4)));

        polyline[1].Should().Be(new Point2D(3, 4));
    }

    [Fact]
    public void VertexCount_CorrectValue()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 1), new Point2D(2, 2), new Point2D(3, 3)));

        polyline.VertexCount.Should().Be(4);
    }

    [Fact]
    public void ToString_ContainsPolyline2D()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 1)));

        polyline.ToString().Should().Contain("Polyline2D");
    }

    [Fact]
    public void ToString_ContainsVertexCount()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 1), new Point2D(2, 2)));

        polyline.ToString().Should().Contain("3");
    }

    [Fact]
    public void ToString_ContainsLength()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(3, 4)));

        polyline.ToString().Should().Contain("5");
    }

    [Fact]
    public void Length_SquarePath()
    {
        var polyline = new Polyline2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 0), new Point2D(1, 1),
            new Point2D(0, 1), new Point2D(0, 0)));

        polyline.Length.Should().BeApproximately(4.0, Tolerance);
    }
}
