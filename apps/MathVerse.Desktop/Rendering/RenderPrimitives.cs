using System.Numerics;
using PixelBuffer = MathVerse.Math.Visualization.Export.PixelBuffer;

namespace MathVerse.Desktop.Rendering;

public readonly record struct Color4(byte R, byte G, byte B, byte A);

public readonly record struct Vertex2(float X, float Y);

public sealed class RenderLine : IRenderObject
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid SourceObjectId { get; }
    public RenderObjectType Type => RenderObjectType.Line;
    public bool IsVisible { get; set; } = true;
    public bool IsHidden { get; set; }
    public bool IsLocked { get; set; }
    public bool IsSelected { get; set; }
    public bool IsHovered { get; set; }
    public int Layer { get; set; }
    public int ZOrder { get; set; }
    public DirtyFlag Dirty { get; set; } = DirtyFlag.GeometryDirty;

    public Vertex2 Start;
    public Vertex2 End;
    public Color4 Color;
    public float Width;

    public RenderLine(Guid sourceObjectId, Vertex2 start, Vertex2 end, Color4 color, float width = 1f)
    {
        SourceObjectId = sourceObjectId;
        Start = start;
        End = end;
        Color = color;
        Width = width;
    }

    public void Draw(PixelBuffer buffer, in RenderContext context)
    {
        if (!IsVisible || IsHidden) return;
        var vp = context.ViewProjectionMatrix;
        var from = Project(vp, new Vector3(Start.X, Start.Y, 0));
        var to = Project(vp, new Vector3(End.X, End.Y, 0));
        DrawBresenham(buffer, from, to, Color.R, Color.G, Color.B, Color.A);
    }

    private static (float sx, float sy) Project(Matrix4x4 vp, Vector3 world)
    {
        var clip = Vector4.Transform(new Vector4(world, 1), vp);
        if (System.Math.Abs(clip.W) > 0.0001f) { clip.X /= clip.W; clip.Y /= clip.W; }
        return (clip.X, clip.Y);
    }

    internal static void DrawBresenham(PixelBuffer buffer, (float sx, float sy) from, (float sx, float sy) to,
        byte r, byte g, byte b, byte a)
    {
        int x0 = (int)((from.sx + 1) * 0.5f * buffer.Width);
        int y0 = (int)((1 - from.sy) * 0.5f * buffer.Height);
        int x1 = (int)((to.sx + 1) * 0.5f * buffer.Width);
        int y1 = (int)((1 - to.sy) * 0.5f * buffer.Height);
        if (a < 5) return;

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

public sealed class RenderPolyline : IRenderObject
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid SourceObjectId { get; }
    public RenderObjectType Type => RenderObjectType.Polyline;
    public bool IsVisible { get; set; } = true;
    public bool IsHidden { get; set; }
    public bool IsLocked { get; set; }
    public bool IsSelected { get; set; }
    public bool IsHovered { get; set; }
    public int Layer { get; set; }
    public int ZOrder { get; set; }
    public DirtyFlag Dirty { get; set; } = DirtyFlag.GeometryDirty;

    public Vertex2[] Points;
    public Color4 Color;
    public float Width;
    public bool Closed;

    public RenderPolyline(Guid sourceObjectId, Vertex2[] points, Color4 color, bool closed = false, float width = 1f)
    {
        SourceObjectId = sourceObjectId;
        Points = points;
        Color = color;
        Closed = closed;
        Width = width;
    }

    public void Draw(PixelBuffer buffer, in RenderContext context)
    {
        if (!IsVisible || IsHidden || Points.Length < 2) return;
        var vp = context.ViewProjectionMatrix;
        for (int i = 1; i < Points.Length; i++)
        {
            var from = Project(vp, new Vector3(Points[i - 1].X, Points[i - 1].Y, 0));
            var to = Project(vp, new Vector3(Points[i].X, Points[i].Y, 0));
            RenderLine.DrawBresenham(buffer, from, to, Color.R, Color.G, Color.B, Color.A);
        }
        if (Closed && Points.Length > 2)
        {
            var from = Project(vp, new Vector3(Points[^1].X, Points[^1].Y, 0));
            var to = Project(vp, new Vector3(Points[0].X, Points[0].Y, 0));
            RenderLine.DrawBresenham(buffer, from, to, Color.R, Color.G, Color.B, Color.A);
        }
    }

    private static (float sx, float sy) Project(Matrix4x4 vp, Vector3 world)
    {
        var clip = Vector4.Transform(new Vector4(world, 1), vp);
        if (System.Math.Abs(clip.W) > 0.0001f) { clip.X /= clip.W; clip.Y /= clip.W; }
        return (clip.X, clip.Y);
    }
}

