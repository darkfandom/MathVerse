using System.Numerics;
using MathVerse.Desktop.Services;
using PixelBuffer = MathVerse.Math.Visualization.Export.PixelBuffer;

namespace MathVerse.Desktop.Rendering;

public sealed class SelectionPass : IRenderPass
{
    public string Name => "SelectionPass";
    public int Order => 2;

    public void Execute(PixelBuffer buffer, in RenderContext context)
    {
        DrawSelectionHighlights(buffer, context);
        DrawHoverHighlight(buffer, context);
        DrawActiveObjectHighlight(buffer, context);
        DrawSelectionBox(buffer, context);
    }

    private static void DrawSelectionHighlights(PixelBuffer buffer, in RenderContext context)
    {
        var sel = AppServices.SelectionService;
        if (!sel.HasSelection) return;

        foreach (var id in sel.SelectedIds)
        {
            var node = AppServices.SceneGraph.Get(id);
            if (node is null || !node.IsVisible) continue;

            foreach (var ro in node.RenderObjects)
            {
                if (!ro.IsVisible || ro.IsHidden) continue;

                // Draw bright outline around selected objects
                var outlineColor = new Color4(74, 158, 255, 200);
                DrawObjectOutline(buffer, context, ro, outlineColor);
            }
        }
    }

    private static void DrawHoverHighlight(PixelBuffer buffer, in RenderContext context)
    {
        var hoveredId = AppServices.SelectionService.HoveredObjectId;
        if (hoveredId is null) return;

        var node = AppServices.SceneGraph.Get(hoveredId.Value);
        if (node is null || !node.IsVisible) return;

        foreach (var ro in node.RenderObjects)
        {
            if (!ro.IsVisible || ro.IsHidden) continue;
            var hoverColor = new Color4(255, 220, 80, 120);
            DrawObjectOutline(buffer, context, ro, hoverColor);
        }
    }

    private static void DrawActiveObjectHighlight(PixelBuffer buffer, in RenderContext context)
    {
        var activeId = AppServices.SelectionService.ActiveObjectId;
        if (activeId is null) return;

        var node = AppServices.SceneGraph.Get(activeId.Value);
        if (node is null || !node.IsVisible) return;

        foreach (var ro in node.RenderObjects)
        {
            if (!ro.IsVisible || ro.IsHidden) continue;
            var activeColor = new Color4(255, 255, 255, 200);
            DrawObjectOutline(buffer, context, ro, activeColor, dashed: true);
        }
    }

    private static void DrawSelectionBox(PixelBuffer buffer, in RenderContext context)
    {
        if (context.SelectionBox is not { } box) return;

        int sx = (int)(box.x * context.Width);
        int sy = (int)(box.y * context.Height);
        int sw = (int)(box.w * context.Width);
        int sh = (int)(box.h * context.Height);

        // Fill
        for (int y = sy; y < sy + sh && y < context.Height; y++)
        {
            for (int x = sx; x < sx + sw && x < context.Width; x++)
            {
                if (x >= 0 && y >= 0)
                    buffer.SetPixel(x, y, 74, 158, 255, 40);
            }
        }

        // Border
        for (int x = sx; x < sx + sw && x < context.Width; x++)
        {
            if (x >= 0 && sy >= 0 && sy < context.Height) buffer.SetPixel(x, sy, 74, 158, 255, 200);
            if (x >= 0 && (sy + sh - 1) >= 0 && (sy + sh - 1) < context.Height) buffer.SetPixel(x, sy + sh - 1, 74, 158, 255, 200);
        }
        for (int y = sy; y < sy + sh && y < context.Height; y++)
        {
            if (sx >= 0 && sx < context.Width && y >= 0) buffer.SetPixel(sx, y, 74, 158, 255, 200);
            if ((sx + sw - 1) >= 0 && (sx + sw - 1) < context.Width && y >= 0) buffer.SetPixel(sx + sw - 1, y, 74, 158, 255, 200);
        }
    }

    private static void DrawObjectOutline(PixelBuffer buffer, in RenderContext context, IRenderObject obj, Color4 color, bool dashed = false)
    {
        var vp = context.ViewProjectionMatrix;

        switch (obj)
        {
            case RenderLine line:
                DrawLineOutline(buffer, vp, line.Start, line.End, color, context.Width, context.Height, dashed);
                break;
            case RenderPolyline poly:
                for (int i = 1; i < poly.Points.Length; i++)
                    DrawLineOutline(buffer, vp, poly.Points[i - 1], poly.Points[i], color, context.Width, context.Height, dashed);
                if (poly.Closed && poly.Points.Length > 2)
                    DrawLineOutline(buffer, vp, poly.Points[^1], poly.Points[0], color, context.Width, context.Height, dashed);
                break;
            case RenderRectangle rect:
                DrawRectOutline(buffer, vp, rect.Min, rect.Max, color, context.Width, context.Height, dashed);
                break;
            case RenderCircle circle:
                DrawCircleOutline(buffer, vp, circle.Center, circle.Radius, color, context.Width, context.Height, dashed);
                break;
            case RenderPoint pt:
                DrawPointHighlight(buffer, vp, pt.Position, color, context.Width, context.Height);
                break;
        }
    }

