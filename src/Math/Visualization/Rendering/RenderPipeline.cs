namespace MathVerse.Math.Visualization.Rendering;
using System.Diagnostics;

/// <summary>Multi-pass rendering pipeline that executes render passes in order and collects diagnostics.</summary>
public sealed class RenderingPipeline
{
    private readonly List<IRenderPass> _passes = [];
    private readonly Core.VisualizationOptions _options;
    private readonly Diagnostics.RenderingDiagnostics _diagnostics = new();

    /// <summary>Gets the diagnostics collector for this pipeline.</summary>
    public Diagnostics.RenderingDiagnostics Diagnostics => _diagnostics;

    /// <summary>Gets the number of registered render passes.</summary>
    public int PassCount => _passes.Count;

    /// <summary>Initializes a new instance of the <see cref="RenderingPipeline"/> class with the specified visualization options.</summary>
    /// <param name="options">The visualization options controlling render quality and features.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
    public RenderingPipeline(Core.VisualizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <summary>Adds a render pass to the pipeline.</summary>
    /// <param name="pass">The render pass to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pass"/> is <c>null</c>.</exception>
    public void AddPass(IRenderPass pass)
    {
        ArgumentNullException.ThrowIfNull(pass);
        _passes.Add(pass);
    }

    /// <summary>Removes a render pass from the pipeline by name.</summary>
    /// <param name="passName">The name of the pass to remove.</param>
    /// <returns><c>true</c> if the pass was found and removed; otherwise <c>false</c>.</returns>
    public bool RemovePass(string passName)
    {
        int index = _passes.FindIndex(p => p.Name == passName);
        if (index < 0)
            return false;

        _passes.RemoveAt(index);
        return true;
    }

    /// <summary>Executes all registered render passes in order against the given scene, returning combined results.</summary>
    /// <param name="scene">The scene graph to render.</param>
    /// <returns>A <see cref="RenderFrameResult"/> containing aggregated statistics from all passes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="scene"/> is <c>null</c>.</exception>
    public RenderFrameResult Execute(SceneGraph scene)
    {
        ArgumentNullException.ThrowIfNull(scene);

        List<IRenderPass> sorted = [.. _passes];
        sorted.Sort(static (a, b) => a.Order.CompareTo(b.Order));

        int totalDrawCalls = 0;
        int totalTriangles = 0;
        int totalVertices = 0;
        Stopwatch stopwatch = Stopwatch.StartNew();

        RenderPassContext context = new()
        {
            Scene = scene,
            Options = _options
        };

        for (int i = 0; i < sorted.Count; i++)
        {
            context.Commands.Clear();
            sorted[i].Execute(context);
            totalDrawCalls += context.Commands.Count;

            for (int j = 0; j < context.Commands.Count; j++)
            {
                RenderCommand cmd = context.Commands[j];
                totalVertices += cmd.VertexCount;
                totalTriangles += EstimateTriangleCount(cmd);
            }
        }

        stopwatch.Stop();
        _diagnostics.RecordFrame(stopwatch.Elapsed.TotalMilliseconds, totalDrawCalls, totalTriangles);

        return new RenderFrameResult
        {
            DrawCalls = totalDrawCalls,
            TrianglesRendered = totalTriangles,
            VerticesProcessed = totalVertices,
            RenderTimeMs = stopwatch.Elapsed.TotalMilliseconds,
            PassCount = sorted.Count
        };
    }

    /// <summary>Removes all registered render passes from the pipeline.</summary>
    public void ClearPasses() => _passes.Clear();

    private static int EstimateTriangleCount(RenderCommand command) => command.PrimitiveType switch
    {
        RenderPrimitiveType.Triangles => command.IndexCount > 0 ? command.IndexCount / 3 : command.VertexCount / 3,
        RenderPrimitiveType.TriangleStrip => (command.IndexCount > 0 ? command.IndexCount : command.VertexCount) - 2,
        RenderPrimitiveType.TriangleFan => (command.IndexCount > 0 ? command.IndexCount : command.VertexCount) - 2,
        RenderPrimitiveType.Quads => (command.IndexCount > 0 ? command.IndexCount : command.VertexCount) / 4 * 2,
        _ => 0
    };
}
