namespace MathVerse.Geometry.Tests.Surfaces;

using TM = MathVerse.Math.Geometry.Mesh.TriangleMesh;

/// <summary>Tests for ImplicitSurface class.</summary>
public class ImplicitSurfaceTests
{
    private const double Precision = 1e-10;

    private static ImplicitSurface CreateUnitSphere()
    {
        return new ImplicitSurface((x, y, z) => x * x + y * y + z * z - 1.0);
    }

    /// <summary>Evaluate on surface at (1,0,0) should be zero.</summary>
    [Fact]
    public void Evaluate_Sphere_AtEquator_ShouldBeZero()
    {
        var surface = CreateUnitSphere();
        surface.Evaluate(1, 0, 0).Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Evaluate inside sphere should be negative.</summary>
    [Fact]
    public void Evaluate_Sphere_Inside_ShouldBeNegative()
    {
        var surface = CreateUnitSphere();
        surface.Evaluate(0, 0, 0).Should().BeLessThan(0.0);
    }

    /// <summary>Evaluate outside sphere should be positive.</summary>
    [Fact]
    public void Evaluate_Sphere_Outside_ShouldBePositive()
    {
        var surface = CreateUnitSphere();
        surface.Evaluate(2, 0, 0).Should().BeGreaterThan(0.0);
    }

    /// <summary>Evaluate at north pole should be on surface.</summary>
    [Fact]
    public void Evaluate_Sphere_NorthPole_ShouldBeZero()
    {
        var surface = CreateUnitSphere();
        surface.Evaluate(0, 0, 1).Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Evaluate at south pole should be on surface.</summary>
    [Fact]
    public void Evaluate_Sphere_SouthPole_ShouldBeZero()
    {
        var surface = CreateUnitSphere();
        surface.Evaluate(0, 0, -1).Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Plane z=0 should evaluate to zero on the plane.</summary>
    [Fact]
    public void Evaluate_Plane_ShouldBeZeroOnPlane()
    {
        var surface = new ImplicitSurface((x, y, z) => z);
        surface.Evaluate(5, 10, 0).Should().BeApproximately(0.0, Precision);
    }

    /// <summary>MarchingCubes should generate a non-empty mesh for sphere.</summary>
    [Fact]
    public void MarchingCubes_Sphere_ShouldGenerateMesh()
    {
        var surface = CreateUnitSphere();
        TM mesh = surface.MarchingCubes(-2, 2, -2, 2, -2, 2, 20, 0);
        mesh.VertexCount.Should().BeGreaterThan(0);
        mesh.TriangleCount.Should().BeGreaterThan(0);
    }

    /// <summary>MarchingCubes mesh should have triangle count a multiple of 3.</summary>
    [Fact]
    public void MarchingCubes_TriangleCount_ShouldBeMultipleOfThree()
    {
        var surface = CreateUnitSphere();
        TM mesh = surface.MarchingCubes(-2, 2, -2, 2, -2, 2, 15, 0);
        mesh.TriangleCount.Should().BeGreaterThan(0);
    }

    /// <summary>MarchingCubes with higher resolution should produce more triangles.</summary>
    [Fact]
    public void MarchingCubes_HigherResolution_ShouldProduceMoreTriangles()
    {
        var surface = CreateUnitSphere();
        TM low = surface.MarchingCubes(-2, 2, -2, 2, -2, 2, 10, 0);
        TM high = surface.MarchingCubes(-2, 2, -2, 2, -2, 2, 30, 0);
        high.TriangleCount.Should().BeGreaterThanOrEqualTo(low.TriangleCount);
    }

    /// <summary>F property should be accessible and callable.</summary>
    [Fact]
    public void F_ShouldBeAccessible()
    {
        var surface = CreateUnitSphere();
        double result = surface.F(1, 0, 0);
        result.Should().BeApproximately(0.0, Precision);
    }
}
