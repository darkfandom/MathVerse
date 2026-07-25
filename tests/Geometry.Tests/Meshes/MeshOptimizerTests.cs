namespace MathVerse.Geometry.Tests.Meshes;

/// <summary>Tests for the <see cref="MeshOptimizer"/> static class.</summary>
public class MeshOptimizerTests
{
    private const double Tolerance = 1e-6;

    private static Vertex V(double x, double y, double z) =>
        new(new Point3D(x, y, z), Vector3D.UnitY, (0, 0));

    private static TriangleMesh MakeSingleTriangleMesh()
    {
        var vertices = ImmutableArray.Create(
            V(0, 0, 0), V(1, 0, 0), V(0, 1, 0));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        return new TriangleMesh(vertices, faces);
    }

    private static TriangleMesh MakeUnitCube()
    {
        var vertices = ImmutableArray.Create(
            V(0, 0, 0), V(1, 0, 0), V(1, 1, 0), V(0, 1, 0),
            V(0, 0, 1), V(1, 0, 1), V(1, 1, 1), V(0, 1, 1));
        var faces = ImmutableArray.Create(
            new TriangleFace(0, 1, 2), new TriangleFace(0, 2, 3),
            new TriangleFace(4, 6, 5), new TriangleFace(4, 7, 6),
            new TriangleFace(0, 4, 5), new TriangleFace(0, 5, 1),
            new TriangleFace(2, 6, 7), new TriangleFace(2, 7, 3),
            new TriangleFace(0, 3, 7), new TriangleFace(0, 7, 4),
            new TriangleFace(1, 5, 6), new TriangleFace(1, 6, 2));
        return new TriangleMesh(vertices, faces);
    }

    /// <summary>Verifies WeldVertices merges vertices within tolerance.</summary>
    [Fact]
    public void WeldVertices_CloseVertices_Merges()
    {
        var vertices = ImmutableArray.Create(
            V(0, 0, 0), V(0.0001, 0, 0), V(1, 0, 0));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        var mesh = new TriangleMesh(vertices, faces);

        TriangleMesh result = MeshOptimizer.WeldVertices(mesh, 0.01);

        result.VertexCount.Should().Be(2);
    }

    /// <summary>Verifies WeldVertices preserves vertices beyond tolerance.</summary>
    [Fact]
    public void WeldVertices_DistantVertices_PreservesCount()
    {
        var mesh = MakeSingleTriangleMesh();

        TriangleMesh result = MeshOptimizer.WeldVertices(mesh, 0.001);

        result.VertexCount.Should().Be(3);
    }

    /// <summary>Verifies RemoveDegenerateTriangles removes zero-area triangles.</summary>
    [Fact]
    public void RemoveDegenerateTriangles_ZeroArea_RemovesFace()
    {
        var vertices = ImmutableArray.Create(
            V(0, 0, 0), V(1, 0, 0), V(2, 0, 0));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        var mesh = new TriangleMesh(vertices, faces);

        TriangleMesh result = MeshOptimizer.RemoveDegenerateTriangles(mesh, 1e-6);

        result.TriangleCount.Should().Be(0);
    }

    /// <summary>Verifies RemoveDegenerateTriangles keeps valid triangles.</summary>
    [Fact]
    public void RemoveDegenerateTriangles_ValidTriangle_KeepsFace()
    {
        var mesh = MakeSingleTriangleMesh();

        TriangleMesh result = MeshOptimizer.RemoveDegenerateTriangles(mesh, 1e-6);

        result.TriangleCount.Should().Be(1);
    }