public sealed class RenderText : IRenderObject
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid SourceObjectId { get; }
    public RenderObjectType Type => RenderObjectType.Text;
    public bool IsVisible { get; set; } = true;
    public bool IsHidden { get; set; }
    public bool IsLocked { get; set; }
    public bool IsSelected { get; set; }
    public bool IsHovered { get; set; }
    public int Layer { get; set; }
    public int ZOrder { get; set; }
    public DirtyFlag Dirty { get; set; } = DirtyFlag.GeometryDirty;

    public string Text;
    public Vertex2 Position;
    public Color4 Color;
    public float Size;

    public RenderText(Guid sourceObjectId, string text, Vertex2 position, Color4 color, float size = 12f)
    {
        SourceObjectId = sourceObjectId;
        Text = text;
        Position = position;
        Color = color;
        Size = size;
    }

    public void Draw(PixelBuffer buffer, in RenderContext context)
    {
        if (!IsVisible || IsHidden || string.IsNullOrEmpty(Text)) return;
        // Text rendering requires font rasterization — deferred
    }
}

public sealed class RenderPoint : IRenderObject
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid SourceObjectId { get; }
    public RenderObjectType Type => RenderObjectType.Point;
    public bool IsVisible { get; set; } = true;
    public bool IsHidden { get; set; }
    public bool IsLocked { get; set; }
    public bool IsSelected { get; set; }
    public bool IsHovered { get; set; }
    public int Layer { get; set; }
    public int ZOrder { get; set; }
    public DirtyFlag Dirty { get; set; } = DirtyFlag.GeometryDirty;

    public Vertex2 Position;
    public Color4 Color;
    public float Radius;

    public RenderPoint(Guid sourceObjectId, Vertex2 position, Color4 color, float radius = 2f)
    {
        SourceObjectId = sourceObjectId;
        Position = position;
        Color = color;
        Radius = radius;
    }

    public void Draw(PixelBuffer buffer, in RenderContext context)
    {
        if (!IsVisible || IsHidden) return;
        var vp = context.ViewProjectionMatrix;
        var clip = Vector4.Transform(new Vector4(Position.X, Position.Y, 0, 1), vp);
        if (System.Math.Abs(clip.W) < 0.0001f) return;
        int px = (int)((clip.X / clip.W + 1) * 0.5f * buffer.Width);
        int py = (int)((1 - clip.Y / clip.W) * 0.5f * buffer.Height);
        int r = (int)System.Math.Max(1, Radius);
        for (int dy = -r; dy <= r; dy++)
            for (int dx = -r; dx <= r; dx++)
                if (dx * dx + dy * dy <= r * r)
                    buffer.SetPixel(px + dx, py + dy, Color.R, Color.G, Color.B, Color.A);
    }
}

