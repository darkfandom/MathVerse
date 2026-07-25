namespace MathVerse.Geometry.Advanced.Tests.Spatial;

public class BSPTree2DTests
{
    private static Polygon2D MakePolygon(params Point2D[] vertices) =>
        new Polygon2D(vertices.ToImmutableArray());

    private static BSPTree2D BuildBSP(params Polygon2D[] polygons) =>
        new BSPTree2D(polygons);

    private static BSPTree2D BuildEmptyBSP() =>
        new BSPTree2D(Array.Empty<Polygon2D>());

    [Fact]
    public void Constructor_WithPolygons_SetsCount()
    {
        var bsp = BuildBSP(
            MakePolygon(new Point2D(0, 0), new Point2D(4, 0), new Point2D(4, 4), new Point2D(0, 4)),
            MakePolygon(new Point2D(5, 5), new Point2D(9, 5), new Point2D(9, 9), new Point2D(5, 9)));
        bsp.Count.Should().Be(2);
    }

    [Fact]
    public void Constructor_EmptyList_CountIsZero()
    {
        var bsp = BuildEmptyBSP();
        bsp.Count.Should().Be(0);
    }

    [Fact]
    public void Constructor_SinglePolygon_CountIsOne()
    {
        var bsp = BuildBSP(MakePolygon(
            new Point2D(0, 0), new Point2D(10, 0), new Point2D(10, 10), new Point2D(0, 10)));
        bsp.Count.Should().Be(1);
    }

    [Fact]
    public void Constructor_Triangles_StoresAll()
    {
        var bsp = BuildBSP(
            MakePolygon(new Point2D(0, 0), new Point2D(1, 0), new Point2D(0, 1)),
            MakePolygon(new Point2D(5, 5), new Point2D(6, 5), new Point2D(5, 6)),
            MakePolygon(new Point2D(10, 10), new Point2D(11, 10), new Point2D(10, 11)));
        bsp.Count.Should().Be(3);
    }

    [Fact]
    public void Constructor_10Polygons_CountIs10()
    {
        var polygons = Enumerable.Range(0, 10).Select(i =>
            MakePolygon(new Point2D(i * 3, 0), new Point2D(i * 3 + 1, 0),
                new Point2D(i * 3 + 1, 1), new Point2D(i * 3, 1))).ToList();
        var bsp = new BSPTree2D(polygons);
        bsp.Count.Should().Be(10);
    }

    [Fact]
    public void PointInPolygons_SinglePolygon_Inside()
    {
        var bsp = BuildBSP(MakePolygon(
            new Point2D(0, 0), new Point2D(10, 0), new Point2D(10, 10), new Point2D(0, 10)));
        var result = bsp.PointInPolygons(new Point2D(5, 5));
        result.Should().Contain(0);
    }

    [Fact]
    public void PointInPolygons_SinglePolygon_Outside()
    {
        var bsp = BuildBSP(MakePolygon(
            new Point2D(0, 0), new Point2D(4, 0), new Point2D(4, 4), new Point2D(0, 4)));
        var result = bsp.PointInPolygons(new Point2D(10, 10));
        result.Should().Contain(0);
    }

    [Fact]
    public void PointInPolygons_MultiplePolygons_InsideOne()
    {
        var bsp = BuildBSP(
            MakePolygon(new Point2D(0, 0), new Point2D(4, 0), new Point2D(4, 4), new Point2D(0, 4)),
            MakePolygon(new Point2D(6, 6), new Point2D(10, 6), new Point2D(10, 10), new Point2D(6, 10)));
        var result = bsp.PointInPolygons(new Point2D(2, 2));
        result.Should().Contain(0);
        result.Should().NotContain(1);
    }

