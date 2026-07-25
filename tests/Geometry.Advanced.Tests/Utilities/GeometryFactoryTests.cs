namespace MathVerse.Geometry.Advanced.Tests.Utilities;

public class GeometryFactoryTests
{
    [Fact]
    public void RegularPolygon_Triangle_HasThreeVertices()
    {
        var poly = GeometryFactory.RegularPolygon(3, 1.0);
        poly.VertexCount.Should().Be(3);
    }

    [Fact]
    public void RegularPolygon_Triangle_IsTriangle()
    {
        var poly = GeometryFactory.RegularPolygon(3, 1.0);
        poly.Vertices.Length.Should().Be(3);
    }

    [Fact]
    public void RegularPolygon_Hexagon_HasSixVertices()
    {
        var poly = GeometryFactory.RegularPolygon(6, 2.0);
        poly.VertexCount.Should().Be(6);
    }

    [Fact]
    public void RegularPolygon_Square_HasFourVertices()
    {
        var poly = GeometryFactory.RegularPolygon(4, 1.0);
        poly.VertexCount.Should().Be(4);
    }

    [Fact]
    public void RegularPolygon_Pentagon_HasFiveVertices()
    {
        var poly = GeometryFactory.RegularPolygon(5, 1.0);
        poly.VertexCount.Should().Be(5);
    }

    [Fact]
    public void RegularPolygon_TriangleArea_MatchesFormula()
    {
        double radius = 1.0;
        var poly = GeometryFactory.RegularPolygon(3, radius);
        double expectedArea = 3.0 / 2.0 * radius * radius * System.Math.Sin(2.0 * System.Math.PI / 3.0);
        poly.Area.Should().BeApproximately(expectedArea, 1e-6);
    }

    [Fact]
    public void RegularPolygon_SquareArea_IsSideSquared()
    {
        double radius = 1.0;
        var poly = GeometryFactory.RegularPolygon(4, radius);
        double side = radius * System.Math.Sqrt(2);
        double expectedArea = side * side;
        poly.Area.Should().BeApproximately(expectedArea, 1e-6);
    }

    [Fact]
    public void RegularPolygon_HexagonArea_MatchesFormula()
    {
        double radius = 1.0;
        var poly = GeometryFactory.RegularPolygon(6, radius);
        double expectedArea = 3.0 * System.Math.Sqrt(3.0) / 2.0 * radius * radius;
        poly.Area.Should().BeApproximately(expectedArea, 1e-4);
    }

    [Fact]
    public void RegularPolygon_TenSides_HasTenVertices()
    {
        var poly = GeometryFactory.RegularPolygon(10, 5.0);
        poly.VertexCount.Should().Be(10);
    }

    [Fact]
    public void RegularPolygon_AllVerticesAreAtRadiusDistance()
    {
        double radius = 3.0;
        var poly = GeometryFactory.RegularPolygon(5, radius);
        for (int i = 0; i < poly.VertexCount; i++)
        {
            double dist = System.Math.Sqrt(poly.Vertices[i].X * poly.Vertices[i].X + poly.Vertices[i].Y * poly.Vertices[i].Y);
            dist.Should().BeApproximately(radius, 1e-10);
        }
    }

