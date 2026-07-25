namespace MathVerse.Math.Visualization.Interaction;
using System.Numerics;

/// <summary>Provides zoom interaction functionality for visualization.</summary>
public sealed class ZoomTool
{
    private const double MinZoom = 0.01;
    private const double MaxZoom = 1000.0;

    /// <summary>Computes the new zoom level from a scroll delta.</summary>
    /// <param name="currentZoom">The current zoom level.</param>
    /// <param name="scrollDelta">The scroll delta (positive = zoom in).</param>
    /// <param name="zoomFactor">The zoom factor per scroll unit.</param>
    /// <returns>The new zoom level, clamped to valid range.</returns>
    public static double ComputeZoom(double currentZoom, double scrollDelta, double zoomFactor = 1.1)
    {
        if (zoomFactor <= 1.0)
            zoomFactor = 1.1;

        double exponent = scrollDelta > 0 ? scrollDelta : -scrollDelta;
        double factor = System.Math.Pow(zoomFactor, exponent);

        double newZoom = scrollDelta > 0 ? currentZoom * factor : currentZoom / factor;

        return System.Math.Max(MinZoom, System.Math.Min(MaxZoom, newZoom));
    }

    /// <summary>Computes the zoom level and offset to fit a bounding box in the viewport.</summary>
    /// <param name="bounds">The bounding box to fit.</param>
    /// <param name="viewWidth">The viewport width.</param>
    /// <param name="viewHeight">The viewport height.</param>
    /// <returns>The offset and scale to fit the bounds.</returns>
    public static (Vector2 offset, double scale) ComputeZoomToFit(BoundingBox2D bounds, int viewWidth, int viewHeight)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return (Vector2.Zero, 1.0);

        double padding = 0.1;
        double availableWidth = viewWidth * (1.0 - padding);
        double availableHeight = viewHeight * (1.0 - padding);

        double scaleX = availableWidth / bounds.Width;
        double scaleY = availableHeight / bounds.Height;
        double scale = System.Math.Min(scaleX, scaleY);

        scale = System.Math.Max(MinZoom, System.Math.Min(MaxZoom, scale));

        double centerX = (bounds.MinX + bounds.MaxX) / 2.0;
        double centerY = (bounds.MinY + bounds.MaxY) / 2.0;

        double offsetX = viewWidth / 2.0 / scale - centerX;
        double offsetY = viewHeight / 2.0 / scale - centerY;

        return (new Vector2((float)offsetX, (float)offsetY), scale);
    }

    /// <summary>Computes zoom centered on a specific point.</summary>
    /// <param name="currentZoom">The current zoom level.</param>
    /// <param name="scrollDelta">The scroll delta.</param>
    /// <param name="screenPoint">The point to zoom toward.</param>
    /// <param name="panOffset">The current pan offset.</param>
    /// <param name="zoomFactor">The zoom factor.</param>
    /// <returns>The new zoom and adjusted pan offset.</returns>
    public static (double zoom, Vector2 panOffset) ComputeZoomAtPoint(
        double currentZoom, double scrollDelta, Vector2 screenPoint,
        Vector2 panOffset, double zoomFactor = 1.1)
    {
        double newZoom = ComputeZoom(currentZoom, scrollDelta, zoomFactor);
        double zoomRatio = newZoom / currentZoom;

        float newPanX = (float)(screenPoint.X * (1.0 - zoomRatio) + panOffset.X * zoomRatio);
        float newPanY = (float)(screenPoint.Y * (1.0 - zoomRatio) + panOffset.Y * zoomRatio);

        return (newZoom, new Vector2(newPanX, newPanY));
    }

    /// <summary>Smoothly interpolates between two zoom levels.</summary>
    /// <param name="fromZoom">The starting zoom level.</param>
    /// <param name="toZoom">The target zoom level.</param>
    /// <param name="t">Interpolation parameter (0-1).</param>
    /// <returns>The interpolated zoom level.</returns>
    public static double SmoothZoom(double fromZoom, double toZoom, double t)
    {
        t = System.Math.Max(0.0, System.Math.Min(1.0, t));

        double logFrom = System.Math.Log(fromZoom);
        double logTo = System.Math.Log(toZoom);
        double logResult = logFrom + (logTo - logFrom) * t;

        return System.Math.Exp(logResult);
    }

    /// <summary>Computes the visible world bounds given the current view state.</summary>
    /// <param name="panOffset">The current pan offset.</param>
    /// <param name="scale">The current zoom scale.</param>
    /// <param name="viewWidth">The viewport width.</param>
    /// <param name="viewHeight">The viewport height.</param>
    /// <returns>The visible bounds in world coordinates.</returns>
    public static BoundingBox2D ComputeVisibleBounds(Vector2 panOffset, double scale, double viewWidth, double viewHeight)
    {
        double worldWidth = viewWidth / scale;
        double worldHeight = viewHeight / scale;

        double minX = -panOffset.X - worldWidth / 2.0;
        double minY = -panOffset.Y - worldHeight / 2.0;
        double maxX = minX + worldWidth;
        double maxY = minY + worldHeight;

        return new BoundingBox2D
        {
            MinX = minX,
            MinY = minY,
            MaxX = maxX,
            MaxY = maxY
        };
    }

    /// <summary>Clamps a zoom value to the valid range.</summary>
    /// <param name="zoom">The zoom value to clamp.</param>
    /// <returns>The clamped zoom value.</returns>
    public static double ClampZoom(double zoom)
    {
        return System.Math.Max(MinZoom, System.Math.Min(MaxZoom, zoom));
    }
}

