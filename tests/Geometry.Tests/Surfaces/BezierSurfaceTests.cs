namespace MathVerse.Geometry.Tests.Surfaces;

using BS = MathVerse.Math.Geometry.Surfaces.BezierSurface;

/// <summary>Tests for BezierSurface class.</summary>
public class BezierSurfaceTests
{
    private const double Precision = 1e-10;

    private static BS CreateSimpleSurface()
    {
        var controlPoints = ImmutableArray.Create(
            ImmutableArray.Create(
                new Point3D(0, 0, 0), new Point3D(1, 0, 0)),
            ImmutableArray.Create(
                new Point3D(0, 1, 0), new Point3D(1, 1, 0)));
        return new BS(controlPoints);
    }

    /// <summary>PointAt (0,0) should return top-left control point.</summary>
    [Fact]
    public void PointAt_AtOrigin_ShouldReturnTopLeftCP()
    {
        var surface = CreateSimpleSurface();
        Point3D result = surface.PointAt(0, 0);
        result.X.Should().BeApproximately(0.0, Precision);
        result.Y.Should().BeApproximately(0.0, Precision);
        result.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>PointAt (1,1) should return bottom-right control point.</summary>
    [Fact]
    public void PointAt_AtOne_ShouldReturnBottomRightCP()
    {
        var surface = CreateSimpleSurface();
        Point3D result = surface.PointAt(1, 1);
        result.X.Should().BeApproximately(1.0, Precision);
        result.Y.Should().BeApproximately(1.0, Precision);
        result.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>PointAt (1,0) should return bottom-left control point (row=1, col=0).</summary>
    [Fact]
    public void PointAt_AtOneZero_ShouldReturnBottomLeftCP()
    {
        var surface = CreateSimpleSurface();
        Point3D result = surface.PointAt(1, 0);
        result.X.Should().BeApproximately(0.0, Precision);
        result.Y.Should().BeApproximately(1.0, Precision);
        result.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>PointAt (0,1) should return top-right control point (row=0, col=1).</summary>
    [Fact]
    public void PointAt_AtZeroOne_ShouldReturnTopRightCP()
    {
        var surface = CreateSimpleSurface();
        Point3D result = surface.PointAt(0, 1);
        result.X.Should().BeApproximately(1.0, Precision);
        result.Y.Should().BeApproximately(0.0, Precision);
        result.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>PointAt (0.5, 0.5) should be the midpoint of the bilinear surface.</summary>
    [Fact]
    public void PointAt_AtCenter_ShouldBeMidpoint()
    {
        var surface = CreateSimpleSurface();
        Point3D result = surface.PointAt(0.5, 0.5);
        result.X.Should().BeApproximately(0.5, Precision);
        result.Y.Should().BeApproximately(0.5, Precision);
        result.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Sample should return correct grid dimensions.</summary>
    [Fact]
    public void Sample_ShouldReturnCorrectGridDimensions()
    {
        var surface = CreateSimpleSurface();
        ImmutableArray<ImmutableArray<Point3D>> grid = surface.Sample(5, 6);
        grid.Length.Should().Be(5);
        grid[0].Length.Should().Be(6);
    }

    /// <summary>Sample corner (0,0) should match PointAt(0,0).</summary>
    [Fact]
    public void Sample_Corner_ShouldMatchPointAt()
    {
        var surface = CreateSimpleSurface();
        ImmutableArray<ImmutableArray<Point3D>> grid = surface.Sample(3, 3);
        Point3D evaluated = surface.PointAt(0, 0);
        grid[0][0].X.Should().BeApproximately(evaluated.X, Precision);
        grid[0][0].Y.Should().BeApproximately(evaluated.Y, Precision);
    }

    /// <summary>DegreeU should equal columns - 1.</summary>
    [Fact]
    public void DegreeU_ShouldBeColumnsMinusOne()
    {
        var surface = CreateSimpleSurface();
        surface.DegreeU.Should().Be(1);
    }

    /// <summary>DegreeV should equal rows - 1.</summary>
    [Fact]
    public void DegreeV_ShouldBeRowsMinusOne()
    {
        var surface = CreateSimpleSurface();
        surface.DegreeV.Should().Be(1);
    }

    /// <summary>Quadratic Bezier surface at corners should match control points.</summary>
    [Fact]
    public void QuadraticBezierSurface_Corners_ShouldMatchCPs()
    {
        var cp = ImmutableArray.Create(
            ImmutableArray.Create(
                new Point3D(0, 0, 0), new Point3D(1, 0, 1), new Point3D(2, 0, 0)),
            ImmutableArray.Create(
                new Point3D(0, 1, 1), new Point3D(1, 1, 2), new Point3D(2, 1, 1)),
            ImmutableArray.Create(
                new Point3D(0, 2, 0), new Point3D(1, 2, 1), new Point3D(2, 2, 0)));
        var surface = new BS(cp);
        Point3D at00 = surface.PointAt(0, 0);
        at00.Z.Should().BeApproximately(0.0, Precision);
        Point3D at11 = surface.PointAt(1, 1);
        at11.Z.Should().BeApproximately(0.0, Precision);
    }
}
