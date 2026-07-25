namespace MathVerse.Geometry.Tests.Curves;

/// <summary>Tests for HermiteCurve struct.</summary>
public class HermiteCurveTests
{
    private const double Precision = 1e-10;

    /// <summary>PointAt t=0 should return the starting point.</summary>
    [Fact]
    public void PointAt_AtZero_ShouldReturnStartPoint()
    {
        var curve = new HermiteCurve(
            new Point3D(1, 2, 3), Vector3D.UnitX,
            new Point3D(5, 6, 7), Vector3D.UnitY);
        Point3D result = curve.PointAt(0);
        result.X.Should().BeApproximately(1.0, Precision);
        result.Y.Should().BeApproximately(2.0, Precision);
        result.Z.Should().BeApproximately(3.0, Precision);
    }

    /// <summary>PointAt t=1 should return the ending point.</summary>
    [Fact]
    public void PointAt_AtOne_ShouldReturnEndPoint()
    {
        var curve = new HermiteCurve(
            new Point3D(1, 2, 3), Vector3D.UnitX,
            new Point3D(5, 6, 7), Vector3D.UnitY);
        Point3D result = curve.PointAt(1);
        result.X.Should().BeApproximately(5.0, Precision);
        result.Y.Should().BeApproximately(6.0, Precision);
        result.Z.Should().BeApproximately(7.0, Precision);
    }

    /// <summary>PointAt t=0.5 should be a valid interpolation.</summary>
    [Fact]
    public void PointAt_AtHalf_ShouldBeBetweenEndpoints()
    {
        var curve = new HermiteCurve(
            new Point3D(0, 0, 0), Vector3D.UnitX,
            new Point3D(10, 0, 0), Vector3D.UnitX);
        Point3D result = curve.PointAt(0.5);
        result.X.Should().BeGreaterThan(0.0);
        result.X.Should().BeLessThan(10.0);
    }

    /// <summary>ToBezier should produce a cubic Bezier curve.</summary>
    [Fact]
    public void ToBezier_ShouldProduceCubicCurve()
    {
        var curve = new HermiteCurve(
            new Point3D(0, 0, 0), Vector3D.UnitX,
            new Point3D(1, 0, 0), Vector3D.UnitX);
        BezierCurve3D bezier = curve.ToBezier();
        bezier.Degree.Should().Be(3);
    }

    /// <summary>ToBezier start endpoint should match Hermite start.</summary>
    [Fact]
    public void ToBezier_StartEndpoint_ShouldMatch()
    {
        var p0 = new Point3D(1, 2, 3);
        var p1 = new Point3D(7, 8, 9);
        var curve = new HermiteCurve(p0, Vector3D.UnitX, p1, Vector3D.UnitY);
        BezierCurve3D bezier = curve.ToBezier();
        Point3D start = bezier.PointAt(0);
        start.X.Should().BeApproximately(p0.X, Precision);
        start.Y.Should().BeApproximately(p0.Y, Precision);
        start.Z.Should().BeApproximately(p0.Z, Precision);
    }

    /// <summary>ToBezier end endpoint should match Hermite end.</summary>
    [Fact]
    public void ToBezier_EndEndpoint_ShouldMatch()
    {
        var p0 = new Point3D(1, 2, 3);
        var p1 = new Point3D(7, 8, 9);
        var curve = new HermiteCurve(p0, Vector3D.UnitX, p1, Vector3D.UnitY);
        BezierCurve3D bezier = curve.ToBezier();
        Point3D end = bezier.PointAt(1);
        end.X.Should().BeApproximately(p1.X, Precision);
        end.Y.Should().BeApproximately(p1.Y, Precision);
        end.Z.Should().BeApproximately(p1.Z, Precision);
    }

    /// <summary>Hermite and converted Bezier should agree at midpoint.</summary>
    [Fact]
    public void ToBezier_Midpoint_ShouldAgreeWithHermite()
    {
        var curve = new HermiteCurve(
            new Point3D(0, 0, 0), new Vector3D(3, 0, 0),
            new Point3D(6, 0, 0), new Vector3D(3, 0, 0));
        BezierCurve3D bezier = curve.ToBezier();
        Point3D hMid = curve.PointAt(0.5);
        Point3D bMid = bezier.PointAt(0.5);
        bMid.X.Should().BeApproximately(hMid.X, Precision);
        bMid.Y.Should().BeApproximately(hMid.Y, Precision);
        bMid.Z.Should().BeApproximately(hMid.Z, Precision);
    }

    /// <summary>Zero tangents should produce linear interpolation.</summary>
    [Fact]
    public void ZeroTangents_ShouldProduceLinearInterpolation()
    {
        var curve = new HermiteCurve(
            new Point3D(0, 0, 0), Vector3D.Zero,
            new Point3D(10, 10, 10), Vector3D.Zero);
        Point3D mid = curve.PointAt(0.5);
        mid.X.Should().BeApproximately(5.0, Precision);
        mid.Y.Should().BeApproximately(5.0, Precision);
        mid.Z.Should().BeApproximately(5.0, Precision);
    }

    /// <summary>P0, T0, P1, T1 properties should be set correctly.</summary>
    [Fact]
    public void Properties_ShouldBeSetCorrectly()
    {
        var p0 = new Point3D(1, 2, 3);
        var t0 = new Vector3D(4, 5, 6);
        var p1 = new Point3D(7, 8, 9);
        var t1 = new Vector3D(10, 11, 12);
        var curve = new HermiteCurve(p0, t0, p1, t1);
        curve.P0.Should().Be(p0);
        curve.T0.Should().Be(t0);
        curve.P1.Should().Be(p1);
        curve.T1.Should().Be(t1);
    }

    /// <summary>ToBezier control point 1 should be P0 + T0/3.</summary>
    [Fact]
    public void ToBezier_ControlPoint1_ShouldBeP0PlusT0Over3()
    {
        var p0 = new Point3D(0, 0, 0);
        var t0 = new Vector3D(3, 0, 0);
        var curve = new HermiteCurve(p0, t0, new Point3D(1, 0, 0), Vector3D.Zero);
        BezierCurve3D bezier = curve.ToBezier();
        Point3D cp1 = bezier.ControlPoints[1];
        cp1.X.Should().BeApproximately(1.0, Precision);
        cp1.Y.Should().BeApproximately(0.0, Precision);
        cp1.Z.Should().BeApproximately(0.0, Precision);
    }
}
