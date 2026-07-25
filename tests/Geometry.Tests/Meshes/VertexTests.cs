namespace MathVerse.Geometry.Tests.Meshes;

/// <summary>Tests for the <see cref="Vertex"/> struct.</summary>
public class VertexTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies that a vertex stores the correct position, normal, and UV.</summary>
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        var pos = new Point3D(1, 2, 3);
        var normal = new Vector3D(0, 1, 0);
        var uv = (0.5, 0.75);

        var vertex = new Vertex(pos, normal, uv);

        vertex.Position.Should().Be(pos);
        vertex.Normal.Should().Be(normal);
        vertex.UV.Should().Be(uv);
    }

    /// <summary>Verifies default vertex has zero position, zero normal, and zero UV.</summary>
    [Fact]
    public void DefaultVertex_HasZeroValues()
    {
        Vertex defaultVertex = default;

        defaultVertex.Position.X.Should().Be(0.0);
        defaultVertex.Position.Y.Should().Be(0.0);
        defaultVertex.Position.Z.Should().Be(0.0);
        defaultVertex.Normal.X.Should().Be(0.0);
        defaultVertex.Normal.Y.Should().Be(0.0);
        defaultVertex.Normal.Z.Should().Be(0.0);
        defaultVertex.UV.U.Should().Be(0.0);
        defaultVertex.UV.V.Should().Be(0.0);
    }

    /// <summary>Verifies that two vertices with the same values are equal.</summary>
    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var a = new Vertex(new Point3D(1, 2, 3), new Vector3D(0, 1, 0), (0.5, 0.5));
        var b = new Vertex(new Point3D(1, 2, 3), new Vector3D(0, 1, 0), (0.5, 0.5));

        a.Should().Be(b);
    }

    /// <summary>Verifies that two vertices with different positions are not equal.</summary>
    [Fact]
    public void Equals_DifferentPosition_ReturnsFalse()
    {
        var a = new Vertex(new Point3D(1, 2, 3), new Vector3D(0, 1, 0), (0.0, 0.0));
        var b = new Vertex(new Point3D(4, 5, 6), new Vector3D(0, 1, 0), (0.0, 0.0));

        a.Should().NotBe(b);
    }

    /// <summary>Verifies Lerp at t=0 returns the original vertex.</summary>
    [Fact]
    public void Lerp_AtZero_ReturnsOriginal()
    {
        var a = new Vertex(new Point3D(1, 2, 3), new Vector3D(1, 0, 0), (0.0, 0.0));
        var b = new Vertex(new Point3D(5, 6, 7), new Vector3D(0, 1, 0), (1.0, 1.0));

        Vertex result = a.Lerp(b, 0.0);

        result.Position.X.Should().BeApproximately(1.0, Tolerance);
        result.Position.Y.Should().BeApproximately(2.0, Tolerance);
        result.Position.Z.Should().BeApproximately(3.0, Tolerance);
        result.Normal.X.Should().BeApproximately(1.0, Tolerance);
        result.UV.U.Should().BeApproximately(0.0, Tolerance);
        result.UV.V.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies Lerp at t=1 returns the target vertex.</summary>
    [Fact]
    public void Lerp_AtOne_ReturnsTarget()
    {
        var a = new Vertex(new Point3D(1, 2, 3), new Vector3D(1, 0, 0), (0.0, 0.0));
        var b = new Vertex(new Point3D(5, 6, 7), new Vector3D(0, 1, 0), (1.0, 1.0));

        Vertex result = a.Lerp(b, 1.0);

        result.Position.X.Should().BeApproximately(5.0, Tolerance);
        result.Position.Y.Should().BeApproximately(6.0, Tolerance);
        result.Position.Z.Should().BeApproximately(7.0, Tolerance);
        result.Normal.Y.Should().BeApproximately(1.0, Tolerance);
        result.UV.U.Should().BeApproximately(1.0, Tolerance);
        result.UV.V.Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies Lerp at t=0.5 returns the midpoint.</summary>
    [Fact]
    public void Lerp_AtHalf_ReturnsMidpoint()
    {
        var a = new Vertex(new Point3D(0, 0, 0), new Vector3D(0, 0, 1), (0.0, 0.0));
        var b = new Vertex(new Point3D(2, 4, 6), new Vector3D(0, 0, 1), (1.0, 0.0));

        Vertex result = a.Lerp(b, 0.5);

        result.Position.X.Should().BeApproximately(1.0, Tolerance);
        result.Position.Y.Should().BeApproximately(2.0, Tolerance);
        result.Position.Z.Should().BeApproximately(3.0, Tolerance);
        result.UV.U.Should().BeApproximately(0.5, Tolerance);
        result.UV.V.Should().BeApproximately(0.0, Tolerance);
    }

    /// <summary>Verifies record struct equality semantics.</summary>
    [Fact]
    public void RecordEquality_SameType_ReturnsTrue()
    {
        var a = new Vertex(new Point3D(1, 1, 1), new Vector3D(0, 0, 1), (0.25, 0.75));
        var b = new Vertex(new Point3D(1, 1, 1), new Vector3D(0, 0, 1), (0.25, 0.75));

        (a == b).Should().BeTrue();
    }

    /// <summary>Verifies that Vertex is a value type (struct).</summary>
    [Fact]
    public void Vertex_IsValueType()
    {
        typeof(Vertex).IsValueType.Should().BeTrue();
    }

    /// <summary>Verifies Lerp interpolates normals linearly.</summary>
    [Fact]
    public void Lerp_InterpolatesNormal()
    {
        var a = new Vertex(Point3D.Origin, new Vector3D(1, 0, 0), (0, 0));
        var b = new Vertex(Point3D.Origin, new Vector3D(0, 1, 0), (0, 0));

        Vertex result = a.Lerp(b, 0.5);

        result.Normal.X.Should().BeApproximately(0.5, Tolerance);
        result.Normal.Y.Should().BeApproximately(0.5, Tolerance);
        result.Normal.Z.Should().BeApproximately(0.0, Tolerance);
    }
}
