namespace MathVerse.Geometry.Tests.Surfaces;

/// <summary>Tests for ParametricSurface class.</summary>
public class ParametricSurfaceTests
{
    private const double Precision = 1e-5;

    private static ParametricSurface CreateXYPlane()
    {
        return new ParametricSurface(
            (u, v) => new Point3D(u, v, 0),
            0, 1, 0, 1);
    }

    private static ParametricSurface CreateSphere()
    {
        return new ParametricSurface(
            (u, v) => new Point3D(
                System.Math.Cos(u) * System.Math.Sin(v),
                System.Math.Sin(u) * System.Math.Sin(v),
                System.Math.Cos(v)),
            0, 2 * System.Math.PI, 0, System.Math.PI);
    }

    /// <summary>Evaluate at corner should return correct point.</summary>
    [Fact]
    public void Evaluate_AtCorner_ShouldReturnCorrectPoint()
    {
        var surface = CreateXYPlane();
        Point3D result = surface.Evaluate(0.5, 0.3);
        result.X.Should().BeApproximately(0.5, Precision);
        result.Y.Should().BeApproximately(0.3, Precision);
        result.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Evaluate at origin should return (0,0,0) for XY plane.</summary>
    [Fact]
    public void Evaluate_AtOrigin_ShouldReturnZero()
    {
        var surface = CreateXYPlane();
        Point3D result = surface.Evaluate(0, 0);
        result.X.Should().BeApproximately(0.0, Precision);
        result.Y.Should().BeApproximately(0.0, Precision);
        result.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Normal of XY plane should point along Z axis.</summary>
    [Fact]
    public void Normal_XYPlane_ShouldPointAlongZ()
    {
        var surface = CreateXYPlane();
        Vector3D normal = surface.Normal(0.5, 0.5);
        normal.Z.Should().BeApproximately(1.0, Precision);
        normal.X.Should().BeApproximately(0.0, Precision);
        normal.Y.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Normal should be unit length.</summary>
    [Fact]
    public void Normal_Sphere_ShouldBeUnitLength()
    {
        var surface = CreateSphere();
        Vector3D normal = surface.Normal(System.Math.PI / 4, System.Math.PI / 3);
        normal.Length.Should().BeApproximately(1.0, 1e-4);
    }

    /// <summary>Sample should return correct grid dimensions.</summary>
    [Fact]
    public void Sample_ShouldReturnCorrectGridDimensions()
    {
        var surface = CreateXYPlane();
        ImmutableArray<ImmutableArray<Point3D>> grid = surface.Sample(5, 4);
        grid.Length.Should().Be(5);
        grid[0].Length.Should().Be(4);
    }

    /// <summary>Sample corner should match Evaluate at corners.</summary>
    [Fact]
    public void Sample_Corner_ShouldMatchEvaluate()
    {
        var surface = CreateXYPlane();
        ImmutableArray<ImmutableArray<Point3D>> grid = surface.Sample(3, 3);
        grid[0][0].X.Should().BeApproximately(0.0, Precision);
        grid[0][0].Y.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>TangentU of XY plane should point along X axis.</summary>
    [Fact]
    public void TangentU_XYPlane_ShouldPointAlongX()
    {
        var surface = CreateXYPlane();
        Vector3D tangent = surface.TangentU(0.5, 0.5);
        tangent.X.Should().BeApproximately(1.0, Precision);
        tangent.Y.Should().BeApproximately(0.0, Precision);
        tangent.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>TangentV of XY plane should point along Y axis.</summary>
    [Fact]
    public void TangentV_XYPlane_ShouldPointAlongY()
    {
        var surface = CreateXYPlane();
        Vector3D tangent = surface.TangentV(0.5, 0.5);
        tangent.X.Should().BeApproximately(0.0, Precision);
        tangent.Y.Should().BeApproximately(1.0, Precision);
        tangent.Z.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>UMin/UMax/VMin/VMax should be set correctly.</summary>
    [Fact]
    public void Bounds_ShouldBeSetCorrectly()
    {
        var surface = new ParametricSurface(
            (u, v) => new Point3D(u, v, 0), -1, 2, 0, 5);
        surface.UMin.Should().BeApproximately(-1.0, Precision);
        surface.UMax.Should().BeApproximately(2.0, Precision);
        surface.VMin.Should().BeApproximately(0.0, Precision);
        surface.VMax.Should().BeApproximately(5.0, Precision);
    }

    /// <summary>Sphere normal at north pole should point along Z axis.</summary>
    [Fact]
    public void Normal_SphereNorthPole_ShouldPointAlongZ()
    {
        var surface = CreateSphere();
        Vector3D normal = surface.Normal(System.Math.PI, 0.01);
        System.Math.Abs(normal.Z).Should().BeApproximately(1.0, 1e-2);
    }

    /// <summary>TangentU and TangentV should be perpendicular for sphere.</summary>
    [Fact]
    public void Sphere_TangentUAndV_ShouldBePerpendicular()
    {
        var surface = CreateSphere();
        Vector3D tu = surface.TangentU(System.Math.PI / 4, System.Math.PI / 3);
        Vector3D tv = surface.TangentV(System.Math.PI / 4, System.Math.PI / 3);
        double dot = tu.Dot(tv);
        dot.Should().BeApproximately(0.0, 1e-3);
    }

    /// <summary>Normal of sphere should point radially (inward for this parameterization).</summary>
    [Fact]
    public void Normal_Sphere_ShouldPointRadiallyOutward()
    {
        var surface = CreateSphere();
        double u = System.Math.PI / 2;
        double v = System.Math.PI / 2;
        Point3D point = surface.Evaluate(u, v);
        Vector3D normal = surface.Normal(u, v);
        double dot = normal.X * point.X + normal.Y * point.Y + normal.Z * point.Z;
        System.Math.Abs(dot).Should().BeApproximately(1.0, 1e-4);
    }
}