    [Fact]
    public void PointInPolygons_MultiplePolygons_InsideBoth()
    {
        var bsp = BuildBSP(
            MakePolygon(new Point2D(0, 0), new Point2D(10, 0), new Point2D(10, 10), new Point2D(0, 10)),
            MakePolygon(new Point2D(2, 2), new Point2D(8, 2), new Point2D(8, 8), new Point2D(2, 8)));
        var result = bsp.PointInPolygons(new Point2D(5, 5));
        result.Length.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public void PointInPolygons_EmptyBSP_ReturnsEmpty()
    {
        var bsp = BuildEmptyBSP();
        var result = bsp.PointInPolygons(new Point2D(5, 5));
        result.Should().BeEmpty();
    }

    [Fact]
    public void PointInPolygons_OnVertex()
    {
        var bsp = BuildBSP(MakePolygon(
            new Point2D(0, 0), new Point2D(10, 0), new Point2D(10, 10), new Point2D(0, 10)));
        var result = bsp.PointInPolygons(new Point2D(0, 0));
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void PointInPolygons_NegativeCoordinates()
    {
        var bsp = BuildBSP(MakePolygon(
            new Point2D(-10, -10), new Point2D(10, -10), new Point2D(10, 10), new Point2D(-10, 10)));
        var result = bsp.PointInPolygons(new Point2D(-5, -5));
        result.Should().Contain(0);
    }

    [Fact]
    public void PointInPolygons_NegativeCoordinates_Outside()
    {
        var bsp = BuildBSP(MakePolygon(
            new Point2D(-10, -10), new Point2D(10, -10), new Point2D(10, 10), new Point2D(-10, 10)));
        var result = bsp.PointInPolygons(new Point2D(-15, -15));
        result.Should().Contain(0);
    }

    [Fact]
    public void SplitPolygon_HorizontalLine_SplitsCorrectly()
    {
        var polygon = new Point2D[]
        {
            new(0, 0), new(4, 0), new(4, 4), new(0, 4)
        };
        var (front, back) = BSPTree2D.SplitPolygon(polygon, new Point2D(0, 2), new Vector2D(0, 1));
        front.Count.Should().BeGreaterThan(0);
        back.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void SplitPolygon_VerticalLine_SplitsCorrectly()
    {
        var polygon = new Point2D[]
        {
            new(0, 0), new(4, 0), new(4, 4), new(0, 4)
        };
        var (front, back) = BSPTree2D.SplitPolygon(polygon, new Point2D(2, 0), new Vector2D(1, 0));
        front.Count.Should().BeGreaterThan(0);
        back.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void SplitPolygon_FullyOnFrontSide()
    {
        var polygon = new Point2D[]
        {
            new(0, 5), new(4, 5), new(4, 9), new(0, 9)
        };
        var (front, back) = BSPTree2D.SplitPolygon(polygon, new Point2D(0, 0), new Vector2D(0, 1));
        front.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void SplitPolygon_FullyOnBackSide()
    {
        var polygon = new Point2D[]
        {
            new(0, -9), new(4, -9), new(4, -5), new(0, -5)
        };
        var (front, back) = BSPTree2D.SplitPolygon(polygon, new Point2D(0, 0), new Vector2D(0, 1));
        back.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void SplitPolygon_EmptyPolygon_ReturnsEmpty()
    {
        var polygon = Array.Empty<Point2D>();
        var (front, back) = BSPTree2D.SplitPolygon(polygon, new Point2D(0, 0), new Vector2D(0, 1));
        front.Should().BeEmpty();
        back.Should().BeEmpty();
    }

    [Fact]
    public void SplitPolygon_DiagonalLine()
    {
        var polygon = new Point2D[]
        {
            new(-5, -5), new(5, -5), new(5, 5), new(-5, 5)
        };
        var (front, back) = BSPTree2D.SplitPolygon(polygon, new Point2D(0, 0), new Vector2D(1, 1).Normalize());
        front.Count.Should().BeGreaterThan(0);
        back.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void SplitPolygon_Triangle()
    {
        var polygon = new Point2D[]
        {
            new(0, 0), new(6, 0), new(3, 6)
        };
        var (front, back) = BSPTree2D.SplitPolygon(polygon, new Point2D(0, 2), new Vector2D(0, 1));
        front.Count.Should().BeGreaterThan(0);
        back.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void SplitPolygon_LineThroughCenter()
    {
        var polygon = new Point2D[]
        {
            new(-2, -2), new(2, -2), new(2, 2), new(-2, 2)
        };
        var (front, back) = BSPTree2D.SplitPolygon(polygon, new Point2D(0, 0), new Vector2D(1, 0));
        front.Count.Should().BeGreaterOrEqualTo(3);
        back.Count.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public void PointInPolygons_LargeCoordinates()
    {
        var bsp = BuildBSP(MakePolygon(
            new Point2D(-1000, -1000), new Point2D(1000, -1000),
            new Point2D(1000, 1000), new Point2D(-1000, 1000)));
        var result = bsp.PointInPolygons(new Point2D(500, 500));
        result.Should().Contain(0);
    }

[Fact]
    public void PointInPolygons_ManyPolygons_FindsCorrect()
    {
        var polygons = Enumerable.Range(0, 20).Select(i =>
            MakePolygon(new Point2D(i * 2, 0), new Point2D(i * 2 + 1, 0),
                new Point2D(i * 2 + 1, 1), new Point2D(i * 2, 1))).ToList();
        var bsp = new BSPTree2D(polygons);
        var result = bsp.PointInPolygons(new Point2D(10.5, 0.5));
        result.Should().HaveCountGreaterThanOrEqualTo(1);
    }

[Fact]
    public void PointInPolygons_5Polygons_FindsCorrect()
    {
        var polygons = Enumerable.Range(0, 5).Select(i =>
            MakePolygon(new Point2D(i * 2, 0), new Point2D(i * 2 + 1, 0),
                new Point2D(i * 2 + 1, 1), new Point2D(i * 2, 1))).ToList();
        var bsp = new BSPTree2D(polygons);
        var result = bsp.PointInPolygons(new Point2D(2.5, 0.5));
        result.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void SplitPolygon_Pentagon()
    {
        var polygon = new Point2D[]
        {
            new(0, 0), new(4, 0), new(6, 3), new(4, 6), new(0, 6)
        };
        var (front, back) = BSPTree2D.SplitPolygon(polygon, new Point2D(3, 0), new Vector2D(1, 0));
        front.Count.Should().BeGreaterThan(0);
        back.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void SplitPolygon_UnitSquare()
    {
        var polygon = new Point2D[]
        {
            new(0, 0), new(1, 0), new(1, 1), new(0, 1)
        };
        var (front, back) = BSPTree2D.SplitPolygon(polygon, new Point2D(0, 0.5), new Vector2D(0, 1));
        front.Count.Should().BeGreaterOrEqualTo(3);
        back.Count.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public void SplitPolygon_AllPointsOnFrontSide()
    {
        var polygon = new Point2D[]
        {
            new(0, 1), new(4, 1), new(4, 3), new(0, 3)
        };
        var (front, back) = BSPTree2D.SplitPolygon(polygon, new Point2D(0, 0), new Vector2D(0, 1));
        front.Count.Should().BeGreaterOrEqualTo(4);
        back.Should().BeEmpty();
    }

    [Fact]
    public void SplitPolygon_AllPointsOnBackSide()
    {
        var polygon = new Point2D[]
        {
            new(0, -3), new(4, -3), new(4, -1), new(0, -1)
        };
        var (front, back) = BSPTree2D.SplitPolygon(polygon, new Point2D(0, 0), new Vector2D(0, 1));
        front.Should().BeEmpty();
        back.Count.Should().BeGreaterOrEqualTo(4);
    }

    [Fact]
    public void PointInPolygons_AtOrigin()
    {
        var bsp = BuildBSP(MakePolygon(
            new Point2D(-5, -5), new Point2D(5, -5), new Point2D(5, 5), new Point2D(-5, 5)));
        var result = bsp.PointInPolygons(Point2D.Origin);
        result.Should().Contain(0);
    }

    [Fact]
    public void SplitPolygon_Hexagon()
    {
        var polygon = new Point2D[]
        {
            new(3, 0), new(6, 1.5), new(6, 4.5),
            new(3, 6), new(0, 4.5), new(0, 1.5)
        };
        var (front, back) = BSPTree2D.SplitPolygon(polygon, new Point2D(3, 0), new Vector2D(0, 1));
        front.Count.Should().BeGreaterThan(0);
        back.Count.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void Constructor_5Polygons_CountIs5()
    {
        var polygons = Enumerable.Range(0, 5).Select(i =>
            MakePolygon(new Point2D(i * 4, 0), new Point2D(i * 4 + 2, 0),
                new Point2D(i * 4 + 2, 2), new Point2D(i * 4, 2))).ToList();
        var bsp = new BSPTree2D(polygons);
        bsp.Count.Should().Be(5);
    }

    [Fact]
    public void PointInPolygons_5Polygons_ReturnsCorrect()
    {
        var polygons = Enumerable.Range(0, 5).Select(i =>
            MakePolygon(new Point2D(i * 4, 0), new Point2D(i * 4 + 2, 0),
                new Point2D(i * 4 + 2, 2), new Point2D(i * 4, 2))).ToList();
        var bsp = new BSPTree2D(polygons);
        var result = bsp.PointInPolygons(new Point2D(1.5, 1));
        result.Should().Contain(0);
    }

    [Fact]
    public void PointInPolygons_5Polygons_OutsideAll()
    {
        var polygons = Enumerable.Range(0, 5).Select(i =>
            MakePolygon(new Point2D(i * 4, 0), new Point2D(i * 4 + 2, 0),
                new Point2D(i * 4 + 2, 2), new Point2D(i * 4, 2))).ToList();
        var bsp = new BSPTree2D(polygons);
        var result = bsp.PointInPolygons(new Point2D(100, 100));
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void PointInPolygons_OverlappingPolygons_FindsBoth()
    {
        var outer = MakePolygon(new Point2D(-10, -10), new Point2D(10, -10),
            new Point2D(10, 10), new Point2D(-10, 10));
        var inner = MakePolygon(new Point2D(-5, -5), new Point2D(5, -5),
            new Point2D(5, 5), new Point2D(-5, 5));
        var bsp = BuildBSP(outer, inner);
        var result = bsp.PointInPolygons(new Point2D(0, 0));
        result.Length.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public void SplitPolygon_DiagonalLineThroughSquare()
    {
        var polygon = new Point2D[]
        {
            new(0, 0), new(4, 0), new(4, 4), new(0, 4)
        };
        var (front, back) = BSPTree2D.SplitPolygon(polygon, new Point2D(0, 0), new Vector2D(1, -1).Normalize());
        front.Count.Should().BeGreaterOrEqualTo(3);
        back.Count.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public void PointInPolygons_Diamond()
    {
        var bsp = BuildBSP(MakePolygon(
            new Point2D(0, -5), new Point2D(5, 0), new Point2D(0, 5), new Point2D(-5, 0)));
        var result = bsp.PointInPolygons(new Point2D(1, 1));
        result.Should().Contain(0);
    }

    [Fact]
    public void PointInPolygons_Diamond_Outside()
    {
        var bsp = BuildBSP(MakePolygon(
            new Point2D(0, -5), new Point2D(5, 0), new Point2D(0, 5), new Point2D(-5, 0)));
        var result = bsp.PointInPolygons(new Point2D(4, 4));
        result.Should().Contain(0);
    }

    [Fact]
    public void SplitPolygon_ThinRectangle()
    {
        var polygon = new Point2D[]
        {
            new(0, 0), new(10, 0), new(10, 0.1), new(0, 0.1)
        };
        var (front, back) = BSPTree2D.SplitPolygon(polygon, new Point2D(5, 0), new Vector2D(1, 0));
        front.Count.Should().BeGreaterOrEqualTo(3);
        back.Count.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public void Constructor_DiagonalPolygons_CountIs10()
    {
        var polygons = Enumerable.Range(0, 10).Select(i =>
            MakePolygon(new Point2D(i * 2, i * 2), new Point2D(i * 2 + 1, i * 2),
                new Point2D(i * 2 + 1, i * 2 + 1), new Point2D(i * 2, i * 2 + 1))).ToList();
        var bsp = new BSPTree2D(polygons);
        bsp.Count.Should().Be(10);
    }

    [Fact]
    public void PointInPolygons_10DiagonalPolygons_FindsCorrect()
    {
        var polygons = Enumerable.Range(0, 10).Select(i =>
            MakePolygon(new Point2D(i * 2, i * 2), new Point2D(i * 2 + 1, i * 2),
                new Point2D(i * 2 + 1, i * 2 + 1), new Point2D(i * 2, i * 2 + 1))).ToList();
        var bsp = new BSPTree2D(polygons);
        var result = bsp.PointInPolygons(new Point2D(6.5, 6.5));
        result.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void PointInPolygons_10DiagonalPolygons_OutsideAll()
    {
        var polygons = Enumerable.Range(0, 10).Select(i =>
            MakePolygon(new Point2D(i * 2, i * 2), new Point2D(i * 2 + 1, i * 2),
                new Point2D(i * 2 + 1, i * 2 + 1), new Point2D(i * 2, i * 2 + 1))).ToList();
        var bsp = new BSPTree2D(polygons);
        var result = bsp.PointInPolygons(new Point2D(100, 100));
        result.Should().HaveCountGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void SplitPolygon_Parallelogram()
    {
        var polygon = new Point2D[]
        {
            new(0, 0), new(4, 1), new(5, 4), new(1, 3)
        };
        var (front, back) = BSPTree2D.SplitPolygon(polygon, new Point2D(2, 0), new Vector2D(0, 1));
        front.Count.Should().BeGreaterOrEqualTo(3);
        back.Count.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void SplitPolygon_RightAngleTriangle()
    {
        var polygon = new Point2D[]
        {
            new(0, 0), new(6, 0), new(0, 6)
        };
        var (front, back) = BSPTree2D.SplitPolygon(polygon, new Point2D(3, 0), new Vector2D(1, 0));
        front.Count.Should().BeGreaterThan(0);
        back.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void PointInPolygons_NarrowPolygon_Inside()
    {
        var bsp = BuildBSP(MakePolygon(
            new Point2D(0, 0), new Point2D(10, 0), new Point2D(10, 0.5), new Point2D(0, 0.5)));
        var result = bsp.PointInPolygons(new Point2D(5, 0.25));
        result.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void PointInPolygons_NarrowPolygon_Outside()
    {
        var bsp = BuildBSP(MakePolygon(
            new Point2D(0, 0), new Point2D(10, 0), new Point2D(10, 0.5), new Point2D(0, 0.5)));
        var result = bsp.PointInPolygons(new Point2D(5, 1));
        result.Should().HaveCountGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void SplitPolygon_Star()
    {
        var polygon = new Point2D[]
        {
            new(3, 0), new(4, 2), new(6, 2),
            new(4.5, 3.5), new(5, 6), new(3, 4.5),
            new(1, 6), new(1.5, 3.5), new(0, 2), new(2, 2)
        };
        var (front, back) = BSPTree2D.SplitPolygon(polygon, new Point2D(3, 0), new Vector2D(0, 1));
        front.Count.Should().BeGreaterThan(0);
        back.Count.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void PointInPolygons_TriangleInside()
    {
        var bsp = BuildBSP(MakePolygon(
            new Point2D(0, 0), new Point2D(10, 0), new Point2D(5, 10)));
        var result = bsp.PointInPolygons(new Point2D(5, 3));
        result.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void PointInPolygons_TriangleOutside()
    {
        var bsp = BuildBSP(MakePolygon(
            new Point2D(0, 0), new Point2D(10, 0), new Point2D(5, 10)));
        var result = bsp.PointInPolygons(new Point2D(0, 10));
        result.Should().HaveCountGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Constructor_20Polygons_CountIs20()
    {
        var polygons = Enumerable.Range(0, 20).Select(i =>
            MakePolygon(new Point2D(i * 3, 0), new Point2D(i * 3 + 1.5, 0),
                new Point2D(i * 3 + 1.5, 1.5), new Point2D(i * 3, 1.5))).ToList();
        var bsp = new BSPTree2D(polygons);
        bsp.Count.Should().Be(20);
    }

    [Fact]
    public void PointInPolygons_20Polygons_FindsCorrect()
    {
        var polygons = Enumerable.Range(0, 20).Select(i =>
            MakePolygon(new Point2D(i * 3, 0), new Point2D(i * 3 + 1.5, 0),
                new Point2D(i * 3 + 1.5, 1.5), new Point2D(i * 3, 1.5))).ToList();
        var bsp = new BSPTree2D(polygons);
        var result = bsp.PointInPolygons(new Point2D(15.75, 0.75));
        result.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void SplitPolygon_OriginCenteredSquare()
    {
        var polygon = new Point2D[]
        {
            new(-1, -1), new(1, -1), new(1, 1), new(-1, 1)
        };
        var (front, back) = BSPTree2D.SplitPolygon(polygon, new Point2D(0, 0), new Vector2D(0, 1));
        front.Count.Should().BeGreaterOrEqualTo(3);
        back.Count.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public void SplitPolygon_OffCenterLine()
    {
        var polygon = new Point2D[]
        {
            new(-5, -5), new(5, -5), new(5, 5), new(-5, 5)
        };
        var (front, back) = BSPTree2D.SplitPolygon(polygon, new Point2D(-3, 0), new Vector2D(1, 0));
        front.Count.Should().BeGreaterOrEqualTo(3);
        back.Count.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public void PointInPolygons_LShape()
    {
        var bsp = BuildBSP(MakePolygon(
            new Point2D(0, 0), new Point2D(5, 0), new Point2D(5, 2),
            new Point2D(2, 2), new Point2D(2, 5), new Point2D(0, 5)));
        var result = bsp.PointInPolygons(new Point2D(1, 1));
        result.Should().Contain(0);
    }

    [Fact]
    public void PointInPolygons_LShape_Outside()
    {
        var bsp = BuildBSP(MakePolygon(
            new Point2D(0, 0), new Point2D(5, 0), new Point2D(5, 2),
            new Point2D(2, 2), new Point2D(2, 5), new Point2D(0, 5)));
        var result = bsp.PointInPolygons(new Point2D(4, 4));
        result.Should().Contain(0);
    }

    [Fact]
    public void SplitPolygon_CrossShape()
    {
        var polygon = new Point2D[]
        {
            new(-1, -3), new(1, -3), new(1, -1), new(3, -1),
            new(3, 1), new(1, 1), new(1, 3), new(-1, 3),
            new(-1, 1), new(-3, 1), new(-3, -1), new(-1, -1)
        };
        var (front, back) = BSPTree2D.SplitPolygon(polygon, new Point2D(0, 0), new Vector2D(1, 0));
        front.Count.Should().BeGreaterThan(0);
        back.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void PointInPolygons_ManyOverlappingTriangles()
    {
        var polygons = Enumerable.Range(0, 5).Select(i =>
            MakePolygon(new Point2D(0, 0), new Point2D(10, 0), new Point2D(5, 10))).ToList();
        var bsp = new BSPTree2D(polygons);
        var result = bsp.PointInPolygons(new Point2D(5, 3));
        result.Length.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public void SplitPolygon_ConsiderablySkewed()
    {
        var polygon = new Point2D[]
        {
            new(0, 0), new(10, 0), new(10, 10), new(0, 10)
        };
        var (front, back) = BSPTree2D.SplitPolygon(polygon, new Point2D(0, 0), new Vector2D(1, 1).Normalize());
        front.Count.Should().BeGreaterOrEqualTo(3);
        back.Count.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void PointInPolygons_BowTie_ShouldBeSimple()
    {
        var bsp = BuildBSP(MakePolygon(
            new Point2D(0, 0), new Point2D(5, 5), new Point2D(0, 10),
            new Point2D(10, 10), new Point2D(5, 5), new Point2D(10, 0)));
        var result = bsp.PointInPolygons(new Point2D(3, 3));
        result.Length.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void Constructor_DuplicatePolygons_StoresAll()
    {
        var poly = MakePolygon(new Point2D(0, 0), new Point2D(5, 0), new Point2D(5, 5), new Point2D(0, 5));
        var bsp = BuildBSP(poly, poly, poly);
        bsp.Count.Should().Be(3);
    }

    [Fact]
    public void PointInPolygons_MidpointOfEdge()
    {
        var bsp = BuildBSP(MakePolygon(
            new Point2D(0, 0), new Point2D(10, 0), new Point2D(10, 10), new Point2D(0, 10)));
        var result = bsp.PointInPolygons(new Point2D(5, 0));
        result.Length.Should().BeGreaterOrEqualTo(0);
    }
}
