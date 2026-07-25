using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

using Circle2D = MathVerse.Math.Geometry.Geometry2D.Circle2D;

namespace MathVerse.Geometry.Tests.Core;

/// <summary>Tests for the <see cref="GeometryDiagnostics"/> static class.</summary>
public class GeometryDiagnosticsTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies that IsValid returns true for a valid Point2D.</summary>
    [Fact]
    public void IsValid_Point2DValid_ReturnsTrue()
    {
        var p = new Point2D(1.0, 2.0);

        GeometryDiagnostics.IsValid(p).Should().BeTrue();
    }

    /// <summary>Verifies that IsValid returns false for Point2D with NaN.</summary>
    [Fact]
    public void IsValid_Point2DNaN_ReturnsFalse()
    {
        var p = new Point2D(double.NaN, 2.0);

        GeometryDiagnostics.IsValid(p).Should().BeFalse();
    }

    /// <summary>Verifies that IsValid returns true for a valid Point3D.</summary>
    [Fact]
    public void IsValid_Point3DValid_ReturnsTrue()
    {
        var p = new Point3D(1.0, 2.0, 3.0);

        GeometryDiagnostics.IsValid(p).Should().BeTrue();
    }

    /// <summary>Verifies that IsValid returns false for Point3D with NaN.</summary>
    [Fact]
    public void IsValid_Point3DNaN_ReturnsFalse()
    {
        var p = new Point3D(1.0, double.NaN, 3.0);

        GeometryDiagnostics.IsValid(p).Should().BeFalse();
    }

    /// <summary>Verifies that IsValid returns true for a valid Vector2D.</summary>
    [Fact]
    public void IsValid_Vector2DValid_ReturnsTrue()
    {
        var v = new Vector2D(1.0, 2.0);

        GeometryDiagnostics.IsValid(v).Should().BeTrue();
    }

    /// <summary>Verifies that IsValid returns false for Vector2D with NaN.</summary>
    [Fact]
    public void IsValid_Vector2DNaN_ReturnsFalse()
    {
        var v = new Vector2D(double.NaN, 0.0);

        GeometryDiagnostics.IsValid(v).Should().BeFalse();
    }

    /// <summary>Verifies that IsValid returns true for a valid Triangle2D.</summary>
    [Fact]
    public void IsValid_Triangle2DValid_ReturnsTrue()
    {
        var t = new Triangle2D(new Point2D(0, 0), new Point2D(1, 0), new Point2D(0, 1));

        GeometryDiagnostics.IsValid(t).Should().BeTrue();
    }

    /// <summary>Verifies that IsValid returns false for a degenerate triangle.</summary>
    [Fact]
    public void IsValid_Triangle2DDegenerate_ReturnsFalse()
    {
        var t = new Triangle2D(new Point2D(0, 0), new Point2D(1, 0), new Point2D(2, 0));

        GeometryDiagnostics.IsValid(t).Should().BeFalse();
    }

    /// <summary>Verifies that IsValid returns true for a valid Circle2D.</summary>
    [Fact]
    public void IsValid_Circle2DValid_ReturnsTrue()
    {
        var c = new Circle2D(new Point2D(0, 0), 5.0);

        GeometryDiagnostics.IsValid(c).Should().BeTrue();
    }

    /// <summary>Verifies that IsValid returns false for a circle with negative radius.</summary>
    [Fact]
    public void IsValid_Circle2DNegRadius_ReturnsFalse()
    {
        var c = new Circle2D(new Point2D(0, 0), -1.0);

        GeometryDiagnostics.IsValid(c).Should().BeFalse();
    }

    /// <summary>Verifies that IsValid returns false for a sphere with negative radius.</summary>
    [Fact]
    public void IsValid_Sphere3DNegRadius_ReturnsFalse()
    {
        var s = new Sphere3D(new Point3D(0, 0, 0), -1.0);

        GeometryDiagnostics.IsValid(s).Should().BeFalse();
    }

    /// <summary>Verifies that DegeneracyScore returns 0 for a degenerate triangle.</summary>
    [Fact]
    public void DegeneracyScore_Triangle2DDegenerate_ReturnsZero()
    {
        var t = new Triangle2D(new Point2D(0, 0), new Point2D(1, 0), new Point2D(2, 0));

        double score = GeometryDiagnostics.DegeneracyScore(t);

        score.Should().Be(0.0);
    }

    /// <summary>Verifies that IsConvex returns true for a convex polygon.</summary>
    [Fact]
    public void IsConvex_ConvexPolygon_ReturnsTrue()
    {
        var polygon = new List<Point2D> { new(0, 0), new(1, 0), new(1, 1), new(0, 1) };

        GeometryDiagnostics.IsConvex(polygon).Should().BeTrue();
    }

    /// <summary>Verifies that IsConvex returns false for a non-convex polygon.</summary>
    [Fact]
    public void IsConvex_ConcavePolygon_ReturnsFalse()
    {
        var polygon = new List<Point2D> { new(0, 0), new(1, 0), new(0.5, 1), new(-0.5, -0.5) };

        bool result = GeometryDiagnostics.IsConvex(polygon);

        result.Should().BeFalse();
    }

    /// <summary>Verifies that ComputeWindingOrder returns Clockwise for clockwise vertices.</summary>
    [Fact]
    public void ComputeWindingOrder_Clockwise_ReturnsClockwise()
    {
        var polygon = new List<Point2D> { new(0, 0), new(0, 1), new(1, 1), new(1, 0) };

        var order = GeometryDiagnostics.ComputeWindingOrder(polygon);

        order.Should().Be(WindingOrder.Clockwise);
    }
}
