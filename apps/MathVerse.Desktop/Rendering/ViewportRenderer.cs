using System.Numerics;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MathVerse.Math.Visualization.Rendering;
using PixelBuffer = MathVerse.Math.Visualization.Export.PixelBuffer;

namespace MathVerse.Desktop.Rendering;

public sealed class ViewportRenderer
{
    private readonly Camera _camera;
    private readonly List<IRenderPass> _passes = [];
    private WriteableBitmap? _bitmap;
    private int _width = 1;
    private int _height = 1;
    private float _aspectRatio = 16f / 9f;
    private bool _dirty = true;

    // Camera system
    private Vector3 _targetPosition;
    private Vector3 _targetTarget;
    private float _cameraDistance;
    private float _targetDistance;
    private bool _animating;

    // Timing
    private DateTime _lastFrameTime = DateTime.UtcNow;
    private float _deltaTime;
    private float _totalTime;

    // Cursor world position
    private float _cursorWorldX;
    private float _cursorWorldY;

    // Status
    private string _statusMessage = "";
    private string _activeToolName = "SelectTool";

    // Selection box (set by SelectTool during drag)
    public (float x, float y, float w, float h)? SelectionBox { get; private set; }

    public Camera Camera => _camera;
    public WriteableBitmap? Bitmap => _bitmap;
    public int Width => _width;
    public int Height => _height;
    public float ZoomLevel { get; private set; } = 1f;
    public float CursorWorldX => _cursorWorldX;
    public float CursorWorldY => _cursorWorldY;

    public ViewportRenderer()
    {
        _camera = new Camera
        {
            Position = new Vector3(0, 0, 10),
            Target = Vector3.Zero,
            Projection = ProjectionType.Orthographic,
            AspectRatio = _aspectRatio,
        };
        _targetPosition = _camera.Position;
        _targetTarget = _camera.Target;
        _cameraDistance = Vector3.Distance(_camera.Position, _camera.Target);
        _targetDistance = _cameraDistance;

        // Register default passes
        _passes.Add(new GridPass());
        _passes.Add(new ScenePass());
        _passes.Add(new SelectionPass());
        _passes.Add(new GizmoPass());
    }

    public void SetOverlayPass(IRenderPass pass)
    {
        _passes.Add(pass);
        _passes.Sort((a, b) => a.Order.CompareTo(b.Order));
    }

    public void Resize(int width, int height)
    {
        if (width < 1) width = 1;
        if (height < 1) height = 1;

        if (_width != width || _height != height)
        {
            _width = width;
            _height = height;
            _aspectRatio = (float)width / height;
            _bitmap?.Dispose();
            _bitmap = new WriteableBitmap(
                new PixelSize(width, height),
                new Avalonia.Vector(96, 96),
                PixelFormat.Bgra8888,
                AlphaFormat.Opaque);
            _dirty = true;
        }
    }

    public void Invalidate() => _dirty = true;

    public void SetToolName(string name) => _activeToolName = name;
    public void SetStatus(string message) => _statusMessage = message;
    public void SetSelectionBox(float x, float y, float w, float h) { SelectionBox = (x, y, w, h); Invalidate(); }
    public void ClearSelectionBox() { SelectionBox = null; Invalidate(); }

    // --- Camera System ---

    public void Pan(float dx, float dy)
    {
        float scale = _cameraDistance * 0.1f;
        _camera.Pan(-dx * scale, dy * scale);
        _targetPosition = _camera.Position;
        _targetTarget = _camera.Target;
        _dirty = true;
    }

    public void ZoomOnCursor(float delta, float cursorNX, float cursorNY)
    {
        // Convert cursor to world
        float ndcX = cursorNX * 2f - 1f;
        float ndcY = 1f - cursorNY * 2f;
        Vector3 worldBefore = ScreenToWorld(new Vector2(ndcX, ndcY));
        if (float.IsNaN(worldBefore.X)) return;

        float zoomFactor = 1f + delta * 0.1f;
        zoomFactor = System.Math.Clamp(zoomFactor, 0.3f, 3f);
        _targetDistance = System.Math.Clamp(_targetDistance * zoomFactor, 0.1f, 100f);

        // After zoom, adjust target so cursor stays fixed in world
        Vector3 forward = Vector3.Normalize(_targetTarget - _targetPosition);
        _targetPosition = _targetTarget - forward * _targetDistance;

        _animating = true;
        _dirty = true;
    }

