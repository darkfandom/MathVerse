namespace MathVerse.Geometry.Tests.CoordinateSystems;

/// <summary>Tests for CoordinateTransform static class.</summary>
public class CoordinateTransformTests
{
    private const double Precision = 1e-10;

    /// <summary>SphericalToCartesian at origin should return (0,0,0).</summary>
    [Fact]
    public void SphericalToCartesian_AtOrigin_ShouldReturnZero()
    {
        var c = CoordinateTransform.SphericalToCartesian(0, 0, 0);
        c.X.Should().BeApproximately(0.0, Precision);
        c.Y.Should().BeApproximately(0.0, Precision);
        c.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>SphericalToCartesian at north pole should return (0,0,R).</summary>
    [Fact]
    public void SphericalToCartesian_NorthPole_ShouldReturnOnZAxis()
    {
        var c = CoordinateTransform.SphericalToCartesian(5, 0, 0);
        c.X.Should().BeApproximately(0.0, Precision);
        c.Y.Should().BeApproximately(0.0, Precision);
        c.Z.Should().BeApproximately(5.0, Precision);
    }

    /// <summary>SphericalToCartesian at equator should return on XY plane.</summary>
    [Fact]
    public void SphericalToCartesian_Equator_ShouldBeOnXYPlane()
    {
        var c = CoordinateTransform.SphericalToCartesian(3, System.Math.PI / 4, System.Math.PI / 2);
        c.Z.Should().BeApproximately(0.0, Precision);
        c.X.Should().BeGreaterThan(0.0);
    }

    /// <summary>CylindricalToCartesian at theta=0 should return on X axis.</summary>
    [Fact]
    public void CylindricalToCartesian_ThetaZero_ShouldBeOnXAxis()
    {
        var c = CoordinateTransform.CylindricalToCartesian(4, 0, 7);
        c.X.Should().BeApproximately(4.0, Precision);
        c.Y.Should().BeApproximately(0.0, Precision);
        c.Z.Should().BeApproximately(7.0, Precision);
    }

    /// <summary>CylindricalToCartesian should preserve Z.</summary>
    [Fact]
    public void CylindricalToCartesian_ShouldPreserveZ()
    {
        var c = CoordinateTransform.CylindricalToCartesian(2, System.Math.PI / 3, 9);
        c.Z.Should().BeApproximately(9.0, Precision);
    }

    /// <summary>SphericalToCartesian round trip should recover original.</summary>
    [Fact]
    public void SphericalToCartesian_RoundTrip_ShouldRecoverOriginal()
    {
        var original = new CartesianCoordinate(1, 2, 3);
        var s = original.ToSpherical();
        var back = CoordinateTransform.SphericalToCartesian(s.R, s.Theta, s.Phi);
        back.X.Should().BeApproximately(original.X, Precision);
        back.Y.Should().BeApproximately(original.Y, Precision);
        back.Z.Should().BeApproximately(original.Z, Precision);
    }

    /// <summary>CylindricalToCartesian round trip should recover original.</summary>
    [Fact]
    public void CylindricalToCartesian_RoundTrip_ShouldRecoverOriginal()
    {
        var original = new CartesianCoordinate(3, 4, 5);
        var cyl = original.ToCylindrical();
        var back = CoordinateTransform.CylindricalToCartesian(cyl.R, cyl.Theta, cyl.Z);
        back.X.Should().BeApproximately(original.X, Precision);
        back.Y.Should().BeApproximately(original.Y, Precision);
        back.Z.Should().BeApproximately(original.Z, Precision);
    }

    /// <summary>LocalToWorld should place origin at the given position.</summary>
    [Fact]
    public void LocalToWorld_ShouldPlaceOriginAtPosition()
    {
        var origin = new Point3D(10, 20, 30);
        var forward = Vector3D.UnitZ;
        var up = Vector3D.UnitY;
        var l2w = CoordinateTransform.LocalToWorld(origin, forward, up);
        Point3D result = l2w.TransformPoint(Point3D.Origin);
        result.X.Should().BeApproximately(10.0, Precision);
        result.Y.Should().BeApproximately(20.0, Precision);
        result.Z.Should().BeApproximately(30.0, Precision);
    }

    /// <summary>LocalToWorld then WorldToLocal should restore original point.</summary>
    [Fact]
    public void WorldToLocal_InverseOfLocalToWorld_ShouldRestorePoint()
    {
        var origin = new Point3D(5, 10, 15);
        var forward = new Vector3D(1, 1, 0).Normalize();
        var up = Vector3D.UnitZ;
        var l2w = CoordinateTransform.LocalToWorld(origin, forward, up);
        var w2l = CoordinateTransform.WorldToLocal(origin, forward, up);
        var p = new Point3D(7, 3, 9);
        Point3D result = w2l.TransformPoint(l2w.TransformPoint(p));
        result.X.Should().BeApproximately(p.X, 1e-8);
        result.Y.Should().BeApproximately(p.Y, 1e-8);
        result.Z.Should().BeApproximately(p.Z, 1e-8);
    }

    /// <summary>WorldToLocal times LocalToWorld should approximate identity.</summary>
    [Fact]
    public void WorldToLocal_TimesLocalToWorld_ShouldApproxIdentity()
    {
        var origin = new Point3D(1, 2, 3);
        var forward = Vector3D.UnitZ;
        var up = Vector3D.UnitY;
        var l2w = CoordinateTransform.LocalToWorld(origin, forward, up);
        var w2l = CoordinateTransform.WorldToLocal(origin, forward, up);
        var product = w2l.Multiply(l2w);
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                product[i, j].Should().BeApproximately(i == j ? 1.0 : 0.0, 1e-8);
    }

    /// <summary>LocalToWorld determinant should be non-zero.</summary>
    [Fact]
    public void LocalToWorld_Determinant_ShouldBeNonZero()
    {
        var l2w = CoordinateTransform.LocalToWorld(
            new Point3D(0, 0, 0), Vector3D.UnitZ, Vector3D.UnitY);
        System.Math.Abs(l2w.Determinant()).Should().BeGreaterThan(1e-10);
    }

    /// <summary>CylindricalToCartesian with theta=PI/2 should be on Y axis.</summary>
    [Fact]
    public void CylindricalToCartesian_ThetaPiOver2_ShouldBeOnYAxis()
    {
        var c = CoordinateTransform.CylindricalToCartesian(3, System.Math.PI / 2, 0);
        c.X.Should().BeApproximately(0.0, Precision);
        c.Y.Should().BeApproximately(3.0, Precision);
        c.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>SphericalToCartesian at south pole should give negative Z.</summary>
    [Fact]
    public void SphericalToCartesian_SouthPole_ShouldGiveNegativeZ()
    {
        var c = CoordinateTransform.SphericalToCartesian(5, 0, System.Math.PI);
        c.X.Should().BeApproximately(0.0, Precision);
        c.Y.Should().BeApproximately(0.0, Precision);
        c.Z.Should().BeApproximately(-5.0, Precision);
    }

    /// <summary>LocalToWorld forward direction should map to -Z in local space.</summary>
    [Fact]
    public void LocalToWorld_ForwardDirection_ShouldMapCorrectly()
    {
        var origin = Point3D.Origin;
        var forward = Vector3D.UnitZ;
        var up = Vector3D.UnitY;
        var l2w = CoordinateTransform.LocalToWorld(origin, forward, up);
        var pointAhead = new Point3D(0, 0, 5);
        Point3D result = l2w.TransformPoint(pointAhead);
        result.Z.Should().BeLessThan(0.0);
    }
}
