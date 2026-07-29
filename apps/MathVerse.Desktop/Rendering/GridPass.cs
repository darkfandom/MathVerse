using System.Numerics;
using PixelBuffer = MathVerse.Math.Visualization.Export.PixelBuffer;

namespace MathVerse.Desktop.Rendering;

public sealed class GridPass : IRenderPass
{
    public string Name => "GridPass";
    public int Order => 0;

    private const int GridColor = 0x1E1E2E;
    private const int MajorColor = 0x2A2A3E;
    private const int AxisXColor = 0xC84040;
    private const int AxisYColor = 0x40C840;

    public void Execute(PixelBuffer buffer, in RenderContext ctx)
    {
        DrawBackground(buffer, ctx);
        DrawGridLines(buffer, ctx);
        DrawAxes(buffer, ctx);
        DrawOriginMarker(buffer, ctx);
    }

    private static void DrawBackground(PixelBuffer buffer, in RenderContext ctx)
    {
        buffer.Clear(11, 11, 18, 255);
    }

    private static void DrawGridLines(PixelBuffer buffer, in RenderContext ctx)
    {
        var vp = ctx.ViewProjectionMatrix;
        float minStep = GetAdaptiveStep(vp, ctx.Width, ctx.Height);

        // Determine visible range in world units
        var bottomLeft = ScreenToWorld(vp, 0, ctx.Height, ctx.Width, ctx.Height);
        var topRight = ScreenToWorld(vp, ctx.Width, 0, ctx.Width, ctx.Height);

        float xMin = bottomLeft.X;
        float xMax = topRight.X;
        float yMin = topRight.Y;
        float yMax = bottomLeft.Y;

        // Add a margin so lines extend slightly past viewport
        float xMargin = (xMax - xMin) * 0.1f;
        float yMargin = (yMax - yMin) * 0.1f;
        xMin -= xMargin;
        xMax += xMargin;
        yMin -= yMargin;
        yMax += yMargin;

        // Draw minor grid lines
        float minorStep = minStep / 5f;
        if (minorStep > 0.01f)
        {
            float minorAlpha = System.Math.Clamp(1f - ctx.ZoomLevel * 0.5f, 0.1f, 0.6f);
            byte a = (byte)(minorAlpha * 40);
            DrawGridLinesInRange(buffer, vp, xMin, xMax, yMin, yMax, minorStep, 25, 25, 40, a);
        }

        // Draw major grid lines
        byte majorA = (byte)(System.Math.Clamp(1f - ctx.ZoomLevel * 0.3f, 0.2f, 0.8f) * 60);
        DrawGridLinesInRange(buffer, vp, xMin, xMax, yMin, yMax, minStep, 42, 42, 62, majorA);
    }

    private static void DrawGridLinesInRange(PixelBuffer buffer, Matrix4x4 vp,
        float xMin, float xMax, float yMin, float yMax,
        float step, byte r, byte g, byte b, byte a)
    {
        if (a < 5) return;

        // Align to nearest step
        float xStart = System.MathF.Floor(xMin / step) * step;
        float yStart = System.MathF.Floor(yMin / step) * step;

        for (float x = xStart; x <= xMax; x += step)
        {
            if (System.Math.Abs(x) < 0.0001f) continue; // axis drawn separately
            var start = Project(vp, new Vector3(x, yMin, 0));
            var end = Project(vp, new Vector3(x, yMax, 0));
            DrawLine(buffer, start, end, r, g, b, a);
        }

        for (float y = yStart; y <= yMax; y += step)
        {
            if (System.Math.Abs(y) < 0.0001f) continue;
            var start = Project(vp, new Vector3(xMin, y, 0));
            var end = Project(vp, new Vector3(xMax, y, 0));
            DrawLine(buffer, start, end, r, g, b, a);
        }
    }

