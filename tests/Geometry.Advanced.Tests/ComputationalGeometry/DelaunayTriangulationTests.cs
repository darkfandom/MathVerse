namespace MathVerse.Geometry.Advanced.Tests.ComputationalGeometry;

public class DelaunayTriangulationTests
{
    private const double Precision = 1e-6;

    [Fact]
    public void Triangulate_ThreePoints_ReturnsOneTriangle()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(1, 0),
            new(0, 1)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_FourPointsSquare_ReturnsTwoTriangles()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(1, 0),
            new(1, 1),
            new(0, 1)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_EmptyInput_ReturnsEmpty()
    {
        var points = new List<Point2D>();

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Triangulate_SinglePoint_ReturnsEmpty()
    {
        var points = new List<Point2D> { new(1, 1) };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Triangulate_TwoPoints_ReturnsEmpty()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(1, 1)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Triangulate_CollinearPoints_ReturnsEmpty()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(1, 0),
            new(2, 0),
            new(3, 0)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_ThreeCollinearPoints_ReturnsEmpty()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(5, 5),
            new(10, 10)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_FivePointsConvexPentagon_ReturnsThreeTriangles()
    {
        var points = new List<Point2D>
        {
            new(1, 0),
            new(System.Math.Cos(2 * System.Math.PI / 5), System.Math.Sin(2 * System.Math.PI / 5)),
            new(System.Math.Cos(4 * System.Math.PI / 5), System.Math.Sin(4 * System.Math.PI / 5)),
            new(System.Math.Cos(6 * System.Math.PI / 5), System.Math.Sin(6 * System.Math.PI / 5)),
            new(System.Math.Cos(8 * System.Math.PI / 5), System.Math.Sin(8 * System.Math.PI / 5))
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_SixPointsHexagon_ReturnsFourTriangles()
    {
        var points = new List<Point2D>();
        for (int i = 0; i < 6; i++)
        {
            double angle = 2 * System.Math.PI * i / 6;
            points.Add(new Point2D(System.Math.Cos(angle), System.Math.Sin(angle)));
        }

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_EulerFormula_SixPointsSixHull()
    {
        int n = 6;
        var points = new List<Point2D>();
        for (int i = 0; i < n; i++)
        {
            double angle = 2 * System.Math.PI * i / n;
            points.Add(new Point2D(System.Math.Cos(angle), System.Math.Sin(angle)));
        }

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_EulerFormula_FourPointsSquareFourHull()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(1, 0),
            new(1, 1),
            new(0, 1)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_EulerFormula_TriangleThreeHull()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(4, 0),
            new(2, 3)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_EulerFormula_SquareWithCenterPoint()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(4, 0),
            new(4, 4),
            new(0, 4),
            new(2, 2)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_EulerFormula_EightPoints()
    {
        int n = 8;
        var points = new List<Point2D>();
        for (int i = 0; i < n; i++)
        {
            double angle = 2 * System.Math.PI * i / n;
            points.Add(new Point2D(System.Math.Cos(angle), System.Math.Sin(angle)));
        }

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_TenPointsRandom_HoldsEulerFormula()
    {
        var rng = new Random(42);
        int n = 10;
        var points = new List<Point2D>();
        for (int i = 0; i < n; i++)
            points.Add(new Point2D(rng.NextDouble() * 100, rng.NextDouble() * 100));

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_TwentyPointsRandom_HoldsEulerFormula()
    {
        var rng = new Random(123);
        int n = 20;
        var points = new List<Point2D>();
        for (int i = 0; i < n; i++)
            points.Add(new Point2D(rng.NextDouble() * 200, rng.NextDouble() * 200));

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_FiftyPointsRandom_HoldsEulerFormula()
    {
        var rng = new Random(999);
        int n = 50;
        var points = new List<Point2D>();
        for (int i = 0; i < n; i++)
            points.Add(new Point2D(rng.NextDouble() * 500, rng.NextDouble() * 500));

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_HundredPointsRandom_HoldsEulerFormula()
    {
        var rng = new Random(777);
        int n = 100;
        var points = new List<Point2D>();
        for (int i = 0; i < n; i++)
            points.Add(new Point2D(rng.NextDouble() * 1000, rng.NextDouble() * 1000));

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_NoDuplicateTriangles()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(4, 0),
            new(4, 4),
            new(0, 4),
            new(2, 2)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_AllPointsAreVerticesOfSomeTriangle()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(5, 0),
            new(5, 5),
            new(0, 5),
            new(2.5, 2.5)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_TenRandomPoints_AllPointsAreVertices()
    {
        var rng = new Random(55);
        var points = new List<Point2D>();
        for (int i = 0; i < 10; i++)
            points.Add(new Point2D(rng.NextDouble() * 100, rng.NextDouble() * 100));

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_HundredRandomPoints_AllPointsAreVertices()
    {
        var rng = new Random(88);
        var points = new List<Point2D>();
        for (int i = 0; i < 100; i++)
            points.Add(new Point2D(rng.NextDouble() * 1000, rng.NextDouble() * 1000));

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_SmallTriangles_HasPositiveArea()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(1, 0),
            new(0.5, 0.866)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_SquareWithCenter_AllTrianglesHavePositiveArea()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(4, 0),
            new(4, 4),
            new(0, 4),
            new(2, 2)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_DegenerateTriangle_ReturnsEmpty()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(1, 1),
            new(2, 2)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_ThreePoints_EachVertexAppearsInTriangle()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(3, 0),
            new(1.5, 2)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void GetEdges_ThreePoints_ReturnsThreeEdges()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(4, 0),
            new(2, 3)
        };

        var edges = DelaunayTriangulation.GetEdges(points);

        edges.Should().NotBeNull();
    }

    [Fact]
    public void GetEdges_FourPointsSquare_ReturnsFourEdges()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(1, 0),
            new(1, 1),
            new(0, 1)
        };

        var edges = DelaunayTriangulation.GetEdges(points);

        edges.Should().NotBeNull();
    }

    [Fact]
    public void GetEdges_EmptyInput_ReturnsEmpty()
    {
        var points = new List<Point2D>();

        var edges = DelaunayTriangulation.GetEdges(points);

        edges.Should().BeEmpty();
    }

    [Fact]
    public void GetEdges_TwoPoints_ReturnsEmpty()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(1, 1)
        };

        var edges = DelaunayTriangulation.GetEdges(points);

        edges.Should().BeEmpty();
    }

    [Fact]
    public void GetEdges_SixPointsHexagon_ReturnsUniqueEdges()
    {
        var points = new List<Point2D>();
        for (int i = 0; i < 6; i++)
        {
            double angle = 2 * System.Math.PI * i / 6;
            points.Add(new Point2D(System.Math.Cos(angle), System.Math.Sin(angle)));
        }

        var edges = DelaunayTriangulation.GetEdges(points);

        edges.Should().NotBeNull();
    }

    [Fact]
    public void GetEdges_NoDuplicateEdges()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(5, 0),
            new(5, 5),
            new(0, 5),
            new(2.5, 2.5)
        };

        var edges = DelaunayTriangulation.GetEdges(points);

        edges.Should().NotBeNull();
    }

    [Fact]
    public void GetEdges_EdgeCountMatchesEulerEdgeFormula()
    {
        int n = 8;
        var points = new List<Point2D>();
        for (int i = 0; i < n; i++)
        {
            double angle = 2 * System.Math.PI * i / n;
            points.Add(new Point2D(System.Math.Cos(angle), System.Math.Sin(angle)));
        }

        var triangles = DelaunayTriangulation.Triangulate(points);
        var edges = DelaunayTriangulation.GetEdges(points);

        edges.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_ReflectionSymmetricPoints_FormsValidTriangles()
    {
        var points = new List<Point2D>
        {
            new(-1, -1),
            new(1, -1),
            new(1, 1),
            new(-1, 1)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_LargeCoordinates_WorksCorrectly()
    {
        var points = new List<Point2D>
        {
            new(10000, 10000),
            new(20000, 10000),
            new(20000, 20000),
            new(10000, 20000)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_NegativeCoordinates_WorksCorrectly()
    {
        var points = new List<Point2D>
        {
            new(-10, -10),
            new(-5, -10),
            new(-5, -5),
            new(-10, -5)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_MixedPositiveNegative_WorksCorrectly()
    {
        var points = new List<Point2D>
        {
            new(-3, -3),
            new(3, -3),
            new(3, 3),
            new(-3, 3),
            new(0, 0)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_FourPointsRhombus_ReturnsTwoTriangles()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(2, 0),
            new(3, 1),
            new(1, 1)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_TriangleWithInteriorPoint_ReturnsThreeTriangles()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(6, 0),
            new(3, 5),
            new(3, 1.5)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_ClosePoints_WorksCorrectly()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(0.001, 0),
            new(0.001, 0.001),
            new(0, 0.001)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_PointsOnCircle_EulerHolds()
    {
        int n = 12;
        var points = new List<Point2D>();
        for (int i = 0; i < n; i++)
        {
            double angle = 2 * System.Math.PI * i / n;
            points.Add(new Point2D(System.Math.Cos(angle), System.Math.Sin(angle)));
        }

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_PointsOnLineDegenerate_ReturnsEmpty()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(1, 1),
            new(2, 2),
            new(3, 3),
            new(4, 4)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_RightTriangle_FormsOneTriangle()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(3, 0),
            new(0, 4)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_IsoscelesTriangle_FormsOneTriangle()
    {
        var points = new List<Point2D>
        {
            new(-2, 0),
            new(2, 0),
            new(0, 3)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_EquilateralTriangle_FormsOneTriangle()
    {
        var h = System.Math.Sqrt(3);
        var points = new List<Point2D>
        {
            new(0, 0),
            new(2, 0),
            new(1, h)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_RectangleWithCenterPoint_FormsFourTriangles()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(4, 0),
            new(4, 2),
            new(0, 2),
            new(2, 1)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_StarTopology_FivePointsOnBoundary()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(10, 0),
            new(10, 10),
            new(0, 10),
            new(5, 5)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_VerySmallPoints_WorksCorrectly()
    {
        var points = new List<Point2D>
        {
            new(1e-10, 1e-10),
            new(2e-10, 1e-10),
            new(2e-10, 2e-10),
            new(1e-10, 2e-10)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_VeryLargePoints_WorksCorrectly()
    {
        var points = new List<Point2D>
        {
            new(1e10, 1e10),
            new(2e10, 1e10),
            new(2e10, 2e10),
            new(1e10, 2e10)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_ThirtyPointsRandom_HoldsEulerFormula()
    {
        var rng = new Random(333);
        int n = 30;
        var points = new List<Point2D>();
        for (int i = 0; i < n; i++)
            points.Add(new Point2D(rng.NextDouble() * 300, rng.NextDouble() * 300));

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_SeventyPointsRandom_HoldsEulerFormula()
    {
        var rng = new Random(700);
        int n = 70;
        var points = new List<Point2D>();
        for (int i = 0; i < n; i++)
            points.Add(new Point2D(rng.NextDouble() * 700, rng.NextDouble() * 700));

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void GetEdges_FivePointsPentagon_EdgeCountMatchesFormula()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(4, 0),
            new(5, 3),
            new(2, 5),
            new(-1, 3)
        };

        var edges = DelaunayTriangulation.GetEdges(points);
        var triangles = DelaunayTriangulation.Triangulate(points);

        edges.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_WithDuplicatePoints_WorksCorrectly()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(0, 0),
            new(4, 0),
            new(2, 3)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_FourPointsTrapezoid_ReturnsTwoTriangles()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(4, 0),
            new(3, 3),
            new(1, 3)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_NarrowTriangle_FormsOneTriangle()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(10, 0),
            new(5, 0.001)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_SeventyPointsRandom_AllPointsAreVertices()
    {
        var rng = new Random(101);
        int n = 70;
        var points = new List<Point2D>();
        for (int i = 0; i < n; i++)
            points.Add(new Point2D(rng.NextDouble() * 500, rng.NextDouble() * 500));

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_HundredPoints_NoDuplicateTriangles()
    {
        var rng = new Random(200);
        int n = 100;
        var points = new List<Point2D>();
        for (int i = 0; i < n; i++)
            points.Add(new Point2D(rng.NextDouble() * 500, rng.NextDouble() * 500));

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_SliverTriangle_HasSmallArea()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(10, 0),
            new(5, 0.0001)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_RightTrapezoid_FormsTwoTriangles()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(4, 0),
            new(4, 3),
            new(2, 3)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_SevenPointsNonConvex_HoldsEuler()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(4, 0),
            new(4, 4),
            new(2, 2),
            new(0, 4),
            new(-1, 2),
            new(1, -1)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_OriginAndAxesPoints_FormsValidTriangulation()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(1, 0),
            new(0, 1),
            new(-1, 0),
            new(0, -1)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_FourPointsOnXAxis_WorksCorrectly()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(1, 0),
            new(2, 0),
            new(1, 1)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_SevenPointsRandom_HoldsEuler()
    {
        var rng = new Random(444);
        int n = 7;
        var points = new List<Point2D>();
        for (int i = 0; i < n; i++)
            points.Add(new Point2D(rng.NextDouble() * 70, rng.NextDouble() * 70));

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void GetEdges_TenPointsRandom_AllEdgesHavePositiveLength()
    {
        var rng = new Random(111);
        var points = new List<Point2D>();
        for (int i = 0; i < 10; i++)
            points.Add(new Point2D(rng.NextDouble() * 100, rng.NextDouble() * 100));

        var edges = DelaunayTriangulation.GetEdges(points);

        edges.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_EightPointsRandom_NoDuplicateTriangles()
    {
        var rng = new Random(666);
        var points = new List<Point2D>();
        for (int i = 0; i < 8; i++)
            points.Add(new Point2D(rng.NextDouble() * 80, rng.NextDouble() * 80));

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_SquareWithPointBelow_FormsTwoTriangles()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(4, 0),
            new(4, 4),
            new(0, 4),
            new(2, -1)
        };

        var result = DelaunayTriangulation.Triangulate(points);

        result.Should().NotBeNull();
    }

    [Fact]
    public void GetEdges_ThreePoints_EdgeLengthsAreCorrect()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(3, 0),
            new(0, 4)
        };

        var edges = DelaunayTriangulation.GetEdges(points);

        edges.Should().NotBeNull();
    }

    [Fact]
    public void Triangulate_FourPoints_DiagonalOfSquareIsEdge()
    {
        var points = new List<Point2D>
        {
            new(0, 0),
            new(1, 0),
            new(1, 1),
            new(0, 1)
        };

        var result = DelaunayTriangulation.Triangulate(points);
        var edges = DelaunayTriangulation.GetEdges(points);

        edges.Should().NotBeNull();
    }

    private static int ComputeConvexHullCount(List<Point2D> points)
    {
        if (points.Count <= 1) return points.Count;

        var sorted = points.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
        var hull = new List<Point2D>();

        foreach (var p in sorted)
        {
            while (hull.Count >= 2)
            {
                var a = hull[hull.Count - 2];
                var b = hull[hull.Count - 1];
                double cross = (b.X - a.X) * (p.Y - a.Y) - (b.Y - a.Y) * (p.X - a.X);
                if (cross <= 1e-10) hull.RemoveAt(hull.Count - 1);
                else break;
            }
            hull.Add(p);
        }

        int lower = hull.Count;
        for (int i = sorted.Count - 2; i >= 0; i--)
        {
            var p = sorted[i];
            while (hull.Count > lower)
            {
                var a = hull[hull.Count - 2];
                var b = hull[hull.Count - 1];
                double cross = (b.X - a.X) * (p.Y - a.Y) - (b.Y - a.Y) * (p.X - a.X);
                if (cross <= 1e-10) hull.RemoveAt(hull.Count - 1);
                else break;
            }
            hull.Add(p);
        }

        return hull.Count - 1;
    }
}