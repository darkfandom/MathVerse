namespace MathVerse.Math.Visualization.Rendering;

/// <summary>Sorts and batches render commands for efficient GPU submission, separating opaque and transparent geometry.</summary>
public sealed class RenderQueue
{
    private readonly List<RenderCommand> _opaqueQueue = [];
    private readonly List<RenderCommand> _transparentQueue = [];
    private readonly Dictionary<string, Material> _materials = [];

    /// <summary>Gets the sorted list of opaque render commands, ready for submission.</summary>
    public IReadOnlyList<RenderCommand> OpaqueCommands => _opaqueQueue;

    /// <summary>Gets the sorted list of transparent render commands, ready for submission in reverse order.</summary>
    public IReadOnlyList<RenderCommand> TransparentCommands => _transparentQueue;

    /// <summary>Gets the total number of commands in both queues.</summary>
    public int TotalCount => _opaqueQueue.Count + _transparentQueue.Count;

    /// <summary>Adds a render command to the appropriate queue based on its material properties.</summary>
    /// <param name="command">The render command to enqueue.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is <c>null</c>.</exception>
    public void Enqueue(RenderCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (TryGetTransparentMaterial(command.MaterialId))
        {
            _transparentQueue.Add(command);
        }
        else
        {
            _opaqueQueue.Add(command);
        }
    }

    /// <summary>Registers a material for transparency lookups during enqueue operations.</summary>
    /// <param name="material">The material to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="material"/> is <c>null</c>.</exception>
    public void RegisterMaterial(Material material)
    {
        ArgumentNullException.ThrowIfNull(material);
        _materials[material.MaterialId] = material;
    }

    /// <summary>Sorts both queues for optimal rendering order. Opaque commands are sorted front-to-back; transparent commands are sorted back-to-front.</summary>
    public void Sort()
    {
        _opaqueQueue.Sort(static (a, b) =>
        {
            int materialCompare = string.Compare(a.MaterialId, b.MaterialId, StringComparison.Ordinal);
            if (materialCompare != 0)
                return materialCompare;
            return a.SortKey.CompareTo(b.SortKey);
        });

        _transparentQueue.Sort(static (a, b) =>
        {
            int materialCompare = string.Compare(a.MaterialId, b.MaterialId, StringComparison.Ordinal);
            if (materialCompare != 0)
                return materialCompare;
            return b.SortKey.CompareTo(a.SortKey);
        });
    }

    /// <summary>Clears all commands from both queues.</summary>
    public void Clear()
    {
        _opaqueQueue.Clear();
        _transparentQueue.Clear();
    }

    private bool TryGetTransparentMaterial(string materialId)
    {
        if (_materials.TryGetValue(materialId, out Material? mat))
        {
            return mat.BlendMode != MaterialBlendMode.Opaque;
        }
        return false;
    }
}
