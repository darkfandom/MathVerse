using MathVerse.Desktop.Core;
using MathVerse.Desktop.Services;

namespace MathVerse.Desktop.Commands;

public sealed class SelectTool : ITool
{
    public string Name => "SelectTool";
    public string Cursor => "Arrow";

    private float _startX, _startY;
    private bool _isDragging;

    public void Activate() { }
    public void Deactivate() => _isDragging = false;
    public void DrawOverlay() { }

    public bool OnMouseDown(float x, float y, int button)
    {
        _startX = x;
        _startY = y;
        _isDragging = true;
        return true;
    }

    public bool OnMouseMove(float x, float y)
    {
        if (!_isDragging) return false;
        return global::System.Math.Abs(x - _startX) > 0.01f || global::System.Math.Abs(y - _startY) > 0.01f;
    }

    public bool OnMouseUp(float x, float y, int button)
    {
        if (!_isDragging) return false;
        _isDragging = false;

        var dx = global::System.Math.Abs(x - _startX);
        var dy = global::System.Math.Abs(y - _startY);

        if (dx < 0.02f && dy < 0.02f)
        {
            AppServices.SelectionManager.DeselectAll();
        }
        else
        {
            AppServices.SelectionManager.DeselectAll();
        }

        return true;
    }

    public bool OnWheel(float delta) => false;
    public bool OnKeyDown(string key) => false;
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
        {
            var cam = AppServices.ViewportRenderer.Camera;
            cam.Pan(dx * 5f, dy * 5f);
            AppServices.ViewportRenderer.Invalidate();
        }
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
        var cam = AppServices.ViewportRenderer.Camera;
        cam.Zoom(delta * -0.5f);
        AppServices.ViewportRenderer.Invalidate();
        return true;
    }
    public bool OnKeyDown(string key) => false;
}