    private static void DrawAxes(PixelBuffer buffer, in RenderContext ctx)
    {
        var vp = ctx.ViewProjectionMatrix;
        var origin = Project(vp, Vector3.Zero);

        // Compute visible range for axis extent
        var bottomLeft = ScreenToWorld(vp, 0, ctx.Height, ctx.Width, ctx.Height);
        var topRight = ScreenToWorld(vp, ctx.Width, 0, ctx.Width, ctx.Height);
        float range = System.Math.Max(
            System.Math.Abs(bottomLeft.X) + System.Math.Abs(topRight.X),
            System.Math.Abs(bottomLeft.Y) + System.Math.Abs(topRight.Y));

        // X axis
        var xEnd = Project(vp, new Vector3(range, 0, 0));
        DrawLine(buffer, origin, xEnd, 200, 60, 60, 200);

        // Y axis
        var yEnd = Project(vp, new Vector3(0, range, 0));
        DrawLine(buffer, origin, yEnd, 60, 200, 60, 200);
    }

    private static void DrawOriginMarker(PixelBuffer buffer, in RenderContext ctx)
    {
        var o = Project(ctx.ViewProjectionMatrix, Vector3.Zero);
        int ox = (int)((o.sx + 1) * 0.5f * ctx.Width);
        int oy = (int)((1 - o.sy) * 0.5f * ctx.Height);

        if (ox < 0 || ox >= ctx.Width || oy < 0 || oy >= ctx.Height)
            return;

        // Draw a small cross at origin
        int size = 4;
        for (int i = -size; i <= size; i++)
        {
            buffer.SetPixel(ox + i, oy, 200, 200, 200, 120);
            buffer.SetPixel(ox, oy + i, 200, 200, 200, 120);
        }
    }

    private static float GetAdaptiveStep(Matrix4x4 vp, int width, int height)
    {
        // Project a unit vector to see how many pixels one world unit covers
        var p1 = Project(vp, Vector3.Zero);
        var p2 = Project(vp, new Vector3(1, 0, 0));
        float pixelPerUnit = System.Math.Abs(p2.sx - p1.sx) * width * 0.5f;

        if (pixelPerUnit < 5) return 10f;
        if (pixelPerUnit < 20) return 5f;
        if (pixelPerUnit < 50) return 2f;
        if (pixelPerUnit < 100) return 1f;
        if (pixelPerUnit < 300) return 0.5f;
        return 0.2f;
    }

    private static (float sx, float sy) Project(Matrix4x4 vp, Vector3 world)
    {
        var clip = Vector4.Transform(new Vector4(world, 1), vp);
        if (System.Math.Abs(clip.W) > 0.0001f)
        {
            clip.X /= clip.W;
            clip.Y /= clip.W;
        }
        return (clip.X, clip.Y);
    }

    private static Vector2 ScreenToWorld(Matrix4x4 vp, float sx, float sy, int w, int h)
    {
        float ndcX = sx / w * 2f - 1f;
        float ndcY = 1f - sy / h * 2f;
        if (Matrix4x4.Invert(vp, out var inv))
        {
            var clip = Vector4.Transform(new Vector4(ndcX, ndcY, 0, 1), inv);
            if (System.Math.Abs(clip.W) > 0.0001f)
            {
                clip.X /= clip.W;
                clip.Y /= clip.W;
            }
            return new Vector2(clip.X, clip.Y);
        }
        return Vector2.Zero;
    }

    private static void DrawLine(PixelBuffer buffer, (float x, float y) from, (float x, float y) to,
        byte r, byte g, byte b, byte a)
    {
        if (a < 5) return;

        int x0 = (int)((from.x + 1) * 0.5f * buffer.Width);
        int y0 = (int)((1 - from.y) * 0.5f * buffer.Height);
        int x1 = (int)((to.x + 1) * 0.5f * buffer.Width);
        int y1 = (int)((1 - to.y) * 0.5f * buffer.Height);

        int dx = System.Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -System.Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy, e2;

        while (true)
        {
            buffer.SetPixel(x0, y0, r, g, b, a);
            if (x0 == x1 && y0 == y1) break;
            e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }
}
