namespace MathVerse.Math.Visualization.Interaction;
using System.Numerics;
using System.Collections.Generic;

/// <summary>Provides rubber band (rectangle) selection functionality.</summary>
public sealed class RubberBandSelection
{
    /// <summary>Computes the selection rectangle from drag start and current position.</summary>
    /// <param name="start">The drag start position.</param>
    /// <param name="current">The current drag position.</param>
    /// <returns>The selection rectangle as a bounding box.</returns>
    public static BoundingBox2D ComputeSelectionRect(Vector2 start, Vector2 current)
    {
        float minX = System.Math.Min(start.X, current.X);
        float minY = System.Math.Min(start.Y, current.Y);
        float maxX = System.Math.Max(start.X, current.X);
        float maxY = System.Math.Max(start.Y, current.Y);

        return new BoundingBox2D
        {
            MinX = minX,
            MinY = minY,
            MaxX = maxX,
            MaxY = maxY
        };
    }

    /// <summary>Filters objects that fall within the selection rectangle.</summary>
    /// <param name="objects">The objects to filter.</param>
    /// <param name="rect">The selection rectangle.</param>
    /// <param name="viewProjection">The combined view-projection matrix.</param>
    /// <returns>Objects within the selection rectangle.</returns>
    public static List<Core.VisualizationObject> FilterObjectsInRect(
        List<Core.VisualizationObject> objects, BoundingBox2D rect, Matrix4x4 viewProjection)
    {
        return SelectionTool.SelectBounds(objects, rect, viewProjection);
    }

    /// <summary>Gets the selection rectangle in normalized device coordinates.</summary>
    /// <param name="rect">The screen-space selection rectangle.</param>
    /// <param name="viewWidth">The viewport width.</param>
    /// <param name="viewHeight">The viewport height.</param>
    /// <returns>The rectangle in NDC (-1 to 1).</returns>
    public static BoundingBox2D ToNDC(BoundingBox2D rect, double viewWidth, double viewHeight)
    {
        return new BoundingBox2D
        {
            MinX = (rect.MinX / viewWidth) * 2.0 - 1.0,
            MinY = 1.0 - (rect.MinY / viewHeight) * 2.0,
            MaxX = (rect.MaxX / viewWidth) * 2.0 - 1.0,
            MaxY = 1.0 - (rect.MaxY / viewHeight) * 2.0
        };
    }

    /// <summary>Checks if a point in screen coordinates is inside the selection rectangle.</summary>
    /// <param name="point">The screen point.</param>
    /// <param name="rect">The selection rectangle.</param>
    /// <returns>True if the point is inside the rectangle.</returns>
    public static bool IsPointInRect(Vector2 point, BoundingBox2D rect)
    {
        return rect.Contains(point);
    }

    /// <summary>Checks if two selection rectangles overlap.</summary>
    /// <param name="rect1">The first rectangle.</param>
    /// <param name="rect2">The second rectangle.</param>
    /// <returns>True if the rectangles overlap.</returns>
    public static bool RectanglesOverlap(BoundingBox2D rect1, BoundingBox2D rect2)
    {
        return rect1.MinX <= rect2.MaxX && rect1.MaxX >= rect2.MinX &&
               rect1.MinY <= rect2.MaxY && rect1.MaxY >= rect2.MinY;
    }

    /// <summary>Computes the intersection of two rectangles.</summary>
    /// <param name="rect1">The first rectangle.</param>
    /// <param name="rect2">The second rectangle.</param>
    /// <returns>The intersection rectangle, or null if no overlap.</returns>
    public static BoundingBox2D? IntersectRectangles(BoundingBox2D rect1, BoundingBox2D rect2)
    {
        double minX = System.Math.Max(rect1.MinX, rect2.MinX);
        double minY = System.Math.Max(rect1.MinY, rect2.MinY);
        double maxX = System.Math.Min(rect1.MaxX, rect2.MaxX);
        double maxY = System.Math.Min(rect1.MaxY, rect2.MaxY);

        if (minX >= maxX || minY >= maxY)
            return null;

        return new BoundingBox2D
        {
            MinX = minX,
            MinY = minY,
            MaxX = maxX,
            MaxY = maxY
        };
    }

    /// <summary>Computes the union of two rectangles.</summary>
    /// <param name="rect1">The first rectangle.</param>
    /// <param name="rect2">The second rectangle.</param>
    /// <returns>The union rectangle.</returns>
    public static BoundingBox2D UnionRectangles(BoundingBox2D rect1, BoundingBox2D rect2)
    {
        return new BoundingBox2D
        {
            MinX = System.Math.Min(rect1.MinX, rect2.MinX),
            MinY = System.Math.Min(rect1.MinY, rect2.MinY),
            MaxX = System.Math.Max(rect1.MaxX, rect2.MaxX),
            MaxY = System.Math.Max(rect1.MaxY, rect2.MaxY)
        };
    }

    /// <summary>Gets the corner points of a selection rectangle.</summary>
    /// <param name="rect">The selection rectangle.</param>
    /// <returns>Array of 4 corner points (top-left, top-right, bottom-right, bottom-left).</returns>
    public static Vector2[] GetCorners(BoundingBox2D rect)
    {
        return new Vector2[]
        {
            new Vector2((float)rect.MinX, (float)rect.MinY),
            new Vector2((float)rect.MaxX, (float)rect.MinY),
            new Vector2((float)rect.MaxX, (float)rect.MaxY),
            new Vector2((float)rect.MinX, (float)rect.MaxY)
        };
    }

    /// <summary>Converts a selection rectangle to line segments for rendering.</summary>
    /// <param name="rect">The selection rectangle.</param>
    /// <returns>Line segments for drawing the rectangle.</returns>
    public static List<(Vector2 Start, Vector2 End)> ToLineSegments(BoundingBox2D rect)
    {
        var corners = GetCorners(rect);
        var segments = new List<(Vector2, Vector2)>();

        for (int i = 0; i < 4; i++)
        {
            segments.Add((corners[i], corners[(i + 1) % 4]));
        }

        return segments;
    }

    /// <summary>Computes the area of a selection rectangle.</summary>
    /// <param name="rect">The selection rectangle.</param>
    /// <returns>The area in square units.</returns>
    public static double ComputeArea(BoundingBox2D rect)
    {
        return rect.Width * rect.Height;
    }
}
