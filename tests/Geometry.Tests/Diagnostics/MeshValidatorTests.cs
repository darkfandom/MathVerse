using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Geometry.Tests.Diagnostics;

/// <summary>Tests for the <see cref="MeshValidator"/> class.</summary>
public class MeshValidatorTests
{
    private static TriangleMesh CreateValidMesh()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitZ, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitZ, (1, 0)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitZ, (0, 1)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        return new TriangleMesh(vertices, faces);
    }

    private static TriangleMesh CreateDegenerateFaceMesh()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitZ, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitZ, (1, 0)),
            new Vertex(new Point3D(2, 0, 0), Vector3D.UnitZ, (2, 0)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        return new TriangleMesh(vertices, faces);
    }

    /// <summary>Verifies that ValidateTopology returns success for a valid mesh.</summary>
    [Fact]
    public void ValidateTopology_ValidMesh_ReturnsSuccess()
    {
        var mesh = CreateValidMesh();

        var result = MeshValidator.ValidateTopology(mesh);

        result.Success.Should().BeTrue();
    }

    /// <summary>Verifies that ValidateTopology returns failure for a degenerate face.</summary>
    [Fact]
    public void ValidateTopology_DegenerateFace_ReturnsFailure()
    {
        var mesh = CreateDegenerateFaceMesh();

        var result = MeshValidator.ValidateTopology(mesh);

        result.Success.Should().BeFalse();
    }

    /// <summary>Verifies that FindDegenerateTriangles returns empty for a valid mesh.</summary>
    [Fact]
    public void FindDegenerateTriangles_ValidMesh_ReturnsEmpty()
    {
        var mesh = CreateValidMesh();

        var result = MeshValidator.FindDegenerateTriangles(mesh);

        result.Should().BeEmpty();
    }

    /// <summary>Verifies that FindDegenerateTriangles finds the degenerate face.</summary>
    [Fact]
    public void FindDegenerateTriangles_DegenerateMesh_FindsIndex()
    {
        var mesh = CreateDegenerateFaceMesh();

        var result = MeshValidator.FindDegenerateTriangles(mesh);

        result.Should().HaveCount(1);
        result[0].Should().Be(0);
    }

    /// <summary>Verifies that FindNonManifoldEdges returns empty for a valid single-triangle mesh.</summary>
    [Fact]
    public void FindNonManifoldEdges_ValidMesh_ReturnsEmpty()
    {
        var mesh = CreateValidMesh();

        var result = MeshValidator.FindNonManifoldEdges(mesh);

        result.Should().BeEmpty();
    }

    /// <summary>Verifies that ValidateTopology returns failure for empty mesh.</summary>
    [Fact]
    public void ValidateTopology_EmptyMesh_ReturnsFailure()
    {
        var result = MeshValidator.ValidateTopology(TriangleMesh.Empty);

        result.Success.Should().BeFalse();
    }

    /// <summary>Verifies that FindDegenerateTriangles returns empty for empty mesh.</summary>
    [Fact]
    public void FindDegenerateTriangles_EmptyMesh_ReturnsEmpty()
    {
        var result = MeshValidator.FindDegenerateTriangles(TriangleMesh.Empty);

        result.Should().BeEmpty();
    }

    /// <summary>Verifies that FindNonManifoldEdges returns empty for empty mesh.</summary>
    [Fact]
    public void FindNonManifoldEdges_EmptyMesh_ReturnsEmpty()
    {
        var result = MeshValidator.FindNonManifoldEdges(TriangleMesh.Empty);

        result.Should().BeEmpty();
    }

    /// <summary>Verifies that ValidateTopology returns success for a mesh with two adjacent triangles.</summary>
    [Fact]
    public void ValidateTopology_TwoTriangles_ReturnsSuccess()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitZ, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitZ, (1, 0)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitZ, (0, 1)),
            new Vertex(new Point3D(1, 1, 0), Vector3D.UnitZ, (1, 1)));
        var faces = ImmutableArray.Create(
            new TriangleFace(0, 1, 2),
            new TriangleFace(1, 3, 2));
        var mesh = new TriangleMesh(vertices, faces);

        var result = MeshValidator.ValidateTopology(mesh);

        result.Success.Should().BeTrue();
    }

    /// <summary>Verifies that FindDegenerateTriangles with custom tolerance detects near-degenerate.</summary>
    [Fact]
    public void FindDegenerateTriangles_CustomTolerance_DetectsNearDegenerate()
    {
        var mesh = CreateDegenerateFaceMesh();

        var result = MeshValidator.FindDegenerateTriangles(mesh, 1.0);

        result.Should().HaveCount(1);
    }

    /// <summary>Verifies that ValidateTopology failure message is non-empty.</summary>
    [Fact]
    public void ValidateTopology_DegenerateFace_ErrorMessage()
    {
        var mesh = CreateDegenerateFaceMesh();

        var result = MeshValidator.ValidateTopology(mesh);

        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }
}
