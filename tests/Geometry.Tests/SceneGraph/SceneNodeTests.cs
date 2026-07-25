namespace MathVerse.Geometry.Tests.SceneGraph;

/// <summary>Tests for the <see cref="SceneNode"/> class.</summary>
public class SceneNodeTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies that a new SceneNode has default name and identity transform.</summary>
    [Fact]
    public void Constructor_DefaultValues()
    {
        var node = new SceneNode();

        node.Name.Should().Be("");
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
                node.LocalTransform[r, c].Should().Be(Transform3D.Identity[r, c]);
        node.Visible.Should().BeTrue();
        node.Parent.Should().BeNull();
        node.Children.Should().BeEmpty();
    }

    /// <summary>Verifies that a SceneNode stores the provided name.</summary>
    [Fact]
    public void Name_SetCorrectly()
    {
        var node = new SceneNode("TestNode");

        node.Name.Should().Be("TestNode");
    }

    /// <summary>Verifies that LocalTransform can be changed.</summary>
    [Fact]
    public void LocalTransform_CanBeSet()
    {
        var node = new SceneNode();
        var t = Transform3D.Translation(1, 2, 3);

        node.LocalTransform = t;

        node.LocalTransform.Should().Be(t);
    }

    /// <summary>Verifies that Visible defaults to true and can be toggled.</summary>
    [Fact]
    public void Visible_CanBeToggled()
    {
        var node = new SceneNode();

        node.Visible.Should().BeTrue();
        node.Visible = false;
        node.Visible.Should().BeFalse();
    }

    /// <summary>Verifies that Parent is null for a root node.</summary>
    [Fact]
    public void Parent_NullForRoot()
    {
        var node = new SceneNode();

        node.Parent.Should().BeNull();
    }

    /// <summary>Verifies that AddChild sets the parent and adds to children.</summary>
    [Fact]
    public void AddChild_SetsParentAndAddsChild()
    {
        var parent = new SceneNode("Parent");
        var child = new SceneNode("Child");

        parent.AddChild(child);

        parent.Children.Should().ContainSingle();
        child.Parent.Should().Be(parent);
    }

    /// <summary>Verifies that RemoveChild removes the child and clears parent.</summary>
    [Fact]
    public void RemoveChild_RemovesChildAndClearsParent()
    {
        var parent = new SceneNode("Parent");
        var child = new SceneNode("Child");
        parent.AddChild(child);

        bool removed = parent.RemoveChild(child);

        removed.Should().BeTrue();
        parent.Children.Should().BeEmpty();
        child.Parent.Should().BeNull();
    }

    /// <summary>Verifies that RemoveChild returns false for a non-existent child.</summary>
    [Fact]
    public void RemoveChild_NonExistentChild_ReturnsFalse()
    {
        var parent = new SceneNode("Parent");
        var child = new SceneNode("Child");

        bool removed = parent.RemoveChild(child);

        removed.Should().BeFalse();
    }

    /// <summary>Verifies ClearChildren removes all children and clears their parents.</summary>
    [Fact]
    public void ClearChildren_RemovesAllChildren()
    {
        var parent = new SceneNode("Parent");
        var child1 = new SceneNode("C1");
        var child2 = new SceneNode("C2");
        parent.AddChild(child1);
        parent.AddChild(child2);

        parent.ClearChildren();

        parent.Children.Should().BeEmpty();
        child1.Parent.Should().BeNull();
        child2.Parent.Should().BeNull();
    }

    /// <summary>Verifies Traverse performs depth-first traversal including self.</summary>
    [Fact]
    public void Traverse_DepthFirst_IncludesAllNodes()
    {
        var root = new SceneNode("Root");
        var child = new SceneNode("Child");
        var grandchild = new SceneNode("Grandchild");
        root.AddChild(child);
        child.AddChild(grandchild);

        var nodes = root.Traverse().ToList();

        nodes.Should().HaveCount(3);
        nodes[0].Name.Should().Be("Root");
        nodes[1].Name.Should().Be("Child");
        nodes[2].Name.Should().Be("Grandchild");
    }

    /// <summary>Verifies WorldTransform for root node equals LocalTransform.</summary>
    [Fact]
    public void WorldTransform_Root_EqualsLocal()
    {
        var node = new SceneNode();
        var t = Transform3D.Translation(1, 2, 3);
        node.LocalTransform = t;

        node.WorldTransform.Should().Be(t);
    }

    /// <summary>Verifies WorldTransform accumulates parent transforms.</summary>
    [Fact]
    public void WorldTransform_WithParent_AccumulatesTransforms()
    {
        var parent = new SceneNode("Parent");
        parent.LocalTransform = Transform3D.Translation(1, 0, 0);
        var child = new SceneNode("Child");
        child.LocalTransform = Transform3D.Translation(0, 1, 0);

        parent.AddChild(child);

        child.WorldTransform[0, 3].Should().BeApproximately(1.0, Tolerance);
        child.WorldTransform[1, 3].Should().BeApproximately(1.0, Tolerance);
    }

    /// <summary>Verifies nested transforms accumulate through multiple levels.</summary>
    [Fact]
    public void NestedTransforms_AccumulateThroughLevels()
    {
        var root = new SceneNode("Root");
        root.LocalTransform = Transform3D.Translation(1, 0, 0);
        var mid = new SceneNode("Mid");
        mid.LocalTransform = Transform3D.Translation(0, 2, 0);
        var leaf = new SceneNode("Leaf");
        leaf.LocalTransform = Transform3D.Translation(0, 0, 3);

        root.AddChild(mid);
        mid.AddChild(leaf);

        leaf.WorldTransform[0, 3].Should().BeApproximately(1.0, Tolerance);
        leaf.WorldTransform[1, 3].Should().BeApproximately(2.0, Tolerance);
        leaf.WorldTransform[2, 3].Should().BeApproximately(3.0, Tolerance);
    }

    /// <summary>Verifies that AddChild reparents a child from another parent.</summary>
    [Fact]
    public void AddChild_ReparentsChildFromOldParent()
    {
        var oldParent = new SceneNode("Old");
        var newParent = new SceneNode("New");
        var child = new SceneNode("Child");
        oldParent.AddChild(child);

        newParent.AddChild(child);

        oldParent.Children.Should().BeEmpty();
        newParent.Children.Should().ContainSingle();
        child.Parent.Should().Be(newParent);
    }

    /// <summary>Verifies Traverse on a leaf node returns only itself.</summary>
    [Fact]
    public void Traverse_LeafNode_ReturnsOnlySelf()
    {
        var leaf = new SceneNode("Leaf");

        var nodes = leaf.Traverse().ToList();

        nodes.Should().HaveCount(1);
        nodes[0].Name.Should().Be("Leaf");
    }

    /// <summary>Verifies Children property is read-only.</summary>
    [Fact]
    public void Children_IsReadOnly()
    {
        IReadOnlyList<SceneNode> children = new SceneNode("Parent").Children;

        children.Should().BeEmpty();
    }
}
