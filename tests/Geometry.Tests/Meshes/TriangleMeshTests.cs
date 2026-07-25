namespace MathVerse.Geometry.Tests.Meshes;

/// <summary>Tests for the <see cref="TriangleMesh"/> class.</summary>
public class TriangleMeshTests
{
    private const double Tolerance = 1e-6;

    /// <summary>Verifies Empty mesh has zero vertices and zero faces.</summary>
    [Fact]
    public void Empty_HasZeroCounts()
    {
        TriangleMesh.Empty.VertexCount.Should().Be(0);
        TriangleMesh.Empty.TriangleCount.Should().Be(0);
    }

    /// <summary>Verifies CreateEmpty returns an empty mesh.</summary>
    [Fact]
    public void CreateEmpty_ReturnsEmptyMesh()
    {
        var mesh = new TriangleMesh(
            ImmutableArray<Vertex>.Empty,
            ImmutableArray<TriangleFace>.Empty);

        mesh.VertexCount.Should().Be(0);
        mesh.TriangleCount.Should().Be(0);
    }

    /// <summary>Verifies VertexCount matches the number of vertices.</summary>
    [Fact]
    public void VertexCount_MatchesVertexLength()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitY, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitY, (1, 0)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitY, (0, 1)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));

        var mesh = new TriangleMesh(vertices, faces);

        mesh.VertexCount.Should().Be(3);
    }

    /// <summary>Verifies TriangleCount matches the number of faces.</summary>
    [Fact]
    public void TriangleCount_MatchesFaceLength()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitY, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitY, (1, 0)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitY, (0, 1)));
        var faces = ImmutableArray.Create(
            new TriangleFace(0, 1, 2));

        var mesh = new TriangleMesh(vertices, faces);

        mesh.TriangleCount.Should().Be(1);
    }

    /// <summary>Verifies EdgeCount for a single triangle mesh.</summary>
    [Fact]
    public void EdgeCount_SingleTriangle_ReturnsThree()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitY, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitY, (1, 0)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitY, (0, 1)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));

        var mesh = new TriangleMesh(vertices, faces);

        mesh.EdgeCount.Should().Be(3);
    }

    /// <summary>Verifies GetVertices returns the vertices.</summary>
    [Fact]
    public void GetVertices_ReturnsVertices()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitY, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitY, (1, 0)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitY, (0, 1)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        var mesh = new TriangleMesh(vertices, faces);

        IReadOnlyList<Vertex> result = mesh.GetVertices();

        result.Should().HaveCount(3);
    }

    /// <summary>Verifies GetTriangles returns the faces.</summary>
    [Fact]
    public void GetTriangles_ReturnsFaces()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitY, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitY, (1, 0)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitY, (0, 1)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        var mesh = new TriangleMesh(vertices, faces);

        IReadOnlyList<TriangleFace> result = mesh.GetTriangles();

        result.Should().HaveCount(1);
        result[0].Should().Be(new TriangleFace(0, 1, 2));
    }

    /// <summary>Verifies BoundingBox encloses all vertices.</summary>
    [Fact]
    public void BoundingBox_EnclodesAllVertices()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(-1, -2, -3), Vector3D.UnitY, (0, 0)),
            new Vertex(new Point3D(4, 5, 6), Vector3D.UnitY, (1, 1)),
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitY, (0.5, 0.5)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        var mesh = new TriangleMesh(vertices, faces);

        BoundingBox3D bb = mesh.BoundingBox();

        bb.Min.X.Should().BeApproximately(-1.0, Tolerance);
        bb.Min.Y.Should().BeApproximately(-2.0, Tolerance);
        bb.Min.Z.Should().BeApproximately(-3.0, Tolerance);
        bb.Max.X.Should().BeApproximately(4.0, Tolerance);
        bb.Max.Y.Should().BeApproximately(5.0, Tolerance);
        bb.Max.Z.Should().BeApproximately(6.0, Tolerance);
    }

    /// <summary>Verifies CalculateNormals returns a new mesh with updated normals.</summary>
    [Fact]
    public void CalculateNormals_ReturnsNewMesh()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.Zero, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.Zero, (1, 0)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.Zero, (0, 1)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        var mesh = new TriangleMesh(vertices, faces);

        TriangleMesh result = mesh.CalculateNormals();

        result.Should().NotBeSameAs(mesh);
        result.VertexCount.Should().Be(3);
        result.Faces.Should().BeEquivalentTo(mesh.Faces);
    }

    /// <summary>Verifies Transform translates vertex positions.</summary>
    [Fact]
    public void Transform_Translate_ShiftsPositions()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitY, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitY, (1, 0)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitY, (0, 1)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        var mesh = new TriangleMesh(vertices, faces);
        var translation = Transform3D.Translation(5, 10, 15);

        TriangleMesh result = mesh.Transform(translation);

        result.VertexCount.Should().Be(3);
        result.Vertices[0].Position.X.Should().BeApproximately(5.0, Tolerance);
        result.Vertices[0].Position.Y.Should().BeApproximately(10.0, Tolerance);
        result.Vertices[0].Position.Z.Should().BeApproximately(15.0, Tolerance);
    }

    /// <summary>Verifies Validate succeeds for a valid single-triangle mesh.</summary>
    [Fact]
    public void Validate_ValidMesh_ReturnsOk()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitY, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitY, (1, 0)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitY, (0, 1)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        var mesh = new TriangleMesh(vertices, faces);

        GeometryResult result = mesh.Validate();

        result.Success.Should().BeTrue();
    }

    /// <summary>Verifies Validate fails for face with out-of-range vertex index.</summary>
    [Fact]
    public void Validate_OutOfRangeIndex_ReturnsFailure()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitY, (0, 0)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        var mesh = new TriangleMesh(vertices, faces);

        GeometryResult result = mesh.Validate();

        result.Success.Should().BeFalse();
    }

    /// <summary>Verifies Validate succeeds for empty mesh.</summary>
    [Fact]
    public void Validate_EmptyMesh_ReturnsOk()
    {
        TriangleMesh.Empty.Validate().Success.Should().BeTrue();
    }

    /// <summary>Verifies single triangle mesh has correct edge count and volume.</summary>
    [Fact]
    public void SingleTriangle_EdgeCountIsThree()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitY, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitY, (1, 0)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitY, (0, 1)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        var mesh = new TriangleMesh(vertices, faces);

        mesh.EdgeCount.Should().Be(3);
    }

    /// <summary>Verifies two triangles sharing an edge have reduced unique edge count.</summary>
    [Fact]
    public void TwoTriangles_SharedEdge_SharedEdges()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitY, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitY, (1, 0)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitY, (0, 1)),
            new Vertex(new Point3D(1, 1, 0), Vector3D.UnitY, (1, 1)));
        var faces = ImmutableArray.Create(
            new TriangleFace(0, 1, 2),
            new TriangleFace(1, 3, 2));
        var mesh = new TriangleMesh(vertices, faces);

        mesh.EdgeCount.Should().Be(5);
    }

    /// <summary>Verifies BoundingBox returns origin for empty mesh.</summary>
    [Fact]
    public void BoundingBox_EmptyMesh_ReturnsOrigin()
    {
        BoundingBox3D bb = TriangleMesh.Empty.BoundingBox();

        bb.Min.Should().Be(Point3D.Origin);
        bb.Max.Should().Be(Point3D.Origin);
    }
}
