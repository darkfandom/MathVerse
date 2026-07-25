namespace MathVerse.Math.Visualization.Rendering;

/// <summary>Interface for a rendering pass that executes within the multi-pass pipeline.</summary>
public interface IRenderPass
{
    /// <summary>Gets the display name of this render pass.</summary>
    string Name { get; }

    /// <summary>Gets the execution order of this pass. Lower values execute first.</summary>
    int Order { get; }

    /// <summary>Executes the render pass with the given context.</summary>
    /// <param name="context">The context containing scene data, camera, lights, and target for this pass.</param>
    void Execute(RenderPassContext context);
}

/// <summary>Provides context data for a single render pass execution, including the scene, camera, lights, and output target.</summary>
public sealed class RenderPassContext
{
    /// <summary>Gets or sets the scene graph to render.</summary>
    public SceneGraph Scene { get; init; } = new();

    /// <summary>Gets or sets the visualization options controlling render quality and features.</summary>
    public Core.VisualizationOptions Options { get; init; } = Core.VisualizationOptions.Default;

    /// <summary>Gets or sets the camera used for viewing the scene.</summary>
    public Camera Camera { get; init; } = new();

    /// <summary>Gets or sets the list of active lights in the scene.</summary>
    public List<Light> Lights { get; init; } = [];

    /// <summary>Gets or sets the render target, or <c>null</c> for default framebuffer.</summary>
    public RenderTarget? Target { get; init; }

    /// <summary>Gets the list of render commands generated during this pass.</summary>
    public List<RenderCommand> Commands { get; } = [];
}
