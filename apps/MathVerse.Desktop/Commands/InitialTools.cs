using System.Numerics;
using MathVerse.Desktop.Core;
using MathVerse.Desktop.Models;
using MathVerse.Desktop.Rendering;
using MathVerse.Desktop.Services;

namespace MathVerse.Desktop.Commands;

public sealed class SelectTool : ITool
{
    public string Name => "SelectTool";
    public string Cursor => "Arrow";

    private float _startX, _startY;
    private float _currentX, _currentY;
    private bool _isDragging;
    private bool _isCtrlDown;

    private const float ClickThreshold = 0.02f;
    private const float HitDistanceWorld = 0.3f;

    public void Activate() { }
    public void Deactivate()
    {
        _isDragging = false;
        AppServices.ViewportRenderer.ClearSelectionBox();
    }
    public void DrawOverlay() { }

    public bool OnMouseDown(float x, float y, int button)
    {
        if (button != 0) return false;
        _startX = x;
        _startY = y;
        _currentX = x;
        _currentY = y;
        _isDragging = true;
        return true;
    }

    public bool OnMouseMove(float x, float y)
    {
        _currentX = x;
        _currentY = y;

        // Update hover in viewport
        var world = AppServices.ViewportRenderer.ScreenToWorld(new Vector2(x * 2f - 1f, 1f - y * 2f));
        var hit = HitTest(world.X, world.Y);
        if (hit.HasValue)
            AppServices.SelectionService.SetHovered(hit.Value);
        else
            AppServices.SelectionService.ClearHovered();

        if (!_isDragging) return false;

        // Update box selection visual
        var dx = global::System.Math.Abs(x - _startX);
        var dy = global::System.Math.Abs(y - _startY);
        if (dx > ClickThreshold || dy > ClickThreshold)
        {
            float sx = global::System.Math.Min(_startX, x);
            float sy = global::System.Math.Min(_startY, y);
            float sw = global::System.Math.Abs(x - _startX);
            float sh = global::System.Math.Abs(y - _startY);
            AppServices.ViewportRenderer.SetSelectionBox(sx, sy, sw, sh);
            return true;
        }
        return false;
    }

    public bool OnMouseUp(float x, float y, int button)
    {
        if (!_isDragging) return false;
        _isDragging = false;

        var dx = global::System.Math.Abs(x - _startX);
        var dy = global::System.Math.Abs(y - _startY);

        AppServices.ViewportRenderer.ClearSelectionBox();

        if (dx < ClickThreshold && dy < ClickThreshold)
        {
            // Single click - hit test
            var world = AppServices.ViewportRenderer.ScreenToWorld(new Vector2(x * 2f - 1f, 1f - y * 2f));
            var hit = HitTest(world.X, world.Y);
            if (hit.HasValue)
            {
                if (_isCtrlDown)
                    AppServices.CommandManager.Execute("ToggleSelectObject",
                        new Dictionary<string, object> { ["ObjectId"] = hit.Value });
                else
                    AppServices.CommandManager.Execute("SelectObject",
                        new Dictionary<string, object> { ["ObjectId"] = hit.Value });
            }
            else
            {
                AppServices.CommandManager.Execute("ClearSelection");
            }
        }
        else
        {
            // Box select - hit test rectangle
            float wx1, wy1, wx2, wy2;
            ScreenToWorld(_startX, _startY, out wx1, out wy1);
            ScreenToWorld(x, y, out wx2, out wy2);

            float minX = global::System.Math.Min(wx1, wx2);
            float maxX = global::System.Math.Max(wx1, wx2);
            float minY = global::System.Math.Min(wy1, wy2);
            float maxY = global::System.Math.Max(wy1, wy2);

            var hitIds = BoxHitTest(minX, maxX, minY, maxY);
            if (hitIds.Count > 0)
            {
                AppServices.CommandManager.Execute("BoxSelect",
                    new Dictionary<string, object> { ["ObjectIds"] = hitIds });
            }
            else
            {
                AppServices.CommandManager.Execute("ClearSelection");
            }
        }

        return true;
    }

    public bool OnWheel(float delta) => false;

    public bool OnKeyDown(string key)
    {
        _isCtrlDown = key == "Control";
        return false;
    }

    private static Guid? HitTest(float worldX, float worldY)
    {
        Guid? closestId = null;
        float closestDist = HitDistanceWorld;

        foreach (var node in AppServices.SceneGraph.GetOrderedNodes())
        {
            if (!node.IsVisible) continue;
            foreach (var ro in node.RenderObjects)
            {
                if (!ro.IsVisible || ro.IsHidden) continue;
                var dist = ro.HitTest(worldX, worldY);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestId = ro.SourceObjectId;
                }
            }
        }
        return closestId;
    }

    private static List<Guid> BoxHitTest(float minX, float maxX, float minY, float maxY)
    {
        var ids = new List<Guid>();
        foreach (var node in AppServices.SceneGraph.GetOrderedNodes())
        {
            if (!node.IsVisible) continue;
            foreach (var ro in node.RenderObjects)
            {
                if (!ro.IsVisible || ro.IsHidden) continue;
                if (ro.IntersectsBox(minX, maxX, minY, maxY) && !ids.Contains(ro.SourceObjectId))
                    ids.Add(ro.SourceObjectId);
            }
        }
        return ids;
    }

    private static void ScreenToWorld(float nx, float ny, out float wx, out float wy)
    {
        var vp = AppServices.ViewportRenderer.Camera.ViewProjectionMatrix;
        float ndcX = nx * 2f - 1f;
        float ndcY = 1f - ny * 2f;
        if (System.Numerics.Matrix4x4.Invert(vp, out var inv))
        {
            var clip = System.Numerics.Vector4.Transform(new System.Numerics.Vector4(ndcX, ndcY, 0, 1), inv);
            if (global::System.Math.Abs(clip.W) > 0.0001f)
            {
                wx = clip.X / clip.W;
                wy = clip.Y / clip.W;
                return;
            }
        }
        wx = 0; wy = 0;
    }
}

public sealed class PanTool : ITool
{
    public string Name => "PanTool";
    public string Cursor => "Hand";

    private float _lastX;
    private float _lastY;
    private bool _isDragging;

    public void Activate() { }
    public void Deactivate() => _isDragging = false;
    public void DrawOverlay() { }

    public bool OnMouseDown(float x, float y, int button)
    {
        _lastX = x;
        _lastY = y;
        _isDragging = true;
        return true;
    }

    public bool OnMouseMove(float x, float y)
    {
        if (!_isDragging) return false;
        var dx = x - _lastX;
        var dy = y - _lastY;
        _lastX = x;
        _lastY = y;
        if (dx != 0 || dy != 0)
            AppServices.ViewportRenderer.Pan(dx, dy);
        return true;
    }

    public bool OnMouseUp(float x, float y, int button)
    {
        _isDragging = false;
        return true;
    }

    public bool OnWheel(float delta) => false;
    public bool OnKeyDown(string key) => false;
}

public sealed class ZoomTool : ITool
{
    public string Name => "ZoomTool";
    public string Cursor => "SizeAll";

    public void Activate() { }
    public void Deactivate() { }
    public void DrawOverlay() { }

    public bool OnMouseDown(float x, float y, int button) => false;
    public bool OnMouseMove(float x, float y) => false;
    public bool OnMouseUp(float x, float y, int button) => false;
    public bool OnWheel(float delta)
    {
        AppServices.ViewportRenderer.ZoomOnCursor(delta, 0.5f, 0.5f);
        return true;
    }
    public bool OnKeyDown(string key) => false;
}
