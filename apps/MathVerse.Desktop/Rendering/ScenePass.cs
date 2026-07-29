using System.Numerics;
using MathVerse.Desktop.Models;
using MathVerse.Desktop.Services;
using PixelBuffer = MathVerse.Math.Visualization.Export.PixelBuffer;

namespace MathVerse.Desktop.Rendering;

public sealed class ScenePass : IRenderPass
{
    public string Name => "ScenePass";
    public int Order => 1;

    private readonly SceneGraph _sceneGraph;
    private readonly IRenderCompiler[] _compilers;
    private readonly Dictionary<string, IRenderCompiler> _compilerMap = [];

    private int _totalRenderObjects;
    private int _visibleRenderObjects;
    private int _dirtyNodeCount;
    private int _compiledThisFrame;

    public ScenePass() : this(AppServices.SceneGraph, AppServices.RenderCompilers) { }

    public ScenePass(SceneGraph sceneGraph, IEnumerable<IRenderCompiler> compilers)
    {
        _sceneGraph = sceneGraph;
        _compilers = compilers.ToArray();
        foreach (var c in _compilers)
            _compilerMap[c.TypeTag] = c;
    }

    public void Execute(PixelBuffer buffer, in RenderContext context)
    {
        CompileDirtyNodes();
        RenderScene(buffer, context);
        DrawDebugOverlay(buffer, context);
    }

    private void CompileDirtyNodes()
    {
        _compiledThisFrame = 0;
        foreach (var node in _sceneGraph.GetOrderedNodes())
        {
            if (node.Dirty == DirtyFlag.None) continue;

            var compiler = FindCompiler(node.SourceObjectId);
            if (compiler != null)
            {
                var obj = AppServices.Registry.GetById(node.SourceObjectId);
                if (obj != null)
                {
                    node.RenderObjects = compiler.Compile(obj);
                    _compiledThisFrame++;
                }
            }
            node.Dirty = DirtyFlag.None;
        }
        _sceneGraph.UpdateMetrics();
        _totalRenderObjects = _sceneGraph.TotalRenderObjectCount;
        _visibleRenderObjects = _sceneGraph.VisibleRenderObjectCount;
        _dirtyNodeCount = _sceneGraph.DirtyNodeCount;
    }

    private void RenderScene(PixelBuffer buffer, in RenderContext context)
    {
        foreach (var node in _sceneGraph.GetOrderedNodes())
        {
            if (!node.IsVisible) continue;

            foreach (var obj in node.RenderObjects)
            {
                if (!obj.IsVisible || obj.IsHidden) continue;

                // Apply selection highlight
                if (obj.IsSelected)
                {
                    var orig = obj switch
                    {
                        RenderLine l => l.Color,
                        RenderPolyline p => p.Color,
                        RenderRectangle r => r.StrokeColor,
                        RenderCircle c => c.StrokeColor,
                        _ => new Color4(255, 255, 255, 255)
                    };
                    // Dim the original, draw selection highlight via draw twice
                    // Keep original color but note selection in the object state
                }

                obj.Draw(buffer, context);
            }
        }
    }

    private void DrawDebugOverlay(PixelBuffer buffer, in RenderContext context)
    {
        string[] lines =
        [
            $"Objects: {_totalRenderObjects} total, {_visibleRenderObjects} visible",
            $"Nodes: {_sceneGraph.NodeCount}, Dirty: {_dirtyNodeCount}",
            $"Compiled this frame: {_compiledThisFrame}",
        ];

        int lineHeight = 14;
        int xStart = context.Width - 300;
        int yStart = 8;

        for (int i = 0; i < lines.Length; i++)
            DrawSimpleText(buffer, xStart, yStart + i * lineHeight, lines[i], new Color4(180, 180, 200, 200));

        string[] ctxLines =
        [
            $"Zoom: {context.ZoomLevel:F2}x",
            $"Camera: ({context.CameraPosition.X:F1}, {context.CameraPosition.Y:F1})",
            $"FPS: {(context.DeltaTime > 0 ? (1f / context.DeltaTime) : 0):F0}",
        ];

        int ctxY = yStart + (lines.Length + 1) * lineHeight;
        for (int i = 0; i < ctxLines.Length; i++)
            DrawSimpleText(buffer, xStart, ctxY + i * lineHeight, ctxLines[i], new Color4(140, 220, 140, 180));
    }

    private static void DrawSimpleText(PixelBuffer buffer, int x, int y, string text, Color4 color)
    {
        int cursorX = x + 2;
        int cursorY = y + 2;
        foreach (char c in text)
        {
            for (int dy = 0; dy < 8; dy++)
                for (int dx = 0; dx < 5; dx++)
                {
                    int px = cursorX + dx;
                    int py = cursorY + dy;
                    if (px >= 0 && px < buffer.Width && py >= 0 && py < buffer.Height)
                        buffer.SetPixel(px, py, color.R, color.G, color.B,
                            (byte)(dx == 0 || dx == 4 || dy == 0 || dy == 7 ? color.A : (byte)0));
                }
            cursorX += 7;
        }
    }

    private IRenderCompiler? FindCompiler(Guid sourceObjectId)
    {
        var obj = AppServices.Registry.GetById(sourceObjectId);
        return obj != null && _compilerMap.TryGetValue(obj.TypeTag, out var compiler) ? compiler : null;
    }
}
