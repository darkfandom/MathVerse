namespace MathVerse.Geometry.Tests.Geometry3D;

/// <summary>Tests for the <see cref="Quad3D"/> struct.</summary>
public class Quad3DTests
{
    private const double Tolerance = 1e-10;

    private static readonly Quad3D UnitSquare = new(
        new Point3D(0, 0, 0),
        new Point3D(1, 0, 0),
        new Point3D(1, 1, 0),
        new Point3D(0, 1, 0));

    /// <summary>Verifies Triangulate returns exactly two triangles.</summary>
    [Fact]
    public void Triangulate_ReturnsTwoTriangles()
    {
        var (tri1, tri2) = UnitSquare.Triangulate();

        tri1.Area.Should().BeGreaterThan(0.0);
        tri2.Area.Should().BeGreaterThan(0.0);
    }

    /// <summary>Verifies Triangulate triangles together equal the quad area.</summary>
    [Fact]
    public void Triangulate_AreasSumToQuadArea()
    {
        var (tri1, tri2) = UnitSquare.Triangulate();

        double sum = tri1.Area + tri2.Area;

        sum.Should().BeApproximately(UnitSquare.Area, Tolerance);
    }

    /// <summary>Verifies Normal is perpendicular to the quad surface.</summary>
    [Fact]
    public void Normal_IsPerpendicularToQuad()
    {
        var ab = new Vector3D(UnitSquare.B.X - UnitSquare.A.X, UnitSquare.B.Y - UnitSquare.A.Y, UnitSquare.B.Z - UnitSquare.A.Z);
        var ad = new Vector3D(UnitSquare.D.X - UnitSquare.A.X, UnitSquare.D.Y - UnitSquare.A.Y, UnitSquare.D.Z - UnitSquare.A.Z);

        UnitSquare.Normal.Dot(ab).Should().BeApproximately(0.0, Tolerance);
        UnitSquare.Normal.Dot(ad).Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies Normal is a unit vector.</summary>
    [Fact]
    public void Normal_IsUnitVector()
    {
        UnitSquare.Normal.Length.Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies Centroid is the average of all four vertices.</summary>
    [Fact]
    public void Centroid_ReturnsAverage()
    {
        var quad = new Quad3D(
            new Point3D(0, 0, 0),
            new Point3D(4, 0, 0),
            new Point3D(4, 4, 0),
            new Point3D(0, 4, 0));

        var centroid = quad.Centroid;

        centroid.X.Should().BeApproximately(2.0, Tolerance);
        centroid.Y.Should().BeApproximately(2.0, Tolerance);
        centroid.Z.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies Area of a unit square is 1.0.</summary>
    [Fact]
    public void Area_UnitSquare_ReturnsOne()
    {
        UnitSquare.Area.Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies Area of a 2x2 square is 4.0.</summary>
    [Fact]
    public void Area_TwoByTwoSquare_ReturnsFour()
    {
        var quad = new Quad3D(
            new Point3D(0, 0, 0),
            new Point3D(2, 0, 0),
            new Point3D(2, 2, 0),
            new Point3D(0, 2, 0));

        quad.Area.Should().BeApproximately(4.0, Tolerance);
    }

    /// <summary>Verifies Contains returns true for a point inside the quad.</summary>
    [Fact]
    public void Contains_InsidePoint_ReturnsTrue()
    {
        UnitSquare.Contains(new Point3D(0.5, 0.5, 0)).Should().BeTrue();
    }

    /// <summary>Verifies Contains returns false for a point outside the quad.</summary>
    [Fact]
    public void Contains_OutsidePoint_ReturnsFalse()
    {
        UnitSquare.Contains(new Point3D(2, 2, 0)).Should().BeFalse();
    }

    /// <summary>Verifies Area of a non-planar quad equals sum of triangulated areas.</summary>
    [Fact]
    public void Area_NonPlanarQuad_SumOfTriangles()
    {
        var quad = new Quad3D(
            new Point3D(0, 0, 0),
            new Point3D(2, 0, 0),
            new Point3D(2, 2, 1),
            new Point3D(0, 2, 0));

        var (tri1, tri2) = quad.Triangulate();
        double expected = tri1.Area + tri2.Area;

        quad.Area.Should().BeApproximately(expected, Tolerance);
    }
}
