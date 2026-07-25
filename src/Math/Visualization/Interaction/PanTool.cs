namespace MathVerse.Math.Visualization.Interaction;
using System.Numerics;

/// <summary>Provides pan interaction functionality for 2D visualization.</summary>
public sealed class PanTool
{
    /// <summary>Computes the pan delta from a drag gesture.</summary>
    /// <param name="start">The start position of the drag.</param>
    /// <param name="end">The end position of the drag.</param>
    /// <param name="scale">The current zoom scale factor.</param>
    /// <returns>The pan offset in world coordinates.</returns>
    public static Vector2 ComputePan(Vector2 start, Vector2 end, double scale)
    {
        if (scale <= 0)
            scale = 1.0;

        float dx = (end.X - start.X) / (float)scale;
        float dy = (end.Y - start.Y) / (float)scale;

        return new Vector2(dx, dy);
    }

    /// <summary>Computes the pan offset with momentum for smooth scrolling.</summary>
    /// <param name="start">The start position.</param>
    /// <param name="end">The end position.</param>
    /// <param name="scale">The current zoom scale.</param>
    /// <param name="velocity">The pan velocity for momentum.</param>
    /// <returns>The pan offset with momentum applied.</returns>
    public static Vector2 ComputePanWithMomentum(Vector2 start, Vector2 end, double scale, Vector2 velocity)
    {
        Vector2 basePan = ComputePan(start, end, scale);

        float momentumX = basePan.X + velocity.X * 0.3f;
        float momentumY = basePan.Y + velocity.Y * 0.3f;

        return new Vector2(momentumX, momentumY);
    }

    /// <summary>Clamps a pan offset to prevent panning beyond scene bounds.</summary>
    /// <param name="offset">The proposed pan offset.</param>
    /// <param name="sceneWidth">The width of the scene.</param>
    /// <param name="sceneHeight">The height of the scene.</param>
    /// <param name="viewWidth">The width of the viewport.</param>
    /// <param name="viewHeight">The height of the viewport.</param>
    /// <param name="scale">The current zoom scale.</param>
    /// <returns>The clamped pan offset.</returns>
    public static Vector2 ClampPan(Vector2 offset, double sceneWidth, double sceneHeight,
        double viewWidth, double viewHeight, double scale)
    {
        double scaledWidth = sceneWidth * scale;
        double scaledHeight = sceneHeight * scale;

        double minX = -(scaledWidth - viewWidth) / 2.0;
        double maxX = (scaledWidth - viewWidth) / 2.0;
        double minY = -(scaledHeight - viewHeight) / 2.0;
        double maxY = (scaledHeight - viewHeight) / 2.0;

        float clampedX = (float)System.Math.Max(minX, System.Math.Min(maxX, offset.X));
        float clampedY = (float)System.Math.Max(minY, System.Math.Min(maxY, offset.Y));

        return new Vector2(clampedX, clampedY);
    }

    /// <summary>Computes the velocity of a pan gesture for momentum effects.</summary>
    /// <param name="positions">The recorded positions during the gesture.</param>
    /// <param name="timestamps">The corresponding timestamps in milliseconds.</param>
    /// <returns>The computed velocity vector.</returns>
    public static Vector2 ComputeVelocity(System.Collections.Generic.List<Vector2> positions, System.Collections.Generic.List<double> timestamps)
    {
        if (positions.Count < 2 || timestamps.Count < 2)
            return Vector2.Zero;

        int lastIdx = positions.Count - 1;
        int prevIdx = positions.Count - 2;

        double dt = (timestamps[lastIdx] - timestamps[prevIdx]) / 1000.0;
        if (dt <= 0)
            return Vector2.Zero;

        float vx = (positions[lastIdx].X - positions[prevIdx].X) / (float)dt;
        float vy = (positions[lastIdx].Y - positions[prevIdx].Y) / (float)dt;

        float damping = 0.8f;
        return new Vector2(vx * damping, vy * damping);
    }

    /// <summary>Converts screen coordinates to world coordinates.</summary>
    /// <param name="screenPos">The screen position.</param>
    /// <param name="panOffset">The current pan offset.</param>
    /// <param name="scale">The current zoom scale.</param>
    /// <param name="viewCenter">The center of the viewport.</param>
    /// <returns>The corresponding world position.</returns>
    public static Vector2 ScreenToWorld(Vector2 screenPos, Vector2 panOffset, double scale, Vector2 viewCenter)
    {
        float worldX = (screenPos.X - viewCenter.X) / (float)scale + panOffset.X;
        float worldY = (screenPos.Y - viewCenter.Y) / (float)scale + panOffset.Y;

        return new Vector2(worldX, worldY);
    }

    /// <summary>Converts world coordinates to screen coordinates.</summary>
    /// <param name="worldPos">The world position.</param>
    /// <param name="panOffset">The current pan offset.</param>
    /// <param name="scale">The current zoom scale.</param>
    /// <param name="viewCenter">The center of the viewport.</param>
    /// <returns>The corresponding screen position.</returns>
    public static Vector2 WorldToScreen(Vector2 worldPos, Vector2 panOffset, double scale, Vector2 viewCenter)
    {
        float screenX = (float)((worldPos.X - panOffset.X) * scale + viewCenter.X);
        float screenY = (float)((worldPos.Y - panOffset.Y) * scale + viewCenter.Y);

        return new Vector2(screenX, screenY);
    }
}
