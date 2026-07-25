namespace MathVerse.Geometry.Tests.Surfaces;

using TM = MathVerse.Math.Geometry.Mesh.TriangleMesh;

/// <summary>Tests for HeightMap class.</summary>
public class HeightMapTests
{
    private const double Precision = 1e-10;

    private static HeightMap CreateSimpleHeightMap()
    {
        var heights = new double[,] {
            { 0, 1, 2 },
            { 3, 4, 5 },
            { 6, 7, 8 }
        };
        return new HeightMap(heights, 0, 2, 0, 2);
    }

    /// <summary>Width should return number of samples along X axis.</summary>
    [Fact]
    public void Width_ShouldReturnXDimension()
    {
        var hm = CreateSimpleHeightMap();
        hm.Width.Should().Be(3);
    }

    /// <summary>Height should return number of samples along Y axis.</summary>
    [Fact]
    public void Height_ShouldReturnYDimension()
    {
        var hm = CreateSimpleHeightMap();
        hm.Height.Should().Be(3);
    }

    /// <summary>Min should return the minimum height value.</summary>
    [Fact]
    public void Min_ShouldReturnMinimumHeight()
    {
        var hm = CreateSimpleHeightMap();
        hm.Min.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Max should return the maximum height value.</summary>
    [Fact]
    public void Max_ShouldReturnMaximumHeight()
    {
        var hm = CreateSimpleHeightMap();
        hm.Max.Should().BeApproximately(8.0, Precision);
    }

    /// <summary>Evaluate at grid point should return exact height value.</summary>
    [Fact]
    public void Evaluate_AtGridPoint_ShouldReturnExactHeight()
    {
        var hm = CreateSimpleHeightMap();
        double result = hm.Evaluate(0, 0);
        result.Should().BeApproximately(0.0, Precision);
    }

    /// <summary>Evaluate at center of grid should interpolate correctly.</summary>
    [Fact]
    public void Evaluate_AtCenter_ShouldInterpolate()
    {
        var hm = CreateSimpleHeightMap();
        double result = hm.Evaluate(1, 1);
        result.Should().BeApproximately(4.0, Precision);
    }

    /// <summary>Evaluate bilinear interpolation between four grid points.</summary>
    [Fact]
    public void Evaluate_Bilinear_ShouldInterpolateCorrectly()
    {
        var heights = new double[,] {
            { 0, 0 },
            { 0, 4 }
        };
        var hm = new HeightMap(heights, 0, 1, 0, 1);
        double result = hm.Evaluate(0.5, 0.5);
        result.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>Normal should return a unit vector.</summary>
    [Fact]
    public void Normal_ShouldReturnUnitVector()
    {
        var hm = CreateSimpleHeightMap();
        Vector3D normal = hm.Normal(1, 1);
        normal.Length.Should().BeApproximately(1.0, 1e-4);
    }

    /// <summary>Normal of flat height map should point along Z axis.</summary>
    [Fact]
    public void Normal_FlatSurface_ShouldPointAlongZ()
    {
        var heights = new double[,] {
            { 5, 5, 5 },
            { 5, 5, 5 },
            { 5, 5, 5 }
        };
        var hm = new HeightMap(heights, 0, 2, 0, 2);
        Vector3D normal = hm.Normal(1, 1);
        normal.Z.Should().BeApproximately(1.0, Precision);
    }

    /// <summary>ToMesh should generate a non-empty mesh.</summary>
    [Fact]
    public void ToMesh_ShouldGenerateNonEmptyMesh()
    {
        var hm = CreateSimpleHeightMap();
        TM mesh = hm.ToMesh(10);
        mesh.VertexCount.Should().BeGreaterThan(0);
        mesh.TriangleCount.Should().BeGreaterThan(0);
    }

    /// <summary>ToMesh vertex count should match expected grid size.</summary>
    [Fact]
    public void ToMesh_VertexCount_ShouldMatchGridSize()
    {
        var hm = CreateSimpleHeightMap();
        int resolution = 5;
        TM mesh = hm.ToMesh(resolution);
        int expectedVertices = (resolution + 1) * (resolution + 1);
        mesh.VertexCount.Should().Be(expectedVertices);
    }

    /// <summary>ToMesh triangle count should match expected formula.</summary>
    [Fact]
    public void ToMesh_TriangleCount_ShouldMatchExpected()
    {
        var hm = CreateSimpleHeightMap();
        int resolution = 4;
        TM mesh = hm.ToMesh(resolution);
        int expectedTriangles = resolution * resolution * 2;
        mesh.TriangleCount.Should().Be(expectedTriangles);
    }
}
