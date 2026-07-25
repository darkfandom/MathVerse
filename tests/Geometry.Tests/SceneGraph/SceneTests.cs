namespace MathVerse.Geometry.Tests.SceneGraph;

/// <summary>Tests for the <see cref="Scene"/> class.</summary>
public class SceneTests
{
    /// <summary>Verifies that a new Scene has a default name and no root nodes.</summary>
    [Fact]
    public void Constructor_DefaultValues()
    {
        var scene = new Scene();

        scene.Name.Should().Be("Scene");
        scene.RootNodes.Should().BeEmpty();
    }

    /// <summary>Verifies that a Scene stores the provided name.</summary>
    [Fact]
    public void Constructor_WithName_StoresName()
    {
        var scene = new Scene("MyScene");

        scene.Name.Should().Be("MyScene");
    }

    /// <summary>Verifies AddRootNode adds a node to RootNodes.</summary>
    [Fact]
    public void AddRootNode_AddsToRootNodes()
    {
        var scene = new Scene();
        var node = new SceneNode("Root");

        scene.AddRootNode(node);

        scene.RootNodes.Should().ContainSingle();
        scene.RootNodes[0].Should().Be(node);
    }

    /// <summary>Verifies RemoveRootNode removes the specified node.</summary>
    [Fact]
    public void RemoveRootNode_RemovesNode()
    {
        var scene = new Scene();
        var node = new SceneNode("Root");
        scene.AddRootNode(node);

        bool removed = scene.RemoveRootNode(node);

        removed.Should().BeTrue();
        scene.RootNodes.Should().BeEmpty();
    }

    /// <summary>Verifies RemoveRootNode returns false for a non-existent node.</summary>
    [Fact]
    public void RemoveRootNode_NonExistent_ReturnsFalse()
    {
        var scene = new Scene();
        var node = new SceneNode("Root");

        bool removed = scene.RemoveRootNode(node);

        removed.Should().BeFalse();
    }

    /// <summary>Verifies Clear removes all root nodes.</summary>
    [Fact]
    public void Clear_RemovesAllRootNodes()
    {
        var scene = new Scene();
        scene.AddRootNode(new SceneNode("A"));
        scene.AddRootNode(new SceneNode("B"));

        scene.Clear();

        scene.RootNodes.Should().BeEmpty();
    }

    /// <summary>Verifies TraverseAll yields all nodes in depth-first order.</summary>
    [Fact]
    public void TraverseAll_YieldsAllNodes()
    {
        var scene = new Scene();
        var root = new SceneNode("Root");
        var child = new SceneNode("Child");
        root.AddChild(child);
        scene.AddRootNode(root);

        var allNodes = scene.TraverseAll().ToList();

        allNodes.Should().HaveCount(2);
    }

    /// <summary>Verifies GetGeometryNodes returns only GeometryNode instances.</summary>
    [Fact]
    public void GetGeometryNodes_ReturnsOnlyGeometryNodes()
    {
        var scene = new Scene();
        var geoNode = new GeometryNode("Geo");
        var lightNode = new LightNode("Light");
        scene.AddRootNode(geoNode);
        scene.AddRootNode(lightNode);

        var geometryNodes = scene.GetGeometryNodes().ToList();

        geometryNodes.Should().ContainSingle();
        geometryNodes[0].Name.Should().Be("Geo");
    }

    /// <summary>Verifies GetCameraNodes returns only CameraNode instances.</summary>
    [Fact]
    public void GetCameraNodes_ReturnsOnlyCameraNodes()
    {
        var scene = new Scene();
        var cameraNode = new CameraNode("Cam");
        var geoNode = new GeometryNode("Geo");
        scene.AddRootNode(cameraNode);
        scene.AddRootNode(geoNode);

        var cameraNodes = scene.GetCameraNodes().ToList();

        cameraNodes.Should().ContainSingle();
        cameraNodes[0].Name.Should().Be("Cam");
    }

    /// <summary>Verifies GetLightNodes returns only LightNode instances.</summary>
    [Fact]
    public void GetLightNodes_ReturnsOnlyLightNodes()
    {
        var scene = new Scene();
        var lightNode = new LightNode("Light");
        var geoNode = new GeometryNode("Geo");
        scene.AddRootNode(lightNode);
        scene.AddRootNode(geoNode);

        var lightNodes = scene.GetLightNodes().ToList();

        lightNodes.Should().ContainSingle();
        lightNodes[0].Name.Should().Be("Light");
    }

    /// <summary>Verifies ComputeBoundingBox returns origin for empty scene.</summary>
    [Fact]
    public void ComputeBoundingBox_EmptyScene_ReturnsOrigin()
    {
        var scene = new Scene();

        BoundingBox3D bb = scene.ComputeBoundingBox();

        bb.Min.Should().Be(Point3D.Origin);
        bb.Max.Should().Be(Point3D.Origin);
    }

    /// <summary>Verifies NodeCount reflects the number of root nodes.</summary>
    [Fact]
    public void NodeCount_ReflectsRootCount()
    {
        var scene = new Scene();
        scene.AddRootNode(new SceneNode("A"));
        scene.AddRootNode(new SceneNode("B"));

        scene.NodeCount.Should().Be(2);
    }

    /// <summary>Verifies TotalNodeCount includes descendant nodes.</summary>
    [Fact]
    public void TotalNodeCount_IncludesDescendants()
    {
        var scene = new Scene();
        var root = new SceneNode("Root");
        var child = new SceneNode("Child");
        var grandchild = new SceneNode("Grandchild");
        root.AddChild(child);
        child.AddChild(grandchild);
        scene.AddRootNode(root);

        scene.TotalNodeCount.Should().Be(3);
    }

    /// <summary>Verifies TotalNodeCount for empty scene is zero.</summary>
    [Fact]
    public void TotalNodeCount_EmptyScene_ReturnsZero()
    {
        var scene = new Scene();

        scene.TotalNodeCount.Should().Be(0);
    }

    /// <summary>Verifies TraverseAll traverses across multiple root nodes.</summary>
    [Fact]
    public void TraverseAll_MultipleRoots_TraversesAll()
    {
        var scene = new Scene();
        var root1 = new SceneNode("R1");
        root1.AddChild(new SceneNode("C1"));
        var root2 = new SceneNode("R2");
        root2.AddChild(new SceneNode("C2"));
        scene.AddRootNode(root1);
        scene.AddRootNode(root2);

        var allNodes = scene.TraverseAll().ToList();

        allNodes.Should().HaveCount(4);
    }
}
