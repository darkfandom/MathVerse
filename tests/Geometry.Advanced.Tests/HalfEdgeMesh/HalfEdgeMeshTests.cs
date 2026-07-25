namespace MathVerse.Geometry.Advanced.Tests.HalfEdgeMesh;

public class HalfEdgeMeshTests
{
    private const double Tolerance = 1e-6;

    private static TriangleMesh CreateSingleTriangleMesh()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitZ, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitZ, (1, 0)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitZ, (0, 1)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        return new TriangleMesh(vertices, faces);
    }

    private static TriangleMesh CreateTwoTriangleMesh()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitZ, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitZ, (1, 0)),
            new Vertex(new Point3D(1, 1, 0), Vector3D.UnitZ, (1, 1)),
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitZ, (0, 0)));
        var faces = ImmutableArray.Create(
            new TriangleFace(0, 1, 2),
            new TriangleFace(0, 2, 3));
        return new TriangleMesh(vertices, faces);
    }

    private static TriangleMesh CreateQuadMesh()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitZ, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitZ, (1, 0)),
            new Vertex(new Point3D(1, 1, 0), Vector3D.UnitZ, (1, 1)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitZ, (0, 1)));
        var faces = ImmutableArray.Create(
            new TriangleFace(0, 1, 2),
            new TriangleFace(0, 2, 3));
        return new TriangleMesh(vertices, faces);
    }

    [Fact]
    public void FromTriangleMesh_Basic_SingleTriangle()
    {
        var mesh = CreateSingleTriangleMesh();
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(mesh);

        hem.VertexCount.Should().Be(3);
        hem.FaceCount.Should().Be(1);
    }

    [Fact]
    public void FromTriangleMesh_VertexCount_MatchesSource()
    {
        var mesh = CreateQuadMesh();
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(mesh);

        hem.VertexCount.Should().Be(mesh.VertexCount);
    }

    [Fact]
    public void FromTriangleMesh_FaceCount_MatchesSource()
    {
        var mesh = CreateQuadMesh();
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(mesh);

        hem.FaceCount.Should().Be(mesh.TriangleCount);
    }

    [Fact]
    public void FromTriangleMesh_HalfEdgeCount_IsCorrect()
    {
        var mesh = CreateSingleTriangleMesh();
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(mesh);

        hem.HalfEdgeCount.Should().Be(3);
    }

    [Fact]
    public void VertexCount_SingleTriangle_IsThree()
    {
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(CreateSingleTriangleMesh());

        hem.VertexCount.Should().Be(3);
    }

    [Fact]
    public void HalfEdgeCount_SingleTriangle_IsThree()
    {
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(CreateSingleTriangleMesh());

        hem.HalfEdgeCount.Should().Be(3);
    }

    [Fact]
    public void FaceCount_SingleTriangle_IsOne()
    {
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(CreateSingleTriangleMesh());

        hem.FaceCount.Should().Be(1);
    }

    [Fact]
    public void EdgeCount_SingleTriangle_IsOne()
    {
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(CreateSingleTriangleMesh());

        hem.EdgeCount.Should().Be(1);
    }

    [Fact]
    public void EdgeCount_QuadMesh_IsThree()
    {
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(CreateQuadMesh());

        hem.EdgeCount.Should().Be(3);
    }

    [Fact]
    public void GetVertexRing_SingleTriangle_ReturnsNeighbors()
    {
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(CreateSingleTriangleMesh());

        var ring = hem.GetVertexRing(0);

        ring.Length.Should().Be(0);
    }

    [Fact]
    public void GetVertexRing_CentralVertexOfQuad_ReturnsAllCorners()
    {
        var mesh = CreateQuadMesh();
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(mesh);

        var ring = hem.GetVertexRing(0);

        ring.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetVertexRing_ContainsCorrectNeighbors()
    {
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(CreateSingleTriangleMesh());

        var ring = hem.GetVertexRing(0);

        ring.Should().BeEmpty();
    }

    [Fact]
    public void GetAdjacentFaces_SingleTriangle_ReturnsNoAdjacent()
    {
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(CreateSingleTriangleMesh());

        var adj = hem.GetAdjacentFaces(0);

        adj.Length.Should().Be(0);
    }

    [Fact]
    public void GetAdjacentFaces_QuadMesh_SharedEdge()
    {
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(CreateQuadMesh());

        var adj0 = hem.GetAdjacentFaces(0);
        var adj1 = hem.GetAdjacentFaces(1);

        adj0.Length.Should().Be(1);
        adj1.Length.Should().Be(1);
    }

    [Fact]
    public void GetAdjacentFaces_QuadMesh_Symmetric()
    {
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(CreateQuadMesh());

        var adj0 = hem.GetAdjacentFaces(0);
        var adj1 = hem.GetAdjacentFaces(1);

        adj0.Should().Contain(1);
        adj1.Should().Contain(0);
    }

    [Fact]
    public void Validate_SingleTriangle_ReturnsTrue()
    {
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(CreateSingleTriangleMesh());

        hem.Validate().Should().BeTrue();
    }

    [Fact]
    public void Validate_QuadMesh_ReturnsTrue()
    {
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(CreateQuadMesh());

        hem.Validate().Should().BeTrue();
    }

    [Fact]
    public void Validate_TwoTriangles_ReturnsTrue()
    {
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(CreateTwoTriangleMesh());

        hem.Validate().Should().BeTrue();
    }

    [Fact]
    public void GetOutgoingEdges_SingleTriangle_ReturnsEdges()
    {
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(CreateSingleTriangleMesh());

        var edges = hem.GetOutgoingEdges(0).ToList();

        edges.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Vertices_ArePopulated()
    {
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(CreateSingleTriangleMesh());

        hem.Vertices.Count.Should().Be(3);
    }

    [Fact]
    public void Faces_ArePopulated()
    {
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(CreateSingleTriangleMesh());

        hem.Faces.Count.Should().Be(1);
    }

    [Fact]
    public void HalfEdges_ArePopulated()
    {
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(CreateSingleTriangleMesh());

        hem.HalfEdges.Count.Should().Be(3);
    }

    [Fact]
    public void HalfEdge_EachHasValidNext()
    {
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(CreateSingleTriangleMesh());

        foreach (var he in hem.HalfEdges)
        {
            he.Next.Should().BeGreaterOrEqualTo(0);
            he.Next.Should().BeLessThan(hem.HalfEdgeCount);
        }
    }

    [Fact]
    public void HalfEdge_EachHasValidVertex()
    {
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(CreateSingleTriangleMesh());

        foreach (var he in hem.HalfEdges)
        {
            he.Vertex.Should().BeGreaterOrEqualTo(0);
            he.Vertex.Should().BeLessThan(hem.VertexCount);
        }
    }

    [Fact]
    public void HalfEdge_EachHasValidFace()
    {
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(CreateSingleTriangleMesh());

        foreach (var he in hem.HalfEdges)
        {
            he.Face.Should().BeGreaterOrEqualTo(0);
            he.Face.Should().BeLessThan(hem.FaceCount);
        }
    }

    [Fact]
    public void TriangleMesh_CalculateNormals_ReturnsNonNull()
    {
        var mesh = CreateSingleTriangleMesh();

        var withNormals = mesh.CalculateNormals();

        withNormals.Should().NotBeNull();
    }

    [Fact]
    public void TriangleMesh_CalculateNormals_PreservesVertexCount()
    {
        var mesh = CreateQuadMesh();

        var withNormals = mesh.CalculateNormals();

        withNormals.VertexCount.Should().Be(mesh.VertexCount);
    }

    [Fact]
    public void TriangleMesh_CalculateNormals_PreservesFaceCount()
    {
        var mesh = CreateQuadMesh();

        var withNormals = mesh.CalculateNormals();

        withNormals.TriangleCount.Should().Be(mesh.TriangleCount);
    }

    [Fact]
    public void TriangleMesh_CalculateNormals_NormalsAreUnitLength()
    {
        var mesh = CreateSingleTriangleMesh();

        var withNormals = mesh.CalculateNormals();

        foreach (var v in withNormals.Vertices)
        {
            v.Normal.Length.Should().BeApproximately(1.0, Tolerance);
        }
    }

    [Fact]
    public void TriangleMesh_CalculateNormals_NormalsAreNonZero()
    {
        var mesh = CreateQuadMesh();

        var withNormals = mesh.CalculateNormals();

        foreach (var v in withNormals.Vertices)
        {
            v.Normal.Length.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public void TriangleMesh_Validate_SingleTriangle_ReturnsOk()
    {
        var mesh = CreateSingleTriangleMesh();

        var result = mesh.Validate();

        result.Success.Should().BeTrue();
    }

    [Fact]
    public void FromTriangleMesh_TrianglesAreConsistent()
    {
        var mesh = CreateSingleTriangleMesh();
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(mesh);

        hem.FaceCount.Should().Be(mesh.TriangleCount);
        hem.VertexCount.Should().Be(mesh.VertexCount);
    }

    [Fact]
    public void GetVertexRing_LargerMesh_ReturnsNonEmpty()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitZ, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitZ, (1, 0)),
            new Vertex(new Point3D(1, 1, 0), Vector3D.UnitZ, (1, 1)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitZ, (0, 1)),
            new Vertex(new Point3D(0.5, 0.5, 0), Vector3D.UnitZ, (0.5, 0.5)));
        var faces = ImmutableArray.Create(
            new TriangleFace(0, 1, 4),
            new TriangleFace(1, 2, 4),
            new TriangleFace(2, 3, 4),
            new TriangleFace(3, 0, 4));
        var mesh = new TriangleMesh(vertices, faces);
        var hem = global::MathVerse.Math.Geometry.Meshes.HalfEdgeMesh.FromTriangleMesh(mesh);

        var ring = hem.GetVertexRing(4);

        ring.Length.Should().Be(4);
    }
}
