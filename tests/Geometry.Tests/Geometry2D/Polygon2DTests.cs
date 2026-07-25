using System.Collections.Immutable;

namespace MathVerse.Geometry.Tests.Geometry2D;

/// <summary>Tests for Polygon2D struct.</summary>
public class Polygon2DTests
{
    private const double Precision = 1e-10;

    private static Polygon2D CreateTriangle() => new(ImmutableArray.Create(
        new Point2D(0, 0), new Point2D(4, 0), new Point2D(0, 3)));

    private static Polygon2D CreateSquare() => new(ImmutableArray.Create(
        new Point2D(0, 0), new Point2D(4, 0), new Point2D(4, 4), new Point2D(0, 4)));

    private static Polygon2D CreatePentagon() => new(ImmutableArray.Create(
        new Point2D(0, 0), new Point2D(2, 0), new Point2D(3, 1.5),
        new Point2D(1.5, 3), new Point2D(-0.5, 1.5)));

    /// <summary>Triangle area should be 0.5 * base * height.</summary>
    [Fact]
    public void Area_Triangle_ShouldBeCorrect()
    {
        var poly = CreateTriangle();
        poly.Area.Should().BeApproximately(6.0, Precision);
    }

    /// <summary>Square area should be side squared.</summary>
    [Fact]
    public void Area_Square_ShouldBeSideSquared()
    {
        var poly = CreateSquare();
        poly.Area.Should().BeApproximately(16.0, Precision);
    }

    /// <summary>Pentagon area should be positive.</summary>
    [Fact]
    public void Area_Pentagon_ShouldBePositive()
    {
        var poly = CreatePentagon();
        poly.Area.Should().BeGreaterThan(0);
    }

    /// <summary>Perimeter of square should be 4 * side.</summary>
    [Fact]
    public void Perimeter_Square_ShouldBeFourTimesSide()
    {
        var poly = CreateSquare();
        poly.Perimeter.Should().BeApproximately(16.0, Precision);
    }

    /// <summary>Centroid of square should be at center.</summary>
    [Fact]
    public void Centroid_Square_ShouldBeAtCenter()
    {
        var poly = CreateSquare();
        Point2D c = poly.Centroid;
        c.X.Should().BeApproximately(2.0, Precision);
        c.Y.Should().BeApproximately(2.0, Precision);
    }

    /// <summary>Centroid of triangle should be average of vertices.</summary>
    [Fact]
    public void Centroid_Triangle_ShouldBeAverageOfVertices()
    {
        var poly = CreateTriangle();
        Point2D c = poly.Centroid;
        c.X.Should().BeApproximately(4.0 / 3.0, Precision);
        c.Y.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>Square should be convex.</summary>
    [Fact]
    public void IsConvex_Square_ShouldBeTrue()
    {
        var poly = CreateSquare();
        poly.IsConvex.Should().BeTrue();
    }

    /// <summary>Simple polygon should be simple.</summary>
    [Fact]
    public void IsSimple_Triangle_ShouldBeTrue()
    {
        var poly = CreateTriangle();
        poly.IsSimple.Should().BeTrue();
    }

    /// <summary>Square with CCW vertices should have CCW winding.</summary>
    [Fact]
    public void WindingOrder_CCWSquare_ShouldBeCounterClockwise()
    {
        var poly = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 0), new Point2D(1, 1), new Point2D(0, 1)));
        poly.WindingOrder.Should().Be(WindingOrder.CounterClockwise);
    }

    /// <summary>Contains interior point should return true.</summary>
    [Fact]
    public void Contains_InteriorPoint_ShouldReturnTrue()
    {
        var poly = CreateSquare();
        poly.Contains(new Point2D(2, 2)).Should().BeTrue();
    }

    /// <summary>Contains exterior point should return false.</summary>
    [Fact]
    public void Contains_ExteriorPoint_ShouldReturnFalse()
    {
        var poly = CreateSquare();
        poly.Contains(new Point2D(10, 10)).Should().BeFalse();
    }

    /// <summary>Triangulate square should produce two triangles.</summary>
    [Fact]
    public void Triangulate_Square_ShouldProduceTwoTriangles()
    {
        var poly = CreateSquare();
        ImmutableArray<Triangle2D> tris = poly.Triangulate();
        tris.Length.Should().Be(2);
    }

    /// <summary>Triangulate triangle should produce one triangle.</summary>
    [Fact]
    public void Triangulate_Triangle_ShouldProduceOneTriangle()
    {
        var poly = CreateTriangle();
        ImmutableArray<Triangle2D> tris = poly.Triangulate();
        tris.Length.Should().Be(1);
    }

    /// <summary>ToBoundingBox should enclose all vertices.</summary>
    [Fact]
    public void ToBoundingBox_ShouldEncloseAllVertices()
    {
        var poly = CreateTriangle();
        BoundingBox2D bbox = poly.ToBoundingBox();
        bbox.Min.X.Should().BeApproximately(0.0, Precision);
        bbox.Min.Y.Should().BeApproximately(0.0, Precision);
        bbox.Max.X.Should().BeApproximately(4.0, Precision);
        bbox.Max.Y.Should().BeApproximately(3.0, Precision);
    }

    /// <summary>Edges should produce correct number of edges.</summary>
    [Fact]
    public void Edges_ShouldProduceCorrectCount()
    {
        var poly = CreateSquare();
        poly.Edges.Count().Should().Be(4);
    }

    /// <summary>VertexCount should match number of vertices.</summary>
    [Fact]
    public void VertexCount_ShouldMatchVertexCount()
    {
        var poly = CreatePentagon();
        poly.VertexCount.Should().Be(5);
    }

    /// <summary>Empty polygon should have zero area.</summary>
    [Fact]
    public void EmptyPolygon_ShouldHaveZeroArea()
    {
        var poly = new Polygon2D(ImmutableArray<Point2D>.Empty);
        poly.Area.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Collinear polygon should be degenerate.</summary>
    [Fact]
    public void Degenerate_CollinearVertices_ShouldHaveZeroArea()
    {
        var poly = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(1, 0), new Point2D(2, 0)));
        poly.Area.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Edges of polygon should be consecutive segments.</summary>
    [Fact]
    public void Edges_ShouldBeConsecutiveSegments()
    {
        var poly = CreateTriangle();
        var edges = poly.Edges.ToList();
        edges[0].P1.Should().Be(poly.Vertices[0]);
        edges[0].P2.Should().Be(poly.Vertices[1]);
        edges[1].P1.Should().Be(poly.Vertices[1]);
        edges[1].P2.Should().Be(poly.Vertices[2]);
        edges[2].P1.Should().Be(poly.Vertices[2]);
        edges[2].P2.Should().Be(poly.Vertices[0]);
    }

    /// <summary>Indexer should return correct vertex.</summary>
    [Fact]
    public void Indexer_ShouldReturnCorrectVertex()
    {
        var poly = CreateSquare();
        poly[0].Should().Be(new Point2D(0, 0));
        poly[2].Should().Be(new Point2D(4, 4));
    }

    /// <summary>Non-convex polygon should not be convex.</summary>
    [Fact]
    public void IsConvex_NonConvexPolygon_ShouldBeFalse()
    {
        var poly = new Polygon2D(ImmutableArray.Create(
            new Point2D(0, 0), new Point2D(4, 0), new Point2D(2, 1), new Point2D(4, 4), new Point2D(0, 4)));
        poly.IsConvex.Should().BeFalse();
    }

    /// <summary>ToString should return formatted string.</summary>
    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var poly = CreateSquare();
        string result = poly.ToString();
        result.Should().Contain("Polygon2D");
    }
}
