namespace MathVerse.Geometry.Tests.Core;

/// <summary>Tests for the <see cref="GeometryEngine"/> class.</summary>
public class GeometryEngineTests
{
    private static GeometryEngine CreateEngine()
    {
        return new GeometryEngine(new GeometryOptions());
    }

    /// <summary>Verifies that CreatePoint2D creates a point with correct coordinates.</summary>
    [Fact]
    public void CreatePoint2D_CreatesCorrectPoint()
    {
        var engine = CreateEngine();

        var p = engine.CreatePoint2D(3.0, 4.0);

        p.X.Should().BeApproximately(3.0, 1e-10);
        p.Y.Should().BeApproximately(4.0, 1e-10);
    }

    /// <summary>Verifies that CreatePoint3D creates a point with correct coordinates.</summary>
    [Fact]
    public void CreatePoint3D_CreatesCorrectPoint()
    {
        var engine = CreateEngine();

        var p = engine.CreatePoint3D(1.0, 2.0, 3.0);

        p.X.Should().BeApproximately(1.0, 1e-10);
        p.Y.Should().BeApproximately(2.0, 1e-10);
        p.Z.Should().BeApproximately(3.0, 1e-10);
    }

    /// <summary>Verifies that CreateLine2D creates a line with correct endpoints.</summary>
    [Fact]
    public void CreateLine2D_CreatesCorrectLine()
    {
        var engine = CreateEngine();

        var line = engine.CreateLine2D(new Point2D(1, 2), new Point2D(3, 4));

        line.P1.X.Should().BeApproximately(1.0, 1e-10);
        line.P2.X.Should().BeApproximately(3.0, 1e-10);
    }

    /// <summary>Verifies that CreateLine3D creates a line with correct endpoints.</summary>
    [Fact]
    public void CreateLine3D_CreatesCorrectLine()
    {
        var engine = CreateEngine();

        var line = engine.CreateLine3D(new Point3D(1, 2, 3), new Point3D(4, 5, 6));

        line.P1.Z.Should().BeApproximately(3.0, 1e-10);
        line.P2.Z.Should().BeApproximately(6.0, 1e-10);
    }

    /// <summary>Verifies that CreateCircle2D creates a circle with correct center and radius.</summary>
    [Fact]
    public void CreateCircle2D_CreatesCorrectCircle()
    {
        var engine = CreateEngine();

        var circle = engine.CreateCircle2D(new Point2D(1, 2), 5.0);

        circle.Center.X.Should().BeApproximately(1.0, 1e-10);
        circle.Radius.Should().BeApproximately(5.0, 1e-10);
    }

    /// <summary>Verifies that CreatePlane creates a plane with correct point and normal.</summary>
    [Fact]
    public void CreatePlane_CreatesCorrectPlane()
    {
        var engine = CreateEngine();

        var plane = engine.CreatePlane(new Point3D(0, 0, 0), new Vector3D(0, 1, 0));

        plane.Normal.Y.Should().BeApproximately(1.0, 1e-10);
    }

    /// <summary>Verifies that CreateSphere creates a sphere with correct center and radius.</summary>
    [Fact]
    public void CreateSphere_CreatesCorrectSphere()
    {
        var engine = CreateEngine();

        var sphere = engine.CreateSphere(new Point3D(1, 2, 3), 4.0);

        sphere.Center.X.Should().BeApproximately(1.0, 1e-10);
        sphere.Radius.Should().BeApproximately(4.0, 1e-10);
    }

    /// <summary>Verifies that CreateMesh returns a MeshBuilder.</summary>
    [Fact]
    public void CreateMesh_ReturnsMeshBuilder()
    {
        var engine = CreateEngine();

        var builder = engine.CreateMesh();

        builder.Should().NotBeNull();
        builder.VertexCount.Should().Be(0);
    }

    /// <summary>Verifies that CreateScene returns a new empty scene.</summary>
    [Fact]
    public void CreateScene_ReturnsEmptyScene()
    {
        var engine = CreateEngine();

        var scene = engine.CreateScene();

        scene.Should().NotBeNull();
        scene.NodeCount.Should().Be(0);
    }

    /// <summary>Verifies that TessellatePolygon returns correct triangle count.</summary>
    [Fact]
    public void TessellatePolygon_ReturnsCorrectTriangleCount()
    {
        var engine = CreateEngine();
        var vertices = new List<Point2D>
        {
            new(0, 0), new(1, 0), new(1, 1), new(0, 1)
        };

        var triangles = engine.TessellatePolygon(vertices);

        triangles.Length.Should().Be(2);
    }

    /// <summary>Verifies that TransformPoint2D applies a translation correctly.</summary>
    [Fact]
    public void TransformPoint2D_Translation()
    {
        var engine = CreateEngine();
        var point = new Point2D(1, 2);
        var transform = Transform2D.Translation(3, 4);

        var result = engine.TransformPoint2D(point, transform);

        result.X.Should().BeApproximately(4.0, 1e-10);
        result.Y.Should().BeApproximately(6.0, 1e-10);
    }

    /// <summary>Verifies that TransformPoint3D applies a translation correctly.</summary>
    [Fact]
    public void TransformPoint3D_Translation()
    {
        var engine = CreateEngine();
        var point = new Point3D(1, 2, 3);
        var transform = Transform3D.Translation(4, 5, 6);

        var result = engine.TransformPoint3D(point, transform);

        result.X.Should().BeApproximately(5.0, 1e-10);
        result.Y.Should().BeApproximately(7.0, 1e-10);
        result.Z.Should().BeApproximately(9.0, 1e-10);
    }

    /// <summary>Verifies that ValidateMesh returns success for a non-null mesh.</summary>
    [Fact]
    public void ValidateMesh_NonNull_ReturnsSuccess()
    {
        var engine = CreateEngine();
        var mesh = new MathVerse.Math.Geometry.Meshes.TriangleMesh(
            ImmutableArray<MathVerse.Math.Geometry.Meshes.Vertex>.Empty,
            ImmutableArray<MathVerse.Math.Geometry.Meshes.TriangleFace>.Empty);

        var result = engine.ValidateMesh(mesh);

        result.Success.Should().BeTrue();
    }

    /// <summary>Verifies that ValidateMesh returns failure for null mesh.</summary>
    [Fact]
    public void ValidateMesh_Null_ReturnsFailure()
    {
        var engine = CreateEngine();

        var result = engine.ValidateMesh(null);

        result.Success.Should().BeFalse();
    }

    /// <summary>Verifies that ValidateGeometry returns failure for null geometry.</summary>
    [Fact]
    public void ValidateGeometry_Null_ReturnsFailure()
    {
        var engine = CreateEngine();

        var result = engine.ValidateGeometry(null);

        result.Success.Should().BeFalse();
    }

    /// <summary>Verifies that ClearCaches succeeds without throwing.</summary>
    [Fact]
    public void ClearCaches_DoesNotThrow()
    {
        var engine = CreateEngine();

        var act = () => engine.ClearCaches();

        act.Should().NotThrow();
    }
}