    private static void DrawLineOutline(PixelBuffer buffer, Matrix4x4 vp, Vertex2 start, Vertex2 end, Color4 color,
        int w, int h, bool dashed)
    {
        var p1 = Project(vp, new Vector3(start.X, start.Y, 0));
        var p2 = Project(vp, new Vector3(end.X, end.Y, 0));

        int x0 = (int)((p1.sx + 1) * 0.5f * w);
        int y0 = (int)((1 - p1.sy) * 0.5f * h);
        int x1 = (int)((p2.sx + 1) * 0.5f * w);
        int y1 = (int)((1 - p2.sy) * 0.5f * h);

        // Draw thicker outline by offset
        for (int ox = -1; ox <= 1; ox++)
        {
            for (int oy = -1; oy <= 1; oy++)
            {
                if (ox == 0 && oy == 0) continue;
                int dx = System.Math.Abs(x1 - (x0 + ox)), sx2 = (x0 + ox) < x1 ? 1 : -1;
                int dy = -System.Math.Abs(y1 - (y0 + oy)), sy2 = (y0 + oy) < y1 ? 1 : -1;
                int err = dx + dy, e2;
                int cx = x0 + ox, cy = y0 + oy;
                int step = 0;
                while (true)
                {
                    if (!dashed || (step % 6) < 3)
                        buffer.SetPixel(cx, cy, color.R, color.G, color.B, color.A);
                    if (cx == x1 && cy == y1) break;
                    e2 = 2 * err;
                    if (e2 >= dy) { err += dy; cx += sx2; }
                    if (e2 <= dx) { err += dx; cy += sy2; }
                    step++;
                }
            }
        }
    }

    private static void DrawRectOutline(PixelBuffer buffer, Matrix4x4 vp, Vertex2 min, Vertex2 max, Color4 color,
        int w, int h, bool dashed)
    {
        var corners = new[]
        {
            new Vertex2(min.X, min.Y),
            new Vertex2(max.X, min.Y),
            new Vertex2(max.X, max.Y),
            new Vertex2(min.X, max.Y),
        };
        for (int i = 0; i < 4; i++)
            DrawLineOutline(buffer, vp, corners[i], corners[(i + 1) % 4], color, w, h, dashed);
    }

    private static void DrawCircleOutline(PixelBuffer buffer, Matrix4x4 vp, Vertex2 center, float radius, Color4 color,
        int w, int h, bool dashed)
    {
        int segments = 24;
        for (int i = 0; i < segments; i++)
        {
            float a1 = (float)(i * System.Math.PI * 2 / segments);
            float a2 = (float)((i + 1) * System.Math.PI * 2 / segments);
            var p1 = new Vertex2(center.X + (float)System.Math.Cos(a1) * radius, center.Y + (float)System.Math.Sin(a1) * radius);
            var p2 = new Vertex2(center.X + (float)System.Math.Cos(a2) * radius, center.Y + (float)System.Math.Sin(a2) * radius);
            DrawLineOutline(buffer, vp, p1, p2, color, w, h, dashed);
        }
    }

    private static void DrawPointHighlight(PixelBuffer buffer, Matrix4x4 vp, Vertex2 pos, Color4 color, int w, int h)
    {
        var p = Project(vp, new Vector3(pos.X, pos.Y, 0));
        int px = (int)((p.sx + 1) * 0.5f * w);
        int py = (int)((1 - p.sy) * 0.5f * h);
        for (int dy = -3; dy <= 3; dy++)
            for (int dx = -3; dx <= 3; dx++)
                if (dx * dx + dy * dy <= 9)
                    buffer.SetPixel(px + dx, py + dy, color.R, color.G, color.B, color.A);
    }

    private static (float sx, float sy) Project(Matrix4x4 vp, Vector3 world)
    {
        var clip = Vector4.Transform(new Vector4(world, 1), vp);
        if (System.Math.Abs(clip.W) > 0.0001f) { clip.X /= clip.W; clip.Y /= clip.W; }
        return (clip.X, clip.Y);
    }
}
