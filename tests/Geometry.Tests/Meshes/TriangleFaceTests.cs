namespace MathVerse.Geometry.Tests.Meshes;

/// <summary>Tests for the <see cref="TriangleFace"/> struct.</summary>
public class TriangleFaceTests
{
    /// <summary>Verifies that TriangleFace stores the correct vertex indices.</summary>
    [Fact]
    public void Constructor_SetsVertexIndices()
    {
        var face = new TriangleFace(0, 1, 2);

        face.V0.Should().Be(0);
        face.V1.Should().Be(1);
        face.V2.Should().Be(2);
    }

    /// <summary>Verifies Edges returns three edges connecting the vertices.</summary>
    [Fact]
    public void Edges_ReturnsThreeEdges()
    {
        var face = new TriangleFace(0, 3, 5);

        (Edge e0, Edge e1, Edge e2) = face.Edges;

        e0.V0.Should().Be(0);
        e0.V1.Should().Be(3);
        e1.V0.Should().Be(3);
        e1.V1.Should().Be(5);
        e2.V0.Should().Be(5);
        e2.V1.Should().Be(0);
    }

    /// <summary>Verifies Indices returns an array of the three vertex indices.</summary>
    [Fact]
    public void Indices_ReturnsArray()
    {
        var face = new TriangleFace(2, 4, 6);

        int[] indices = face.Indices;

        indices.Should().HaveCount(3);
        indices.Should().BeEquivalentTo(new[] { 2, 4, 6 });
    }

    /// <summary>Verifies record equality for faces with same indices.</summary>
    [Fact]
    public void Equality_SameIndices_ReturnsEqual()
    {
        var a = new TriangleFace(0, 1, 2);
        var b = new TriangleFace(0, 1, 2);

        a.Should().Be(b);
    }

    /// <summary>Verifies record inequality for faces with different indices.</summary>
    [Fact]
    public void Equality_DifferentIndices_ReturnsNotEqual()
    {
        var a = new TriangleFace(0, 1, 2);
        var b = new TriangleFace(0, 2, 1);

        a.Should().NotBe(b);
    }

    /// <summary>Verifies that TriangleFace is a value type.</summary>
    [Fact]
    public void TriangleFace_IsValueType()
    {
        typeof(TriangleFace).IsValueType.Should().BeTrue();
    }

    /// <summary>Verifies edges form a closed loop.</summary>
    [Fact]
    public void Edges_FormClosedLoop()
    {
        var face = new TriangleFace(5, 10, 15);

        (Edge e0, Edge e1, Edge e2) = face.Edges;

        e0.V1.Should().Be(e1.V0);
        e1.V1.Should().Be(e2.V0);
        e2.V1.Should().Be(e0.V0);
    }

    /// <summary>Verifies Indices content matches constructor arguments.</summary>
    [Fact]
    public void Indices_MatchConstructorArgs()
    {
        var face = new TriangleFace(7, 8, 9);

        face.Indices[0].Should().Be(7);
        face.Indices[1].Should().Be(8);
        face.Indices[2].Should().Be(9);
    }
}
