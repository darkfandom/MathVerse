namespace MathVerse.Geometry.Advanced.Tests.Geometry3D_New;

public class OBB3DTests
{
    private const double Tolerance = 1e-6;

    [Fact]
    public void FromAABB_CenterIsCorrect()
    {
        var aabb = new BoundingBox3D(new Point3D(-1, -2, -3), new Point3D(1, 2, 3));
        var obb = OBB3D.FromAABB(aabb);

        obb.Center.X.Should().BeApproximately(0.0, Tolerance);
        obb.Center.Y.Should().BeApproximately(0.0, Tolerance);
        obb.Center.Z.Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void FromAABB_ExtentsAreHalfWidths()
    {
        var aabb = new BoundingBox3D(new Point3D(-2, -4, -6), new Point3D(2, 4, 6));
        var obb = OBB3D.FromAABB(aabb);

        obb.ExtentX.Should().BeApproximately(2.0, Tolerance);
        obb.ExtentY.Should().BeApproximately(4.0, Tolerance);
        obb.ExtentZ.Should().BeApproximately(6.0, Tolerance);
    }

    [Fact]
    public void FromAABB_AxesAreIdentity()
    {
        var aabb = new BoundingBox3D(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));
        var obb = OBB3D.FromAABB(aabb);

        obb.AxisX.Should().Be(Vector3D.UnitX);
        obb.AxisY.Should().Be(Vector3D.UnitY);
        obb.AxisZ.Should().Be(Vector3D.UnitZ);
    }

    [Fact]
    public void Volume_IsCorrect()
    {
        var obb = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 2, 3);

        double expected = 8.0 * 1 * 2 * 3;

