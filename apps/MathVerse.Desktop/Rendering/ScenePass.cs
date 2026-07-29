using PixelBuffer = MathVerse.Math.Visualization.Export.PixelBuffer;

namespace MathVerse.Desktop.Rendering;

public sealed class ScenePass : IRenderPass
{
    public string Name => "ScenePass";
    public int Order => 1;

    public void Execute(PixelBuffer buffer, in RenderContext context)
    {
        // Scene rendering — deferred to Phase 4.3+
    }
}
