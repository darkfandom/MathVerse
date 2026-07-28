using MathVerse.Desktop.Core;

namespace MathVerse.Desktop.Commands;

public sealed class SelectTool : ITool
{
    public string Name => "SelectTool";
    public string Cursor => "Arrow";

    public void Activate() { }
    public void Deactivate() { }
    public void DrawOverlay() { }

    public bool OnMouseDown(float x, float y, int button) => false;
    public bool OnMouseMove(float x, float y) => false;
    public bool OnMouseUp(float x, float y, int button) => false;
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
        return dx != 0 || dy != 0;
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
    public bool OnWheel(float delta) => true;
    public bool OnKeyDown(string key) => false;
}
