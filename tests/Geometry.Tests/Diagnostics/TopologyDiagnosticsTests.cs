using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Geometry.Tests.Diagnostics;

/// <summary>Tests for the <see cref="TopologyDiagnostics"/> class.</summary>
public class TopologyDiagnosticsTests
{
    /// <summary>Verifies that FindBoundaryEdges returns boundary edges for an open mesh.</summary>
    [Fact]
    public void FindBoundaryEdges_OpenMesh_ReturnsBoundaryEdges()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitZ, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitZ, (1, 0)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitZ, (0, 1)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        var mesh = new TriangleMesh(vertices, faces);

        var edges = TopologyDiagnostics.FindBoundaryEdges(mesh);

        edges.Length.Should().Be(3);
    }

    /// <summary>Verifies that IsWatertight returns false for an open mesh.</summary>
    [Fact]
    public void IsWatertight_OpenMesh_ReturnsFalse()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitZ, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitZ, (1, 0)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitZ, (0, 1)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        var mesh = new TriangleMesh(vertices, faces);

        TopologyDiagnostics.IsWatertight(mesh).Should().BeFalse();
    }

    /// <summary>Verifies that IsWatertight returns true for a closed tetrahedron mesh.</summary>
    [Fact]
    public void IsWatertight_Tetrahedron_ReturnsTrue()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitZ, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitZ, (1, 0)),
            new Vertex(new Point3D(0.5, 1, 0), Vector3D.UnitZ, (0.5, 1)),
            new Vertex(new Point3D(0.5, 0.5, 1), Vector3D.UnitZ, (0.5, 0.5)));
        var faces = ImmutableArray.Create(
            new TriangleFace(0, 1, 2),
            new TriangleFace(0, 1, 3),
            new TriangleFace(1, 2, 3),
            new TriangleFace(0, 2, 3));
        var mesh = new TriangleMesh(vertices, faces);

        TopologyDiagnostics.IsWatertight(mesh).Should().BeTrue();
    }

    /// <summary>Verifies that ComputeGenus returns 0 for a sphere-like mesh (closed, genus 0).</summary>
    [Fact]
    public void ComputeGenus_Tetrahedron_ReturnsZero()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitZ, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitZ, (1, 0)),
            new Vertex(new Point3D(0.5, 1, 0), Vector3D.UnitZ, (0.5, 1)),
            new Vertex(new Point3D(0.5, 0.5, 1), Vector3D.UnitZ, (0.5, 0.5)));
        var faces = ImmutableArray.Create(
            new TriangleFace(0, 1, 2),
            new TriangleFace(0, 1, 3),
            new TriangleFace(1, 2, 3),
            new TriangleFace(0, 2, 3));
        var mesh = new TriangleMesh(vertices, faces);

        int genus = TopologyDiagnostics.ComputeGenus(mesh);

        genus.Should().Be(0);
    }

    /// <summary>Verifies that ValidateEulerCharacteristic returns success for a valid mesh.</summary>
    [Fact]
    public void ValidateEulerCharacteristic_ValidMesh_ReturnsSuccess()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitZ, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitZ, (1, 0)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitZ, (0, 1)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        var mesh = new TriangleMesh(vertices, faces);

        var result = TopologyDiagnostics.ValidateEulerCharacteristic(mesh);

        result.Success.Should().BeTrue();
    }

    /// <summary>Verifies that FindBoundaryEdges for a closed mesh returns empty.</summary>
    [Fact]
    public void FindBoundaryEdges_ClosedMesh_ReturnsEmpty()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitZ, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitZ, (1, 0)),
            new Vertex(new Point3D(0.5, 1, 0), Vector3D.UnitZ, (0.5, 1)),
            new Vertex(new Point3D(0.5, 0.5, 1), Vector3D.UnitZ, (0.5, 0.5)));
        var faces = ImmutableArray.Create(
            new TriangleFace(0, 1, 2),
            new TriangleFace(0, 1, 3),
            new TriangleFace(1, 2, 3),
            new TriangleFace(0, 2, 3));
        var mesh = new TriangleMesh(vertices, faces);

        var edges = TopologyDiagnostics.FindBoundaryEdges(mesh);

        edges.Length.Should().Be(0);
    }

    /// <summary>Verifies that ComputeGenus for an open triangle mesh returns correct genus.</summary>
    [Fact]
    public void ComputeGenus_OpenTriangle_ReturnsExpected()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitZ, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitZ, (1, 0)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitZ, (0, 1)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        var mesh = new TriangleMesh(vertices, faces);

        int genus = TopologyDiagnostics.ComputeGenus(mesh);

        genus.Should().Be(1);
    }

    /// <summary>Verifies that FindBoundaryEdges returns unique edges only.</summary>
    [Fact]
    public void FindBoundaryEdges_ReturnsUniqueEdges()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitZ, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitZ, (1, 0)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitZ, (0, 1)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        var mesh = new TriangleMesh(vertices, faces);

        var edges = TopologyDiagnostics.FindBoundaryEdges(mesh);

        var distinct = edges.Distinct().ToList();
        distinct.Count.Should().Be(edges.Length);
    }

    /// <summary>Verifies that IsWatertight returns true for a cube mesh (12 triangles, 8 vertices).</summary>
    [Fact]
    public void IsWatertight_Cube_ReturnsTrue()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitZ, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitZ, (1, 0)),
            new Vertex(new Point3D(1, 1, 0), Vector3D.UnitZ, (1, 1)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitZ, (0, 1)),
            new Vertex(new Point3D(0, 0, 1), Vector3D.UnitZ, (0, 0)),
            new Vertex(new Point3D(1, 0, 1), Vector3D.UnitZ, (1, 0)),
            new Vertex(new Point3D(1, 1, 1), Vector3D.UnitZ, (1, 1)),
            new Vertex(new Point3D(0, 1, 1), Vector3D.UnitZ, (0, 1)));
        var faces = ImmutableArray.Create(
            new TriangleFace(0, 2, 1), new TriangleFace(0, 3, 2),
            new TriangleFace(4, 5, 6), new TriangleFace(4, 6, 7),
            new TriangleFace(0, 1, 5), new TriangleFace(0, 5, 4),
            new TriangleFace(2, 3, 7), new TriangleFace(2, 7, 6),
            new TriangleFace(0, 4, 7), new TriangleFace(0, 7, 3),
            new TriangleFace(1, 2, 6), new TriangleFace(1, 6, 5));
        var mesh = new TriangleMesh(vertices, faces);

        TopologyDiagnostics.IsWatertight(mesh).Should().BeTrue();
    }

    /// <summary>Verifies that ValidateEulerCharacteristic for a tetrahedron mesh returns success.</summary>
    [Fact]
    public void ValidateEulerCharacteristic_Tetrahedron_ReturnsSuccess()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitZ, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitZ, (1, 0)),
            new Vertex(new Point3D(0.5, 1, 0), Vector3D.UnitZ, (0.5, 1)),
            new Vertex(new Point3D(0.5, 0.5, 1), Vector3D.UnitZ, (0.5, 0.5)));
        var faces = ImmutableArray.Create(
            new TriangleFace(0, 1, 2),
            new TriangleFace(0, 1, 3),
            new TriangleFace(1, 2, 3),
            new TriangleFace(0, 2, 3));
        var mesh = new TriangleMesh(vertices, faces);

        var result = TopologyDiagnostics.ValidateEulerCharacteristic(mesh);

        result.Success.Should().BeTrue();
    }
}
