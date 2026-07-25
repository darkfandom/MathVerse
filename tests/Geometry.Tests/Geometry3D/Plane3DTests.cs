namespace MathVerse.Geometry.Tests.Geometry3D;

/// <summary>Tests for the <see cref="Plane3D"/> struct.</summary>
public class Plane3DTests
{
    private const double Tolerance = 1e-10;

    private static readonly Plane3D XYPlane = new(Point3D.Origin, Vector3D.UnitZ);
    private static readonly Plane3D XZPlane = new(Point3D.Origin, Vector3D.UnitY);
    private static readonly Plane3D YZPlane = new(Point3D.Origin, Vector3D.UnitX);

    /// <summary>Verifies SignedDistanceTo is positive on the normal side.</summary>
    [Fact]
    public void SignedDistanceTo_NormalSide_ReturnsPositive()
    {
        var p = new Point3D(0, 0, 5);

        XYPlane.SignedDistanceTo(p).Should().BeApproximately(5.0, Tolerance);
    }

    /// <summary>Verifies SignedDistanceTo is negative on the opposite side.</summary>
    [Fact]
    public void SignedDistanceTo_OppositeSide_ReturnsNegative()
    {
        var p = new Point3D(0, 0, -5);

        XYPlane.SignedDistanceTo(p).Should().BeApproximately(-5.0, Tolerance);
    }

