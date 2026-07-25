namespace MathVerse.Geometry.Tests.Tessellation;

/// <summary>Tests for the <see cref="CurveSubdivider"/> static class.</summary>
public class CurveSubdividerTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies 2D subdivision doubles point count minus one per iteration.</summary>
    [Fact]
    public void Subdivide2D_OneIteration_DoublesPointsMinusOne()
    {
        var points = new List<Point2D>
        {
            new(0, 0), new(2, 2)
        };

        var result = CurveSubdivider.Subdivide(points, 1);

        result.Length.Should().Be(3);
    }

    /// <summary>Verifies 2D subdivision preserves endpoints.</summary>
    [Fact]
    public void Subdivide2D_PreservesEndpoints()
    {
        var points = new List<Point2D>
        {
            new(0, 0), new(4, 4)
        };

        var result = CurveSubdivider.Subdivide(points, 2);

        result[0].Should().Be(new Point2D(0, 0));
        result[^1].Should().Be(new Point2D(4, 4));
    }

    /// <summary>Verifies 3D subdivision doubles point count minus one per iteration.</summary>
    [Fact]
    public void Subdivide3D_OneIteration_DoublesPointsMinusOne()
    {
        var points = new List<Point3D>
        {
            new(0, 0, 0), new(2, 2, 2)
        };

        var result = CurveSubdivider.Subdivide(points, 1);

        result.Length.Should().Be(3);
    }

    /// <summary>Verifies 3D subdivision preserves endpoints.</summary>
    [Fact]
    public void Subdivide3D_PreservesEndpoints()
    {
        var points = new List<Point3D>
        {
            new(0, 0, 0), new(3, 3, 3)
        };

        var result = CurveSubdivider.Subdivide(points, 3);

        result[0].Should().Be(new Point3D(0, 0, 0));
        result[^1].Should().Be(new Point3D(3, 3, 3));
    }

    /// <summary>Verifies ChaikinSubdivide on a sharp corner smooths it.</summary>
    [Fact]
    public void ChaikinSubdivide_SmoothesCorner()
    {
        var points = new List<Point2D>
        {
            new(0, 0), new(1, 0), new(1, 1)
        };

        var result = CurveSubdivider.ChaikinSubdivide(points, 1);

        result.Length.Should().Be(4);
    }

    /// <summary>Verifies ChaikinSubdivide produces more points per iteration.</summary>
    [Fact]
    public void ChaikinSubdivide_MultipleIterations_ProducesMorePoints()
    {
        var points = new List<Point2D>
        {
            new(0, 0), new(1, 0), new(1, 1), new(0, 1)
        };

        var result1 = CurveSubdivider.ChaikinSubdivide(points, 1);
        var result2 = CurveSubdivider.ChaikinSubdivide(points, 2);

        result2.Length.Should().BeGreaterThan(result1.Length);
    }

    /// <summary>Verifies subdivision with 0 iterations returns the original points.</summary>
    [Fact]
    public void Subdivide2D_ZeroIterations_ReturnsOriginal()
    {
        var points = new List<Point2D>
        {
            new(0, 0), new(1, 1)
        };

        var result = CurveSubdivider.Subdivide(points, 0);

        result.Length.Should().Be(2);
    }

    /// <summary>Verifies 3D subdivision midpoints are at the center of each segment.</summary>
    [Fact]
    public void Subdivide3D_MidpointAtCenter()
    {
        var points = new List<Point3D>
        {
            new(0, 0, 0), new(2, 2, 2)
        };

        var result = CurveSubdivider.Subdivide(points, 1);

        result[1].X.Should().BeApproximately(1.0, Tolerance);
        result[1].Y.Should().BeApproximately(1.0, Tolerance);
        result[1].Z.Should().BeApproximately(1.0, Tolerance);
    }
}
