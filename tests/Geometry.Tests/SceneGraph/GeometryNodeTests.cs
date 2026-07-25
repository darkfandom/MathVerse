namespace MathVerse.Geometry.Tests.SceneGraph;

/// <summary>Tests for the <see cref="GeometryNode"/> class.</summary>
public class GeometryNodeTests
{
    /// <summary>Verifies GeometryNode construction with name and mesh.</summary>
    [Fact]
    public void Constructor_SetsNameAndMesh()
    {
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitY, (0, 0)),
            new Vertex(new Point3D(1, 0, 0), Vector3D.UnitY, (1, 0)),
            new Vertex(new Point3D(0, 1, 0), Vector3D.UnitY, (0, 1)));
        var faces = ImmutableArray.Create(new TriangleFace(0, 1, 2));
        var mesh = new TriangleMesh(vertices, faces);

        var node = new GeometryNode("MyMesh", mesh);

        node.Name.Should().Be("MyMesh");
        node.Mesh.Should().Be(mesh);
    }

    /// <summary>Verifies GeometryNode defaults to null mesh when not provided.</summary>
    [Fact]
    public void Constructor_DefaultsToNullMesh()
    {
        var node = new GeometryNode("Test");

        node.Mesh.Should().BeNull();
    }

    /// <summary>Verifies MaterialName defaults to "default".</summary>
    [Fact]
    public void MaterialName_DefaultsToDefault()
    {
        var node = new GeometryNode();

        node.MaterialName.Should().Be("default");
    }

    /// <summary>Verifies MaterialName can be changed.</summary>
    [Fact]
    public void MaterialName_CanBeSet()
    {
        var node = new GeometryNode();

        node.MaterialName = "metal";

        node.MaterialName.Should().Be("metal");
    }

    /// <summary>Verifies AddChild works on GeometryNode (inherited from SceneNode).</summary>
    [Fact]
    public void AddChild_WorksOnGeometryNode()
    {
        var parent = new GeometryNode("Parent");
        var child = new SceneNode("Child");

        parent.AddChild(child);

        parent.Children.Should().ContainSingle();
        child.Parent.Should().Be(parent);
    }

    /// <summary>Verifies GeometryNode WorldTransform works correctly.</summary>
    [Fact]
    public void WorldTransform_WithParent_Accumulates()
    {
        var parent = new SceneNode("Parent");
        parent.LocalTransform = Transform3D.Translation(1, 2, 3);
        var geo = new GeometryNode("Geo");
        parent.AddChild(geo);

        geo.WorldTransform[0, 3].Should().BeApproximately(1.0, 1e-10);
        geo.WorldTransform[1, 3].Should().BeApproximately(2.0, 1e-10);
        geo.WorldTransform[2, 3].Should().BeApproximately(3.0, 1e-10);
    }

    /// <summary>Verifies Mesh can be changed after construction.</summary>
    [Fact]
    public void Mesh_CanBeChanged()
    {
        var node = new GeometryNode("Test");
        var vertices = ImmutableArray.Create(
            new Vertex(new Point3D(0, 0, 0), Vector3D.UnitY, (0, 0)));
        var mesh = new TriangleMesh(vertices, ImmutableArray<TriangleFace>.Empty);

        node.Mesh = mesh;

        node.Mesh.Should().Be(mesh);
    }

    /// <summary>Verifies GeometryNode is a SceneNode (inheritance).</summary>
    [Fact]
    public void GeometryNode_InheritsSceneNode()
    {
        var node = new GeometryNode("Test");

        node.Should().BeAssignableTo<SceneNode>();
    }
}
