namespace MathVerse.Geometry.Tests.Meshes;

/// <summary>Tests for the <see cref="QuadFace"/> struct.</summary>
public class QuadFaceTests
{
    /// <summary>Verifies that QuadFace stores the correct vertex indices.</summary>
    [Fact]
    public void Constructor_SetsVertexIndices()
    {
        var quad = new QuadFace(0, 1, 2, 3);

        quad.V0.Should().Be(0);
        quad.V1.Should().Be(1);
        quad.V2.Should().Be(2);
        quad.V3.Should().Be(3);
    }

    /// <summary>Verifies Triangulate returns two triangles.</summary>
    [Fact]
    public void Triangulate_ReturnsTwoTriangles()
    {
        var quad = new QuadFace(0, 1, 2, 3);

        (TriangleFace t0, TriangleFace t1) = quad.Triangulate();

        t0.V0.Should().Be(0);
        t0.V1.Should().Be(1);
        t0.V2.Should().Be(2);
        t1.V0.Should().Be(0);
        t1.V1.Should().Be(2);
        t1.V2.Should().Be(3);
    }

    /// <summary>Verifies both triangles cover all four vertices of the quad.</summary>
    [Fact]
    public void Triangulate_CoversAllFourVertices()
    {
        var quad = new QuadFace(10, 20, 30, 40);

        (TriangleFace t0, TriangleFace t1) = quad.Triangulate();

        HashSet<int> allIndices = new()
        {
            t0.V0, t0.V1, t0.V2,
            t1.V0, t1.V1, t1.V2
        };

        allIndices.Should().BeEquivalentTo(new[] { 10, 20, 30, 40 });
    }

    /// <summary>Verifies record equality for quads with same indices.</summary>
    [Fact]
    public void Equality_SameIndices_ReturnsEqual()
    {
        var a = new QuadFace(0, 1, 2, 3);
        var b = new QuadFace(0, 1, 2, 3);

        a.Should().Be(b);
    }

    /// <summary>Verifies record inequality for quads with different indices.</summary>
    [Fact]
    public void Equality_DifferentIndices_ReturnsNotEqual()
    {
        var a = new QuadFace(0, 1, 2, 3);
        var b = new QuadFace(3, 2, 1, 0);

        a.Should().NotBe(b);
    }

    /// <summary>Verifies that QuadFace is a value type.</summary>
    [Fact]
    public void QuadFace_IsValueType()
    {
        typeof(QuadFace).IsValueType.Should().BeTrue();
    }

    /// <summary>Verifies that first triangle of triangulation uses V0, V1, V2.</summary>
    [Fact]
    public void Triangulate_FirstTriangle_UsesFirstThreeVertices()
    {
        var quad = new QuadFace(5, 6, 7, 8);

        (TriangleFace t0, _) = quad.Triangulate();

        t0.V0.Should().Be(5);
        t0.V1.Should().Be(6);
        t0.V2.Should().Be(7);
    }

    /// <summary>Verifies that second triangle of triangulation uses V0, V2, V3.</summary>
    [Fact]
    public void Triangulate_SecondTriangle_UsesCorrectVertices()
    {
        var quad = new QuadFace(5, 6, 7, 8);

        (_, TriangleFace t1) = quad.Triangulate();

        t1.V0.Should().Be(5);
        t1.V1.Should().Be(7);
        t1.V2.Should().Be(8);
    }
}