        obb.Volume.Should().BeApproximately(expected, Tolerance);
    }

    [Fact]
    public void SurfaceArea_IsCorrect()
    {
        var obb = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 2, 3);

        double expected = 8.0 * (1 * 2 + 2 * 3 + 3 * 1);

        obb.SurfaceArea.Should().BeApproximately(expected, Tolerance);
    }

    [Fact]
    public void Volume_ExtentsScaled_ScalesCorrectly()
    {
        var o1 = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);
        var o2 = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 2, 2, 2);

        o2.Volume.Should().BeApproximately(o1.Volume * 8.0, Tolerance);
    }

    [Fact]
    public void Contains_PointInside_IsTrue()
    {
        var obb = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);

        obb.Contains(new Point3D(0.5, 0.5, 0.5)).Should().BeTrue();
    }

    [Fact]
    public void Contains_PointCenter_IsTrue()
    {
        var obb = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);

        obb.Contains(Point3D.Origin).Should().BeTrue();
    }

    [Fact]
    public void Contains_PointOutside_IsFalse()
    {
        var obb = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);

        obb.Contains(new Point3D(2, 0, 0)).Should().BeFalse();
    }

    [Fact]
    public void Contains_PointFarAway_IsFalse()
    {
        var obb = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);

        obb.Contains(new Point3D(100, 100, 100)).Should().BeFalse();
    }

    [Fact]
    public void Contains_OnBoundary_IsTrue()
    {
        var obb = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);

        obb.Contains(new Point3D(1, 0, 0)).Should().BeTrue();
    }

    [Fact]
    public void Corners_CountIsEight()
    {
        var obb = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 2, 3);

        obb.Corners.Length.Should().Be(8);
    }

    [Fact]
    public void Corners_WithinBounds()
    {
        var obb = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 2, 3);

        foreach (var corner in obb.Corners)
        {
            obb.Contains(corner).Should().BeTrue();
        }
    }

    [Fact]
    public void Corners_ExtentsAABB_MinMaxCorrect()
    {
        var obb = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 2, 3, 4);

        var corners = obb.Corners;
        double minX = corners.Min(c => c.X);
        double maxX = corners.Max(c => c.X);

        minX.Should().BeApproximately(-2.0, Tolerance);
        maxX.Should().BeApproximately(2.0, Tolerance);
    }

    [Fact]
    public void Intersects_OverlappingOBBs_ReturnsTrue()
    {
        var a = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);
        var b = new OBB3D(new Point3D(0.5, 0, 0), Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);

        a.Intersects(b).Should().BeTrue();
    }

    [Fact]
    public void Intersects_NonOverlappingOBBs_ReturnsFalse()
    {
        var a = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);
        var b = new OBB3D(new Point3D(5, 5, 5), Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);

        a.Intersects(b).Should().BeFalse();
    }

    [Fact]
    public void Intersects_Touching_ReturnsTrue()
    {
        var a = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);
        var b = new OBB3D(new Point3D(2, 0, 0), Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);

        a.Intersects(b).Should().BeTrue();
    }

    [Fact]
    public void Intersects_Symmetric()
    {
        var a = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);
        var b = new OBB3D(new Point3D(0.5, 0, 0), Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);

        a.Intersects(b).Should().Be(b.Intersects(a));
    }

    [Fact]
    public void Intersects_ContainedOBB_ReturnsTrue()
    {
        var outer = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 5, 5, 5);
        var inner = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);

        outer.Intersects(inner).Should().BeTrue();
    }

    [Fact]
    public void Intersects_XAlignedSeparated_ReturnsFalse()
    {
        var a = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);
        var b = new OBB3D(new Point3D(10, 0, 0), Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);

        a.Intersects(b).Should().BeFalse();
    }

    [Fact]
    public void ToAABB_RoundTrip_UnrotatedOBB()
    {
        var obb = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 2, 3, 4);

        var aabb = obb.ToAABB();

        aabb.Min.Should().Be(new Point3D(-2, -3, -4));
        aabb.Max.Should().Be(new Point3D(2, 3, 4));
    }

    [Fact]
    public void ToAABB_CenterPreserved()
    {
        var obb = new OBB3D(new Point3D(5, 5, 5), Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);

        var aabb = obb.ToAABB();

        aabb.Center.X.Should().BeApproximately(5.0, Tolerance);
        aabb.Center.Y.Should().BeApproximately(5.0, Tolerance);
        aabb.Center.Z.Should().BeApproximately(5.0, Tolerance);
    }

    [Fact]
    public void ToAABB_EnclosesAllCorners()
    {
        var obb = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 2, 3, 4);
        var aabb = obb.ToAABB();

        foreach (var corner in obb.Corners)
        {
            aabb.Contains(corner).Should().BeTrue();
        }
    }

    [Fact]
    public void FromPoints_SinglePoint_ZeroExtents()
    {
        var points = new List<Point3D> { new Point3D(1, 2, 3) };
        var obb = OBB3D.FromPoints(points);

        obb.ExtentX.Should().BeApproximately(0.0, Tolerance);
        obb.ExtentY.Should().BeApproximately(0.0, Tolerance);
        obb.ExtentZ.Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void FromPoints_EmptyList_ReturnsDefault()
    {
        var points = new List<Point3D>();
        var obb = OBB3D.FromPoints(points);

        obb.ExtentX.Should().BeApproximately(0.0, Tolerance);
        obb.ExtentY.Should().BeApproximately(0.0, Tolerance);
        obb.ExtentZ.Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void FromPoints_TwoPoints_FormsBox()
    {
        var points = new List<Point3D>
        {
            new Point3D(0, 0, 0),
            new Point3D(2, 0, 0)
        };
        var obb = OBB3D.FromPoints(points);

        obb.Center.X.Should().BeApproximately(1.0, Tolerance);
    }

    [Fact]
    public void FromPoints_CubePoints_FormsCube()
    {
        var points = new List<Point3D>
        {
            new Point3D(-1, -1, -1), new Point3D(1, -1, -1),
            new Point3D(1, 1, -1), new Point3D(-1, 1, -1),
            new Point3D(-1, -1, 1), new Point3D(1, -1, 1),
            new Point3D(1, 1, 1), new Point3D(-1, 1, 1)
        };
        var obb = OBB3D.FromPoints(points);

        obb.ExtentX.Should().BeApproximately(1.0, Tolerance);
        obb.ExtentY.Should().BeApproximately(1.0, Tolerance);
        obb.ExtentZ.Should().BeApproximately(1.0, Tolerance);
    }

    [Fact]
    public void FromPoints_ContainsAllPoints()
    {
        var points = new List<Point3D>
        {
            new Point3D(1, 0, 0),
            new Point3D(-1, 0, 0),
            new Point3D(0, 1, 0),
            new Point3D(0, -1, 0),
            new Point3D(0, 0, 1),
            new Point3D(0, 0, -1)
        };
        var obb = OBB3D.FromPoints(points);

        foreach (var p in points)
        {
            obb.Contains(p).Should().BeTrue();
        }
    }

    [Fact]
    public void ToString_ContainsOBB3D()
    {
        var obb = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 2, 3);

        obb.ToString().Should().Contain("OBB3D");
    }

    [Fact]
    public void ToString_ContainsExtents()
    {
        var obb = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 2, 3);

        obb.ToString().Should().Contain("1");
        obb.ToString().Should().Contain("2");
        obb.ToString().Should().Contain("3");
    }

    [Fact]
    public void Axes_AreNormalized()
    {
        var obb = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);

        obb.AxisX.Length.Should().BeApproximately(1.0, Tolerance);
        obb.AxisY.Length.Should().BeApproximately(1.0, Tolerance);
        obb.AxisZ.Length.Should().BeApproximately(1.0, Tolerance);
    }

    [Fact]
    public void Axes_AreOrthogonal()
    {
        var obb = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);

        obb.AxisX.Dot(obb.AxisY).Should().BeApproximately(0.0, Tolerance);
        obb.AxisY.Dot(obb.AxisZ).Should().BeApproximately(0.0, Tolerance);
        obb.AxisZ.Dot(obb.AxisX).Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void FromAABB_VolumeMatches()
    {
        var aabb = new BoundingBox3D(new Point3D(-1, -2, -3), new Point3D(1, 2, 3));
        var obb = OBB3D.FromAABB(aabb);

        obb.Volume.Should().BeApproximately(aabb.Volume, Tolerance);
    }

    [Fact]
    public void Intersects_VariousYOffsets()
    {
        var a = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);

        a.Intersects(new OBB3D(new Point3D(0, 1.5, 0), Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1)).Should().BeTrue();
        a.Intersects(new OBB3D(new Point3D(0, 2.5, 0), Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1)).Should().BeFalse();
    }

    [Fact]
    public void Intersects_VariousZOffsets()
    {
        var a = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1);

        a.Intersects(new OBB3D(new Point3D(0, 0, 1.5), Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1)).Should().BeTrue();
        a.Intersects(new OBB3D(new Point3D(0, 0, 3.0), Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 1, 1, 1)).Should().BeFalse();
    }

    [Fact]
    public void ToAABB_MinLessThanMax()
    {
        var obb = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 2, 3, 4);

        var aabb = obb.ToAABB();

        aabb.Min.X.Should().BeLessThan(aabb.Max.X);
        aabb.Min.Y.Should().BeLessThan(aabb.Max.Y);
        aabb.Min.Z.Should().BeLessThan(aabb.Max.Z);
    }

    [Fact]
    public void Contains_MultiplePointsInside()
    {
        var obb = new OBB3D(Point3D.Origin, Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ, 2, 2, 2);

        obb.Contains(new Point3D(1, 1, 1)).Should().BeTrue();
        obb.Contains(new Point3D(-1, -1, -1)).Should().BeTrue();
        obb.Contains(new Point3D(0, 0, 0)).Should().BeTrue();
    }
}