public sealed class RenderRectangle : IRenderObject
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid SourceObjectId { get; }
    public RenderObjectType Type => RenderObjectType.Rectangle;
    public bool IsVisible { get; set; } = true;
    public bool IsHidden { get; set; }
    public bool IsLocked { get; set; }
    public bool IsSelected { get; set; }
    public bool IsHovered { get; set; }
    public int Layer { get; set; }
    public int ZOrder { get; set; }
    public DirtyFlag Dirty { get; set; } = DirtyFlag.GeometryDirty;

    public Vertex2 Min;
    public Vertex2 Max;
    public Color4 FillColor;
    public Color4 StrokeColor;
    public float StrokeWidth;
    public bool Filled;

    public RenderRectangle(Guid sourceObjectId, Vertex2 min, Vertex2 max, Color4 fill, Color4 stroke,
        bool filled = true, float strokeWidth = 1f)
    {
        SourceObjectId = sourceObjectId;
        Min = min;
        Max = max;
        FillColor = fill;
        StrokeColor = stroke;
        Filled = filled;
        StrokeWidth = strokeWidth;
    }

    public void Draw(PixelBuffer buffer, in RenderContext context)
    {
        if (!IsVisible || IsHidden) return;
        var vp = context.ViewProjectionMatrix;
        var p1 = Vector4.Transform(new Vector4(Min.X, Min.Y, 0, 1), vp);
        var p2 = Vector4.Transform(new Vector4(Max.X, Max.Y, 0, 1), vp);
        if (System.Math.Abs(p1.W) < 0.0001f || System.Math.Abs(p2.W) < 0.0001f) return;

        int x0 = (int)((p1.X / p1.W + 1) * 0.5f * buffer.Width);
        int y0 = (int)((1 - p1.Y / p1.W) * 0.5f * buffer.Height);
        int x1 = (int)((p2.X / p2.W + 1) * 0.5f * buffer.Width);
        int y1 = (int)((1 - p2.Y / p2.W) * 0.5f * buffer.Height);

        if (x0 > x1) { (x0, x1) = (x1, x0); }
        if (y0 > y1) { (y0, y1) = (y1, y0); }

        if (Filled)
        {
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                    buffer.SetPixel(x, y, FillColor.R, FillColor.G, FillColor.B, FillColor.A);
        }

        // Stroke (outline)
        for (int x = x0; x <= x1; x++)
        {
            buffer.SetPixel(x, y0, StrokeColor.R, StrokeColor.G, StrokeColor.B, StrokeColor.A);
            buffer.SetPixel(x, y1, StrokeColor.R, StrokeColor.G, StrokeColor.B, StrokeColor.A);
        }
        for (int y = y0; y <= y1; y++)
        {
            buffer.SetPixel(x0, y, StrokeColor.R, StrokeColor.G, StrokeColor.B, StrokeColor.A);
            buffer.SetPixel(x1, y, StrokeColor.R, StrokeColor.G, StrokeColor.B, StrokeColor.A);
        }
    }
}

public sealed class RenderCircle : IRenderObject
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid SourceObjectId { get; }
    public RenderObjectType Type => RenderObjectType.Circle;
    public bool IsVisible { get; set; } = true;
    public bool IsHidden { get; set; }
    public bool IsLocked { get; set; }
    public bool IsSelected { get; set; }
    public bool IsHovered { get; set; }
    public int Layer { get; set; }
    public int ZOrder { get; set; }
    public DirtyFlag Dirty { get; set; } = DirtyFlag.GeometryDirty;

    public Vertex2 Center;
    public float Radius;
    public Color4 FillColor;
    public Color4 StrokeColor;
    public bool Filled;

    public RenderCircle(Guid sourceObjectId, Vertex2 center, float radius, Color4 fill, Color4 stroke,
        bool filled = true)
    {
        SourceObjectId = sourceObjectId;
        Center = center;
        Radius = radius;
        FillColor = fill;
        StrokeColor = stroke;
        Filled = filled;
    }

    public void Draw(PixelBuffer buffer, in RenderContext context)
    {
        if (!IsVisible || IsHidden || Radius < 0.01f) return;
        var vp = context.ViewProjectionMatrix;
        var clip = Vector4.Transform(new Vector4(Center.X, Center.Y, 0, 1), vp);
        if (System.Math.Abs(clip.W) < 0.0001f) return;

        int cx = (int)((clip.X / clip.W + 1) * 0.5f * buffer.Width);
        int cy = (int)((1 - clip.Y / clip.W) * 0.5f * buffer.Height);

        // Project radius — approximate via screen-space offset
        var clip2 = Vector4.Transform(new Vector4(Center.X + Radius, Center.Y, 0, 1), vp);
        int r = (int)System.Math.Abs((clip2.X / clip2.W + 1) * 0.5f * buffer.Width - cx);
        if (r < 1) r = 1;

        if (Filled)
        {
            for (int dy = -r; dy <= r; dy++)
                for (int dx = -r; dx <= r; dx++)
                    if (dx * dx + dy * dy <= r * r)
                        buffer.SetPixel(cx + dx, cy + dy, FillColor.R, FillColor.G, FillColor.B, FillColor.A);
        }

        // Stroke
        for (int dy = -r; dy <= r; dy++)
        {
            int dx = (int)System.Math.Sqrt(r * r - dy * dy);
            buffer.SetPixel(cx + dx, cy + dy, StrokeColor.R, StrokeColor.G, StrokeColor.B, StrokeColor.A);
            buffer.SetPixel(cx - dx, cy + dy, StrokeColor.R, StrokeColor.G, StrokeColor.B, StrokeColor.A);
        }
    }
}
