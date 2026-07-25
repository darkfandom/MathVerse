namespace MathVerse.Geometry.Tests.Geometry3D;

/// <summary>Tests for the <see cref="Geometry3DOperations"/> static class.</summary>
public class Geometry3DOperationsTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies Distance(Point,Point) matches Point3D.DistanceTo.</summary>
    [Fact]
    public void Distance_PointToPoint_MatchesPointDistanceTo()
    {
        var a = new Point3D(1, 2, 3);
        var b = new Point3D(4, 6, 3);

        Geometry3DOperations.Distance(a, b).Should().BeApproximately(a.DistanceTo(b), Tolerance);
    }

    /// <summary>Verifies Distance(Line,Point) matches Line3D.DistanceTo.</summary>
    [Fact]
    public void Distance_LineToPoint_MatchesLineDistanceTo()
    {
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(10, 0, 0));
        var p = new Point3D(5, 3, 0);

        Geometry3DOperations.Distance(line, p).Should().BeApproximately(line.DistanceTo(p), Tolerance);
    }

    /// <summary>Verifies Distance(Plane,Point) matches Plane3D.DistanceTo.</summary>
    [Fact]
    public void Distance_PlaneToPoint_MatchesPlaneDistanceTo()
    {
        var plane = new Plane3D(Point3D.Origin, Vector3D.UnitZ);
        var p = new Point3D(0, 0, 5);

        Geometry3DOperations.Distance(plane, p).Should().BeApproximately(plane.DistanceTo(p), Tolerance);
    }

    /// <summary>Verifies Intersect(Line,Line) matches Line3D.Intersect.</summary>
    [Fact]
    public void Intersect_LineLine_MatchesLineIntersect()
    {
        var a = new Line3D(new Point3D(-1, 0, 0), new Point3D(1, 0, 0));
        var b = new Line3D(new Point3D(0, -1, 0), new Point3D(0, 1, 0));

        var opsResult = Geometry3DOperations.Intersect(a, b);
        var lineResult = a.Intersect(b);

        opsResult.hit.Should().Be(lineResult.hit);
        opsResult.distance.Should().BeApproximately(lineResult.distance, Tolerance);
    }

    /// <summary>Verifies Intersect(Line,Plane) matches Line3D.Intersect(plane).</summary>
    [Fact]
    public void Intersect_LinePlane_MatchesLineIntersect()
    {
        var line = new Line3D(new Point3D(0, 0, -5), new Point3D(0, 0, 5));
        var plane = new Plane3D(Point3D.Origin, Vector3D.UnitZ);

        var opsResult = Geometry3DOperations.Intersect(line, plane);
        var lineResult = line.Intersect(plane);

        opsResult.hit.Should().Be(lineResult.hit);
    }

    /// <summary>Verifies Intersect(Plane,Plane) matches Plane3D.Intersect.</summary>
    [Fact]
    public void Intersect_PlanePlane_MatchesPlaneIntersect()
    {
        var xy = new Plane3D(Point3D.Origin, Vector3D.UnitZ);
        var xz = new Plane3D(Point3D.Origin, Vector3D.UnitY);

        var opsResult = Geometry3DOperations.Intersect(xy, xz);
        var planeResult = xy.Intersect(xz);

        opsResult.hit.Should().Be(planeResult.hit);
    }

    /// <summary>Verifies Intersect(Line,Sphere) matches Sphere3D.Intersect.</summary>
    [Fact]
    public void Intersect_LineSphere_MatchesSphereIntersect()
    {
        var line = new Line3D(new Point3D(-5, 0, 0), new Point3D(5, 0, 0));
        var sphere = new Sphere3D(Point3D.Origin, 2.0);

        var opsResult = Geometry3DOperations.Intersect(line, sphere);
        var sphereResult = sphere.Intersect(line);

        opsResult.hit.Should().Be(sphereResult.hit);
        opsResult.points.Length.Should().Be(sphereResult.points.Length);
    }

    /// <summary>Verifies Intersect(Triangle,Line) matches Triangle3D.Intersect.</summary>
    [Fact]
    public void Intersect_TriangleLine_MatchesTriangleIntersect()
    {
        var tri = new Triangle3D(
            new Point3D(0, 0, 0),
            new Point3D(1, 0, 0),
            new Point3D(0, 1, 0));
        var line = new Line3D(new Point3D(0.2, 0.2, -5), new Point3D(0.2, 0.2, 5));

        var opsResult = Geometry3DOperations.Intersect(tri, line);
        var triResult = tri.Intersect(line);

        opsResult.hit.Should().Be(triResult.hit);
    }

    /// <summary>Verifies Project(Point,Plane) matches Plane3D.Project.</summary>
    [Fact]
    public void Project_PointOntoPlane_MatchesPlaneProject()
    {
        var plane = new Plane3D(Point3D.Origin, Vector3D.UnitZ);
        var p = new Point3D(5, 5, 10);

        var opsResult = Geometry3DOperations.Project(p, plane);
        var planeResult = plane.Project(p);

        opsResult.Should().Be(planeResult);
    }

    /// <summary>Verifies Project(Point,Line) matches Line3D.ClosestPoint.</summary>
    [Fact]
    public void Project_PointOntoLine_MatchesLineClosestPoint()
    {
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(10, 0, 0));
        var p = new Point3D(5, 3, 0);

        var opsResult = Geometry3DOperations.Project(p, line);
        var lineResult = line.ClosestPoint(p);

        opsResult.Should().Be(lineResult);
    }

    /// <summary>Verifies Normal(Triangle3D) matches Triangle3D.Normal.</summary>
    [Fact]
    public void Normal_Triangle_MatchesTriangleNormal()
    {
        var tri = new Triangle3D(
            new Point3D(0, 0, 0),
            new Point3D(1, 0, 0),
            new Point3D(0, 1, 0));

        Geometry3DOperations.Normal(tri).Should().Be(tri.Normal);
    }

    /// <summary>Verifies Volume(Sphere3D) matches Sphere3D.Volume.</summary>
    [Fact]
    public void Volume_Sphere_MatchesSphereVolume()
    {
        var sphere = new Sphere3D(Point3D.Origin, 3.0);

        Geometry3DOperations.Volume(sphere).Should().BeApproximately(sphere.Volume, Tolerance);
    }

    /// <summary>Verifies Volume(Cylinder3D) matches Cylinder3D.Volume.</summary>
    [Fact]
    public void Volume_Cylinder_MatchesCylinderVolume()
    {
        var cyl = new Cylinder3D(Point3D.Origin, 2.0, 5.0);

        Geometry3DOperations.Volume(cyl).Should().BeApproximately(cyl.Volume, Tolerance);
    }

    /// <summary>Verifies Volume(Cone3D) matches Cone3D.Volume.</summary>
    [Fact]
    public void Volume_Cone_MatchesConeVolume()
    {
        var cone = new Cone3D(Point3D.Origin, Vector3D.UnitY, 3.0, 6.0);

        Geometry3DOperations.Volume(cone).Should().BeApproximately(cone.Volume, Tolerance);
    }

    /// <summary>Verifies SurfaceArea(Sphere3D) matches Sphere3D.SurfaceArea.</summary>
    [Fact]
    public void SurfaceArea_Sphere_MatchesSphereSurfaceArea()
    {
        var sphere = new Sphere3D(Point3D.Origin, 3.0);

        Geometry3DOperations.SurfaceArea(sphere).Should().BeApproximately(sphere.SurfaceArea, Tolerance);
    }

    /// <summary>Verifies SurfaceArea(Cylinder3D) matches Cylinder3D.SurfaceArea.</summary>
    [Fact]
    public void SurfaceArea_Cylinder_MatchesCylinderSurfaceArea()
    {
        var cyl = new Cylinder3D(Point3D.Origin, 2.0, 5.0);

        Geometry3DOperations.SurfaceArea(cyl).Should().BeApproximately(cyl.SurfaceArea, Tolerance);
    }

    /// <summary>Verifies Distance(Point,Point) is symmetric.</summary>
    [Fact]
    public void Distance_PointToPoint_IsSymmetric()
    {
        var a = new Point3D(1, 2, 3);
        var b = new Point3D(4, 5, 6);

        Geometry3DOperations.Distance(a, b).Should().BeApproximately(Geometry3DOperations.Distance(b, a), Tolerance);
    }

    /// <summary>Verifies Intersect(Line,Sphere) with external line returns no hit.</summary>
    [Fact]
    public void Intersect_LineSphere_ExternalLine_ReturnsNoHit()
    {
        var line = new Line3D(new Point3D(-5, 10, 0), new Point3D(5, 10, 0));
        var sphere = new Sphere3D(Point3D.Origin, 2.0);

        var (hit, points) = Geometry3DOperations.Intersect(line, sphere);

        hit.Should().BeFalse();
        points.Length.Should().Be(0);
    }

    /// <summary>Verifies ClosestPoint delegates to Triangle3D.ClosestPoint.</summary>
    [Fact]
    public void ClosestPoint_Triangle_DelegatesCorrectly()
    {
        var tri = new Triangle3D(
            new Point3D(0, 0, 0),
            new Point3D(1, 0, 0),
            new Point3D(0, 1, 0));
        var p = new Point3D(0.2, 0.2, 5);

        var result = Geometry3DOperations.ClosestPoint(tri, p);

        result.Should().Be(tri.ClosestPoint(p));
    }
}
