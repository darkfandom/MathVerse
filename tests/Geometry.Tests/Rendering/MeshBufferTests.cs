using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Geometry.Tests.Rendering;

/// <summary>Tests for the <see cref="MeshBuffer"/> class.</summary>
public class MeshBufferTests
{
    private static TriangleMesh CreateSingleTriangleMesh()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitX, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitY, (1, 0)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitZ, (0, 1)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        return new TriangleMesh(vertices, faces);
    }

    /// <summary>Verifies that FromMesh creates a buffer from a single triangle mesh.</summary>
    [Fact]
    public void FromMesh_SingleTriangle_CreatesBuffer()
    {
        var mesh = CreateSingleTriangleMesh();

        var buffer = MeshBuffer.FromMesh(mesh);

        buffer.Should().NotBeNull();
    }

    /// <summary>Verifies that Positions count equals vertex count times 3.</summary>
    [Fact]
    public void Positions_Count_EqualsVertexCountTimes3()
    {
        var mesh = CreateSingleTriangleMesh();

        var buffer = MeshBuffer.FromMesh(mesh);

        buffer.Positions.Length.Should().Be(9);
    }

    /// <summary>Verifies that Normals count equals vertex count times 3.</summary>
    [Fact]
    public void Normals_Count_EqualsVertexCountTimes3()
    {
        var mesh = CreateSingleTriangleMesh();

        var buffer = MeshBuffer.FromMesh(mesh);

        buffer.Normals.Length.Should().Be(9);
    }

    /// <summary>Verifies that UVs count equals vertex count times 2.</summary>
    [Fact]
    public void UVs_Count_EqualsVertexCountTimes2()
    {
        var mesh = CreateSingleTriangleMesh();

        var buffer = MeshBuffer.FromMesh(mesh);

        buffer.UVs.Length.Should().Be(6);
    }

    /// <summary>Verifies that Indices count equals face count times 3.</summary>
    [Fact]
    public void Indices_Count_EqualsFaceCountTimes3()
    {
        var mesh = CreateSingleTriangleMesh();

        var buffer = MeshBuffer.FromMesh(mesh);

        buffer.Indices.Length.Should().Be(3);
    }

    /// <summary>Verifies that Positions contain correct vertex data.</summary>
    [Fact]
    public void Positions_ContainsCorrectData()
    {
        var mesh = CreateSingleTriangleMesh();

        var buffer = MeshBuffer.FromMesh(mesh);

        buffer.Positions[0].Should().BeApproximately(0.0f, 1e-5f);
        buffer.Positions[1].Should().BeApproximately(0.0f, 1e-5f);
        buffer.Positions[2].Should().BeApproximately(0.0f, 1e-5f);
        buffer.Positions[3].Should().BeApproximately(1.0f, 1e-5f);
    }

    /// <summary>Verifies that Indices contain the correct face indices.</summary>
    [Fact]
    public void Indices_ContainsCorrectFaceData()
    {
        var mesh = CreateSingleTriangleMesh();

        var buffer = MeshBuffer.FromMesh(mesh);

        buffer.Indices[0].Should().Be(0);
        buffer.Indices[1].Should().Be(1);
        buffer.Indices[2].Should().Be(2);
    }

    /// <summary>Verifies that UVs contain correct texture coordinates.</summary>
    [Fact]
    public void UVs_ContainsCorrectData()
    {
        var mesh = CreateSingleTriangleMesh();

        var buffer = MeshBuffer.FromMesh(mesh);

        buffer.UVs[0].Should().BeApproximately(0.0f, 1e-5f);
        buffer.UVs[1].Should().BeApproximately(0.0f, 1e-5f);
        buffer.UVs[2].Should().BeApproximately(1.0f, 1e-5f);
        buffer.UVs[3].Should().BeApproximately(0.0f, 1e-5f);
    }

    /// <summary>Verifies that Normals contain correct normal data.</summary>
    [Fact]
    public void Normals_ContainsCorrectData()
    {
        var mesh = CreateSingleTriangleMesh();

        var buffer = MeshBuffer.FromMesh(mesh);

        buffer.Normals[0].Should().BeApproximately(1.0f, 1e-5f);
        buffer.Normals[1].Should().BeApproximately(0.0f, 1e-5f);
        buffer.Normals[2].Should().BeApproximately(0.0f, 1e-5f);
    }

    /// <summary>Verifies that FromMesh with empty mesh produces empty buffers.</summary>
    [Fact]
    public void FromMesh_EmptyMesh_ProducesEmptyBuffers()
    {
        var mesh = TriangleMesh.Empty;

        var buffer = MeshBuffer.FromMesh(mesh);

        buffer.Positions.Length.Should().Be(0);
        buffer.Normals.Length.Should().Be(0);
        buffer.UVs.Length.Should().Be(0);
        buffer.Indices.Length.Should().Be(0);
    }
}
