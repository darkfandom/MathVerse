namespace MathVerse.Geometry.Tests.Geometry3D;

/// <summary>Tests for the <see cref="Line3D"/> struct.</summary>
public class Line3DTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies Direction returns a normalized vector from P1 to P2.</summary>
    [Fact]
    public void Direction_ReturnsNormalizedVector()
    {
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(3, 0, 0));

        var dir = line.Direction;

        dir.Length.Should().BeApproximately(1.0, Tolerance);
        dir.X.Should().BeApproximately(1.0, Tolerance);
        dir.Y.Should().BeApproximately(0.0, Tolerance);
        dir.Z.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies Direction is correct for a diagonal line.</summary>
    [Fact]
    public void Direction_DiagonalLine_ReturnsCorrectDirection()
    {
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(1, 1, 1));

        var dir = line.Direction;

        dir.Length.Should().BeApproximately(1.0, Tolerance);
        dir.X.Should().BeApproximately(1.0 / System.Math.Sqrt(3), Tolerance);
        dir.Y.Should().BeApproximately(1.0 / System.Math.Sqrt(3), Tolerance);
        dir.Z.Should().BeApproximately(1.0 / System.Math.Sqrt(3), Tolerance);
    }

    /// <summary>Verifies Length returns the Euclidean distance between endpoints.</summary>
    [Fact]
    public void Length_ReturnsCorrectDistance()
    {
        var line = new Line3D(new Point3D(1, 2, 3), new Point3D(4, 6, 3));

        line.Length.Should().BeApproximately(5.0, Tolerance);
    }

    /// <summary>Verifies PointAt at t=0 returns P1.</summary>
    [Fact]
    public void PointAt_AtZero_ReturnsP1()
    {
        var line = new Line3D(new Point3D(1, 2, 3), new Point3D(4, 6, 3));

        var result = line.PointAt(0.0);

        result.Should().Be(line.P1);
    }

    /// <summary>Verifies PointAt at t=1 returns P2.</summary>
    [Fact]
    public void PointAt_AtOne_ReturnsP2()
    {
        var line = new Line3D(new Point3D(1, 2, 3), new Point3D(4, 6, 3));

        var result = line.PointAt(1.0);

        result.Should().Be(line.P2);
    }

    /// <summary>Verifies PointAt at t=0.5 returns the midpoint.</summary>
    [Fact]
    public void PointAt_AtHalf_ReturnsMidpoint()
    {
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(10, 20, 30));

        var result = line.PointAt(0.5);

        result.X.Should().BeApproximately(5.0, Tolerance);
        result.Y.Should().BeApproximately(10.0, Tolerance);
        result.Z.Should().BeApproximately(15.0, Tolerance);
    }

    /// <summary>Verifies DistanceTo returns zero for a point on the line.</summary>
    [Fact]
    public void DistanceTo_PointOnLine_ReturnsZero()
    {
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(10, 0, 0));
        var p = new Point3D(5, 0, 0);

        line.DistanceTo(p).Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies DistanceTo returns correct distance for an off-line point.</summary>
    [Fact]
    public void DistanceTo_OffLinePoint_ReturnsCorrectDistance()
    {
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(10, 0, 0));
        var p = new Point3D(5, 3, 0);

        line.DistanceTo(p).Should().BeApproximately(3.0, Tolerance);
    }

    /// <summary>Verifies DistanceTo clamps beyond endpoints.</summary>
    [Fact]
    public void DistanceTo_BeyondSegment_ClampsToEndpoint()
    {
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(10, 0, 0));
        var p = new Point3D(15, 0, 0);

        line.DistanceTo(p).Should().BeApproximately(5.0, Tolerance);
    }

    /// <summary>Verifies ClosestPoint returns the nearest point on the segment.</summary>
    [Fact]
    public void ClosestPoint_PerpendicularDrop_ReturnsCorrectPoint()
    {
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(10, 0, 0));
        var p = new Point3D(5, 3, 0);

        var closest = line.ClosestPoint(p);

        closest.X.Should().BeApproximately(5.0, Tolerance);
        closest.Y.Should().BeApproximately(0.0, Tolerance);
        closest.Z.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies ClosestPoint clamps when query is beyond the segment.</summary>
    [Fact]
    public void ClosestPoint_BeyondSegment_ClampsToNearestEndpoint()
    {
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(10, 0, 0));
        var p = new Point3D(-5, 0, 0);

        var closest = line.ClosestPoint(p);

        closest.Should().Be(line.P1);
    }

    /// <summary>Verifies Intersect with a plane that crosses the segment.</summary>
    [Fact]
    public void IntersectPlane_CrossingSegment_ReturnsHit()
    {
        var line = new Line3D(new Point3D(0, 0, -5), new Point3D(0, 0, 5));
        var plane = new Plane3D(new Point3D(0, 0, 0), Vector3D.UnitZ);

        var (hit, point) = line.Intersect(plane);

        hit.Should().BeTrue();
        point.X.Should().BeApproximately(0.0, Tolerance);
        point.Y.Should().BeApproximately(0.0, Tolerance);
        point.Z.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies Intersect with a parallel plane returns no hit.</summary>
    [Fact]
    public void IntersectPlane_ParallelPlane_ReturnsNoHit()
    {
        var line = new Line3D(new Point3D(0, 0, -5), new Point3D(0, 0, 5));
        var plane = new Plane3D(new Point3D(10, 0, 0), Vector3D.UnitX);

        var (hit, _) = line.Intersect(plane);

        hit.Should().BeFalse();
    }

    /// <summary>Verifies Intersect with a parallel line returns no hit.</summary>
    [Fact]
    public void IntersectLine_ParallelLines_ReturnsNoHit()
    {
        var line1 = new Line3D(new Point3D(0, 0, 0), new Point3D(10, 0, 0));
        var line2 = new Line3D(new Point3D(0, 1, 0), new Point3D(10, 1, 0));

        var (hit, _, dist) = line1.Intersect(line2);

        hit.Should().BeFalse();
        dist.Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies Intersect with crossing lines returns a hit.</summary>
    [Fact]
    public void IntersectLine_CrossingLines_ReturnsHit()
    {
        var line1 = new Line3D(new Point3D(-1, 0, 0), new Point3D(1, 0, 0));
        var line2 = new Line3D(new Point3D(0, -1, 0), new Point3D(0, 1, 0));

        var (hit, point, dist) = line1.Intersect(line2);

        hit.Should().BeTrue();
        dist.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies ToBoundingBox encloses both endpoints.</summary>
    [Fact]
    public void ToBoundingBox_EnclosesEndpoints()
    {
        var line = new Line3D(new Point3D(-1, -2, -3), new Point3D(4, 5, 6));

        var bbox = line.ToBoundingBox();

        bbox.Min.X.Should().BeApproximately(-1.0, Tolerance);
        bbox.Min.Y.Should().BeApproximately(-2.0, Tolerance);
        bbox.Min.Z.Should().BeApproximately(-3.0, Tolerance);
        bbox.Max.X.Should().BeApproximately(4.0, Tolerance);
        bbox.Max.Y.Should().BeApproximately(5.0, Tolerance);
        bbox.Max.Z.Should().BeApproximately(6.0, Tolerance);
    }

    /// <summary>Verifies ToBoundingBox with reversed endpoints.</summary>
    [Fact]
    public void ToBoundingBox_ReversedEndpoints_CorrectBoundingBox()
    {
        var line = new Line3D(new Point3D(5, 5, 5), new Point3D(-1, -1, -1));

        var bbox = line.ToBoundingBox();

        bbox.Min.X.Should().BeApproximately(-1.0, Tolerance);
        bbox.Max.X.Should().BeApproximately(5.0, Tolerance);
    }

    /// <summary>Verifies DistanceTo is symmetric.</summary>
    [Fact]
    public void DistanceTo_IsSymmetricForSamePoint()
    {
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(10, 0, 0));
        var p = new Point3D(5, 3, 0);

        line.DistanceTo(p).Should().BeApproximately(p.DistanceTo(line.ClosestPoint(p)), Tolerance);
    }
}
