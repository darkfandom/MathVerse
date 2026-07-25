namespace MathVerse.Geometry.Tests.Meshes;

/// <summary>Tests for the <see cref="Edge"/> struct.</summary>
public class EdgeTests
{
    /// <summary>Verifies that an edge stores the correct vertex indices.</summary>
    [Fact]
    public void Constructor_SetsVertexIndices()
    {
        var edge = new Edge(3, 7);

        edge.V0.Should().Be(3);
        edge.V1.Should().Be(7);
    }

    /// <summary>Verifies that Reversed swaps the vertex indices.</summary>
    [Fact]
    public void Reversed_SwapsIndices()
    {
        var edge = new Edge(2, 5);

        Edge reversed = edge.Reversed();

        reversed.V0.Should().Be(5);
        reversed.V1.Should().Be(2);
    }

    /// <summary>Verifies that Reversed applied twice returns the original edge.</summary>
    [Fact]
    public void ReversedTwice_ReturnsOriginal()
    {
        var edge = new Edge(1, 4);

        Edge result = edge.Reversed().Reversed();

        result.V0.Should().Be(edge.V0);
        result.V1.Should().Be(edge.V1);
    }

    /// <summary>Verifies that Canonical orders indices so V0 is less than or equal to V1.</summary>
    [Fact]
    public void Canonical_OrdersIndices()
    {
        var edge = new Edge(7, 2);

        Edge canonical = edge.Canonical();

        canonical.V0.Should().Be(2);
        canonical.V1.Should().Be(7);
    }

    /// <summary>Verifies that Canonical on already canonical edge returns the same edge.</summary>
    [Fact]
    public void Canonical_AlreadyOrdered_ReturnsSame()
    {
        var edge = new Edge(1, 3);

        Edge canonical = edge.Canonical();

        canonical.V0.Should().Be(1);
        canonical.V1.Should().Be(3);
    }

    /// <summary>Verifies Canonical when both indices are equal.</summary>
    [Fact]
    public void Canonical_EqualIndices_ReturnsSame()
    {
        var edge = new Edge(5, 5);

        Edge canonical = edge.Canonical();

        canonical.V0.Should().Be(5);
        canonical.V1.Should().Be(5);
    }

    /// <summary>Verifies record equality for edges with same indices.</summary>
    [Fact]
    public void Equality_SameIndices_ReturnsEqual()
    {
        var a = new Edge(0, 1);
        var b = new Edge(0, 1);

        a.Should().Be(b);
    }

    /// <summary>Verifies record inequality for edges with different indices.</summary>
    [Fact]
    public void Equality_DifferentIndices_ReturnsNotEqual()
    {
        var a = new Edge(0, 1);
        var b = new Edge(1, 0);

        a.Should().NotBe(b);
    }

    /// <summary>Verifies that Edge is a value type (struct).</summary>
    [Fact]
    public void Edge_IsValueType()
    {
        typeof(Edge).IsValueType.Should().BeTrue();
    }

    /// <summary>Verifies Canonical returns self when V0 already less than V1.</summary>
    [Fact]
    public void Canonical_FirstSmaller_ReturnsSame()
    {
        var edge = new Edge(0, 10);

        Edge canonical = edge.Canonical();

        canonical.V0.Should().Be(0);
        canonical.V1.Should().Be(10);
    }
}
