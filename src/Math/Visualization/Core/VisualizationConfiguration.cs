namespace MathVerse.Math.Visualization.Core;
using System.Collections.Immutable;

/// <summary>Full visualization system configuration.</summary>
public sealed class VisualizationConfiguration
{
    public VisualizationOptions Options { get; init; } = new();
    public ImmutableDictionary<string, string> DefaultMaterials { get; init; } = ImmutableDictionary<string, string>.Empty;
    public int MaxSceneNodes { get; init; } = 100000;
    public bool EnableIncrementalRendering { get; init; } = true;
    public static VisualizationConfiguration Default => new();
}
