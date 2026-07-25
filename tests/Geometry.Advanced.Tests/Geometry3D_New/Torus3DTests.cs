namespace MathVerse.Geometry.Advanced.Tests.Geometry3D_New;

public class Torus3DTests
{
    private const double Tolerance = 1e-6;

    [Fact]
    public void Volume_FormulaCorrect()
    {
        double majorR = 5.0;
        double minorR = 1.0;
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, majorR, minorR);

        double expected = 2.0 * System.Math.PI * System.Math.PI * majorR * minorR * minorR;

        torus.Volume.Should().BeApproximately(expected, Tolerance);
    }

    [Fact]
    public void Volume_ScalesWithMajorRadius()
    {
        var t1 = new Torus3D(Point3D.Origin, Vector3D.UnitY, 3.0, 1.0);
        var t2 = new Torus3D(Point3D.Origin, Vector3D.UnitY, 6.0, 1.0);

        t2.Volume.Should().BeApproximately(t1.Volume * 2.0, Tolerance);
    }

    [Fact]
    public void Volume_ScalesWithMinorRadiusSquared()
    {
        var t1 = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);
        var t2 = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 2.0);

        t2.Volume.Should().BeApproximately(t1.Volume * 4.0, Tolerance);
    }

    [Fact]
    public void SurfaceArea_FormulaCorrect()
    {
        double majorR = 5.0;
        double minorR = 1.0;
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, majorR, minorR);

        double expected = 4.0 * System.Math.PI * System.Math.PI * majorR * minorR;

        torus.SurfaceArea.Should().BeApproximately(expected, Tolerance);
    }

    [Fact]
    public void SurfaceArea_ScalesWithMajorRadius()
    {
        var t1 = new Torus3D(Point3D.Origin, Vector3D.UnitY, 3.0, 1.0);
        var t2 = new Torus3D(Point3D.Origin, Vector3D.UnitY, 6.0, 1.0);

        t2.SurfaceArea.Should().BeApproximately(t1.SurfaceArea * 2.0, Tolerance);
    }

    [Fact]
    public void SurfaceArea_ScalesWithMinorRadius()
    {
        var t1 = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);
        var t2 = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 3.0);

        t2.SurfaceArea.Should().BeApproximately(t1.SurfaceArea * 3.0, Tolerance);
    }

    [Fact]
    public void Contains_PointOnTubeCenter_IsTrue()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);
        var point = new Point3D(5, 0, 0);

        torus.Contains(point).Should().BeTrue();
    }

    [Fact]
    public void Contains_PointInsideTube_IsTrue()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 2.0);
        var point = new Point3D(5, 0.5, 0);

        torus.Contains(point).Should().BeTrue();
    }

    [Fact]
    public void Contains_PointOutsideTube_IsFalse()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);
        var point = new Point3D(0, 0, 0);

        torus.Contains(point).Should().BeFalse();
    }

    [Fact]
    public void Contains_PointFarAway_IsFalse()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);
        var point = new Point3D(100, 100, 100);

        torus.Contains(point).Should().BeFalse();
    }

    [Fact]
    public void Contains_PointOnSurface_IsTrue()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);
        var point = new Point3D(6, 0, 0);

        torus.Contains(point).Should().BeTrue();
    }

    [Fact]
    public void DistanceTo_PointOnTube_IsZero()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);
        var point = new Point3D(5, 0, 0);

        torus.DistanceTo(point).Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void DistanceTo_PointOutside_ReturnsPositiveDistance()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);
        var point = new Point3D(0, 0, 0);

        torus.DistanceTo(point).Should().BeGreaterThan(0);
    }

    [Fact]
    public void DistanceTo_PointAtCenter_DistanceIsMajorMinusMinor()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);
        var point = new Point3D(0, 0, 0);

        torus.DistanceTo(point).Should().BeApproximately(4.0, Tolerance);
    }

    [Fact]
    public void DistanceTo_PointInsideTube_IsZero()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 2.0);
        var point = new Point3D(5, 0.5, 0);

        torus.DistanceTo(point).Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void ToBoundingBox_CenterIsCorrect()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);

        var box = torus.ToBoundingBox();

        box.Center.X.Should().BeApproximately(0.0, Tolerance);
        box.Center.Y.Should().BeApproximately(0.0, Tolerance);
        box.Center.Z.Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void ToBoundingBox_EnclosesTorus()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);

        var box = torus.ToBoundingBox();

        box.Contains(new Point3D(5, 0, 0)).Should().BeTrue();
        box.Contains(new Point3D(-5, 0, 0)).Should().BeTrue();
        box.Contains(new Point3D(0, 0, 5)).Should().BeTrue();
    }

    [Fact]
    public void ToBoundingBox_VolumeIsNonZero()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);

        var box = torus.ToBoundingBox();

        box.Volume.Should().BeGreaterThan(0);
    }

    [Fact]
    public void PointAt_OriginAngle_ReturnsPointOnTorus()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);

        var point = torus.PointAt(0, 0);

        torus.Contains(point).Should().BeTrue();
    }

    [Fact]
    public void PointAt_MajorAngle_ReturnsPointOnTorus()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);

        var point = torus.PointAt(System.Math.PI, 0);

        torus.Contains(point).Should().BeTrue();
    }

    [Fact]
    public void PointAt_MinorAngle_ReturnsPointOnTorus()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);

        var point = torus.PointAt(0, System.Math.PI);

        torus.Contains(point).Should().BeTrue();
    }

    [Fact]
    public void PointAt_AllAngles_StayOnSurface()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 4.0, 1.5);

        for (int i = 0; i < 10; i++)
        {
            double majorAngle = i * System.Math.PI / 5;
            for (int j = 0; j < 10; j++)
            {
                double minorAngle = j * System.Math.PI / 5;
                var point = torus.PointAt(majorAngle, minorAngle);
                torus.DistanceTo(point).Should().BeLessThan(1e-2);
            }
        }
    }

    [Fact]
    public void InnerRadius_EqualsMajorMinusMinor()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 2.0);

        torus.InnerRadius.Should().BeApproximately(3.0, Tolerance);
    }

    [Fact]
    public void OuterRadius_EqualsMajorPlusMinor()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 2.0);

        torus.OuterRadius.Should().BeApproximately(7.0, Tolerance);
    }

    [Fact]
    public void InnerRadius_SmallerThanOuterRadius()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);

        torus.InnerRadius.Should().BeLessThan(torus.OuterRadius);
    }

    [Fact]
    public void DefaultAxis_IsNormalized()
    {
        var torus = new Torus3D(Point3D.Origin, new Vector3D(0, 3, 0), 5.0, 1.0);

        torus.Axis.Length.Should().BeApproximately(1.0, Tolerance);
    }

    [Fact]
    public void DefaultAxis_YAxis()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);

        torus.Axis.X.Should().BeApproximately(0.0, Tolerance);
        torus.Axis.Y.Should().BeApproximately(1.0, Tolerance);
        torus.Axis.Z.Should().BeApproximately(0.0, Tolerance);
    }

    [Fact]
    public void ToString_ContainsTorus3D()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);

        torus.ToString().Should().Contain("Torus3D");
    }

    [Fact]
    public void ToString_ContainsMajorRadius()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);

        torus.ToString().Should().Contain("5");
    }

    [Fact]
    public void Center_IsPreserved()
    {
        var center = new Point3D(1, 2, 3);
        var torus = new Torus3D(center, Vector3D.UnitY, 5.0, 1.0);

        torus.Center.Should().Be(center);
    }

    [Fact]
    public void MajorRadius_IsPreserved()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 7.0, 1.0);

        torus.MajorRadius.Should().BeApproximately(7.0, Tolerance);
    }

    [Fact]
    public void MinorRadius_IsPreserved()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 3.0);

        torus.MinorRadius.Should().BeApproximately(3.0, Tolerance);
    }

    [Fact]
    public void Contains_DonutHole_IsFalse()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);

        torus.Contains(Point3D.Origin).Should().BeFalse();
    }

    [Fact]
    public void DistanceTo_DonutHole_EqualsInnerRadius()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);

        torus.DistanceTo(Point3D.Origin).Should().BeApproximately(4.0, Tolerance);
    }

    [Fact]
    public void Volume_NonZero()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);

        torus.Volume.Should().BeGreaterThan(0);
    }

    [Fact]
    public void SurfaceArea_NonZero()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);

        torus.SurfaceArea.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Axis_IsNormalized_NonUnitInput()
    {
        var torus = new Torus3D(Point3D.Origin, new Vector3D(1, 1, 1), 5.0, 1.0);

        torus.Axis.Length.Should().BeApproximately(1.0, Tolerance);
    }

    [Fact]
    public void ToBoundingBox_NonYAxis_CoversCorrectExtent()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitZ, 5.0, 1.0);

        var box = torus.ToBoundingBox();

        box.Contains(new Point3D(5, 0, 0)).Should().BeTrue();
    }

    [Fact]
    public void PointAt_FullCircle_ReturnsToStart()
    {
        var torus = new Torus3D(Point3D.Origin, Vector3D.UnitY, 5.0, 1.0);

        var p1 = torus.PointAt(0, 0);
        var p2 = torus.PointAt(2 * System.Math.PI, 0);

        p1.DistanceTo(p2).Should().BeApproximately(0.0, Tolerance);
    }
}