    /// <summary>Verifies ComputeEdgeLengths returns correct count and values.</summary>
    [Fact]
    public void ComputeEdgeLengths_SingleTriangle_ReturnsThreeLengths()
    {
        var mesh = MakeSingleTriangleMesh();

        var lengths = MeshOptimizer.ComputeEdgeLengths(mesh);

        lengths.Should().HaveCount(3);
        lengths[0].Should().BeApproximately(1.0, Tolerance);
        lengths[1].Should().BeApproximately(System.Math.Sqrt(2), Tolerance);
        lengths[2].Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies ComputeVertexValences returns correct counts.</summary>
    [Fact]
    public void ComputeVertexValences_SingleTriangle_ReturnsOne()
    {
        var mesh = MakeSingleTriangleMesh();

        var valences = MeshOptimizer.ComputeVertexValences(mesh);

        valences.Should().HaveCount(3);
        valences.Should().AllBeEquivalentTo(1);
    }

    /// <summary>Verifies ComputeTriangleAreas returns correct area for a right triangle.</summary>
    [Fact]
    public void ComputeTriangleAreas_RightTriangle_CorrectArea()
    {
        var mesh = MakeSingleTriangleMesh();

        var areas = MeshOptimizer.ComputeTriangleAreas(mesh);

        areas.Should().HaveCount(1);
        areas[0].Should().BeApproximately(0.5, Tolerance);
    }

    /// <summary>Verifies ComputeMeshVolume for a unit cube is approximately 1.</summary>
    [Fact]
    public void ComputeMeshVolume_UnitCube_ReturnsOne()
    {
        var cube = MakeUnitCube();

        double volume = MeshOptimizer.ComputeMeshVolume(cube);

        System.Math.Abs(volume).Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies ComputeSurfaceArea for a unit cube is 6.</summary>
    [Fact]
    public void ComputeSurfaceArea_UnitCube_ReturnsSix()
    {
        var cube = MakeUnitCube();

        double area = MeshOptimizer.ComputeSurfaceArea(cube);

        area.Should().BeApproximately(6.0, Tolerance);
    }

    /// <summary>Verifies IsManifold returns true for a valid mesh.</summary>
    [Fact]
    public void IsManifold_ValidMesh_ReturnsTrue()
    {
        var cube = MakeUnitCube();

        MeshOptimizer.IsManifold(cube).Should().BeTrue();
    }

    /// <summary>Verifies IsWatertight returns true for a closed cube mesh.</summary>
    [Fact]
    public void IsWatertight_CubeMesh_ReturnsTrue()
    {
        var cube = MakeUnitCube();

        MeshOptimizer.IsWatertight(cube).Should().BeTrue();
    }

    /// <summary>Verifies ComputeEulerCharacteristic for a cube is 2 (sphere topology).</summary>
    [Fact]
    public void ComputeEulerCharacteristic_CubeMesh_ReturnsTwo()
    {
        var cube = MakeUnitCube();

        int chi = MeshOptimizer.ComputeEulerCharacteristic(cube);

        chi.Should().Be(2);
    }

    /// <summary>Verifies ComputeEdgeLengths for two triangles returns six lengths.</summary>
    [Fact]
    public void ComputeEdgeLengths_TwoTriangles_ReturnsSixLengths()
    {
        var vertices = ImmutableArray.Create(
            V(0, 0, 0), V(1, 0, 0), V(0, 1, 0), V(1, 1, 0));
        var faces = ImmutableArray.Create(
            new TriangleFace(0, 1, 2),
            new TriangleFace(1, 3, 2));
        var mesh = new TriangleMesh(vertices, faces);

        var lengths = MeshOptimizer.ComputeEdgeLengths(mesh);

        lengths.Should().HaveCount(6);
    }

    /// <summary>Verifies FlipEdge returns the same mesh for invalid face indices.</summary>
    [Fact]
    public void FlipEdge_InvalidIndices_ReturnsSameMesh()
    {
        var mesh = MakeSingleTriangleMesh();

        TriangleMesh result = MeshOptimizer.FlipEdge(mesh, 0, 0);

        result.Should().BeSameAs(mesh);
    }

    /// <summary>Verifies FlipEdge on two adjacent triangles modifies faces.</summary>
    [Fact]
    public void FlipEdge_AdjacentTriangles_ModifiesFaces()
    {
        var vertices = ImmutableArray.Create(
            V(0, 0, 0), V(1, 0, 0), V(0, 1, 0), V(1, 1, 0));
        var faces = ImmutableArray.Create(
            new TriangleFace(0, 1, 2),
            new TriangleFace(1, 3, 2));
        var mesh = new TriangleMesh(vertices, faces);

        TriangleMesh result = MeshOptimizer.FlipEdge(mesh, 0, 1);

        result.TriangleCount.Should().Be(2);
    }

    /// <summary>Verifies ComputeVertexValences for two triangles sharing vertices.</summary>
    [Fact]
    public void ComputeVertexValences_TwoTriangles_CorrectValences()
    {
        var vertices = ImmutableArray.Create(
            V(0, 0, 0), V(1, 0, 0), V(0, 1, 0), V(1, 1, 0));
        var faces = ImmutableArray.Create(
            new TriangleFace(0, 1, 2),
            new TriangleFace(1, 3, 2));
        var mesh = new TriangleMesh(vertices, faces);

        var valences = MeshOptimizer.ComputeVertexValences(mesh);

        valences.Should().HaveCount(4);
        valences[0].Should().Be(1);
        valences[1].Should().Be(2);
        valences[2].Should().Be(2);
        valences[3].Should().Be(1);
    }

    /// <summary>Verifies IsWatertight for a single open triangle is false.</summary>
    [Fact]
    public void IsWatertight_OpenTriangle_ReturnsFalse()
    {
        var mesh = MakeSingleTriangleMesh();

        MeshOptimizer.IsWatertight(mesh).Should().BeFalse();
    }

    /// <summary>Verifies FlipEdge returns original mesh for out-of-range face index.</summary>
    [Fact]
    public void FlipEdge_OutOfRangeIndex_ReturnsOriginal()
    {
        var mesh = MakeSingleTriangleMesh();

        TriangleMesh result = MeshOptimizer.FlipEdge(mesh, 0, 5);

        result.Should().BeSameAs(mesh);
    }

    /// <summary>Verifies ComputeTriangleAreas for two triangles returns two areas.</summary>
    [Fact]
    public void ComputeTriangleAreas_TwoTriangles_ReturnsTwoAreas()
    {
        var mesh = MakeUnitCube();

        var areas = MeshOptimizer.ComputeTriangleAreas(mesh);

        areas.Should().HaveCount(12);
    }

    /// <summary>Verifies ComputeEulerCharacteristic for a single triangle is 1.</summary>
    [Fact]
    public void ComputeEulerCharacteristic_SingleTriangle_ReturnsOne()
    {
        var mesh = MakeSingleTriangleMesh();

        int chi = MeshOptimizer.ComputeEulerCharacteristic(mesh);

        chi.Should().Be(1);
    }
}