    /// <summary>Verifies SignedDistanceTo is zero for a point on the plane.</summary>
    [Fact]
    public void SignedDistanceTo_OnPlane_ReturnsZero()
    {
        var p = new Point3D(3, 7, 0);

        XYPlane.SignedDistanceTo(p).Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies DistanceTo returns absolute value.</summary>
    [Fact]
    public void DistanceTo_ReturnsAbsoluteValue()
    {
        var p = new Point3D(0, 0, -5);

        XYPlane.DistanceTo(p).Should().BeApproximately(5.0, Tolerance);
    }

    /// <summary>Verifies Contains returns true for a point on the plane.</summary>
    [Fact]
    public void Contains_PointOnPlane_ReturnsTrue()
    {
        var p = new Point3D(7, 3, 0);

        XYPlane.Contains(p).Should().BeTrue();
    }

    /// <summary>Verifies Contains returns false for a point off the plane.</summary>
    [Fact]
    public void Contains_PointOffPlane_ReturnsFalse()
    {
        var p = new Point3D(7, 3, 0.1);

        XYPlane.Contains(p).Should().BeFalse();
    }

    /// <summary>Verifies Project projects a point onto the plane along the normal.</summary>
    [Fact]
    public void Project_PointAbovePlane_ProjectsDownward()
    {
        var p = new Point3D(5, 5, 10);

        var projected = XYPlane.Project(p);

        projected.X.Should().BeApproximately(5.0, Tolerance);
        projected.Y.Should().BeApproximately(5.0, Tolerance);
        projected.Z.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies Project onto a point already on the plane returns the same point.</summary>
    [Fact]
    public void Project_PointOnPlane_ReturnsSamePoint()
    {
        var p = new Point3D(3, 7, 0);

        var projected = XYPlane.Project(p);

        projected.Should().Be(p);
    }

    /// <summary>Verifies Intersect(Line) with a line crossing the plane.</summary>
    [Fact]
    public void IntersectLine_CrossingLine_ReturnsHit()
    {
        var line = new Line3D(new Point3D(0, 0, -5), new Point3D(0, 0, 5));

        var (hit, point) = XYPlane.Intersect(line);

        hit.Should().BeTrue();
        point.X.Should().BeApproximately(0.0, Tolerance);
        point.Y.Should().BeApproximately(0.0, Tolerance);
        point.Z.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies Intersect(Line) with a parallel line returns no hit.</summary>
    [Fact]
    public void IntersectLine_ParallelLine_ReturnsNoHit()
    {
        var line = new Line3D(new Point3D(0, 5, 0), new Point3D(10, 5, 0));

        var (hit, _) = XYPlane.Intersect(line);

        hit.Should().BeFalse();
    }

    /// <summary>Verifies Intersect(Plane) with non-parallel planes returns a line.</summary>
    [Fact]
    public void IntersectPlane_NonParallel_ReturnsLine()
    {
        var (hit, line) = XYPlane.Intersect(YZPlane);

        hit.Should().BeTrue();
        line.Direction.Length.Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies Intersect(Plane) with parallel planes returns no hit.</summary>
    [Fact]
    public void IntersectPlane_ParallelPlanes_ReturnsNoHit()
    {
        var parallel = new Plane3D(new Point3D(0, 0, 5), Vector3D.UnitZ);

        var (hit, _) = XYPlane.Intersect(parallel);

        hit.Should().BeFalse();
    }

    /// <summary>Verifies Transform with identity transform returns the same plane.</summary>
    [Fact]
    public void Transform_Identity_ReturnsSamePlane()
    {
        var plane = new Plane3D(new Point3D(1, 2, 3), Vector3D.UnitZ);

        var transformed = plane.Transform(Transform3D.Identity);

        transformed.Point.Should().Be(plane.Point);
    }

    /// <summary>Verifies Transform with translation moves the point but keeps normal.</summary>
    [Fact]
    public void Transform_Translation_MovesPointKeepsNormal()
    {
        var t = Transform3D.Translation(5, 0, 0);

        var transformed = XYPlane.Transform(t);

        transformed.Point.X.Should().BeApproximately(5.0, Tolerance);
        transformed.Point.Y.Should().BeApproximately(0.0, Tolerance);
        transformed.Point.Z.Should().BeApproximately(0.0, Tolerance);
        transformed.Normal.Z.Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies FromTriangle creates a plane containing the triangle.</summary>
    [Fact]
    public void FromTriangle_PlaneContainsVertices()
    {
        var tri = new Triangle3D(
            new Point3D(0, 0, 0),
            new Point3D(1, 0, 0),
            new Point3D(0, 1, 0));

        var plane = Plane3D.FromTriangle(tri);

        plane.Contains(tri.A).Should().BeTrue();
        plane.Contains(tri.B).Should().BeTrue();
        plane.Contains(tri.C).Should().BeTrue();
    }

    /// <summary>Verifies FromPoints creates a plane through three non-collinear points.</summary>
    [Fact]
    public void FromPoints_ThreeNonCollinearPoints_CreatesPlane()
    {
        var a = new Point3D(0, 0, 0);
        var b = new Point3D(1, 0, 0);
        var c = new Point3D(0, 1, 0);

        var plane = Plane3D.FromPoints(a, b, c);

        plane.Contains(a).Should().BeTrue();
        plane.Contains(b).Should().BeTrue();
        plane.Contains(c).Should().BeTrue();
    }

    /// <summary>Verifies parallel planes have the same normal direction.</summary>
    [Fact]
    public void ParallelPlanes_HaveSameNormalDirection()
    {
        var p1 = new Plane3D(new Point3D(0, 0, 0), Vector3D.UnitZ);
        var p2 = new Plane3D(new Point3D(0, 0, 10), Vector3D.UnitZ);

        p1.Normal.Dot(p2.Normal).Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies coincident planes are detected via Contains.</summary>
    [Fact]
    public void CoincidentPlanes_ContainSamePoints()
    {
        var p1 = new Plane3D(new Point3D(0, 0, 0), Vector3D.UnitZ);
        var p2 = new Plane3D(new Point3D(5, 0, 0), Vector3D.UnitZ);

        p1.Contains(new Point3D(3, 7, 0)).Should().BeTrue();
        p2.Contains(new Point3D(3, 7, 0)).Should().BeTrue();
    }

    /// <summary>Verifies perpendicular planes have normal dot product of zero.</summary>
    [Fact]
    public void PerpendicularPlanes_NormalDotProductIsZero()
    {
        var p1 = XYPlane;
        var p2 = XZPlane;

        p1.Normal.Dot(p2.Normal).Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies Project maintains the distance invariant.</summary>
    [Fact]
    public void Project_DistanceInvariant()
    {
        var p = new Point3D(5, 3, 7);

        var projected = XYPlane.Project(p);
        double distToPlane = XYPlane.DistanceTo(p);
        double distPointToProjected = p.DistanceTo(projected);

        distPointToProjected.Should().BeApproximately(distToPlane, Tolerance);
    }

    /// <summary>Verifies DistanceTo of the plane reference point is zero.</summary>
    [Fact]
    public void DistanceTo_PlanePoint_ReturnsZero()
    {
        var plane = new Plane3D(new Point3D(1, 2, 3), Vector3D.UnitZ);

        plane.DistanceTo(plane.Point).Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies Intersect(Line) returns hit at correct midpoint for a bisecting line.</summary>
    [Fact]
    public void IntersectLine_BisectingLine_ReturnsCorrectHit()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), Vector3D.UnitY);
        var line = new Line3D(new Point3D(0, -3, 0), new Point3D(0, 3, 0));

        var (hit, point) = plane.Intersect(line);

        hit.Should().BeTrue();
        point.Y.Should().BeApproximately(0.0, Tolerance);
    }
}
