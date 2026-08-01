using System.Numerics;
using PixelBuffer = MathVerse.Math.Visualization.Export.PixelBuffer;

namespace MathVerse.Desktop.Rendering;

public enum RenderObjectType
{
    Line,
    Polyline,
    Text,
    Point,
    Rectangle,
    Circle,
}

public enum DirtyFlag
{
    None = 0,
    StyleDirty = 1,
    GeometryDirty = 2,
}

public interface IRenderObject
{
    Guid Id { get; }
    Guid SourceObjectId { get; }
    RenderObjectType Type { get; }
    bool IsVisible { get; set; }
    bool IsHidden { get; set; }
    bool IsLocked { get; set; }
    bool IsSelected { get; set; }
    bool IsHovered { get; set; }
    int Layer { get; set; }
    int ZOrder { get; set; }
    DirtyFlag Dirty { get; set; }
    void Draw(PixelBuffer buffer, in RenderContext context);
    float HitTest(float worldX, float worldY);
    bool IntersectsBox(float minX, float maxX, float minY, float maxY);
}
