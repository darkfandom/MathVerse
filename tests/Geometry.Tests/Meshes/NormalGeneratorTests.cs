namespace MathVerse.Geometry.Tests.Meshes;

/// <summary>Tests for the <see cref="NormalGenerator"/> static class.</summary>
public class NormalGeneratorTests
{
    private const double Tolerance = 1e-10;

    private static Vertex V(double x, double y, double z) =>
        new(new Point3D(x, y, z), Vector3D.Zero, (0, 0));

    private static TriangleMesh MakeSingleTriangleMesh()
    {
        var vertices = ImmutableArray.Create(
            V(0, 0, 0), V(1, 0, 0), V(0, 1, 0));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        return new TriangleMesh(vertices, faces);
    }

    private static TriangleMesh MakeQuadMesh()
    {
        var vertices = ImmutableArray.Create(
            V(0, 0, 0), V(1, 0, 0), V(1, 1, 0), V(0, 1, 0));
        var faces = ImmutableArray.Create(
            new TriangleFace(0, 1, 2),
            new TriangleFace(0, 2, 3));
        return new TriangleMesh(vertices, faces);
    }

    /// <summary>Verifies ComputeVertexNormals returns one normal per vertex.</summary>
    [Fact]
    public void ComputeVertexNormals_CountMatchesVertexCount()
    {
        var mesh = MakeQuadMesh();

        var normals = NormalGenerator.ComputeVertexNormals(mesh);

        normals.Should().HaveCount(4);
    }

    /// <summary>Verifies ComputeFaceNormals returns one normal per face.</summary>
    [Fact]
    public void ComputeFaceNormals_CountMatchesFaceCount()
    {
        var mesh = MakeQuadMesh();

        var normals = NormalGenerator.ComputeFaceNormals(mesh);

        normals.Should().HaveCount(2);
    }

    /// <summary>Verifies ComputeVertexNormals returns unit normals.</summary>
    [Fact]
    public void ComputeVertexNormals_ReturnsUnitNormals()
    {
        var mesh = MakeQuadMesh();

        var normals = NormalGenerator.ComputeVertexNormals(mesh);

        foreach (Vector3D n in normals)
        {
            n.Length.Should().BeApproximately(1.0, Tolerance);
        }
    }

    /// <summary>Verifies ComputeFaceNormals for a flat XY triangle points along Z.</summary>
    [Fact]
    public void ComputeFaceNormals_FlatXYTriangle_PointAlongZ()
    {
        var mesh = MakeSingleTriangleMesh();

        var normals = NormalGenerator.ComputeFaceNormals(mesh);

        normals.Should().HaveCount(1);
        (System.Math.Abs(System.Math.Abs(normals[0].Z) - 1.0) < 1e-10).Should().BeTrue();
    }

    /// <summary>Verifies ComputeSmoothNormals returns one normal per vertex.</summary>
    [Fact]
    public void ComputeSmoothNormals_CountMatchesVertexCount()
    {
        var mesh = MakeQuadMesh();

        var normals = NormalGenerator.ComputeSmoothNormals(mesh);

        normals.Should().HaveCount(4);
    }

    /// <summary>Verifies ComputeSmoothNormals returns unit normals.</summary>
    [Fact]
    public void ComputeSmoothNormals_ReturnsUnitNormals()
    {
        var mesh = MakeQuadMesh();

        var normals = NormalGenerator.ComputeSmoothNormals(mesh);

        foreach (Vector3D n in normals)
        {
            n.Length.Should().BeApproximately(1.0, Tolerance);
        }
    }

    /// <summary>Verifies ComputeTangents returns one tangent per vertex.</summary>
    [Fact]
    public void ComputeTangents_CountMatchesVertexCount()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitY, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitY, (1, 0)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitY, (0, 1)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        var mesh = new TriangleMesh(vertices, faces);

        var tangents = NormalGenerator.ComputeTangents(mesh);

        tangents.Should().HaveCount(3);
    }

    /// <summary>Verifies ComputeTangents for a unit-UV triangle returns meaningful tangents.</summary>
    [Fact]
    public void ComputeTangents_UnitUV_ReturnsUnitTangents()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitY, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitY, (1, 0)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitY, (0, 1)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        var mesh = new TriangleMesh(vertices, faces);

        var tangents = NormalGenerator.ComputeTangents(mesh);

        foreach (Vector3D t in tangents)
        {
            t.Length.Should().BeApproximately(1.0, Tolerance);
        }
    }

    /// <summary>Verifies ComputeVertexNormals for single triangle returns normals for all vertices.</summary>
    [Fact]
    public void ComputeVertexNormals_SingleTriangle_ReturnsAllNormals()
    {
        var mesh = MakeSingleTriangleMesh();

        var normals = NormalGenerator.ComputeVertexNormals(mesh);

        normals.Should().HaveCount(3);
        foreach (Vector3D n in normals)
        {
            n.Length.Should().BeApproximately(1.0, Tolerance);
        }
    }

    /// <summary>Verifies ComputeFaceNormals for quad mesh returns two face normals.</summary>
    [Fact]
    public void ComputeFaceNormals_QuadMesh_ReturnsTwoNormals()
    {
        var mesh = MakeQuadMesh();

        var normals = NormalGenerator.ComputeFaceNormals(mesh);

        normals.Should().HaveCount(2);
        foreach (Vector3D n in normals)
        {
            n.Length.Should().BeApproximately(1.0, Tolerance);
        }
    }

    /// <summary>Verifies ComputeSmoothNormals on single triangle returns correct count.</summary>
    [Fact]
    public void ComputeSmoothNormals_SingleTriangle_ReturnsThreeNormals()
    {
        var mesh = MakeSingleTriangleMesh();

        var normals = NormalGenerator.ComputeSmoothNormals(mesh);

        normals.Should().HaveCount(3);
    }

    /// <summary>Verifies ComputeTangents for quad mesh returns four tangents.</summary>
    [Fact]
    public void ComputeTangents_QuadMesh_ReturnsFourTangents()
    {
        var mesh = MakeQuadMesh();

        var tangents = NormalGenerator.ComputeTangents(mesh);

        tangents.Should().HaveCount(4);
    }
}