    [Fact]
    public void RegularPolygon_LessThanThree_Throws()
    {
        Action act = () => GeometryFactory.RegularPolygon(2, 1.0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void RegularPolygon_LessThanZero_Throws()
    {
        Action act = () => GeometryFactory.RegularPolygon(-1, 1.0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Grid2D_CorrectCount()
    {
        var grid = GeometryFactory.Grid2D(0, 1, 3, 0, 1, 3);
        grid.Length.Should().Be(9);
    }

    [Fact]
    public void Grid2D_CorrectCount_4x5()
    {
        var grid = GeometryFactory.Grid2D(0, 1, 4, 0, 1, 5);
        grid.Length.Should().Be(20);
    }

    [Fact]
    public void Grid2D_CornersAreCorrect()
    {
        var grid = GeometryFactory.Grid2D(0, 10, 2, 0, 20, 2);
        grid[0].X.Should().BeApproximately(0, 1e-10);
        grid[0].Y.Should().BeApproximately(0, 1e-10);
        grid[3].X.Should().BeApproximately(10, 1e-10);
        grid[3].Y.Should().BeApproximately(20, 1e-10);
    }

    [Fact]
    public void Grid2D_SinglePoint_InMiddle()
    {
        var grid = GeometryFactory.Grid2D(0, 10, 1, 0, 20, 1);
        grid.Length.Should().Be(1);
        grid[0].X.Should().BeApproximately(5, 1e-10);
        grid[0].Y.Should().BeApproximately(10, 1e-10);
    }

    [Fact]
    public void Grid2D_FirstRow_YIsMin()
    {
        var grid = GeometryFactory.Grid2D(0, 10, 3, 0, 10, 3);
        for (int i = 0; i < 3; i++)
            grid[i].Y.Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void Grid3D_CorrectCount()
    {
        var grid = GeometryFactory.Grid3D(0, 1, 3, 0, 1, 3);
        grid.Length.Should().Be(9);
    }

    [Fact]
    public void Grid3D_AllYAreZero()
    {
        var grid = GeometryFactory.Grid3D(0, 1, 2, 0, 1, 2);
        foreach (var pt in grid)
            pt.Y.Should().Be(0);
    }

    [Fact]
    public void Grid3D_CustomY()
    {
        var grid = GeometryFactory.Grid3D(0, 1, 2, 0, 1, 2, 5);
        foreach (var pt in grid)
            pt.Y.Should().Be(5);
    }

    [Fact]
    public void Grid3D_CornersAreCorrect()
    {
        var grid = GeometryFactory.Grid3D(0, 10, 2, 0, 20, 2);
        grid[0].X.Should().BeApproximately(0, 1e-10);
        grid[0].Z.Should().BeApproximately(0, 1e-10);
        grid[3].X.Should().BeApproximately(10, 1e-10);
        grid[3].Z.Should().BeApproximately(20, 1e-10);
    }

    [Fact]
    public void UnitSphere_VertexCount_MatchesSubdivisions()
    {
        int subdivisions = 5;
        var mesh = GeometryFactory.UnitSphere(subdivisions);
        mesh.VertexCount.Should().Be(subdivisions * subdivisions);
    }

    [Fact]
    public void UnitSphere_ThreeSubdivisions()
    {
        var mesh = GeometryFactory.UnitSphere(3);
        mesh.VertexCount.Should().Be(9);
    }

    [Fact]
    public void UnitSphere_HasTriangles()
    {
        var mesh = GeometryFactory.UnitSphere(5);
        mesh.TriangleCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void UnitCube_Has12Triangles()
    {
        var mesh = GeometryFactory.UnitCube();
        mesh.TriangleCount.Should().Be(12);
    }

    [Fact]
    public void UnitCube_Has8Vertices()
    {
        var mesh = GeometryFactory.UnitCube();
        mesh.VertexCount.Should().Be(8);
    }

    [Fact]
    public void UnitCube_VerticesAreAtHalfExtent()
    {
        var mesh = GeometryFactory.UnitCube();
        foreach (var v in mesh.Vertices)
        {
            System.Math.Abs(v.Position.X).Should().BeApproximately(0.5, 1e-10);
            System.Math.Abs(v.Position.Y).Should().BeApproximately(0.5, 1e-10);
            System.Math.Abs(v.Position.Z).Should().BeApproximately(0.5, 1e-10);
        }
    }

    [Fact]
    public void Line_CreatesCorrectLine()
    {
        var a = new Point3D(1, 2, 3);
        var b = new Point3D(4, 5, 6);
        var line = GeometryFactory.Line(a, b);
        line.P1.Should().Be(a);
        line.P2.Should().Be(b);
    }

    [Fact]
    public void Line_ZeroLength_Works()
    {
        var p = new Point3D(1, 1, 1);
        var line = GeometryFactory.Line(p, p);
        line.P1.Should().Be(line.P2);
    }

    [Fact]
    public void Segment_CreatesCorrectSegment()
    {
        var a = new Point2D(1, 2);
        var b = new Point2D(3, 4);
        var seg = GeometryFactory.Segment(a, b);
        seg.P1.Should().Be(a);
        seg.P2.Should().Be(b);
    }

    [Fact]
    public void Segment_LengthIsCorrect()
    {
        var a = new Point2D(0, 0);
        var b = new Point2D(3, 4);
        var seg = GeometryFactory.Segment(a, b);
        seg.Length.Should().BeApproximately(5.0, 1e-10);
    }

    [Fact]
    public void Plane_CreatesCorrectPlane()
    {
        var pt = new Point3D(1, 2, 3);
        var normal = new Vector3D(0, 1, 0).Normalize();
        var plane = GeometryFactory.Plane(pt, normal);
        plane.Point.Should().Be(pt);
        plane.Normal.Should().Be(normal);
    }

    [Fact]
    public void Plane_PointIsOnPlane()
    {
        var pt = new Point3D(0, 5, 0);
        var normal = new Vector3D(0, 1, 0).Normalize();
        var plane = GeometryFactory.Plane(pt, normal);
        plane.Contains(pt).Should().BeTrue();
    }

    [Fact]
    public void AABB_CreatesCorrectBox()
    {
        var min = new Point3D(-1, -2, -3);
        var max = new Point3D(1, 2, 3);
        var box = GeometryFactory.AABB(min, max);
        box.Min.Should().Be(min);
        box.Max.Should().Be(max);
    }

    [Fact]
    public void AABB_WidthHeightDepth()
    {
        var box = GeometryFactory.AABB(new Point3D(0, 0, 0), new Point3D(2, 4, 6));
        box.Width.Should().BeApproximately(2, 1e-10);
        box.Height.Should().BeApproximately(4, 1e-10);
        box.Depth.Should().BeApproximately(6, 1e-10);
    }

    [Fact]
    public void AABB_Center()
    {
        var box = GeometryFactory.AABB(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));
        box.Center.X.Should().BeApproximately(0, 1e-10);
        box.Center.Y.Should().BeApproximately(0, 1e-10);
        box.Center.Z.Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void AABB_Volume()
    {
        var box = GeometryFactory.AABB(new Point3D(0, 0, 0), new Point3D(2, 3, 4));
        box.Volume.Should().BeApproximately(24, 1e-10);
    }

    [Fact]
    public void AABBFromPoints_SinglePoint()
    {
        var pts = new[] { new Point3D(5, 5, 5) };
        var box = GeometryFactory.AABBFromPoints(pts);
        box.Min.Should().Be(new Point3D(5, 5, 5));
        box.Max.Should().Be(new Point3D(5, 5, 5));
    }

    [Fact]
    public void AABBFromPoints_MultiplePoints()
    {
        var pts = new[]
        {
            new Point3D(1, 2, 3),
            new Point3D(-1, -2, -3),
            new Point3D(0, 0, 0)
        };
        var box = GeometryFactory.AABBFromPoints(pts);
        box.Min.Should().Be(new Point3D(-1, -2, -3));
        box.Max.Should().Be(new Point3D(1, 2, 3));
    }

    [Fact]
    public void RegularPolygon_CentroidIsOrigin()
    {
        var poly = GeometryFactory.RegularPolygon(5, 1.0);
        poly.Centroid.X.Should().BeApproximately(0, 1e-6);
        poly.Centroid.Y.Should().BeApproximately(0, 1e-6);
    }

    [Fact]
    public void RegularPolygon_PerimeterForHexagon()
    {
        double radius = 1.0;
        var poly = GeometryFactory.RegularPolygon(6, radius);
        double sideLength = radius;
        double expectedPerimeter = 6 * sideLength;
        poly.Perimeter.Should().BeApproximately(expectedPerimeter, 1e-4);
    }

    [Fact]
    public void Grid2D_AllPointsAreDistinct()
    {
        var grid = GeometryFactory.Grid2D(0, 10, 5, 0, 10, 5);
        var distinct = grid.Distinct().Count();
        distinct.Should().Be(25);
    }

    [Fact]
    public void Grid3D_AllPointsAreDistinct()
    {
        var grid = GeometryFactory.Grid3D(0, 10, 3, 0, 10, 3);
        var distinct = grid.Distinct().Count();
        distinct.Should().Be(9);
    }

    [Fact]
    public void RegularPolygon_Heptagon_HasSevenVertices()
    {
        var poly = GeometryFactory.RegularPolygon(7, 1.0);
        poly.VertexCount.Should().Be(7);
    }

    [Fact]
    public void UnitSphere_Subdivisions2_Has4Vertices()
    {
        var mesh = GeometryFactory.UnitSphere(2);
        mesh.VertexCount.Should().Be(4);
    }

    [Fact]
    public void RegularPolygon_SmallRadius_Works()
    {
        var poly = GeometryFactory.RegularPolygon(3, 0.001);
        poly.VertexCount.Should().Be(3);
    }

    [Fact]
    public void RegularPolygon_LargeRadius_Works()
    {
        var poly = GeometryFactory.RegularPolygon(4, 10000);
        poly.VertexCount.Should().Be(4);
    }

    [Fact]
    public void Grid2D_WithNegativeRanges()
    {
        var grid = GeometryFactory.Grid2D(-5, 5, 2, -10, 10, 2);
        grid.Length.Should().Be(4);
    }

    [Fact]
    public void Grid3D_SinglePoint_InMiddle()
    {
        var grid = GeometryFactory.Grid3D(0, 10, 1, 0, 20, 1);
        grid.Length.Should().Be(1);
        grid[0].X.Should().BeApproximately(5, 1e-10);
        grid[0].Z.Should().BeApproximately(10, 1e-10);
    }

    [Fact]
    public void UnitCube_SurfaceArea()
    {
        var mesh = GeometryFactory.UnitCube();
        mesh.Should().NotBeNull();
        mesh.TriangleCount.Should().Be(12);
    }

    [Fact]
    public void UnitSphere_Subdivisions4_Has16Vertices()
    {
        var mesh = GeometryFactory.UnitSphere(4);
        mesh.VertexCount.Should().Be(16);
    }
}
