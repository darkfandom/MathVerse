namespace MathVerse.Geometry.Tests.Meshes;

/// <summary>Tests for the <see cref="MeshBuilder"/> class.</summary>
public class MeshBuilderTests
{
    private const double Tolerance = 1e-10;

    private static Vertex MakeVertex(double x = 0, double y = 0, double z = 0) =>
        new(new Point3D(x, y, z), Vector3D.UnitY, (0, 0));

    /// <summary>Verifies AddVertex returns sequential indices starting from zero.</summary>
    [Fact]
    public void AddVertex_ReturnsSequentialIndices()
    {
        var builder = new MeshBuilder();

        int i0 = builder.AddVertex(MakeVertex(0, 0, 0));
        int i1 = builder.AddVertex(MakeVertex(1, 0, 0));
        int i2 = builder.AddVertex(MakeVertex(0, 1, 0));

        i0.Should().Be(0);
        i1.Should().Be(1);
        i2.Should().Be(2);
    }

    /// <summary>Verifies AddTriangle returns sequential face indices.</summary>
    [Fact]
    public void AddTriangle_ReturnsSequentialFaceIndices()
    {
        var builder = new MeshBuilder();
        builder.AddVertex(MakeVertex());
        builder.AddVertex(MakeVertex(1));
        builder.AddVertex(MakeVertex(2));

        int f0 = builder.AddTriangle(0, 1, 2);

        f0.Should().Be(0);
        builder.TriangleCount.Should().Be(1);
    }

    /// <summary>Verifies AddQuad stores a quad face.</summary>
    [Fact]
    public void AddQuad_StoresQuadFace()
    {
        var builder = new MeshBuilder();
        builder.AddVertex(MakeVertex());
        builder.AddVertex(MakeVertex(1));
        builder.AddVertex(MakeVertex(1, 1));
        builder.AddVertex(MakeVertex(0, 1));

        int f0 = builder.AddQuad(0, 1, 2, 3);

        f0.Should().Be(0);
        builder.QuadCount.Should().Be(1);
    }

    /// <summary>Verifies VertexCount reflects added vertices.</summary>
    [Fact]
    public void VertexCount_ReflectsAddedVertices()
    {
        var builder = new MeshBuilder();
        builder.AddVertex(MakeVertex());
        builder.AddVertex(MakeVertex(1));
        builder.AddVertex(MakeVertex(2));

        builder.VertexCount.Should().Be(3);
    }

    /// <summary>Verifies TriangleCount reflects added triangles.</summary>
    [Fact]
    public void TriangleCount_ReflectsAddedTriangles()
    {
        var builder = new MeshBuilder();
        builder.AddVertex(MakeVertex());
        builder.AddVertex(MakeVertex(1));
        builder.AddVertex(MakeVertex(2));
        builder.AddTriangle(0, 1, 2);
        builder.AddVertex(MakeVertex(3));
        builder.AddTriangle(0, 2, 3);

        builder.TriangleCount.Should().Be(2);
    }

    /// <summary>Verifies Build returns a TriangleMesh with correct data.</summary>
    [Fact]
    public void Build_ReturnsTriangleMesh()
    {
        var builder = new MeshBuilder();
        int v0 = builder.AddVertex(MakeVertex(0, 0, 0));
        int v1 = builder.AddVertex(MakeVertex(1, 0, 0));
        int v2 = builder.AddVertex(MakeVertex(0, 1, 0));
        builder.AddTriangle(v0, v1, v2);

        TriangleMesh mesh = builder.Build();

        mesh.VertexCount.Should().Be(3);
        mesh.TriangleCount.Should().Be(1);
    }

    /// <summary>Verifies BuildQuadMesh returns a QuadMesh.</summary>
    [Fact]
    public void BuildQuadMesh_ReturnsQuadMesh()
    {
        var builder = new MeshBuilder();
        int v0 = builder.AddVertex(MakeVertex(0, 0, 0));
        int v1 = builder.AddVertex(MakeVertex(1, 0, 0));
        int v2 = builder.AddVertex(MakeVertex(1, 1, 0));
        int v3 = builder.AddVertex(MakeVertex(0, 1, 0));
        builder.AddQuad(v0, v1, v2, v3);

        QuadMesh mesh = builder.BuildQuadMesh();

        mesh.VertexCount.Should().Be(4);
        mesh.QuadCount.Should().Be(1);
    }

    /// <summary>Verifies Clear resets all counts to zero.</summary>
    [Fact]
    public void Clear_ResetsCountsToZero()
    {
        var builder = new MeshBuilder();
        builder.AddVertex(MakeVertex());
        builder.AddVertex(MakeVertex(1));
        builder.AddTriangle(0, 1, 0);
        builder.Clear();

        builder.VertexCount.Should().Be(0);
        builder.TriangleCount.Should().Be(0);
        builder.QuadCount.Should().Be(0);
    }

    /// <summary>Verifies SetVertexNormal updates the vertex normal.</summary>
    [Fact]
    public void SetVertexNormal_UpdatesNormal()
    {
        var builder = new MeshBuilder();
        builder.AddVertex(MakeVertex(0, 0, 0));

        builder.SetVertexNormal(0, new Vector3D(0, 0, 1));

        builder.GetVertex(0).Normal.Z.Should().BeApproximately(1.0, Tolerance);
        builder.GetVertex(0).Normal.X.Should().BeApproximately(0.0, Tolerance);
        builder.GetVertex(0).Normal.Y.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies SetVertexUV updates the UV coordinates.</summary>
    [Fact]
    public void SetVertexUV_UpdatesUVCoordinates()
    {
        var builder = new MeshBuilder();
        builder.AddVertex(MakeVertex(0, 0, 0));

        builder.SetVertexUV(0, 0.5, 0.75);

        Vertex v = builder.GetVertex(0);
        v.UV.U.Should().BeApproximately(0.5, Tolerance);
        v.UV.V.Should().BeApproximately(0.75, Tolerance);
    }

    /// <summary>Verifies complex mesh with multiple triangles builds correctly.</summary>
    [Fact]
    public void ComplexMesh_MultipleTriangles_BuildsCorrectly()
    {
        var builder = new MeshBuilder();
        int v0 = builder.AddVertex(MakeVertex(0, 0, 0));
        int v1 = builder.AddVertex(MakeVertex(1, 0, 0));
        int v2 = builder.AddVertex(MakeVertex(1, 1, 0));
        int v3 = builder.AddVertex(MakeVertex(0, 1, 0));
        builder.AddTriangle(v0, v1, v2);
        builder.AddTriangle(v0, v2, v3);

        TriangleMesh mesh = builder.Build();

        mesh.VertexCount.Should().Be(4);
        mesh.TriangleCount.Should().Be(2);
    }

    /// <summary>Verifies AddVertex with Point3D overload works correctly.</summary>
    [Fact]
    public void AddVertex_PositionOverload_WorksCorrectly()
    {
        var builder = new MeshBuilder();

        int idx = builder.AddVertex(new Point3D(3, 4, 5));

        idx.Should().Be(0);
        builder.VertexCount.Should().Be(1);
        builder.GetVertex(0).Position.X.Should().BeApproximately(3.0, Tolerance);
        builder.GetVertex(0).Position.Y.Should().BeApproximately(4.0, Tolerance);
        builder.GetVertex(0).Position.Z.Should().BeApproximately(5.0, Tolerance);
    }
}
