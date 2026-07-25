namespace MathVerse.Geometry.Tests.SceneGraph;

/// <summary>Tests for the <see cref="TransformNode"/> class.</summary>
public class TransformNodeTests
{
    private const double Tolerance = 1e-10;

    /// <summary>Verifies TransformNode construction with default values.</summary>
    [Fact]
    public void Constructor_DefaultValues()
    {
        var node = new TransformNode();

        node.Name.Should().Be("");
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
                node.LocalTransform[r, c].Should().Be(Transform3D.Identity[r, c]);
    }

    /// <summary>Verifies TransformNode stores the provided name.</summary>
    [Fact]
    public void Constructor_StoresName()
    {
        var node = new TransformNode("Pivot");

        node.Name.Should().Be("Pivot");
    }

    /// <summary>Verifies TransformNode stores the provided transform.</summary>
    [Fact]
    public void Constructor_StoresTransform()
    {
        var t = Transform3D.Translation(1, 2, 3);
        var node = new TransformNode("T", t);

        node.LocalTransform.Should().Be(t);
    }

    /// <summary>Verifies TransformNode with null transform uses identity.</summary>
    [Fact]
    public void Constructor_NullTransform_UsesIdentity()
    {
        Transform3D? t = null;
        var node = new TransformNode("T", t);

        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
                node.LocalTransform[r, c].Should().Be(Transform3D.Identity[r, c]);
    }

    /// <summary>Verifies WorldTransform for a root TransformNode equals LocalTransform.</summary>
    [Fact]
    public void WorldTransform_Root_EqualsLocal()
    {
        var t = Transform3D.Translation(5, 5, 5);
        var node = new TransformNode("T", t);

        node.WorldTransform.Should().Be(t);
    }

    /// <summary>Verifies WorldTransform inherits from parent.</summary>
    [Fact]
    public void WorldTransform_WithParent_AccumulatesTransforms()
    {
        var parent = new TransformNode("Parent", Transform3D.Translation(1, 0, 0));
        var child = new TransformNode("Child", Transform3D.Translation(0, 2, 0));

        parent.AddChild(child);

        child.WorldTransform[0, 3].Should().BeApproximately(1.0, Tolerance);
        child.WorldTransform[1, 3].Should().BeApproximately(2.0, Tolerance);
    }

    /// <summary>Verifies TransformNode is a SceneNode (inheritance).</summary>
    [Fact]
    public void TransformNode_InheritsSceneNode()
    {
        var node = new TransformNode();

        node.Should().BeAssignableTo<SceneNode>();
    }

    /// <summary>Verifies TransformNode LocalTransform can be changed after construction.</summary>
    [Fact]
    public void LocalTransform_CanBeChanged()
    {
        var node = new TransformNode();
        var newTransform = Transform3D.Translation(10, 10, 10);

        node.LocalTransform = newTransform;

        node.LocalTransform.Should().Be(newTransform);
    }
}
