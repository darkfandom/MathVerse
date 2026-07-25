namespace MathVerse.Geometry.Tests.Geometry3D;

/// <summary>Tests for the <see cref="Cylinder3D"/> struct.</summary>
public class Cylinder3DTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies Volume matches the formula pi * r^2 * h.</summary>
    [Fact]
    public void Volume_MatchesFormula()
    {
        var cyl = new Cylinder3D(Point3D.Origin, 3.0, 5.0);

        double expected = System.Math.PI * 9.0 * 5.0;

        cyl.Volume.Should().BeApproximately(expected, Tolerance);
    }

    /// <summary>Verifies Volume with unit cylinder equals pi.</summary>
    [Fact]
    public void Volume_UnitCylinder_ReturnsPi()
    {
        var cyl = new Cylinder3D(Point3D.Origin, 1.0, 1.0);

        cyl.Volume.Should().BeApproximately(System.Math.PI, Tolerance);
    }

    /// <summary>Verifies SurfaceArea matches the formula 2*pi*r*h + 2*pi*r^2.</summary>
    [Fact]
    public void SurfaceArea_MatchesFormula()
    {
        var cyl = new Cylinder3D(Point3D.Origin, 2.0, 3.0);

        double expected = 2.0 * System.Math.PI * 2.0 * 3.0 + 2.0 * System.Math.PI * 4.0;

        cyl.SurfaceArea.Should().BeApproximately(expected, Tolerance);
    }

    /// <summary>Verifies PointAt at t=0 returns the bottom circle.</summary>
    [Fact]
    public void PointAt_BottomCircle_ReturnsCorrectHeight()
    {
        var cyl = new Cylinder3D(new Point3D(0, 5, 0), 2.0, 4.0);

        var point = cyl.PointAt(0.0, 0.0);

        point.Y.Should().BeApproximately(3.0, Tolerance);
    }

    /// <summary>Verifies PointAt at t=1 returns the top circle.</summary>
    [Fact]
    public void PointAt_TopCircle_ReturnsCorrectHeight()
    {
        var cyl = new Cylinder3D(new Point3D(0, 5, 0), 2.0, 4.0);

        var point = cyl.PointAt(1.0, 0.0);

        point.Y.Should().BeApproximately(7.0, Tolerance);
    }

    /// <summary>Verifies PointAt at t=0.5 returns the midpoint height.</summary>
    [Fact]
    public void PointAt_MidHeight_ReturnsCorrectHeight()
    {
        var cyl = new Cylinder3D(new Point3D(0, 5, 0), 2.0, 4.0);

        var point = cyl.PointAt(0.5, 0.0);

        point.Y.Should().BeApproximately(5.0, Tolerance);
    }

    /// <summary>Verifies PointAt returns a point at the correct radius distance.</summary>
    [Fact]
    public void PointAt_PointOnSurface_CorrectRadiusDistance()
    {
        var cyl = new Cylinder3D(Point3D.Origin, 3.0, 6.0);
        double angle = System.Math.PI / 4.0;

        var point = cyl.PointAt(0.5, angle);

        double xzDist = System.Math.Sqrt(point.X * point.X + point.Z * point.Z);
        xzDist.Should().BeApproximately(3.0, Tolerance);
    }

    /// <summary>Verifies ToBoundingBox encloses the cylinder.</summary>
    [Fact]
    public void ToBoundingBox_EnclosesCylinder()
    {
        var cyl = new Cylinder3D(Point3D.Origin, 2.0, 4.0);

        var bbox = cyl.ToBoundingBox();

        bbox.Min.X.Should().BeApproximately(-2.0, Tolerance);
        bbox.Min.Y.Should().BeApproximately(-2.0, Tolerance);
        bbox.Min.Z.Should().BeApproximately(-2.0, Tolerance);
        bbox.Max.X.Should().BeApproximately(2.0, Tolerance);
        bbox.Max.Y.Should().BeApproximately(2.0, Tolerance);
        bbox.Max.Z.Should().BeApproximately(2.0, Tolerance);
    }

    /// <summary>Verifies ToBoundingBox height equals cylinder height.</summary>
    [Fact]
    public void ToBoundingBox_HeightEqualsCylinderHeight()
    {
        var cyl = new Cylinder3D(Point3D.Origin, 1.0, 10.0);

        var bbox = cyl.ToBoundingBox();

        bbox.Height.Should().BeApproximately(10.0, Tolerance);
    }

    /// <summary>Verifies ToBoundingBox width and depth equal twice the radius.</summary>
    [Fact]
    public void ToBoundingBox_WidthDepthEqualDiameter()
    {
        var cyl = new Cylinder3D(Point3D.Origin, 5.0, 10.0);

        var bbox = cyl.ToBoundingBox();

        bbox.Width.Should().BeApproximately(10.0, Tolerance);
        bbox.Depth.Should().BeApproximately(10.0, Tolerance);
    }
}
