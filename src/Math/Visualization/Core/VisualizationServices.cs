namespace MathVerse.Math.Visualization.Core;

/// <summary>Service locator for visualization subsystems.</summary>
public sealed class VisualizationServices
{
    private readonly Lazy<VisualizationRegistry> _registry;

    public VisualizationServices(VisualizationConfiguration? config = null)
    {
        _registry = new Lazy<VisualizationRegistry>(() => VisualizationRegistry.CreateDefault());
    }

    public VisualizationRegistry Registry => _registry.Value;
}
