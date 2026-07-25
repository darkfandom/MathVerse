namespace MathVerse.Geometry.Tests.Geometry3D;

/// <summary>Tests for the <see cref="Cone3D"/> struct.</summary>
public class Cone3DTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies SlantHeight matches sqrt(r^2 + h^2).</summary>
    [Fact]
    public void SlantHeight_MatchesFormula()
    {
        var cone = new Cone3D(Point3D.Origin, Vector3D.UnitY, 3.0, 4.0);

        double expected = System.Math.Sqrt(9.0 + 16.0);

        cone.SlantHeight.Should().BeApproximately(expected, Tolerance);
    }

    /// <summary>Verifies SlantHeight is always greater than or equal to Height.</summary>
    [Fact]
    public void SlantHeight_GreaterOrEqualToHeight()
    {
        var cone = new Cone3D(Point3D.Origin, Vector3D.UnitY, 5.0, 12.0);

        cone.SlantHeight.Should().BeGreaterThanOrEqualTo(cone.Height);
    }

    /// <summary>Verifies Volume matches the formula 1/3 * pi * r^2 * h.</summary>
    [Fact]
    public void Volume_MatchesFormula()
    {
        var cone = new Cone3D(Point3D.Origin, Vector3D.UnitY, 3.0, 6.0);

        double expected = (1.0 / 3.0) * System.Math.PI * 9.0 * 6.0;

        cone.Volume.Should().BeApproximately(expected, Tolerance);
    }

    /// <summary>Verifies SurfaceArea matches the formula pi * r * (r + slant).</summary>
    [Fact]
    public void SurfaceArea_MatchesFormula()
    {
        var cone = new Cone3D(Point3D.Origin, Vector3D.UnitY, 3.0, 4.0);
        double slant = cone.SlantHeight;

        double expected = System.Math.PI * 3.0 * (3.0 + slant);

        cone.SurfaceArea.Should().BeApproximately(expected, Tolerance);
    }

    /// <summary>Verifies ToBoundingBox encloses a cone pointing down the Y axis.</summary>
    [Fact]
    public void ToBoundingBox_ConeAlongY_EnclosesCone()
    {
        var cone = new Cone3D(Point3D.Origin, Vector3D.UnitY, 3.0, 6.0);

        var bbox = cone.ToBoundingBox();

        bbox.Contains(cone.Apex).Should().BeTrue();
        bbox.Height.Should().BeApproximately(6.0, Tolerance);
    }

    /// <summary>Verifies ToBoundingBox width is approximately twice the radius for an axial cone.</summary>
    [Fact]
    public void ToBoundingBox_WidthApproximatelyDiameter()
    {
        var cone = new Cone3D(Point3D.Origin, Vector3D.UnitY, 4.0, 10.0);

        var bbox = cone.ToBoundingBox();

        bbox.Width.Should().BeApproximately(8.0, Tolerance);
        bbox.Depth.Should().BeApproximately(8.0, Tolerance);
    }

    /// <summary>Verifies Volume is exactly one third of a cylinder with same radius and height.</summary>
    [Fact]
    public void Volume_IsThirdOfCylinder()
    {
        var cone = new Cone3D(Point3D.Origin, Vector3D.UnitY, 3.0, 5.0);
        var cyl = new Cylinder3D(Point3D.Origin, 3.0, 5.0);

        cone.Volume.Should().BeApproximately(cyl.Volume / 3.0, Tolerance);
    }

    /// <summary>Verifies SlantHeight for r=3, h=4 is exactly 5.</summary>
    [Fact]
    public void SlantHeight_345Triangle_Returns5()
    {
        var cone = new Cone3D(Point3D.Origin, Vector3D.UnitY, 3.0, 4.0);

        cone.SlantHeight.Should().BeApproximately(5.0, Tolerance);
    }

    /// <summary>Verifies ToBoundingBox apex is contained.</summary>
    [Fact]
    public void ToBoundingBox_ApexContained()
    {
        var cone = new Cone3D(new Point3D(5, 10, 15), Vector3D.UnitZ, 2.0, 4.0);

        var bbox = cone.ToBoundingBox();

        bbox.Contains(cone.Apex).Should().BeTrue();
    }

    /// <summary>Verifies Volume with unit radius and height equals pi/3.</summary>
    [Fact]
    public void Volume_UnitDimensions_ReturnsPiOver3()
    {
        var cone = new Cone3D(Point3D.Origin, Vector3D.UnitY, 1.0, 1.0);

        cone.Volume.Should().BeApproximately(System.Math.PI / 3.0, Tolerance);
    }
}
