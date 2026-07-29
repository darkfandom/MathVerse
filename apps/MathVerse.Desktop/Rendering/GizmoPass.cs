using PixelBuffer = MathVerse.Math.Visualization.Export.PixelBuffer;

namespace MathVerse.Desktop.Rendering;

public sealed class GizmoPass : IRenderPass
{
    public string Name => "GizmoPass";
    public int Order => 3;

    public void Execute(PixelBuffer buffer, in RenderContext context)
    {
        // Manipulator gizmos — deferred to later phase
    }
}
