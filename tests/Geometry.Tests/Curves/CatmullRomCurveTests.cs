namespace MathVerse.Geometry.Tests.Curves;

/// <summary>Tests for CatmullRomCurve class.</summary>
public class CatmullRomCurveTests
{
    private const double Precision = 1e-8;

    private static CatmullRomCurve CreateSimpleCurve()
    {
        var points = ImmutableArray.Create(
            new Point3D(0, 0, 0),
            new Point3D(1, 2, 0),
            new Point3D(3, 1, 0),
            new Point3D(4, 3, 0));
        return new CatmullRomCurve(points);
    }

    /// <summary>PointAt t=0 should return the first point.</summary>
    [Fact]
    public void PointAt_AtZero_ShouldReturnFirstPoint()
    {
        var curve = CreateSimpleCurve();
        Point3D result = curve.PointAt(0);
        result.X.Should().BeApproximately(0.0, Precision);
        result.Y.Should().BeApproximately(0.0, Precision);
        result.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>PointAt t=3 (last index) should return the last point.</summary>
    [Fact]
    public void PointAt_AtMax_ShouldReturnLastPoint()
    {
        var curve = CreateSimpleCurve();
        Point3D result = curve.PointAt(3);
        result.X.Should().BeApproximately(4.0, Precision);
        result.Y.Should().BeApproximately(3.0, Precision);
        result.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Sample should return the correct number of points.</summary>
    [Fact]
    public void Sample_ShouldReturnCorrectCount()
    {
        var curve = CreateSimpleCurve();
        IReadOnlyList<Point3D> points = curve.Sample(20);
        points.Count.Should().Be(20);
    }

    /// <summary>PointAt t=1 should pass through or near the second control point.</summary>
    [Fact]
    public void PointAt_AtOne_ShouldBeNearSecondPoint()
    {
        var curve = CreateSimpleCurve();
        Point3D result = curve.PointAt(1);
        result.X.Should().BeApproximately(1.0, 0.5);
        result.Y.Should().BeApproximately(2.0, 0.5);
    }

    /// <summary>PointAt t=2 should be near the third control point.</summary>
    [Fact]
    public void PointAt_AtTwo_ShouldBeNearThirdPoint()
    {
        var curve = CreateSimpleCurve();
        Point3D result = curve.PointAt(2);
        result.X.Should().BeApproximately(3.0, 0.5);
        result.Y.Should().BeApproximately(1.0, 0.5);
    }

    /// <summary>Curve should pass through interior points approximately.</summary>
    [Fact]
    public void Curve_ShouldPassThroughInteriorPoints()
    {
        var points = ImmutableArray.Create(
            new Point3D(0, 0, 0),
            new Point3D(2, 4, 0),
            new Point3D(4, 0, 0),
            new Point3D(6, 4, 0));
        var curve = new CatmullRomCurve(points);
        Point3D at1 = curve.PointAt(1);
        at1.X.Should().BeApproximately(2.0, 0.5);
        Point3D at2 = curve.PointAt(2);
        at2.X.Should().BeApproximately(4.0, 0.5);
    }

    /// <summary>Sample first point should match PointAt(0).</summary>
    [Fact]
    public void Sample_FirstPoint_ShouldMatchPointAtZero()
    {
        var curve = CreateSimpleCurve();
        IReadOnlyList<Point3D> sampled = curve.Sample(10);
        Point3D evaluated = curve.PointAt(0);
        sampled[0].X.Should().BeApproximately(evaluated.X, Precision);
        sampled[0].Y.Should().BeApproximately(evaluated.Y, Precision);
    }

    /// <summary>Sample last point should match PointAt(max).</summary>
    [Fact]
    public void Sample_LastPoint_ShouldMatchPointAtMax()
    {
        var curve = CreateSimpleCurve();
        IReadOnlyList<Point3D> sampled = curve.Sample(10);
        Point3D evaluated = curve.PointAt(3);
        sampled[^1].X.Should().BeApproximately(evaluated.X, Precision);
        sampled[^1].Y.Should().BeApproximately(evaluated.Y, Precision);
    }

    /// <summary>Tension parameter should be stored correctly.</summary>
    [Fact]
    public void Tension_ShouldBeStoredCorrectly()
    {
        var points = ImmutableArray.Create(
            new Point3D(0, 0, 0),
            new Point3D(1, 1, 0),
            new Point3D(2, 0, 0),
            new Point3D(3, 1, 0));
        var curve = new CatmullRomCurve(points, 0.75);
        curve.Tension.Should().BeApproximately(0.75, Precision);
    }

    /// <summary>Z coordinates should remain zero for planar points.</summary>
    [Fact]
    public void PlanarPoints_ZShouldRemainZero()
    {
        var curve = CreateSimpleCurve();
        IReadOnlyList<Point3D> sampled = curve.Sample(15);
        foreach (var p in sampled)
            p.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Sample should produce a smooth sequence of points.</summary>
    [Fact]
    public void Sample_ShouldProduceSmoothSequence()
    {
        var curve = CreateSimpleCurve();
        IReadOnlyList<Point3D> sampled = curve.Sample(50);
        for (int i = 1; i < sampled.Count; i++)
        {
            double dx = sampled[i].X - sampled[i - 1].X;
            double dy = sampled[i].Y - sampled[i - 1].Y;
            double dist = System.Math.Sqrt(dx * dx + dy * dy);
            dist.Should().BeGreaterThan(0.0);
        }
    }
}
