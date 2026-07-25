namespace MathVerse.Geometry.Tests.Geometry3D;

/// <summary>Tests for the <see cref="Sphere3D"/> struct.</summary>
public class Sphere3DTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies Volume matches the formula 4/3 * pi * r^3.</summary>
    [Fact]
    public void Volume_MatchesFormula()
    {
        var sphere = new Sphere3D(Point3D.Origin, 2.0);

        double expected = (4.0 / 3.0) * System.Math.PI * 8.0;

        sphere.Volume.Should().BeApproximately(expected, Tolerance);
    }

    /// <summary>Verifies SurfaceArea matches the formula 4 * pi * r^2.</summary>
    [Fact]
    public void SurfaceArea_MatchesFormula()
    {
        var sphere = new Sphere3D(Point3D.Origin, 3.0);

        double expected = 4.0 * System.Math.PI * 9.0;

        sphere.SurfaceArea.Should().BeApproximately(expected, Tolerance);
    }

    /// <summary>Verifies Contains returns true for an interior point.</summary>
    [Fact]
    public void Contains_InteriorPoint_ReturnsTrue()
    {
        var sphere = new Sphere3D(Point3D.Origin, 5.0);
        var p = new Point3D(1, 1, 1);

        sphere.Contains(p).Should().BeTrue();
    }

    /// <summary>Verifies Contains returns true for a point on the surface.</summary>
    [Fact]
    public void Contains_SurfacePoint_ReturnsTrue()
    {
        var sphere = new Sphere3D(Point3D.Origin, 5.0);
        var p = new Point3D(5, 0, 0);

        sphere.Contains(p).Should().BeTrue();
    }

    /// <summary>Verifies Contains returns false for an exterior point.</summary>
    [Fact]
    public void Contains_ExteriorPoint_ReturnsFalse()
    {
        var sphere = new Sphere3D(Point3D.Origin, 5.0);
        var p = new Point3D(10, 0, 0);

        sphere.Contains(p).Should().BeFalse();
    }

    /// <summary>Verifies Contains(BoundingBox) for a small box inside the sphere.</summary>
    [Fact]
    public void Contains_BoundingBoxInside_ReturnsTrue()
    {
        var sphere = new Sphere3D(Point3D.Origin, 10.0);
        var box = new BoundingBox3D(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));

        sphere.Contains(box).Should().BeTrue();
    }

    /// <summary>Verifies Contains(BoundingBox) for a box that extends outside.</summary>
    [Fact]
    public void Contains_BoundingBoxOutside_ReturnsFalse()
    {
        var sphere = new Sphere3D(Point3D.Origin, 2.0);
        var box = new BoundingBox3D(new Point3D(-5, -5, -5), new Point3D(5, 5, 5));

        sphere.Contains(box).Should().BeFalse();
    }

    /// <summary>Verifies DistanceTo returns zero for a point inside the sphere.</summary>
    [Fact]
    public void DistanceTo_InsidePoint_ReturnsZero()
    {
        var sphere = new Sphere3D(Point3D.Origin, 5.0);
        var p = new Point3D(1, 0, 0);

        sphere.DistanceTo(p).Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies DistanceTo returns correct distance for an external point.</summary>
    [Fact]
    public void DistanceTo_ExternalPoint_ReturnsCorrectDistance()
    {
        var sphere = new Sphere3D(Point3D.Origin, 3.0);
        var p = new Point3D(8, 0, 0);

        sphere.DistanceTo(p).Should().BeApproximately(5.0, Tolerance);
    }

    /// <summary>Verifies Intersect with a secant line returns two intersection points.</summary>
    [Fact]
    public void IntersectLine_SecantLine_ReturnsTwoPoints()
    {
        var sphere = new Sphere3D(Point3D.Origin, 2.0);
        var line = new Line3D(new Point3D(-5, 0, 0), new Point3D(5, 0, 0));

        var (hit, points) = sphere.Intersect(line);

        hit.Should().BeTrue();
        points.Length.Should().Be(2);
    }

    /// <summary>Verifies Intersect with a tangent line returns one intersection point.</summary>
    [Fact]
    public void IntersectLine_TangentLine_ReturnsOnePoint()
    {
        var sphere = new Sphere3D(Point3D.Origin, 2.0);
        var line = new Line3D(new Point3D(-5, 2, 0), new Point3D(5, 2, 0));

        var (hit, points) = sphere.Intersect(line);

        hit.Should().BeTrue();
        points.Length.Should().Be(1);
    }

    /// <summary>Verifies Intersect with an external line returns no intersection.</summary>
    [Fact]
    public void IntersectLine_ExternalLine_ReturnsNoHit()
    {
        var sphere = new Sphere3D(Point3D.Origin, 2.0);
        var line = new Line3D(new Point3D(-5, 5, 0), new Point3D(5, 5, 0));

        var (hit, points) = sphere.Intersect(line);

        hit.Should().BeFalse();
        points.Length.Should().Be(0);
    }

    /// <summary>Verifies ClosestPointOnSurface returns the correct closest point.</summary>
    [Fact]
    public void ClosestPointOnSurface_ExternalPoint_ReturnsOnSurface()
    {
        var sphere = new Sphere3D(Point3D.Origin, 3.0);
        var p = new Point3D(10, 0, 0);

        var closest = sphere.ClosestPointOnSurface(p);

        closest.X.Should().BeApproximately(3.0, Tolerance);
        closest.Y.Should().BeApproximately(0.0, Tolerance);
        closest.Z.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies ClosestPointOnSurface for a point at the center returns a valid surface point.</summary>
    [Fact]
    public void ClosestPointOnSurface_CenterPoint_ReturnsSurfacePoint()
    {
        var sphere = new Sphere3D(Point3D.Origin, 5.0);

        var closest = sphere.ClosestPointOnSurface(Point3D.Origin);

        double dist = closest.DistanceTo(Point3D.Origin);
        dist.Should().BeApproximately(5.0, Tolerance);
    }

    /// <summary>Verifies ToBoundingBox encloses the sphere.</summary>
    [Fact]
    public void ToBoundingBox_EnclosesSphere()
    {
        var sphere = new Sphere3D(new Point3D(1, 2, 3), 4.0);

        var bbox = sphere.ToBoundingBox();

        bbox.Min.X.Should().BeApproximately(-3.0, Tolerance);
        bbox.Min.Y.Should().BeApproximately(-2.0, Tolerance);
        bbox.Min.Z.Should().BeApproximately(-1.0, Tolerance);
        bbox.Max.X.Should().BeApproximately(5.0, Tolerance);
        bbox.Max.Y.Should().BeApproximately(6.0, Tolerance);
        bbox.Max.Z.Should().BeApproximately(7.0, Tolerance);
    }

    /// <summary>Verifies ClosestPointOnSurface distance equals radius.</summary>
    [Fact]
    public void ClosestPointOnSurface_DistanceEqualsRadius()
    {
        var sphere = new Sphere3D(new Point3D(1, 2, 3), 7.0);
        var p = new Point3D(10, 10, 10);

        var closest = sphere.ClosestPointOnSurface(p);

        closest.DistanceTo(sphere.Center).Should().BeApproximately(sphere.Radius, Tolerance);
    }

    /// <summary>Verifies Volume with unit radius sphere equals 4/3 * pi.</summary>
    [Fact]
    public void Volume_UnitSphere_ReturnsFourThirdsPi()
    {
        var sphere = new Sphere3D(Point3D.Origin, 1.0);

        sphere.Volume.Should().BeApproximately(4.0 / 3.0 * System.Math.PI, Tolerance);
    }

    /// <summary>Verifies SurfaceArea with unit radius sphere equals 4 * pi.</summary>
    [Fact]
    public void SurfaceArea_UnitSphere_ReturnsFourPi()
    {
        var sphere = new Sphere3D(Point3D.Origin, 1.0);

        sphere.SurfaceArea.Should().BeApproximately(4.0 * System.Math.PI, Tolerance);
    }

    /// <summary>Verifies Intersect with secant line has intersection points at correct distances from center.</summary>
    [Fact]
    public void IntersectLine_SecantLine_PointsOnSurface()
    {
        var sphere = new Sphere3D(Point3D.Origin, 2.0);
        var line = new Line3D(new Point3D(-5, 0, 0), new Point3D(5, 0, 0));

        var (_, points) = sphere.Intersect(line);

        foreach (var pt in points)
        {
            pt.DistanceTo(sphere.Center).Should().BeApproximately(sphere.Radius, Tolerance);
        }
    }
}