/// <summary>Represents a 2D axis-aligned bounding box.</summary>
public sealed class BoundingBox2D
{
    /// <summary>Gets or sets the minimum X coordinate.</summary>
    public double MinX { get; set; }

    /// <summary>Gets or sets the minimum Y coordinate.</summary>
    public double MinY { get; set; }

    /// <summary>Gets or sets the maximum X coordinate.</summary>
    public double MaxX { get; set; }

    /// <summary>Gets or sets the maximum Y coordinate.</summary>
    public double MaxY { get; set; }

    /// <summary>Gets the width of the bounding box.</summary>
    public double Width => MaxX - MinX;

    /// <summary>Gets the height of the bounding box.</summary>
    public double Height => MaxY - MinY;

    /// <summary>Gets the center point of the bounding box.</summary>
    public Vector2 Center => new Vector2((float)((MinX + MaxX) / 2.0), (float)((MinY + MaxY) / 2.0));

    /// <summary>Creates a bounding box from a set of points.</summary>
    /// <param name="points">The points to bound.</param>
    /// <returns>The bounding box.</returns>
    public static BoundingBox2D FromPoints(System.Collections.Generic.IEnumerable<Vector2> points)
    {
        double minX = double.MaxValue;
        double minY = double.MaxValue;
        double maxX = double.MinValue;
        double maxY = double.MinValue;

        foreach (var pt in points)
        {
            minX = System.Math.Min(minX, pt.X);
            minY = System.Math.Min(minY, pt.Y);
            maxX = System.Math.Max(maxX, pt.X);
            maxY = System.Math.Max(maxY, pt.Y);
        }

        return new BoundingBox2D { MinX = minX, MinY = minY, MaxX = maxX, MaxY = maxY };
    }

    /// <summary>Checks if a point is inside the bounding box.</summary>
    /// <param name="point">The point to test.</param>
    /// <returns>True if the point is inside.</returns>
    public bool Contains(Vector2 point)
    {
        return point.X >= MinX && point.X <= MaxX && point.Y >= MinY && point.Y <= MaxY;
    }

    /// <summary>Expands the bounding box by a margin.</summary>
    /// <param name="margin">The margin to add on all sides.</param>
    /// <returns>The expanded bounding box.</returns>
    public BoundingBox2D Expand(double margin)
    {
        return new BoundingBox2D
        {
            MinX = MinX - margin,
            MinY = MinY - margin,
            MaxX = MaxX + margin,
            MaxY = MaxY + margin
        };
    }
}
