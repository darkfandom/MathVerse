namespace MathVerse.Geometry.Tests.Geometry3D;

/// <summary>Tests for the <see cref="Triangle3D"/> struct.</summary>
public class Triangle3DTests
{
    private const double Tolerance = 1e-10;

    private static readonly Triangle3D UnitRightTriangle = new(
        new Point3D(0, 0, 0),
        new Point3D(1, 0, 0),
        new Point3D(0, 1, 0));

    private static readonly Triangle3D EquilateralTriangle = new(
        new Point3D(0, 0, 0),
        new Point3D(1, 0, 0),
        new Point3D(0.5, System.Math.Sqrt(3) / 2.0, 0));

    /// <summary>Verifies Normal is perpendicular to the triangle surface.</summary>
    [Fact]
    public void Normal_IsPerpendicularToEdges()
    {
        var tri = UnitRightTriangle;

        var ab = new Vector3D(tri.B.X - tri.A.X, tri.B.Y - tri.A.Y, tri.B.Z - tri.A.Z);
        var ac = new Vector3D(tri.C.X - tri.A.X, tri.C.Y - tri.A.Y, tri.C.Z - tri.A.Z);

        tri.Normal.Dot(ab).Should().BeApproximately(0.0, Tolerance);
        tri.Normal.Dot(ac).Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies Normal is a unit vector.</summary>
    [Fact]
    public void Normal_IsUnitVector()
    {
        UnitRightTriangle.Normal.Length.Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies Area of a 1x1 right triangle is 0.5.</summary>
    [Fact]
    public void Area_RightTriangle_ReturnsHalf()
    {
        UnitRightTriangle.Area.Should().BeApproximately(0.5, Tolerance);
    }

    /// <summary>Verifies Area of equilateral triangle with side 1.</summary>
    [Fact]
    public void Area_EquilateralTriangle_ReturnsCorrectValue()
    {
        double expected = System.Math.Sqrt(3) / 4.0;

        EquilateralTriangle.Area.Should().BeApproximately(expected, Tolerance);
    }

    /// <summary>Verifies Perimeter of a unit right triangle.</summary>
    [Fact]
    public void Perimeter_UnitRightTriangle_ReturnsCorrectValue()
    {
        double expected = 1.0 + 1.0 + System.Math.Sqrt(2);

        UnitRightTriangle.Perimeter.Should().BeApproximately(expected, Tolerance);
    }

    /// <summary>Verifies Perimeter of equilateral triangle with side 1 is 3.</summary>
    [Fact]
    public void Perimeter_EquilateralTriangle_ReturnsThree()
    {
        EquilateralTriangle.Perimeter.Should().BeApproximately(3.0, Tolerance);
    }

    /// <summary>Verifies Centroid is the average of the three vertices.</summary>
    [Fact]
    public void Centroid_ReturnsAverageOfVertices()
    {
        var tri = new Triangle3D(
            new Point3D(0, 0, 0),
            new Point3D(6, 0, 0),
            new Point3D(0, 6, 0));

        var centroid = tri.Centroid;

        centroid.X.Should().BeApproximately(2.0, Tolerance);
        centroid.Y.Should().BeApproximately(2.0, Tolerance);
        centroid.Z.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies Circumcenter is equidistant from all three vertices.</summary>
    [Fact]
    public void Circumcenter_EquidistantFromVertices()
    {
        var tri = UnitRightTriangle;

        double dA = tri.Circumcenter.DistanceTo(tri.A);
        double dB = tri.Circumcenter.DistanceTo(tri.B);
        double dC = tri.Circumcenter.DistanceTo(tri.C);

        dA.Should().BeApproximately(dB, Tolerance);
        dB.Should().BeApproximately(dC, Tolerance);
    }

    /// <summary>Verifies BarycentricCoords at vertex A returns (1, 0, 0).</summary>
    [Fact]
    public void BarycentricCoords_AtVertexA_ReturnsCorrect()
    {
        var tri = UnitRightTriangle;

        var (u, v, w) = tri.BarycentricCoords(tri.A);

        u.Should().BeApproximately(1.0, Tolerance);
        v.Should().BeApproximately(0.0, Tolerance);
        w.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies BarycentricCoords at centroid returns (1/3, 1/3, 1/3).</summary>
    [Fact]
    public void BarycentricCoords_AtCentroid_ReturnsEqual()
    {
        var tri = UnitRightTriangle;

        var (u, v, w) = tri.BarycentricCoords(tri.Centroid);

        u.Should().BeApproximately(1.0 / 3.0, Tolerance);
        v.Should().BeApproximately(1.0 / 3.0, Tolerance);
        w.Should().BeApproximately(1.0 / 3.0, Tolerance);
    }

    /// <summary>Verifies BarycentricCoords sum to 1.</summary>
    [Fact]
    public void BarycentricCoords_SumToOne()
    {
        var tri = UnitRightTriangle;
        var p = new Point3D(0.2, 0.2, 0);

        var (u, v, w) = tri.BarycentricCoords(p);

        (u + v + w).Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies Contains returns true for a point inside the triangle.</summary>
    [Fact]
    public void Contains_InteriorPoint_ReturnsTrue()
    {
        UnitRightTriangle.Contains(new Point3D(0.2, 0.2, 0)).Should().BeTrue();
    }

    /// <summary>Verifies Contains returns false for a point outside the triangle.</summary>
    [Fact]
    public void Contains_ExteriorPoint_ReturnsFalse()
    {
        UnitRightTriangle.Contains(new Point3D(2, 2, 0)).Should().BeFalse();
    }

    /// <summary>Verifies Contains returns true for a point on an edge.</summary>
    [Fact]
    public void Contains_EdgePoint_ReturnsTrue()
    {
        var p = new Point3D(0.5, 0.0, 0);

        UnitRightTriangle.Contains(p).Should().BeTrue();
    }

    /// <summary>Verifies ClosestPoint to a point above the triangle plane returns the in-plane projection.</summary>
    [Fact]
    public void ClosestPoint_AbovePlane_ReturnsProjectedPoint()
    {
        var tri = UnitRightTriangle;
        var p = new Point3D(0.2, 0.2, 10);

        var closest = tri.ClosestPoint(p);

        closest.Z.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies ClosestPoint to a point already inside the triangle returns the same point.</summary>
    [Fact]
    public void ClosestPoint_InsideTriangle_ReturnsSamePoint()
    {
        var tri = UnitRightTriangle;
        var p = new Point3D(0.2, 0.2, 0);

        var closest = tri.ClosestPoint(p);

        closest.Should().Be(p);
    }

    /// <summary>Verifies Intersect(Line) via Möller-Trumbore returns a hit for a piercing line.</summary>
    [Fact]
    public void IntersectLine_PiercingLine_ReturnsHit()
    {
        var tri = UnitRightTriangle;
        var line = new Line3D(new Point3D(0.2, 0.2, -5), new Point3D(0.2, 0.2, 5));

        var (hit, point) = tri.Intersect(line);

        hit.Should().BeTrue();
        point.X.Should().BeApproximately(0.2, Tolerance);
        point.Y.Should().BeApproximately(0.2, Tolerance);
        point.Z.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies Intersect(Line) returns no hit for a line that misses the triangle.</summary>
    [Fact]
    public void IntersectLine_MissingLine_ReturnsNoHit()
    {
        var tri = UnitRightTriangle;
        var line = new Line3D(new Point3D(5, 5, -5), new Point3D(5, 5, 5));

        var (hit, _) = tri.Intersect(line);

        hit.Should().BeFalse();
    }

    /// <summary>Verifies Intersect(Line) returns no hit for a parallel line.</summary>
    [Fact]
    public void IntersectLine_ParallelLine_ReturnsNoHit()
    {
        var tri = UnitRightTriangle;
        var line = new Line3D(new Point3D(0, 0, 5), new Point3D(1, 0, 5));

        var (hit, _) = tri.Intersect(line);

        hit.Should().BeFalse();
    }

    /// <summary>Verifies Plane property contains all three vertices.</summary>
    [Fact]
    public void Plane_ContainsAllVertices()
    {
        var tri = new Triangle3D(
            new Point3D(1, 0, 0),
            new Point3D(0, 2, 0),
            new Point3D(0, 0, 3));

        var plane = tri.Plane;

        plane.Contains(tri.A).Should().BeTrue();
        plane.Contains(tri.B).Should().BeTrue();
        plane.Contains(tri.C).Should().BeTrue();
    }

    /// <summary>Verifies IsDegenerate returns true for a zero-area triangle.</summary>
    [Fact]
    public void IsDegenerate_CollinearPoints_ReturnsTrue()
    {
        var degenerate = new Triangle3D(
            new Point3D(0, 0, 0),
            new Point3D(1, 1, 0),
            new Point3D(2, 2, 0));

        degenerate.IsDegenerate().Should().BeTrue();
    }

    /// <summary>Verifies IsDegenerate returns false for a normal triangle.</summary>
    [Fact]
    public void IsDegenerate_NormalTriangle_ReturnsFalse()
    {
        UnitRightTriangle.IsDegenerate().Should().BeFalse();
    }

    /// <summary>Verifies equilateral triangle has equal side lengths.</summary>
    [Fact]
    public void EquilateralTriangle_HasEqualSides()
    {
        var tri = EquilateralTriangle;

        double ab = tri.A.DistanceTo(tri.B);
        double bc = tri.B.DistanceTo(tri.C);
        double ca = tri.C.DistanceTo(tri.A);

        ab.Should().BeApproximately(bc, Tolerance);
        bc.Should().BeApproximately(ca, Tolerance);
    }

    /// <summary>Verifies right triangle has correct right-angle vertex barycentric coords.</summary>
    [Fact]
    public void RightTriangle_RightAngleAtOrigin()
    {
        var tri = UnitRightTriangle;

        var ab = new Vector3D(tri.B.X - tri.A.X, tri.B.Y - tri.A.Y, tri.B.Z - tri.A.Z);
        var ac = new Vector3D(tri.C.X - tri.A.X, tri.C.Y - tri.A.Y, tri.C.Z - tri.A.Z);

        ab.Dot(ac).Should().BeApproximately(0.0, Tolerance);
    }
}
