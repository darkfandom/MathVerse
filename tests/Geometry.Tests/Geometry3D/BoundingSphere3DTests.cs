namespace MathVerse.Geometry.Tests.Geometry3D;

/// <summary>Tests for the <see cref="BoundingSphere3D"/> struct.</summary>
public class BoundingSphere3DTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies Contains returns true for an interior point.</summary>
    [Fact]
    public void Contains_InteriorPoint_ReturnsTrue()
    {
        var sphere = new BoundingSphere3D(Point3D.Origin, 5.0);
        var p = new Point3D(1, 1, 1);

        sphere.Contains(p).Should().BeTrue();
    }

    /// <summary>Verifies Contains returns false for an exterior point.</summary>
    [Fact]
    public void Contains_ExteriorPoint_ReturnsFalse()
    {
        var sphere = new BoundingSphere3D(Point3D.Origin, 2.0);
        var p = new Point3D(10, 0, 0);

        sphere.Contains(p).Should().BeFalse();
    }

    /// <summary>Verifies Contains(BoundingSphere3D) for a smaller sphere inside.</summary>
    [Fact]
    public void ContainsSphere_SmallerInside_ReturnsTrue()
    {
        var outer = new BoundingSphere3D(Point3D.Origin, 10.0);
        var inner = new BoundingSphere3D(new Point3D(1, 0, 0), 1.0);

        outer.Contains(inner).Should().BeTrue();
    }

    /// <summary>Verifies Contains(BoundingSphere3D) for a larger sphere returns false.</summary>
    [Fact]
    public void ContainsSphere_Larger_ReturnsFalse()
    {
        var small = new BoundingSphere3D(Point3D.Origin, 1.0);
        var big = new BoundingSphere3D(Point3D.Origin, 10.0);

        small.Contains(big).Should().BeFalse();
    }

    /// <summary>Verifies Intersects returns true for overlapping spheres.</summary>
    [Fact]
    public void Intersects_Overlapping_ReturnsTrue()
    {
        var a = new BoundingSphere3D(Point3D.Origin, 3.0);
        var b = new BoundingSphere3D(new Point3D(4, 0, 0), 3.0);

        a.Intersects(b).Should().BeTrue();
    }

    /// <summary>Verifies Intersects returns false for distant spheres.</summary>
    [Fact]
    public void Intersects_Distant_ReturnsFalse()
    {
        var a = new BoundingSphere3D(Point3D.Origin, 1.0);
        var b = new BoundingSphere3D(new Point3D(100, 0, 0), 1.0);

        a.Intersects(b).Should().BeFalse();
    }

    /// <summary>Verifies Intersects returns true for touching spheres.</summary>
    [Fact]
    public void Intersects_TangentSpheres_ReturnsTrue()
    {
        var a = new BoundingSphere3D(Point3D.Origin, 3.0);
        var b = new BoundingSphere3D(new Point3D(6, 0, 0), 3.0);

        a.Intersects(b).Should().BeTrue();
    }

    /// <summary>Verifies Union encloses both spheres.</summary>
    [Fact]
    public void Union_EnclosesBothSpheres()
    {
        var a = new BoundingSphere3D(Point3D.Origin, 2.0);
        var b = new BoundingSphere3D(new Point3D(10, 0, 0), 2.0);

        var result = a.Union(b);

        result.Contains(new Point3D(0, 0, 0)).Should().BeTrue();
        result.Contains(new Point3D(10, 0, 0)).Should().BeTrue();
    }

    /// <summary>Verifies Union returns the enclosing sphere when one contains the other.</summary>
    [Fact]
    public void Union_OneContainsOther_ReturnsLarger()
    {
        var big = new BoundingSphere3D(Point3D.Origin, 10.0);
        var small = new BoundingSphere3D(new Point3D(1, 0, 0), 1.0);

        var result = big.Union(small);

        result.Radius.Should().BeApproximately(10.0, Tolerance);
    }

    /// <summary>Verifies FromPoints creates a sphere enclosing all points.</summary>
    [Fact]
    public void FromPoints_EnclosesAllPoints()
    {
        var points = new[]
        {
            new Point3D(-1, 0, 0),
            new Point3D(1, 0, 0),
            new Point3D(0, 1, 0),
            new Point3D(0, -1, 0)
        };

        var sphere = BoundingSphere3D.FromPoints(points);

        foreach (var p in points)
        {
            sphere.Contains(p).Should().BeTrue();
        }
    }

    /// <summary>Verifies FromPoints with single point creates zero-radius sphere.</summary>
    [Fact]
    public void FromPoints_SinglePoint_ZeroRadius()
    {
        var p = new Point3D(5, 5, 5);
        var sphere = BoundingSphere3D.FromPoints(new[] { p });

        sphere.Center.Should().Be(p);
        sphere.Radius.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies FromPoints with empty collection returns origin with zero radius.</summary>
    [Fact]
    public void FromPoints_EmptyCollection_ReturnsOriginZeroRadius()
    {
        var sphere = BoundingSphere3D.FromPoints(Array.Empty<Point3D>());

        sphere.Center.Should().Be(Point3D.Origin);
        sphere.Radius.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies Union center lies between the two sphere centers.</summary>
    [Fact]
    public void Union_CenterBetweenInputs()
    {
        var a = new BoundingSphere3D(new Point3D(0, 0, 0), 2.0);
        var b = new BoundingSphere3D(new Point3D(10, 0, 0), 2.0);

        var result = a.Union(b);

        result.Center.X.Should().BeGreaterThanOrEqualTo(0.0);
        result.Center.X.Should().BeLessThanOrEqualTo(10.0);
    }

    /// <summary>Verifies Intersects is symmetric.</summary>
    [Fact]
    public void Intersects_IsSymmetric()
    {
        var a = new BoundingSphere3D(Point3D.Origin, 3.0);
        var b = new BoundingSphere3D(new Point3D(4, 0, 0), 3.0);

        a.Intersects(b).Should().Be(b.Intersects(a));
    }

    /// <summary>Verifies Contains for a point on the surface returns true.</summary>
    [Fact]
    public void Contains_SurfacePoint_ReturnsTrue()
    {
        var sphere = new BoundingSphere3D(Point3D.Origin, 5.0);
        var p = new Point3D(5, 0, 0);

        sphere.Contains(p).Should().BeTrue();
    }
}
