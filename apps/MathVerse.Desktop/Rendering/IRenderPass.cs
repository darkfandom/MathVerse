using System.Numerics;
using PixelBuffer = MathVerse.Math.Visualization.Export.PixelBuffer;

namespace MathVerse.Desktop.Rendering;

public readonly record struct RenderContext(
    int Width,
    int Height,
    float AspectRatio,
    float DeltaTime,
    float TotalTime,
    Vector3 CameraPosition,
    Vector3 CameraTarget,
    Matrix4x4 ViewProjectionMatrix,
    float ZoomLevel,
    float CursorWorldX,
    float CursorWorldY,
    string ActiveToolName,
    int SelectionCount,
    string StatusMessage);

public interface IRenderPass
{
    string Name { get; }
    int Order { get; }
    void Execute(PixelBuffer buffer, in RenderContext context);
}
