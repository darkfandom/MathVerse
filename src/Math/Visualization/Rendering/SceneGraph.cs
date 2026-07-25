namespace MathVerse.Math.Visualization.Rendering;
using System.Numerics;

/// <summary>Hierarchical scene graph for organizing visualization objects.</summary>
public sealed class SceneGraph
{
    private readonly SceneNode _root;
    private int _nextId;
    private int _count;

    /// <summary>Gets the root node of the scene graph.</summary>
    public SceneNode Root => _root;

    /// <summary>Gets the total number of nodes in the scene graph.</summary>
    public int Count => _count;

    /// <summary>Initializes a new instance of the <see cref="SceneGraph"/> class with a default root node.</summary>
    public SceneGraph()
    {
        _root = new SceneNode(CreateNodeId(), "Root");
        _count = 1;
    }

    /// <summary>Adds a child node under the specified parent with an associated visualization object.</summary>
    /// <param name="parent">The parent node to attach the child to.</param>
    /// <param name="obj">The visualization object to associate with the new node.</param>
    /// <returns>The newly created child node.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="parent"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the parent node does not belong to this scene graph.</exception>
    public SceneNode AddChild(SceneNode parent, Core.VisualizationObject obj)
    {
        ArgumentNullException.ThrowIfNull(parent);
        if (!ContainsNode(_root, parent))
            throw new InvalidOperationException("Parent node does not belong to this scene graph.");

        string nodeId = CreateNodeId();
        SceneNode child = new(nodeId, obj.GetType().Name) { Object = obj };
        child.Parent = parent;
        parent.Children.Add(child);
        _count++;
        return child;
    }

    /// <summary>Removes a node and all of its descendants from the scene graph by identifier.</summary>
    /// <param name="nodeId">The unique identifier of the node to remove.</param>
    /// <returns><c>true</c> if the node was found and removed; otherwise <c>false</c>.</returns>
    public bool RemoveNode(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId))
            return false;

        SceneNode? target = FindNode(_root, nodeId);
        if (target is null || target == _root)
            return false;

        int removed = RemoveFromParent(target);
        _count -= removed;
        return true;
    }

    /// <summary>Traverses the entire scene graph depth-first, invoking the visitor for each node with its accumulated world transform.</summary>
    /// <param name="visitor">The action to invoke for each node, receiving the node and its world transform.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="visitor"/> is <c>null</c>.</exception>
    public void Traverse(Action<SceneNode, Matrix4x4> visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        TraverseNode(_root, Matrix4x4.Identity, visitor);
    }

    /// <summary>Finds a node by its unique identifier.</summary>
    /// <param name="nodeId">The unique identifier to search for.</param>
    /// <returns>The matching <see cref="SceneNode"/>, or <c>null</c> if not found.</returns>
    public SceneNode? Find(string nodeId) => FindNode(_root, nodeId);

    private void TraverseNode(SceneNode node, Matrix4x4 parentTransform, Action<SceneNode, Matrix4x4> visitor)
    {
        node.UpdateWorldTransform(parentTransform);
        visitor(node, node.WorldTransform);

        for (int i = 0; i < node.Children.Count; i++)
        {
            TraverseNode(node.Children[i], node.WorldTransform, visitor);
        }
    }

    private string CreateNodeId() => $"node_{_nextId++}";

    private static SceneNode? FindNode(SceneNode current, string nodeId)
    {
        if (current.NodeId == nodeId)
            return current;

        for (int i = 0; i < current.Children.Count; i++)
        {
            SceneNode? found = FindNode(current.Children[i], nodeId);
            if (found is not null)
                return found;
        }

        return null;
    }

    private static bool ContainsNode(SceneNode current, SceneNode target)
    {
        if (current == target)
            return true;

        for (int i = 0; i < current.Children.Count; i++)
        {
            if (ContainsNode(current.Children[i], target))
                return true;
        }

        return false;
    }

    private static int RemoveFromParent(SceneNode node)
    {
        int removed = 1;
        node.Parent?.Children.Remove(node);

        for (int i = 0; i < node.Children.Count; i++)
        {
            removed += RemoveFromParent(node.Children[i]);
        }

        node.Parent = null;
        node.Children.Clear();
        return removed;
    }
}

/// <summary>Represents a single node in the scene graph hierarchy, containing a transform and optional visualization object.</summary>
public sealed class SceneNode
{
    /// <summary>Gets the unique identifier of this node.</summary>
    public string NodeId { get; }

    /// <summary>Gets or sets the human-readable name of this node.</summary>
    public string Name { get; init; }

    /// <summary>Gets or sets the local transform of this node relative to its parent.</summary>
    public Matrix4x4 LocalTransform { get; set; } = Matrix4x4.Identity;

    /// <summary>Gets or sets the visualization object associated with this node, or <c>null</c> if the node is empty.</summary>
    public Core.VisualizationObject? Object { get; set; }

    /// <summary>Gets or sets the parent of this node. Set internally by the scene graph.</summary>
    public SceneNode? Parent { get; internal set; }

    /// <summary>Gets the list of child nodes.</summary>
    public List<SceneNode> Children { get; } = [];

    /// <summary>Gets or sets a value indicating whether this node and its children should be rendered.</summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>Gets the accumulated world transform of this node, computed during traversal.</summary>
    public Matrix4x4 WorldTransform { get; private set; } = Matrix4x4.Identity;

    /// <summary>Initializes a new instance of the <see cref="SceneNode"/> class with the specified identifier and name.</summary>
    /// <param name="nodeId">The unique identifier for this node.</param>
    /// <param name="name">The display name for this node.</param>
    public SceneNode(string nodeId, string name = "")
    {
        NodeId = nodeId;
        Name = name;
    }

    /// <summary>Initializes a new instance of the <see cref="SceneNode"/> class with the specified identifier, name, and visualization object.</summary>
    /// <param name="nodeId">The unique identifier for this node.</param>
    /// <param name="name">The display name for this node.</param>
    /// <param name="obj">The visualization object to associate with this node.</param>
    public SceneNode(string nodeId, string name, Core.VisualizationObject obj)
    {
        NodeId = nodeId;
        Name = name;
        Object = obj;
    }

    /// <summary>Updates the world transform by combining the local transform with the parent's world transform.</summary>
    /// <param name="parentWorld">The parent node's world transform.</param>
    internal void UpdateWorldTransform(Matrix4x4 parentWorld)
    {
        WorldTransform = Matrix4x4.Multiply(LocalTransform, parentWorld);
    }
}
