namespace MathVerse.Geometry.Tests.Picking;

/// <summary>Tests for the <see cref="Ray"/> struct.</summary>
public class RayTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies that PointAt at t=0 returns the origin.</summary>
    [Fact]
    public void PointAt_AtZero_ReturnsOrigin()
    {
        var ray = new Ray(new Point3D(1, 2, 3), new Vector3D(0, 0, 1));

        var p = ray.PointAt(0.0);

        p.Should().Be(new Point3D(1, 2, 3));
    }

    /// <summary>Verifies that PointAt at t=1 returns origin plus direction.</summary>
    [Fact]
    public void PointAt_AtOne_ReturnsOriginPlusDirection()
    {
        var ray = new Ray(new Point3D(0, 0, 0), new Vector3D(1, 2, 3));

        var p = ray.PointAt(1.0);

        p.X.Should().BeApproximately(1.0, Tolerance);
        p.Y.Should().BeApproximately(2.0, Tolerance);
        p.Z.Should().BeApproximately(3.0, Tolerance);
    }

    /// <summary>Verifies that ClosestParameter returns the correct projection.</summary>
    [Fact]
    public void ClosestParameter_ProjectedPoint()
    {
        var ray = new Ray(Point3D.Origin, Vector3D.UnitX);
        var point = new Point3D(5, 0, 0);

        double t = ray.ClosestParameter(point);

        t.Should().BeApproximately(5.0, Tolerance);
    }

    /// <summary>Verifies that ClosestParameter with perpendicular point returns 0.</summary>
    [Fact]
    public void ClosestParameter_PerpendicularPoint_ReturnsZero()
    {
        var ray = new Ray(Point3D.Origin, Vector3D.UnitX);
        var point = new Point3D(0, 5, 0);

        double t = ray.ClosestParameter(point);

        t.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies that ClosestPoint returns the nearest point on the ray.</summary>
    [Fact]
    public void ClosestPoint_ReturnsNearestPoint()
    {
        var ray = new Ray(Point3D.Origin, Vector3D.UnitX);
        var point = new Point3D(3, 4, 0);

        var closest = ray.ClosestPoint(point);

        closest.X.Should().BeApproximately(3.0, Tolerance);
        closest.Y.Should().BeApproximately(0.0, Tolerance);
        closest.Z.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies that DistanceTo for a point on the ray returns zero.</summary>
    [Fact]
    public void DistanceTo_PointOnRay_ReturnsZero()
    {
        var ray = new Ray(Point3D.Origin, Vector3D.UnitX);
        var point = new Point3D(5, 0, 0);

        double dist = ray.DistanceTo(point);

        dist.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies that DistanceTo for a perpendicular point returns the perpendicular distance.</summary>
    [Fact]
    public void DistanceTo_PerpendicularPoint_ReturnsDistance()
    {
        var ray = new Ray(Point3D.Origin, Vector3D.UnitX);
        var point = new Point3D(0, 3, 0);

        double dist = ray.DistanceTo(point);

        dist.Should().BeApproximately(3.0, Tolerance);
    }

    /// <summary>Verifies that a ray with perpendicular direction computes correct closest parameter.</summary>
    [Fact]
    public void PerpendicularRay_ClosestParameter()
    {
        var ray = new Ray(new Point3D(1, 0, 0), new Vector3D(0, 1, 0));
        var point = new Point3D(1, 5, 0);

        double t = ray.ClosestParameter(point);

        t.Should().BeApproximately(5.0, Tolerance);
    }

    /// <summary>Verifies that two parallel rays have consistent PointAt behavior.</summary>
    [Fact]
    public void ParallelRay_PointAt_Consistent()
    {
        var ray1 = new Ray(Point3D.Origin, Vector3D.UnitZ);
        var ray2 = new Ray(new Point3D(10, 0, 0), Vector3D.UnitZ);

        var p1 = ray1.PointAt(3.0);
        var p2 = ray2.PointAt(3.0);

        p1.Z.Should().BeApproximately(p2.Z, Tolerance);
    }

    /// <summary>Verifies PointAt with negative parameter.</summary>
    [Fact]
    public void PointAt_NegativeT()
    {
        var ray = new Ray(new Point3D(5, 0, 0), Vector3D.UnitX);

        var p = ray.PointAt(-2.0);

        p.X.Should().BeApproximately(3.0, Tolerance);
    }

    /// <summary>Verifies DistanceTo with a point behind the origin returns distance to origin.</summary>
    [Fact]
    public void DistanceTo_PointBehindOrigin_ReturnsToOrigin()
    {
        var ray = new Ray(new Point3D(1, 0, 0), Vector3D.UnitX);
        var point = new Point3D(-5, 0, 0);

        double dist = ray.DistanceTo(point);

        dist.Should().BeApproximately(6.0, Tolerance);
    }

    /// <summary>Verifies ClosestParameter for a point exactly at the origin.</summary>
    [Fact]
    public void ClosestParameter_AtOrigin()
    {
        var ray = new Ray(new Point3D(2, 3, 4), Vector3D.UnitX);
        var point = new Point3D(2, 3, 4);

        double t = ray.ClosestParameter(point);

        t.Should().BeApproximately(0.0, Tolerance);
    }
}
