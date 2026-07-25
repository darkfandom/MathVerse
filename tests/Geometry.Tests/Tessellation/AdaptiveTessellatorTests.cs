namespace MathVerse.Geometry.Tests.Tessellation;

/// <summary>Tests for the <see cref="AdaptiveTessellator"/> static class.</summary>
public class AdaptiveTessellatorTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies TessellateCurve of a straight line returns at least 2 points.</summary>
    [Fact]
    public void TessellateCurve_LinearFunction_ReturnsAtLeastTwoPoints()
    {
        Func<double, Point2D> line = t => new Point2D(t, t);

        var result = AdaptiveTessellator.TessellateCurve(line, 0, 1, 1, 10, 0.1);

        result.Length.Should().BeGreaterThanOrEqualTo(2);
    }

    /// <summary>Verifies TessellateCurve starts at tMin and ends at tMax.</summary>
    [Fact]
    public void TessellateCurve_StartsAndEndsCorrectly()
    {
        Func<double, Point2D> f = t => new Point2D(t * 2, t * 3);

        var result = AdaptiveTessellator.TessellateCurve(f, 0, 1, 1, 10, 0.1);

        result[0].X.Should().BeApproximately(0.0, Tolerance);
        result[0].Y.Should().BeApproximately(0.0, Tolerance);
        result[^1].X.Should().BeApproximately(2.0, Tolerance);
        result[^1].Y.Should().BeApproximately(3.0, Tolerance);
    }

    /// <summary>Verifies TessellateCurve of a highly curved function produces more points.</summary>
    [Fact]
    public void TessellateCurve_HighCurvature_ProducesMorePoints()
    {
        Func<double, Point2D> curve = t => new Point2D(t, System.Math.Sin(t * System.Math.PI * 4));

        var result = AdaptiveTessellator.TessellateCurve(curve, 0, 1, 1, 20, 0.01);

        result.Length.Should().BeGreaterThan(2);
    }

    /// <summary>Verifies SubdivideEdge returns at least the start point.</summary>
    [Fact]
    public void SubdivideEdge_NeverSplit_ReturnsStartPoint()
    {
        var a = new Point2D(0, 0);
        var b = new Point2D(1, 1);

        var result = AdaptiveTessellator.SubdivideEdge(a, b, _ => false, 10);

        result.Length.Should().BeGreaterThanOrEqualTo(1);
    }

    /// <summary>Verifies SubdivideEdge with maxDepth 0 returns only the start point.</summary>
    [Fact]
    public void SubdivideEdge_ZeroDepth_ReturnsStartPoint()
    {
        var a = new Point2D(0, 0);
        var b = new Point2D(1, 1);

        var result = AdaptiveTessellator.SubdivideEdge(a, b, _ => true, 0);

        result.Length.Should().Be(1);
    }

    /// <summary>Verifies SubdivideEdge with splitting always true subdivides recursively.</summary>
    [Fact]
    public void SubdivideEdge_AlwaysSplit_ProducesMultiplePoints()
    {
        var a = new Point2D(0, 0);
        var b = new Point2D(1, 1);

        var result = AdaptiveTessellator.SubdivideEdge(a, b, _ => true, 3);

        result.Length.Should().BeGreaterThan(1);
    }

    /// <summary>Verifies TessellateCurve of a constant function produces the minimum points.</summary>
    [Fact]
    public void TessellateCurve_ConstantFunction_ProducesMinimumPoints()
    {
        Func<double, Point2D> constant = _ => new Point2D(5, 5);

        var result = AdaptiveTessellator.TessellateCurve(constant, 0, 1, 1, 10, 0.001);

        result.Length.Should().BeGreaterThanOrEqualTo(2);
    }

    /// <summary>Verifies TessellateCurve first and last points match function evaluation.</summary>
    [Fact]
    public void TessellateCurve_EndPointsMatchEvaluation()
    {
        Func<double, Point2D> f = t => new Point2D(t * t, t);

        var result = AdaptiveTessellator.TessellateCurve(f, 0, 2, 1, 10, 0.1);

        result[0].Should().Be(new Point2D(0, 0));
        result[^1].Should().Be(new Point2D(4, 2));
    }

    /// <summary>Verifies SubdivideEdge with split always returns at least 2 points at depth 1.</summary>
    [Fact]
    public void SubdivideEdge_DepthOne_SplitTrue_ReturnsTwoPoints()
    {
        var a = new Point2D(0, 0);
        var b = new Point2D(1, 1);

        var result = AdaptiveTessellator.SubdivideEdge(a, b, _ => true, 1);

        result.Length.Should().Be(2);
    }

    /// <summary>Verifies TessellateCurve result points are ordered along the curve.</summary>
    [Fact]
    public void TessellateCurve_PointsAreOrderedByParameter()
    {
        Func<double, Point2D> f = t => new Point2D(t, t);

        var result = AdaptiveTessellator.TessellateCurve(f, 0, 5, 1, 10, 0.5);

        for (int i = 1; i < result.Length; i++)
        {
            result[i].X.Should().BeGreaterThanOrEqualTo(result[i - 1].X);
        }
    }
}
