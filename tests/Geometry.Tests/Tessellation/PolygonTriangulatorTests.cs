namespace MathVerse.Geometry.Tests.Tessellation;

/// <summary>Tests for the <see cref="PolygonTriangulator"/> static class.</summary>
public class PolygonTriangulatorTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies Triangulate of a triangle produces exactly one triangle.</summary>
    [Fact]
    public void Triangulate_Triangle_ReturnsOneTriangle()
    {
        var triangle = new List<Point2D>
        {
            new(0, 0), new(1, 0), new(0, 1)
        };

        var result = PolygonTriangulator.Triangulate(triangle);

        result.Should().HaveCount(1);
    }

    /// <summary>Verifies Triangulate of a square produces exactly two triangles.</summary>
    [Fact]
    public void Triangulate_Square_ReturnsTwoTriangles()
    {
        var square = new List<Point2D>
        {
            new(0, 0), new(1, 0), new(1, 1), new(0, 1)
        };

        var result = PolygonTriangulator.Triangulate(square);

        result.Should().HaveCount(2);
    }

    /// <summary>Verifies Triangulate of a pentagon produces exactly three triangles.</summary>
    [Fact]
    public void Triangulate_Pentagon_ReturnsThreeTriangles()
    {
        var pentagon = new List<Point2D>
        {
            new(0, 0), new(1, 0), new(1.5, 0.8),
            new(0.5, 1.5), new(-0.5, 0.8)
        };

        var result = PolygonTriangulator.Triangulate(pentagon);

        result.Should().HaveCount(3);
    }

    /// <summary>Verifies IsEar returns true for a convex corner of a triangle.</summary>
    [Fact]
    public void IsEar_TriangleVertex_ReturnsTrue()
    {
        var triangle = new List<Point2D>
        {
            new(0, 0), new(1, 0), new(0, 1)
        };

        bool result = PolygonTriangulator.IsEar(triangle, 0, 1, 2);

        result.Should().BeTrue();
    }

    /// <summary>Verifies SignedArea of a CCW triangle is positive.</summary>
    [Fact]
    public void SignedArea_CCWTriangle_ReturnsPositive()
    {
        var triangle = new List<Point2D>
        {
            new(0, 0), new(1, 0), new(0, 1)
        };

        double area = PolygonTriangulator.SignedArea(triangle);

        area.Should().BeGreaterThan(0);
    }

    /// <summary>Verifies SignedArea of a CW triangle is negative.</summary>
    [Fact]
    public void SignedArea_CWTriangle_ReturnsNegative()
    {
        var triangle = new List<Point2D>
        {
            new(0, 0), new(0, 1), new(1, 0)
        };

        double area = PolygonTriangulator.SignedArea(triangle);

        area.Should().BeLessThan(0);
    }

    /// <summary>Verifies EnsureWinding with CCW order returns CCW polygon.</summary>
    [Fact]
    public void EnsureWinding_CWInput_EnsuresCCW()
    {
        var cwTriangle = new List<Point2D>
        {
            new(0, 0), new(0, 1), new(1, 0)
        };

        var result = PolygonTriangulator.EnsureWinding(cwTriangle, WindingOrder.CounterClockwise);

        PolygonTriangulator.SignedArea(result).Should().BeGreaterThan(0);
    }

    /// <summary>Verifies EnsureWinding with CW order returns CW polygon.</summary>
    [Fact]
    public void EnsureWinding_CCWInput_EnsuresCW()
    {
        var ccwTriangle = new List<Point2D>
        {
            new(0, 0), new(1, 0), new(0, 1)
        };

        var result = PolygonTriangulator.EnsureWinding(ccwTriangle, WindingOrder.Clockwise);

        PolygonTriangulator.SignedArea(result).Should().BeLessThan(0);
    }

    /// <summary>Verifies Triangulate of a convex polygon produces n-2 triangles.</summary>
    [Fact]
    public void Triangulate_ConvexHexagon_ReturnsFourTriangles()
    {
        var hexagon = new List<Point2D>
        {
            new(1, 0), new(0.5, 0.866), new(-0.5, 0.866),
            new(-1, 0), new(-0.5, -0.866), new(0.5, -0.866)
        };

        var result = PolygonTriangulator.Triangulate(hexagon);

        result.Should().HaveCount(4);
    }

    /// <summary>Verifies Triangulate of a concave polygon produces valid triangles.</summary>
    [Fact]
    public void Triangulate_ConcavePolygon_ProducesValidTriangles()
    {
        var concave = new List<Point2D>
        {
            new(0, 0), new(2, 0), new(2, 2),
            new(1, 0.5), new(0, 2)
        };

        var result = PolygonTriangulator.Triangulate(concave);

        result.Should().HaveCount(3);
    }

    /// <summary>Verifies Triangulate of polygon with fewer than 3 vertices returns empty.</summary>
    [Fact]
    public void Triangulate_TwoVertices_ReturnsEmpty()
    {
        var line = new List<Point2D> { new(0, 0), new(1, 1) };

        var result = PolygonTriangulator.Triangulate(line);

        result.Should().BeEmpty();
    }

    /// <summary>Verifies SignedArea magnitude for unit square is 1.0.</summary>
    [Fact]
    public void SignedArea_UnitSquare_ReturnsMagnitudeOne()
    {
        var square = new List<Point2D>
        {
            new(0, 0), new(1, 0), new(1, 1), new(0, 1)
        };

        double area = PolygonTriangulator.SignedArea(square);

        System.Math.Abs(area).Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies IsEar returns false for a reflex vertex.</summary>
    [Fact]
    public void IsEar_ReflexVertex_ReturnsFalse()
    {
        var concave = new List<Point2D>
        {
            new(0, 0), new(2, 0), new(2, 2),
            new(1, 0.5), new(0, 2)
        };

        bool result = PolygonTriangulator.IsEar(concave, 2, 3, 4);

        result.Should().BeFalse();
    }

    /// <summary>Verifies Triangulate triangle returns same triangle as the only element.</summary>
    [Fact]
    public void Triangulate_Triangle_ReturnsSameVertices()
    {
        var triangle = new List<Point2D>
        {
            new(0, 0), new(3, 0), new(0, 4)
        };

        var result = PolygonTriangulator.Triangulate(triangle);

        result[0].A.Should().Be(triangle[2]);
        result[0].B.Should().Be(triangle[0]);
        result[0].C.Should().Be(triangle[1]);
    }

    /// <summary>Verifies EnsureWinding on already-correct winding returns original polygon.</summary>
    [Fact]
    public void EnsureWinding_AlreadyCorrect_ReturnsSamePolygon()
    {
        var ccwTriangle = new List<Point2D>
        {
            new(0, 0), new(1, 0), new(0, 1)
        };

        var result = PolygonTriangulator.EnsureWinding(ccwTriangle, WindingOrder.CounterClockwise);

        result.Should().BeSameAs(ccwTriangle);
    }
}
