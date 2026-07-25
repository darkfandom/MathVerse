namespace MathVerse.Math.Geometry.SceneGraph;

using Geometry3D;
using Transformations;

/// <summary>Represents a node in a hierarchical scene graph.</summary>
public class SceneNode
{
    private readonly List<SceneNode> _children = [];

    /// <summary>Initializes a new scene node.</summary>
    /// <param name="name">The node name.</param>
    public SceneNode(string name = "")
    {
        Name = name;
        LocalTransform = Transform3D.Identity;
        Visible = true;
    }

    /// <summary>Node name.</summary>
    public string Name { get; set; }

    /// <summary>Local transform relative to parent.</summary>
    public Transform3D LocalTransform { get; set; }

    /// <summary>Whether this node is visible.</summary>
    public bool Visible { get; set; }

    /// <summary>Parent node (null if root).</summary>
    public SceneNode? Parent { get; private set; }

    /// <summary>Child nodes.</summary>
    public IReadOnlyList<SceneNode> Children => _children;

    /// <summary>World transform (accumulated from root).</summary>
    public Transform3D WorldTransform => Parent != null
        ? Parent.WorldTransform * LocalTransform
        : LocalTransform;

    /// <summary>Adds a child node.</summary>
    /// <param name="child">The child node to add.</param>
    public void AddChild(SceneNode child)
    {
        child.Parent?.RemoveChild(child);
        child.Parent = this;
        _children.Add(child);
    }

    /// <summary>Removes a child node.</summary>
    /// <param name="child">The child node to remove.</param>
    /// <returns>True if the child was found and removed; otherwise, false.</returns>
    public bool RemoveChild(SceneNode child)
    {
        if (_children.Remove(child))
        {
            child.Parent = null;
            return true;
        }
        return false;
    }

    /// <summary>Removes all children.</summary>
    public void ClearChildren()
    {
        foreach (SceneNode child in _children) child.Parent = null;
        _children.Clear();
    }

    /// <summary>Depth-first traversal yielding all descendant nodes.</summary>
    /// <returns>A sequence of all nodes in depth-first order.</returns>
    public IEnumerable<SceneNode> Traverse()
    {
        yield return this;
        foreach (SceneNode child in _children)
            foreach (SceneNode desc in child.Traverse())
                yield return desc;
    }
}
