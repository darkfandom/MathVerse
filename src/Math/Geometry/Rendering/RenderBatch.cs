namespace MathVerse.Math.Geometry.Rendering;

using System.Collections.Immutable;
using Meshes;

/// <summary>Batches render commands by material for efficient draw calls.</summary>
public sealed class RenderBatch
{
    private readonly Dictionary<string, List<RenderCommand>> _commandsByMaterial = [];
    
    /// <summary>Adds a render command.</summary>
    public void AddCommand(RenderCommand command)
    {
        if (!_commandsByMaterial.TryGetValue(command.MaterialName, out var list))
        {
            list = [];
            _commandsByMaterial[command.MaterialName] = list;
        }
        list.Add(command);
    }
    
    /// <summary>Gets commands grouped by material.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<RenderCommand>> GetGroupedCommands()
    {
        var result = new Dictionary<string, IReadOnlyList<RenderCommand>>();
        foreach (var kvp in _commandsByMaterial)
            result[kvp.Key] = kvp.Value.ToArray();
        return result;
    }
    
    /// <summary>Total command count.</summary>
    public int CommandCount => _commandsByMaterial.Values.Sum(l => l.Count);
    
    /// <summary>Number of unique materials.</summary>
    public int MaterialCount => _commandsByMaterial.Count;
    
    /// <summary>Clears all commands.</summary>
    public void Clear() => _commandsByMaterial.Clear();
}
