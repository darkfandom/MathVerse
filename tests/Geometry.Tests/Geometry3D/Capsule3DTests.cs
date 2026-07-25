namespace MathVerse.Geometry.Tests.Geometry3D;

/// <summary>Tests for the <see cref="Capsule3D"/> struct.</summary>
public class Capsule3DTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies Length equals the distance between the two centers.</summary>
    [Fact]
    public void Length_EqualsDistanceBetweenCenters()
    {
        var capsule = new Capsule3D(new Point3D(0, 0, 0), new Point3D(3, 4, 0), 1.0);

        capsule.Length.Should().BeApproximately(5.0, Tolerance);
    }

    /// <summary>Verifies Volume with overlapping spheres (d &lt; 2r) uses spherical formula.</summary>
    [Fact]
    public void Volume_OverlappingSpheres_UsesSphericalVolume()
    {
        var capsule = new Capsule3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0), 1.0);

        double expected = (4.0 / 3.0) * System.Math.PI;

        capsule.Volume.Should().BeApproximately(expected, Tolerance);
    }

    /// <summary>Verifies Volume with separated spheres adds cylindrical part.</summary>
    [Fact]
    public void Volume_SeparatedSpheres_IncludesCylinderPart()
    {
        var capsule = new Capsule3D(new Point3D(0, 0, 0), new Point3D(4, 0, 0), 1.0);

        double cylLen = 4.0 - 2.0;
        double cylVol = System.Math.PI * 1.0 * 1.0 * cylLen;
        double sphereVol = (4.0 / 3.0) * System.Math.PI;
        double expected = cylVol + sphereVol;

        capsule.Volume.Should().BeApproximately(expected, Tolerance);
    }

    /// <summary>Verifies SurfaceArea includes the lateral cylinder area and hemisphere caps.</summary>
    [Fact]
    public void SurfaceArea_SeparatedSpheres_IncludesLateralAndCaps()
    {
        var capsule = new Capsule3D(new Point3D(0, 0, 0), new Point3D(6, 0, 0), 1.0);

        double cylLen = 6.0 - 2.0;
        double expected = 2.0 * System.Math.PI * 1.0 * cylLen + 4.0 * System.Math.PI;

        capsule.SurfaceArea.Should().BeApproximately(expected, Tolerance);
    }

    /// <summary>Verifies ToBoundingBox encloses both spheres.</summary>
    [Fact]
    public void ToBoundingBox_EnclosesBothSpheres()
    {
        var capsule = new Capsule3D(new Point3D(0, 0, 0), new Point3D(4, 0, 0), 1.0);

        var bbox = capsule.ToBoundingBox();

        bbox.Min.X.Should().BeApproximately(-1.0, Tolerance);
        bbox.Max.X.Should().BeApproximately(5.0, Tolerance);
        bbox.Min.Y.Should().BeApproximately(-1.0, Tolerance);
        bbox.Max.Y.Should().BeApproximately(1.0, Tolerance);
        bbox.Min.Z.Should().BeApproximately(-1.0, Tolerance);
        bbox.Max.Z.Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies Contains returns true for the center of sphere A.</summary>
    [Fact]
    public void Contains_CenterA_ReturnsTrue()
    {
        var capsule = new Capsule3D(new Point3D(0, 0, 0), new Point3D(4, 0, 0), 1.0);

        capsule.Contains(new Point3D(0, 0, 0)).Should().BeTrue();
    }

    /// <summary>Verifies Contains returns true for the center of sphere B.</summary>
    [Fact]
    public void Contains_CenterB_ReturnsTrue()
    {
        var capsule = new Capsule3D(new Point3D(0, 0, 0), new Point3D(4, 0, 0), 1.0);

        capsule.Contains(new Point3D(4, 0, 0)).Should().BeTrue();
    }

    /// <summary>Verifies Contains returns true for a point on the central axis.</summary>
    [Fact]
    public void Contains_MidAxisPoint_ReturnsTrue()
    {
        var capsule = new Capsule3D(new Point3D(0, 0, 0), new Point3D(4, 0, 0), 1.0);

        capsule.Contains(new Point3D(2, 0, 0)).Should().BeTrue();
    }

    /// <summary>Verifies Contains returns false for a distant point.</summary>
    [Fact]
    public void Contains_DistantPoint_ReturnsFalse()
    {
        var capsule = new Capsule3D(new Point3D(0, 0, 0), new Point3D(4, 0, 0), 1.0);

        capsule.Contains(new Point3D(2, 5, 0)).Should().BeFalse();
    }

    /// <summary>Verifies ClosestPoint for a point on the axis returns a point at the correct radius distance.</summary>
    [Fact]
    public void ClosestPoint_OnAxis_ReturnsOnSurface()
    {
        var capsule = new Capsule3D(new Point3D(0, 0, 0), new Point3D(4, 0, 0), 2.0);

        var closest = capsule.ClosestPoint(new Point3D(2, 0, 0));

        closest.DistanceTo(new Point3D(2, 0, 0)).Should().BeApproximately(2.0, Tolerance);
    }

    /// <summary>Verifies ClosestPoint for a point outside returns a surface point.</summary>
    [Fact]
    public void ClosestPoint_Outside_ReturnsOnSurface()
    {
        var capsule = new Capsule3D(new Point3D(0, 0, 0), new Point3D(4, 0, 0), 1.0);
        var p = new Point3D(2, 5, 0);

        var closest = capsule.ClosestPoint(p);

        Point3D axisPoint = new Point3D(2, 0, 0);
        closest.DistanceTo(axisPoint).Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies ClosestPoint for a point at sphere center returns a point on the cap.</summary>
    [Fact]
    public void ClosestPoint_AtCenterA_ReturnsOnCapSurface()
    {
        var capsule = new Capsule3D(new Point3D(0, 0, 0), new Point3D(4, 0, 0), 2.0);

        var closest = capsule.ClosestPoint(new Point3D(0, 0, 0));

        closest.DistanceTo(new Point3D(0, 0, 0)).Should().BeApproximately(2.0, Tolerance);
    }

    /// <summary>Verifies Length of a zero-length capsule equals zero.</summary>
    [Fact]
    public void Length_ZeroLengthCapsule_ReturnsZero()
    {
        var capsule = new Capsule3D(new Point3D(3, 3, 3), new Point3D(3, 3, 3), 1.0);

        capsule.Length.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies Volume of a zero-length capsule equals a sphere.</summary>
    [Fact]
    public void Volume_ZeroLengthCapsule_EqualsSphere()
    {
        var capsule = new Capsule3D(new Point3D(3, 3, 3), new Point3D(3, 3, 3), 2.0);
        double sphereVol = (4.0 / 3.0) * System.Math.PI * 8.0;

        capsule.Volume.Should().BeApproximately(sphereVol, Tolerance);
    }

    /// <summary>Verifies SurfaceArea of a zero-length capsule equals sphere area.</summary>
    [Fact]
    public void SurfaceArea_ZeroLengthCapsule_EqualsSphereArea()
    {
        var capsule = new Capsule3D(new Point3D(3, 3, 3), new Point3D(3, 3, 3), 2.0);
        double sphereArea = 4.0 * System.Math.PI * 4.0;

        capsule.SurfaceArea.Should().BeApproximately(sphereArea, Tolerance);
    }
}
