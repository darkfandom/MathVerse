using PixelBuffer = MathVerse.Math.Visualization.Export.PixelBuffer;

namespace MathVerse.Desktop.Rendering;

public sealed class SelectionPass : IRenderPass
{
    public string Name => "SelectionPass";
    public int Order => 2;

    public void Execute(PixelBuffer buffer, in RenderContext context)
    {
        // Selection highlights — deferred to Phase 4.3+
    }
}
