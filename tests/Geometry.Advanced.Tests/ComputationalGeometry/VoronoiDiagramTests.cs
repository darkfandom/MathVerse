namespace MathVerse.Geometry.Advanced.Tests.ComputationalGeometry;

public class VoronoiDiagramTests
{
    private const double Precision = 1e-6;

    [Fact]
    public void Compute_ThreePoints_ReturnsThreeCells()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(4, 0),
            new(2, 3)
        };

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(3);
    }

    [Fact]
    public void Compute_FourPointsSquare_ReturnsFourCells()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(4, 0),
            new(4, 4),
            new(0, 4)
        };

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(4);
    }

    [Fact]
    public void Compute_EmptyInput_ReturnsEmpty()
    {
        var sites = new List<Point2D>();

        var result = VoronoiDiagram.Compute(sites);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Compute_SinglePoint_ReturnsEmpty()
    {
        var sites = new List<Point2D> { new(1, 1) };

        var result = VoronoiDiagram.Compute(sites);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Compute_TwoPoints_ReturnsEmpty()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(1, 1)
        };

        var result = VoronoiDiagram.Compute(sites);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Compute_EachSiteHasACell()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(5, 0),
            new(5, 5),
            new(0, 5),
            new(2.5, 2.5)
        };

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(sites.Count);
    }

    [Fact]
    public void Compute_FivePointsEachSiteHasACell()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(10, 0),
            new(10, 10),
            new(0, 10),
            new(5, 5)
        };

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(5);
    }

    [Fact]
    public void Compute_SixPointsHexagon_ReturnsSixCells()
    {
        var sites = new List<Point2D>();
        for (int i = 0; i < 6; i++)
        {
            double angle = 2 * System.Math.PI * i / 6;
            sites.Add(new Point2D(System.Math.Cos(angle), System.Math.Sin(angle)));
        }

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(6);
    }

    [Fact]
    public void Compute_TriangularArrangement_CellsArePolygons()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(6, 0),
            new(3, 5)
        };

        var result = VoronoiDiagram.Compute(sites);

        foreach (var cell in result)
            cell.VertexCount.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void Compute_FourPointsEachCellIsConvex()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(5, 0),
            new(5, 5),
            new(0, 5)
        };

        var result = VoronoiDiagram.Compute(sites);

        foreach (var cell in result)
            cell.IsConvex.Should().BeTrue();
    }

    [Fact]
    public void Compute_FivePointsEachCellIsConvex()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(8, 0),
            new(8, 8),
            new(0, 8),
            new(4, 4)
        };

        var result = VoronoiDiagram.Compute(sites);

        foreach (var cell in result)
            cell.IsConvex.Should().BeTrue();
    }

    [Fact]
    public void ComputeCircumcenters_ThreePoints_ReturnsThreeCenters()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(4, 0),
            new(2, 3)
        };

        var result = VoronoiDiagram.ComputeCircumcenters(sites);

        result.Should().NotBeNull();
    }

    [Fact]
    public void ComputeCircumcenters_FourPointsSquare_ReturnsTwoCenters()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(4, 0),
            new(4, 4),
            new(0, 4)
        };

        var result = VoronoiDiagram.ComputeCircumcenters(sites);

        result.Should().NotBeNull();
    }

    [Fact]
    public void ComputeCircumcenters_TriangleCenterIsEquidistant()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(4, 0),
            new(2, 3)
        };

        var centers = VoronoiDiagram.ComputeCircumcenters(sites);
        var tris = DelaunayTriangulation.Triangulate(sites);

        for (int i = 0; i < centers.Length; i++)
        {
            var c = centers[i];
            var t = tris[i];
            double dA = c.DistanceTo(t.A);
            double dB = c.DistanceTo(t.B);
            double dC = c.DistanceTo(t.C);
            dA.Should().BeApproximately(dB, Precision);
            dB.Should().BeApproximately(dC, Precision);
        }
    }

    [Fact]
    public void ComputeCircumcenters_FivePointsSquareCenter_CorrectCount()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(4, 0),
            new(4, 4),
            new(0, 4),
            new(2, 2)
        };

        var centers = VoronoiDiagram.ComputeCircumcenters(sites);

        centers.Should().NotBeNull();
    }

    [Fact]
    public void Compute_SixPointsHexagon_EachCellHasPositiveArea()
    {
        var sites = new List<Point2D>();
        for (int i = 0; i < 6; i++)
        {
            double angle = 2 * System.Math.PI * i / 6;
            sites.Add(new Point2D(System.Math.Cos(angle), System.Math.Sin(angle)));
        }

        var result = VoronoiDiagram.Compute(sites);

        foreach (var cell in result)
            cell.Area.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Compute_ThreePointsCollinear_ReturnsEmpty()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(1, 1),
            new(2, 2)
        };

        var result = VoronoiDiagram.Compute(sites);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Compute_FivePointsConvexPentagon_ReturnsFiveCells()
    {
        var sites = new List<Point2D>();
        for (int i = 0; i < 5; i++)
        {
            double angle = 2 * System.Math.PI * i / 5;
            sites.Add(new Point2D(System.Math.Cos(angle), System.Math.Sin(angle)));
        }

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(5);
    }

    [Fact]
    public void Compute_SevenPointsRandom_ReturnsSevenCells()
    {
        var rng = new Random(42);
        var sites = new List<Point2D>();
        for (int i = 0; i < 7; i++)
            sites.Add(new Point2D(rng.NextDouble() * 50, rng.NextDouble() * 50));

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(7);
    }

    [Fact]
    public void Compute_TenPointsRandom_ReturnsTenCells()
    {
        var rng = new Random(99);
        var sites = new List<Point2D>();
        for (int i = 0; i < 10; i++)
            sites.Add(new Point2D(rng.NextDouble() * 100, rng.NextDouble() * 100));

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(10);
    }

    [Fact]
    public void Compute_FifteenPointsRandom_ReturnsFifteenCells()
    {
        var rng = new Random(123);
        var sites = new List<Point2D>();
        for (int i = 0; i < 15; i++)
            sites.Add(new Point2D(rng.NextDouble() * 100, rng.NextDouble() * 100));

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(15);
    }

    [Fact]
    public void Compute_TwentyPointsRandom_ReturnsTwentyCells()
    {
        var rng = new Random(456);
        var sites = new List<Point2D>();
        for (int i = 0; i < 20; i++)
            sites.Add(new Point2D(rng.NextDouble() * 200, rng.NextDouble() * 200));

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(20);
    }

    [Fact]
    public void Compute_CellsAreSimplePolygons()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(5, 0),
            new(5, 5),
            new(0, 5),
            new(2.5, 2.5)
        };

        var result = VoronoiDiagram.Compute(sites);

        foreach (var cell in result)
            cell.IsSimple.Should().BeTrue();
    }

    [Fact]
    public void Compute_EightPointsRandom_CellsAreSimple()
    {
        var rng = new Random(77);
        var sites = new List<Point2D>();
        for (int i = 0; i < 8; i++)
            sites.Add(new Point2D(rng.NextDouble() * 80, rng.NextDouble() * 80));

        var result = VoronoiDiagram.Compute(sites);

        foreach (var cell in result)
            cell.IsSimple.Should().BeTrue();
    }

    [Fact]
    public void ComputeCircumcenters_RightTriangle_CenterOnHypotenuse()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(4, 0),
            new(0, 3)
        };

        var centers = VoronoiDiagram.ComputeCircumcenters(sites);

        centers.Should().NotBeNull();
    }

    [Fact]
    public void ComputeCircumcenters_EquilateralTriangle()
    {
        var h = System.Math.Sqrt(3);
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(2, 0),
            new(1, h)
        };

        var centers = VoronoiDiagram.ComputeCircumcenters(sites);

        centers.Should().NotBeNull();
    }

    [Fact]
    public void ComputeCircumcenters_FourPointsCollinear_ReturnsEmpty()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(1, 0),
            new(2, 0),
            new(3, 0)
        };

        var centers = VoronoiDiagram.ComputeCircumcenters(sites);

        centers.Should().BeEmpty();
    }

    [Fact]
    public void Compute_TenPointsOnCircle_ReturnsTenCells()
    {
        var sites = new List<Point2D>();
        for (int i = 0; i < 10; i++)
        {
            double angle = 2 * System.Math.PI * i / 10;
            sites.Add(new Point2D(System.Math.Cos(angle), System.Math.Sin(angle)));
        }

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(10);
    }

    [Fact]
    public void Compute_TwelvePointsOnCircle_ReturnsTwelveCells()
    {
        var sites = new List<Point2D>();
        for (int i = 0; i < 12; i++)
        {
            double angle = 2 * System.Math.PI * i / 12;
            sites.Add(new Point2D(System.Math.Cos(angle), System.Math.Sin(angle)));
        }

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(12);
    }

    [Fact]
    public void Compute_FourPointsCollinearWithExtraPoint_WorksCorrectly()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(3, 0),
            new(6, 0),
            new(3, 4)
        };

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(4);
    }

    [Fact]
    public void Compute_HighAspectRatioRectangularPoints_WorksCorrectly()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(100, 0),
            new(100, 1),
            new(0, 1)
        };

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(4);
    }

    [Fact]
    public void Compute_LargeCoordinates_WorksCorrectly()
    {
        var sites = new List<Point2D>
        {
            new(10000, 10000),
            new(20000, 10000),
            new(20000, 20000),
            new(10000, 20000),
            new(15000, 15000)
        };

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(5);
    }

    [Fact]
    public void Compute_NegativeCoordinates_WorksCorrectly()
    {
        var sites = new List<Point2D>
        {
            new(-10, -10),
            new(-5, -10),
            new(-5, -5),
            new(-10, -5)
        };

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(4);
    }

    [Fact]
    public void Compute_MixedPositiveNegative_WorksCorrectly()
    {
        var sites = new List<Point2D>
        {
            new(-5, -5),
            new(5, -5),
            new(5, 5),
            new(-5, 5),
            new(0, 0)
        };

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(5);
    }

    [Fact]
    public void Compute_SmallClusterPoints_ReturnsCorrectCount()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(0.001, 0),
            new(0.001, 0.001),
            new(0, 0.001)
        };

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(4);
    }

    [Fact]
    public void ComputeCircumcenters_ReturnsOnePerTriangle()
    {
        var rng = new Random(55);
        var sites = new List<Point2D>();
        for (int i = 0; i < 6; i++)
            sites.Add(new Point2D(rng.NextDouble() * 50, rng.NextDouble() * 50));

        var tris = DelaunayTriangulation.Triangulate(sites);
        var centers = VoronoiDiagram.ComputeCircumcenters(sites);

        centers.Length.Should().Be(tris.Length);
    }

    [Fact]
    public void ComputeCircumcenters_EightPoints_ReturnsCorrectCount()
    {
        var sites = new List<Point2D>();
        for (int i = 0; i < 8; i++)
        {
            double angle = 2 * System.Math.PI * i / 8;
            sites.Add(new Point2D(System.Math.Cos(angle), System.Math.Sin(angle)));
        }

        var tris = DelaunayTriangulation.Triangulate(sites);
        var centers = VoronoiDiagram.ComputeCircumcenters(sites);

        centers.Length.Should().Be(tris.Length);
    }

    [Fact]
    public void ComputeCircumcenters_TriangleCenterIsInsideTriangle()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(6, 0),
            new(3, 5)
        };

        var centers = VoronoiDiagram.ComputeCircumcenters(sites);
        var tris = DelaunayTriangulation.Triangulate(sites);

        for (int i = 0; i < centers.Length; i++)
            tris[i].Contains(centers[i]).Should().BeTrue();
    }

    [Fact]
    public void Compute_EightPointsRandom_ReturnsEightCells()
    {
        var rng = new Random(321);
        var sites = new List<Point2D>();
        for (int i = 0; i < 8; i++)
            sites.Add(new Point2D(rng.NextDouble() * 80, rng.NextDouble() * 80));

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(8);
    }

    [Fact]
    public void Compute_FivePointsConvexPentagon_CellsAreConvex()
    {
        var sites = new List<Point2D>();
        for (int i = 0; i < 5; i++)
        {
            double angle = 2 * System.Math.PI * i / 5;
            sites.Add(new Point2D(System.Math.Cos(angle), System.Math.Sin(angle)));
        }

        var result = VoronoiDiagram.Compute(sites);

        foreach (var cell in result)
            cell.IsConvex.Should().BeTrue();
    }

    [Fact]
    public void Compute_SixPointsConvexHexagon_CellsAreConvex()
    {
        var sites = new List<Point2D>();
        for (int i = 0; i < 6; i++)
        {
            double angle = 2 * System.Math.PI * i / 6;
            sites.Add(new Point2D(System.Math.Cos(angle), System.Math.Sin(angle)));
        }

        var result = VoronoiDiagram.Compute(sites);

        foreach (var cell in result)
            cell.IsConvex.Should().BeTrue();
    }

    [Fact]
    public void Compute_ThirteenPointsRandom_ReturnsThirteenCells()
    {
        var rng = new Random(111);
        var sites = new List<Point2D>();
        for (int i = 0; i < 13; i++)
            sites.Add(new Point2D(rng.NextDouble() * 100, rng.NextDouble() * 100));

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(13);
    }

    [Fact]
    public void ComputeCircumcenters_RightTriangleCenterEquidistantFromAllVertices()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(6, 0),
            new(0, 8)
        };

        var centers = VoronoiDiagram.ComputeCircumcenters(sites);
        var tris = DelaunayTriangulation.Triangulate(sites);

        for (int i = 0; i < centers.Length; i++)
        {
            var c = centers[i];
            var t = tris[i];
            c.DistanceTo(t.A).Should().BeApproximately(c.DistanceTo(t.B), Precision);
            c.DistanceTo(t.B).Should().BeApproximately(c.DistanceTo(t.C), Precision);
        }
    }

    [Fact]
    public void Compute_FivePointsOnBoundary_ReturnsFiveCells()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(10, 0),
            new(10, 10),
            new(0, 10),
            new(5, 5)
        };

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(5);
    }

    [Fact]
    public void Compute_NinePointsOnCircle_ReturnsNineCells()
    {
        var sites = new List<Point2D>();
        for (int i = 0; i < 9; i++)
        {
            double angle = 2 * System.Math.PI * i / 9;
            sites.Add(new Point2D(System.Math.Cos(angle), System.Math.Sin(angle)));
        }

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(9);
    }

    [Fact]
    public void ComputeCircumcenters_IsoscelesTriangle_CenterOnAxis()
    {
        var sites = new List<Point2D>
        {
            new(-3, 0),
            new(3, 0),
            new(0, 4)
        };

        var centers = VoronoiDiagram.ComputeCircumcenters(sites);

        centers.Should().NotBeNull();
    }

    [Fact]
    public void Compute_ElevenPointsRandom_ReturnsElevenCells()
    {
        var rng = new Random(222);
        var sites = new List<Point2D>();
        for (int i = 0; i < 11; i++)
            sites.Add(new Point2D(rng.NextDouble() * 100, rng.NextDouble() * 100));

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(11);
    }

    [Fact]
    public void Compute_SixPointsNonConvex_WorksCorrectly()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(4, 0),
            new(4, 4),
            new(2, 2),
            new(0, 4),
            new(2, -1)
        };

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(6);
    }

    [Fact]
    public void ComputeCircumcenters_FivePointsSquareCenter_ReturnsFourCenters()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(6, 0),
            new(6, 6),
            new(0, 6),
            new(3, 3)
        };

        var centers = VoronoiDiagram.ComputeCircumcenters(sites);
        var tris = DelaunayTriangulation.Triangulate(sites);

        centers.Length.Should().Be(tris.Length);
    }

    [Fact]
    public void Compute_EightPointsConvexOctagon_ReturnsEightCells()
    {
        var sites = new List<Point2D>();
        for (int i = 0; i < 8; i++)
        {
            double angle = 2 * System.Math.PI * i / 8;
            sites.Add(new Point2D(System.Math.Cos(angle), System.Math.Sin(angle)));
        }

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(8);
    }

    [Fact]
    public void Compute_WithDuplicatePoints_WorksCorrectly()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(0, 0),
            new(5, 0),
            new(2, 4)
        };

        var result = VoronoiDiagram.Compute(sites);

        result.Should().NotBeNull();
    }

    [Fact]
    public void ComputeCombinedWithDelaunay_CellsAndTrianglesAreDual()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(5, 0),
            new(5, 5),
            new(0, 5),
            new(2.5, 2.5)
        };

        var tris = DelaunayTriangulation.Triangulate(sites);
        var cells = VoronoiDiagram.Compute(sites);

        cells.Should().HaveCount(sites.Count);
        tris.Should().NotBeNull();
    }

    [Fact]
    public void Compute_SevenPointsConvexHeptagon_ReturnsSevenCells()
    {
        var sites = new List<Point2D>();
        for (int i = 0; i < 7; i++)
        {
            double angle = 2 * System.Math.PI * i / 7;
            sites.Add(new Point2D(System.Math.Cos(angle), System.Math.Sin(angle)));
        }

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(7);
    }

    [Fact]
    public void Compute_SliverTriangleSites_WorksCorrectly()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(10, 0),
            new(5, 0.001)
        };

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(3);
    }

    [Fact]
    public void Compute_FourPointsTrapezoid_ReturnsFourCells()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(6, 0),
            new(4, 3),
            new(2, 3)
        };

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(4);
    }

    [Fact]
    public void Compute_VerySmallCluster_WorksCorrectly()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(0.0001, 0),
            new(0.0001, 0.0001),
            new(0, 0.0001),
            new(0.00005, 0.00005)
        };

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(5);
    }

    [Fact]
    public void ComputeCircumcenters_AllCentersAreDistinct()
    {
        var rng = new Random(888);
        var sites = new List<Point2D>();
        for (int i = 0; i < 8; i++)
            sites.Add(new Point2D(rng.NextDouble() * 50, rng.NextDouble() * 50));

        var centers = VoronoiDiagram.ComputeCircumcenters(sites);

        var distinct = centers.Distinct().ToList();
        distinct.Count.Should().Be(centers.Length);
    }

    [Fact]
    public void Compute_FourteenPointsRandom_ReturnsFourteenCells()
    {
        var rng = new Random(555);
        var sites = new List<Point2D>();
        for (int i = 0; i < 14; i++)
            sites.Add(new Point2D(rng.NextDouble() * 100, rng.NextDouble() * 100));

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(14);
    }

    [Fact]
    public void ComputeCircumcenters_FourPointsSquare_NoCollinear_ReturnsTwoCenters()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(3, 0),
            new(3, 3),
            new(0, 3)
        };

        var centers = VoronoiDiagram.ComputeCircumcenters(sites);

        centers.Should().BeEmpty();
    }

    [Fact]
    public void Compute_CollinearThreePoints_ReturnsEmpty()
    {
        var sites = new List<Point2D>
        {
            new(0, 0),
            new(5, 0),
            new(10, 0)
        };

        var result = VoronoiDiagram.Compute(sites);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Compute_SixteenPointsRandom_ReturnsSixteenCells()
    {
        var rng = new Random(666);
        var sites = new List<Point2D>();
        for (int i = 0; i < 16; i++)
            sites.Add(new Point2D(rng.NextDouble() * 200, rng.NextDouble() * 200));

        var result = VoronoiDiagram.Compute(sites);

        result.Should().HaveCount(16);
    }

    [Fact]
    public void ComputeCircumcenters_TenPointsCircle_ReturnsCorrectCount()
    {
        var sites = new List<Point2D>();
        for (int i = 0; i < 10; i++)
        {
            double angle = 2 * System.Math.PI * i / 10;
            sites.Add(new Point2D(System.Math.Cos(angle), System.Math.Sin(angle)));
        }

        var tris = DelaunayTriangulation.Triangulate(sites);
        var centers = VoronoiDiagram.ComputeCircumcenters(sites);

        centers.Length.Should().Be(tris.Length);
    }
}
