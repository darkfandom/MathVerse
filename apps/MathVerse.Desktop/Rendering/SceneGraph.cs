using System.Numerics;
using MathVerse.Desktop.Models;

namespace MathVerse.Desktop.Rendering;

public sealed class SceneNode
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid SourceObjectId { get; }
    public string Name { get; set; } = "";
    public bool IsVisible { get; set; } = true;
    public bool IsLocked { get; set; }
    public int Layer { get; set; }
    public Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;
    public IRenderObject[] RenderObjects { get; set; } = [];
    public DirtyFlag Dirty { get; set; } = DirtyFlag.GeometryDirty;

    public SceneNode(Guid sourceObjectId)
    {
        SourceObjectId = sourceObjectId;
    }
}

public sealed class SceneGraph
{
    private readonly Dictionary<Guid, SceneNode> _nodes = [];
    private readonly List<SceneNode> _sortedNodes = [];
    private bool _orderDirty = true;

    public int NodeCount => _nodes.Count;
    public int TotalRenderObjectCount { get; private set; }
    public int VisibleRenderObjectCount { get; private set; }
    public int DirtyNodeCount { get; private set; }

    public SceneNode AddOrUpdate(Guid sourceObjectId, string name, int layer)
    {
        if (_nodes.TryGetValue(sourceObjectId, out var existing))
        {
            existing.Name = name;
            existing.Layer = layer;
            return existing;
        }
        var node = new SceneNode(sourceObjectId) { Name = name, Layer = layer };
        _nodes[sourceObjectId] = node;
        _orderDirty = true;
        return node;
    }

    public bool Remove(Guid sourceObjectId)
    {
        if (_nodes.Remove(sourceObjectId))
        {
            _orderDirty = true;
            return true;
        }
        return false;
    }

    public SceneNode? Get(Guid sourceObjectId) =>
        _nodes.TryGetValue(sourceObjectId, out var node) ? node : null;

    public void SetDirty(Guid sourceObjectId, DirtyFlag flag)
    {
        if (_nodes.TryGetValue(sourceObjectId, out var node))
            node.Dirty |= flag;
    }

    public void Clear()
    {
        _nodes.Clear();
        _sortedNodes.Clear();
        _orderDirty = true;
    }

    public void UpdateMetrics()
    {
        int total = 0, visible = 0, dirty = 0;
        foreach (var node in _nodes.Values)
        {
            total += node.RenderObjects.Length;
            visible += node.RenderObjects.Count(r => r.IsVisible && !r.IsHidden);
            if (node.Dirty != DirtyFlag.None) dirty++;
        }
        TotalRenderObjectCount = total;
        VisibleRenderObjectCount = visible;
        DirtyNodeCount = dirty;
    }

    public IReadOnlyList<SceneNode> GetOrderedNodes()
    {
        if (_orderDirty)
        {
            _sortedNodes.Clear();
            _sortedNodes.AddRange(_nodes.Values);
            _sortedNodes.Sort((a, b) =>
            {
                int layer = a.Layer.CompareTo(b.Layer);
                return layer != 0 ? layer : a.Id.CompareTo(b.Id);
            });
            _orderDirty = false;
        }
        return _sortedNodes;
    }
}
