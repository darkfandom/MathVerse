namespace MathVerse.Geometry.Tests.Geometry2D;

/// <summary>Tests for Triangle2D struct.</summary>
public class Triangle2DTests
{
    private const double Precision = 1e-10;

    /// <summary>Right triangle area should be 0.5 * base * height.</summary>
    [Fact]
    public void Area_RightTriangle_ShouldBeCorrect()
    {
        var tri = new Triangle2D(new Point2D(0, 0), new Point2D(3, 0), new Point2D(0, 4));
        tri.Area.Should().BeApproximately(6.0, Precision);
    }

    /// <summary>Equilateral triangle area should be sqrt(3)/4 * side^2.</summary>
    [Fact]
    public void Area_EquilateralTriangle_ShouldBeCorrect()
    {
        double side = 2.0;
        var tri = new Triangle2D(
            new Point2D(0, 0),
            new Point2D(side, 0),
            new Point2D(side / 2.0, side * System.Math.Sqrt(3.0) / 2.0));
        tri.Area.Should().BeApproximately(System.Math.Sqrt(3.0), Precision);
    }

    /// <summary>Degenerate triangle should have zero area.</summary>
    [Fact]
    public void Area_DegenerateTriangle_ShouldBeZero()
    {
        var tri = new Triangle2D(new Point2D(0, 0), new Point2D(1, 1), new Point2D(2, 2));
        tri.Area.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Perimeter should be sum of side lengths.</summary>
    [Fact]
    public void Perimeter_ShouldBeSumOfSides()
    {
        var tri = new Triangle2D(new Point2D(0, 0), new Point2D(3, 0), new Point2D(0, 4));
        tri.Perimeter.Should().BeApproximately(3.0 + 4.0 + 5.0, Precision);
    }

    /// <summary>Centroid should be average of vertices.</summary>
    [Fact]
    public void Centroid_ShouldBeAverageOfVertices()
    {
        var tri = new Triangle2D(new Point2D(0, 0), new Point2D(6, 0), new Point2D(0, 6));
        Point2D c = tri.Centroid;
        c.X.Should().BeApproximately(2.0, Precision);
        c.Y.Should().BeApproximately(2.0, Precision);
    }

    /// <summary>Incenter of right triangle should be correct.</summary>
    [Fact]
    public void Incenter_RightTriangle_ShouldBeCorrect()
    {
        var tri = new Triangle2D(new Point2D(0, 0), new Point2D(3, 0), new Point2D(0, 4));
        Point2D ic = tri.Incenter;
        double r = tri.Inradius;
        ic.X.Should().BeApproximately(r, Precision);
        ic.Y.Should().BeApproximately(r, Precision);
    }

    /// <summary>Circumcenter of right triangle should be at hypotenuse midpoint.</summary>
    [Fact]
    public void Circumcenter_RightTriangle_ShouldBeHypotenuseMidpoint()
    {
        var tri = new Triangle2D(new Point2D(0, 0), new Point2D(6, 0), new Point2D(0, 4));
        Point2D cc = tri.Circumcenter;
        cc.X.Should().BeApproximately(3.0, Precision);
        cc.Y.Should().BeApproximately(2.0, Precision);
    }

    /// <summary>Circumradius should equal distance from circumcenter to any vertex.</summary>
    [Fact]
    public void Circumradius_ShouldEqualDistanceToVertices()
    {
        var tri = new Triangle2D(new Point2D(0, 0), new Point2D(4, 0), new Point2D(0, 3));
        double r = tri.Circumradius;
        r.Should().BeApproximately(2.5, Precision);
    }

    /// <summary>Inradius of right triangle should equal (a+b-c)/2.</summary>
    [Fact]
    public void Inradius_RightTriangle_ShouldBeCorrect()
    {
        var tri = new Triangle2D(new Point2D(0, 0), new Point2D(3, 0), new Point2D(0, 4));
        double expected = (3.0 + 4.0 - 5.0) / 2.0;
        tri.Inradius.Should().BeApproximately(expected, Precision);
    }

    /// <summary>Barycentric coordinates at centroid should be (1/3, 1/3, 1/3).</summary>
    [Fact]
    public void BarycentricCoords_AtCentroid_ShouldBeEqual()
    {
        var tri = new Triangle2D(new Point2D(0, 0), new Point2D(6, 0), new Point2D(0, 6));
        var (u, v, w) = tri.BarycentricCoords(tri.Centroid);
        u.Should().BeApproximately(1.0 / 3.0, Precision);
        v.Should().BeApproximately(1.0 / 3.0, Precision);
        w.Should().BeApproximately(1.0 / 3.0, Precision);
    }

    /// <summary>Barycentric coordinates at vertex A should be (1, 0, 0).</summary>
    [Fact]
    public void BarycentricCoords_AtVertexA_ShouldBeOneZeroZero()
    {
        var tri = new Triangle2D(new Point2D(0, 0), new Point2D(3, 0), new Point2D(0, 3));
        var (u, v, w) = tri.BarycentricCoords(tri.A);
        u.Should().BeApproximately(1.0, Precision);
        v.Should().BeApproximately(0.0, Precision);
        w.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Contains should return true for interior point.</summary>
    [Fact]
    public void Contains_InteriorPoint_ShouldReturnTrue()
    {
        var tri = new Triangle2D(new Point2D(0, 0), new Point2D(10, 0), new Point2D(5, 10));
        tri.Contains(new Point2D(5, 2)).Should().BeTrue();
    }

    /// <summary>Contains should return false for exterior point.</summary>
    [Fact]
    public void Contains_ExteriorPoint_ShouldReturnFalse()
    {
        var tri = new Triangle2D(new Point2D(0, 0), new Point2D(10, 0), new Point2D(5, 10));
        tri.Contains(new Point2D(20, 20)).Should().BeFalse();
    }

    /// <summary>ToBoundingBox should enclose all vertices.</summary>
    [Fact]
    public void ToBoundingBox_ShouldEncloseAllVertices()
    {
        var tri = new Triangle2D(new Point2D(1, 2), new Point2D(5, 3), new Point2D(3, 7));
        BoundingBox2D bbox = tri.ToBoundingBox();
        bbox.Min.X.Should().BeApproximately(1.0, Precision);
        bbox.Min.Y.Should().BeApproximately(2.0, Precision);
        bbox.Max.X.Should().BeApproximately(5.0, Precision);
        bbox.Max.Y.Should().BeApproximately(7.0, Precision);
    }

    /// <summary>IsDegenerate should return true for collinear points.</summary>
    [Fact]
    public void IsDegenerate_CollinearPoints_ShouldReturnTrue()
    {
        var tri = new Triangle2D(new Point2D(0, 0), new Point2D(1, 1), new Point2D(2, 2));
        tri.IsDegenerate().Should().BeTrue();
    }

    /// <summary>IsDegenerate should return false for valid triangle.</summary>
    [Fact]
    public void IsDegenerate_ValidTriangle_ShouldReturnFalse()
    {
        var tri = new Triangle2D(new Point2D(0, 0), new Point2D(1, 0), new Point2D(0, 1));
        tri.IsDegenerate().Should().BeFalse();
    }

    /// <summary>Equilateral triangle should have equal side lengths.</summary>
    [Fact]
    public void Equilateral_ShouldHaveEqualSides()
    {
        double s = 3.0;
        var tri = new Triangle2D(
            new Point2D(0, 0),
            new Point2D(s, 0),
            new Point2D(s / 2.0, s * System.Math.Sqrt(3.0) / 2.0));
        double ab = tri.A.DistanceTo(tri.B);
        double bc = tri.B.DistanceTo(tri.C);
        double ca = tri.C.DistanceTo(tri.A);
        ab.Should().BeApproximately(s, Precision);
        bc.Should().BeApproximately(s, Precision);
        ca.Should().BeApproximately(s, Precision);
    }

    /// <summary>Isosceles triangle should have two equal sides.</summary>
    [Fact]
    public void Isosceles_ShouldHaveTwoEqualSides()
    {
        var tri = new Triangle2D(new Point2D(0, 0), new Point2D(4, 0), new Point2D(2, 3));
        double ab = tri.A.DistanceTo(tri.B);
        double ac = tri.A.DistanceTo(tri.C);
        double bc = tri.B.DistanceTo(tri.C);
        (ab == bc || ab == ac || bc == ac).Should().BeTrue();
    }

    /// <summary>Collinear vertices should produce degenerate triangle.</summary>
    [Fact]
    public void Collinear_ShouldProduceDegenerateTriangle()
    {
        var tri = new Triangle2D(new Point2D(1, 1), new Point2D(2, 2), new Point2D(3, 3));
        tri.IsDegenerate().Should().BeTrue();
        tri.Area.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Incenter should be equidistant from all edges.</summary>
    [Fact]
    public void Incenter_ShouldBeEquidistantFromEdges()
    {
        var tri = new Triangle2D(new Point2D(0, 0), new Point2D(6, 0), new Point2D(0, 4));
        Point2D ic = tri.Incenter;
        double d1 = new Line2D(tri.A, tri.B).DistanceTo(ic);
        double d2 = new Line2D(tri.B, tri.C).DistanceTo(ic);
        double d3 = new Line2D(tri.C, tri.A).DistanceTo(ic);
        d1.Should().BeApproximately(d2, Precision);
        d2.Should().BeApproximately(d3, Precision);
    }

    /// <summary>Area times 2 divided by perimeter should equal inradius.</summary>
    [Fact]
    public void AreaAndPerimeter_ShouldRelateToInradius()
    {
        var tri = new Triangle2D(new Point2D(0, 0), new Point2D(5, 0), new Point2D(0, 12));
        double expected = 2.0 * tri.Area / tri.Perimeter;
        tri.Inradius.Should().BeApproximately(expected, Precision);
    }

    /// <summary>Barycentric coords outside triangle should have negative component.</summary>
    [Fact]
    public void BarycentricCoords_Outside_ShouldHaveNegativeComponent()
    {
        var tri = new Triangle2D(new Point2D(0, 0), new Point2D(1, 0), new Point2D(0, 1));
        var (u, v, w) = tri.BarycentricCoords(new Point2D(10, 10));
        (u < 0 || v < 0 || w < 0).Should().BeTrue();
    }

    /// <summary>Indexer should return correct vertices.</summary>
    [Fact]
    public void Indexer_ShouldReturnCorrectVertices()
    {
        var tri = new Triangle2D(new Point2D(1, 2), new Point2D(3, 4), new Point2D(5, 6));
        tri[0].Should().Be(new Point2D(1, 2));
        tri[1].Should().Be(new Point2D(3, 4));
        tri[2].Should().Be(new Point2D(5, 6));
    }

    /// <summary>ToString should return formatted string.</summary>
    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var tri = new Triangle2D(new Point2D(0, 0), new Point2D(1, 0), new Point2D(0, 1));
        string result = tri.ToString();
        result.Should().Contain("Triangle2D");
    }
}
