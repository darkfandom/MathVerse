using Avalonia.Threading;
using PixelBuffer = MathVerse.Math.Visualization.Export.PixelBuffer;

namespace MathVerse.Desktop.Rendering;

public sealed class OverlayPass : IRenderPass
{
    public string Name => "OverlayPass";
    public int Order => 4;

    private readonly Action<OverlayData> _updateAction;
    private OverlayData _lastData;

    public readonly record struct OverlayData(
        string Coordinates,
        string ZoomLevel,
        string ActiveTool,
        string CameraPos,
        string Fps,
        string SelectionInfo,
        string StatusMessage);

    public OverlayPass(Action<OverlayData> updateAction)
    {
        _updateAction = updateAction;
    }

    public void Execute(PixelBuffer buffer, in RenderContext context)
    {
        var data = new OverlayData(
            Coordinates: $"({context.CursorWorldX:F2}, {context.CursorWorldY:F2})",
            ZoomLevel: $"{context.ZoomLevel * 100:F0}%",
            ActiveTool: context.ActiveToolName,
            CameraPos: $"({context.CameraPosition.X:F1}, {context.CameraPosition.Y:F1})",
            Fps: $"{(context.DeltaTime > 0 ? 1f / context.DeltaTime : 0):F0}",
            SelectionInfo: context.SelectionCount > 0 ? $"{context.SelectionCount} selected" : "",
            StatusMessage: context.StatusMessage);

        if (!data.Equals(_lastData))
        {
            _lastData = data;
            Dispatcher.UIThread.Post(() => _updateAction(data));
        }
    }
}