    public void ResetCamera()
    {
        _targetTarget = Vector3.Zero;
        _targetPosition = new Vector3(0, 0, 10);
        _targetDistance = 10f;
        _animating = true;
        _dirty = true;
    }

    public void FitAll(float xMin, float xMax, float yMin, float yMax)
    {
        float cx = (xMin + xMax) * 0.5f;
        float cy = (yMin + yMax) * 0.5f;
        float range = System.Math.Max(xMax - xMin, yMax - yMin) * 0.6f;
        if (range < 0.1f) range = 5f;

        _targetTarget = new Vector3(cx, cy, 0);
        _targetDistance = range;
        _targetPosition = new Vector3(cx, cy, range + 0.1f);
        _animating = true;
        _dirty = true;
    }

    public void UpdateCameraAnimation()
    {
        if (!_animating) return;
        float speed = System.Math.Min(1f, _deltaTime * 8f);

        _camera.Position = Vector3.Lerp(_camera.Position, _targetPosition, speed);
        _camera.Target = Vector3.Lerp(_camera.Target, _targetTarget, speed);
        _cameraDistance = Vector3.Distance(_camera.Position, _camera.Target);

        float dist = Vector3.Distance(_camera.Position, _targetPosition);
        if (dist < 0.01f)
        {
            _camera.Position = _targetPosition;
            _camera.Target = _targetTarget;
            _cameraDistance = _targetDistance;
            _animating = false;
        }
        _dirty = true;
    }

    public void SetCursorWorld(float nx, float ny)
    {
        float ndcX = nx * 2f - 1f;
        float ndcY = 1f - ny * 2f;
        var world = ScreenToWorld(new Vector2(ndcX, ndcY));
        _cursorWorldX = world.X;
        _cursorWorldY = world.Y;
    }

    // --- World ↔ Screen ---

    public Vector2 WorldToScreen(Vector3 world)
    {
        var vp = _camera.ViewProjectionMatrix;
        var clip = Vector4.Transform(new Vector4(world, 1), vp);
        if (System.Math.Abs(clip.W) > 0.0001f)
        {
            clip.X /= clip.W;
            clip.Y /= clip.W;
        }
        float sx = (clip.X + 1) * 0.5f * _width;
        float sy = (1 - clip.Y) * 0.5f * _height;
        return new Vector2(sx, sy);
    }

    public Vector3 ScreenToWorld(Vector2 ndc)
    {
        if (Matrix4x4.Invert(_camera.ViewProjectionMatrix, out var inv))
        {
            var clip = Vector4.Transform(new Vector4(ndc.X, ndc.Y, 0, 1), inv);
            if (System.Math.Abs(clip.W) > 0.0001f)
            {
                clip.X /= clip.W;
                clip.Y /= clip.W;
            }
            return new Vector3(clip.X, clip.Y, 0);
        }
        return Vector3.Zero;
    }

    // --- Render ---

    public WriteableBitmap? Render()
    {
        var now = DateTime.UtcNow;
        _deltaTime = (float)(now - _lastFrameTime).TotalSeconds;
        _totalTime += _deltaTime;
        _lastFrameTime = now;

        UpdateCameraAnimation();

        if (!_dirty || _bitmap is null) return _bitmap;
        _dirty = false;

        ZoomLevel = _targetDistance / 10f;

        var buffer = new PixelBuffer(_width, _height);
        var ctx = new RenderContext(
            Width: _width,
            Height: _height,
            AspectRatio: _aspectRatio,
            DeltaTime: _deltaTime,
            TotalTime: _totalTime,
            CameraPosition: _camera.Position,
            CameraTarget: _camera.Target,
            ViewProjectionMatrix: _camera.ViewProjectionMatrix,
            ZoomLevel: ZoomLevel,
            CursorWorldX: _cursorWorldX,
            CursorWorldY: _cursorWorldY,
            ActiveToolName: _activeToolName,
            SelectionCount: Services.AppServices.SelectionService.Count,
            StatusMessage: _statusMessage,
            SelectionBox: SelectionBox);

        foreach (var pass in _passes)
            pass.Execute(buffer, ctx);

        var data = buffer.Data;
        using var frame = _bitmap.Lock();
        System.Runtime.InteropServices.Marshal.Copy(data, 0, frame.Address, data.Length);

        return _bitmap;
    }
}
