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
    private WriteableBitmap? _bitmap;
    private int _width = 1;
    private int _height = 1;
    private float _aspectRatio = 16f / 9f;
    private bool _dirty = true;

    public Camera Camera => _camera;
    public WriteableBitmap? Bitmap => _bitmap;
    public int Width => _width;
    public int Height => _height;

    public float ZoomLevel { get; private set; } = 1f;

    public ViewportRenderer()
    {
        _camera = new Camera
        {
            Position = new Vector3(0, 0, 10),
            Target = Vector3.Zero,
            Projection = ProjectionType.Orthographic,
            AspectRatio = _aspectRatio,
        };
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

    public WriteableBitmap? Render()
    {
        if (!_dirty || _bitmap is null) return _bitmap;
        _dirty = false;

        var buffer = new PixelBuffer(_width, _height);

        DrawBackground(buffer);
        DrawGrid(buffer, _aspectRatio);
        DrawAxes(buffer, _aspectRatio);

        // Copy pixel data to bitmap
        var data = buffer.Data;
        using var frame = _bitmap.Lock();
        System.Runtime.InteropServices.Marshal.Copy(data, 0, frame.Address, data.Length);

        return _bitmap;
    }

    private static void DrawBackground(PixelBuffer buffer)
    {
        buffer.Clear(11, 11, 18, 255);
    }

    private void DrawGrid(PixelBuffer buffer, float aspect)
    {
        float step = 0.5f * ZoomLevel;
        float range = 5f * ZoomLevel;

        for (float x = -range; x <= range; x += step)
        {
            var start = Project(new Vector3(x, -range / aspect, 0));
            var end = Project(new Vector3(x, range / aspect, 0));
            DrawLine(buffer, start, end, 30, 30, 40, 60);
        }

        for (float y = -range / aspect; y <= range / aspect; y += step / aspect)
        {
            var start = Project(new Vector3(-range, y, 0));
            var end = Project(new Vector3(range, y, 0));
            DrawLine(buffer, start, end, 30, 30, 40, 60);
        }
    }

    private void DrawAxes(PixelBuffer buffer, float aspect)
    {
        var origin = Project(Vector3.Zero);
        var xEnd = Project(new Vector3(0.8f, 0, 0));
        DrawLine(buffer, origin, xEnd, 200, 60, 60, 200);

        var yEnd = Project(new Vector3(0, 0.8f / aspect, 0));
        DrawLine(buffer, origin, yEnd, 60, 200, 60, 200);
    }

    private (float sx, float sy) Project(Vector3 world)
    {
        var vp = _camera.ViewProjectionMatrix;
        var clip = Vector4.Transform(new Vector4(world, 1), vp);
        if (System.Math.Abs(clip.W) > 0.0001f)
        {
            clip.X /= clip.W;
            clip.Y /= clip.W;
        }
        return (clip.X, clip.Y);
    }

    private static void DrawLine(PixelBuffer buffer, (float x, float y) from, (float x, float y) to,
        byte r, byte g, byte bl, byte alpha)
    {
        int x0 = (int)((from.x + 1) * 0.5f * buffer.Width);
        int y0 = (int)((1 - from.y) * 0.5f * buffer.Height);
        int x1 = (int)((to.x + 1) * 0.5f * buffer.Width);
        int y1 = (int)((1 - to.y) * 0.5f * buffer.Height);

        int dx = System.Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -System.Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy, e2;

        while (true)
        {
            buffer.SetPixel(x0, y0, r, g, bl, alpha);
            if (x0 == x1 && y0 == y1) break;
            e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }
}
