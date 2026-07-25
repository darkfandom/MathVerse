namespace MathVerse.Geometry.Tests.Curves;

/// <summary>Tests for ImplicitCurve2D class.</summary>
public class ImplicitCurve2DTests
{
    private const double Precision = 1e-10;

    /// <summary>Circle x^2+y^2-1=0 should evaluate to 0 at (1,0).</summary>
    [Fact]
    public void Evaluate_Circle_ShouldBeZeroOnCurve()
    {
        var curve = new ImplicitCurve2D((x, y) => x * x + y * y - 1.0);
        curve.Evaluate(1, 0).Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Circle should evaluate to positive outside.</summary>
    [Fact]
    public void Evaluate_Circle_ShouldBePositiveOutside()
    {
        var curve = new ImplicitCurve2D((x, y) => x * x + y * y - 1.0);
        curve.Evaluate(2, 0).Should().BeGreaterThan(0.0);
    }

    /// <summary>Circle should evaluate to negative inside.</summary>
    [Fact]
    public void Evaluate_Circle_ShouldBeNegativeInside()
    {
        var curve = new ImplicitCurve2D((x, y) => x * x + y * y - 1.0);
        curve.Evaluate(0, 0).Should().BeLessThan(0.0);
    }

    /// <summary>Circle at (0,1) should be on curve.</summary>
    [Fact]
    public void Evaluate_Circle_AtTop_ShouldBeZero()
    {
        var curve = new ImplicitCurve2D((x, y) => x * x + y * y - 1.0);
        curve.Evaluate(0, 1).Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Line x+y=0 should evaluate to 0 at (1,-1).</summary>
    [Fact]
    public void Evaluate_Line_ShouldBeZeroOnLine()
    {
        var curve = new ImplicitCurve2D((x, y) => x + y);
        curve.Evaluate(1, -1).Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Contour of a circle should generate segments.</summary>
    [Fact]
    public void Contour_Circle_ShouldGenerateSegments()
    {
        var curve = new ImplicitCurve2D((x, y) => x * x + y * y - 1.0);
        ImmutableArray<Segment2D> segments = curve.Contour(-2, 2, -2, 2, 50);
        segments.Length.Should().BeGreaterThan(0);
    }

    /// <summary>Contour segments should approximate the circle shape.</summary>
    [Fact]
    public void Contour_Circle_ShouldApproximateCircle()
    {
        var curve = new ImplicitCurve2D((x, y) => x * x + y * y - 1.0);
        ImmutableArray<Segment2D> segments = curve.Contour(-2, 2, -2, 2, 100);
        segments.Length.Should().BeGreaterThan(10);
    }

    /// <summary>Contour of a line should generate segments.</summary>
    [Fact]
    public void Contour_Line_ShouldGenerateSegments()
    {
        var curve = new ImplicitCurve2D((x, y) => x + y);
        ImmutableArray<Segment2D> segments = curve.Contour(-5, 5, -5, 5, 20);
        segments.Length.Should().BeGreaterThan(0);
    }

    /// <summary>Contour with higher resolution should produce more segments.</summary>
    [Fact]
    public void Contour_HigherResolution_ShouldProduceMoreSegments()
    {
        var curve = new ImplicitCurve2D((x, y) => x * x + y * y - 1.0);
        ImmutableArray<Segment2D> low = curve.Contour(-2, 2, -2, 2, 20);
        ImmutableArray<Segment2D> high = curve.Contour(-2, 2, -2, 2, 80);
        high.Length.Should().BeGreaterThanOrEqualTo(low.Length);
    }

    /// <summary>Contour of a constant-positive function should produce no segments.</summary>
    [Fact]
    public void Contour_AllPositive_ShouldProduceNoSegments()
    {
        var curve = new ImplicitCurve2D((x, y) => 1.0);
        ImmutableArray<Segment2D> segments = curve.Contour(-1, 1, -1, 1, 10);
        segments.Length.Should().Be(0);
    }
}
