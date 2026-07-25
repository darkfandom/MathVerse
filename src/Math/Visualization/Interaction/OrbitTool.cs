namespace MathVerse.Math.Visualization.Interaction;
using System.Numerics;

/// <summary>Provides orbit camera interaction functionality.</summary>
public sealed class OrbitTool
{
    private const double DefaultSensitivity = 0.5;
    private const double MinDistance = 0.1;
    private const double MaxDistance = 10000.0;
    private const double MaxElevation = System.Math.PI / 2.0 - 0.01;

    /// <summary>Computes new orbit angles from a mouse drag.</summary>
    /// <param name="start">The start position of the drag.</param>
    /// <param name="end">The end position of the drag.</param>
    /// <param name="center">The orbit center point.</param>
    /// <param name="distance">The distance from center.</param>
    /// <param name="sensitivity">The rotation sensitivity.</param>
    /// <returns>The new azimuth and elevation angles in radians.</returns>
    public static (double azimuth, double elevation) ComputeOrbit(
        Vector2 start, Vector2 end, Vector3 center, double distance, double sensitivity = 0.5)
    {
        double dx = end.X - start.X;
        double dy = end.Y - start.Y;

        double azimuthDelta = dx * sensitivity * System.Math.PI / 180.0;
        double elevationDelta = -dy * sensitivity * System.Math.PI / 180.0;

        return (azimuthDelta, elevationDelta);
    }

    /// <summary>Computes the full orbit state from deltas and current state.</summary>
    /// <param name="currentAzimuth">The current azimuth angle.</param>
    /// <param name="currentElevation">The current elevation angle.</param>
    /// <param name="azimuthDelta">The azimuth change.</param>
    /// <param name="elevationDelta">The elevation change.</param>
    /// <returns>The updated azimuth and elevation.</returns>
    public static (double azimuth, double elevation) UpdateOrbit(
        double currentAzimuth, double currentElevation,
        double azimuthDelta, double elevationDelta)
    {
        double newAzimuth = currentAzimuth + azimuthDelta;
        double newElevation = currentElevation + elevationDelta;

        newElevation = System.Math.Max(-MaxElevation, System.Math.Min(MaxElevation, newElevation));

        newAzimuth = newAzimuth % (2.0 * System.Math.PI);
        if (newAzimuth < 0)
            newAzimuth += 2.0 * System.Math.PI;

        return (newAzimuth, newElevation);
    }

    /// <summary>Computes the camera position for an orbit view.</summary>
    /// <param name="azimuth">The azimuth angle in radians.</param>
    /// <param name="elevation">The elevation angle in radians.</param>
    /// <param name="distance">The distance from the center.</param>
    /// <param name="center">The center point to orbit around.</param>
    /// <returns>The camera position in world coordinates.</returns>
    public static Vector3 ComputeOrbitPosition(double azimuth, double elevation, double distance, Vector3 center)
    {
        float x = (float)(distance * System.Math.Cos(elevation) * System.Math.Sin(azimuth));
        float y = (float)(distance * System.Math.Sin(elevation));
        float z = (float)(distance * System.Math.Cos(elevation) * System.Math.Cos(azimuth));

        return center + new Vector3(x, y, z);
    }

    /// <summary>Computes the view matrix for an orbit camera.</summary>
    /// <param name="azimuth">The azimuth angle.</param>
    /// <param name="elevation">The elevation angle.</param>
    /// <param name="distance">The distance from center.</param>
    /// <param name="center">The orbit center.</param>
    /// <param name="up">The up vector.</param>
    /// <returns>The view matrix.</returns>
    public static Matrix4x4 ComputeViewMatrix(double azimuth, double elevation, double distance, Vector3 center, Vector3 up)
    {
        Vector3 position = ComputeOrbitPosition(azimuth, elevation, distance, center);
        return RotateTool.ComputeViewMatrix(position, center, up);
    }

    /// <summary>Computes the perspective projection matrix.</summary>
    /// <param name="fieldOfView">The field of view in radians.</param>
    /// <param name="aspectRatio">The aspect ratio (width/height).</param>
    /// <param name="nearPlane">The near clipping plane.</param>
    /// <param name="farPlane">The far clipping plane.</param>
    /// <returns>The perspective projection matrix.</returns>
    public static Matrix4x4 ComputePerspectiveProjection(double fieldOfView, double aspectRatio, double nearPlane, double farPlane)
    {
        float f = 1.0f / (float)System.Math.Tan(fieldOfView / 2.0);

        float range = (float)(farPlane - nearPlane);

        return new Matrix4x4(
            f / (float)aspectRatio, 0, 0, 0,
            0, f, 0, 0,
            0, 0, (float)(-farPlane / range), -1,
            0, 0, (float)(-nearPlane * farPlane / range), 0
        );
    }

    /// <summary>Zooms the orbit camera by adjusting distance.</summary>
    /// <param name="currentDistance">The current distance.</param>
    /// <param name="scrollDelta">The scroll delta.</param>
    /// <param name="zoomFactor">The zoom factor per scroll unit.</param>
    /// <returns>The new distance.</returns>
    public static double ZoomOrbit(double currentDistance, double scrollDelta, double zoomFactor = 1.1)
    {
        double factor = scrollDelta > 0 ? zoomFactor : 1.0 / zoomFactor;
        double newDistance = currentDistance * System.Math.Pow(factor, System.Math.Abs(scrollDelta));

        return System.Math.Max(MinDistance, System.Math.Min(MaxDistance, newDistance));
    }

    /// <summary>Smoothly animates the orbit camera to a target state.</summary>
    /// <param name="currentAzimuth">The current azimuth.</param>
    /// <param name="currentElevation">The current elevation.</param>
    /// <param name="currentDistance">The current distance.</param>
    /// <param name="targetAzimuth">The target azimuth.</param>
    /// <param name="targetElevation">The target elevation.</param>
    /// <param name="targetDistance">The target distance.</param>
    /// <param name="t">Interpolation parameter (0-1).</param>
    /// <returns>The interpolated orbit state.</returns>
    public static (double azimuth, double elevation, double distance) AnimateOrbit(
        double currentAzimuth, double currentElevation, double currentDistance,
        double targetAzimuth, double targetElevation, double targetDistance,
        double t)
    {
        t = System.Math.Max(0.0, System.Math.Min(1.0, t));
        float ft = (float)t;

        double newAzimuth = currentAzimuth + (targetAzimuth - currentAzimuth) * t;
        double newElevation = currentElevation + (targetElevation - currentElevation) * t;
        double newDistance = currentDistance + (targetDistance - currentDistance) * t;

        newDistance = System.Math.Max(MinDistance, System.Math.Min(MaxDistance, newDistance));
        newElevation = System.Math.Max(-MaxElevation, System.Math.Min(MaxElevation, newElevation));

        return (newAzimuth, newElevation, newDistance);
    }

    /// <summary>Resets orbit to default front view.</summary>
    /// <returns>Default azimuth, elevation, and distance.</returns>
    public static (double azimuth, double elevation, double distance) GetDefaultOrbit()
    {
        return (0.0, 0.3, 5.0);
    }

    /// <summary>Gets preset orbit angles for standard views.</summary>
    /// <param name="view">The standard view.</param>
    /// <returns>The azimuth and elevation for the view.</returns>
    public static (double azimuth, double elevation) GetPresetView(StandardView view)
    {
        return view switch
        {
            StandardView.Front => (0.0, 0.0),
            StandardView.Back => (System.Math.PI, 0.0),
            StandardView.Left => (-System.Math.PI / 2.0, 0.0),
            StandardView.Right => (System.Math.PI / 2.0, 0.0),
            StandardView.Top => (0.0, System.Math.PI / 2.0 - 0.01),
            StandardView.Bottom => (0.0, -System.Math.PI / 2.0 + 0.01),
            StandardView.FrontTopRight => (System.Math.PI / 4.0, System.Math.PI / 4.0),
            _ => (0.0, 0.0)
        };
    }
}

/// <summary>Defines standard camera view presets.</summary>
public enum StandardView
{
    /// <summary>Front view.</summary>
    Front,

    /// <summary>Back view.</summary>
    Back,

    /// <summary>Left view.</summary>
    Left,

    /// <summary>Right view.</summary>
    Right,

    /// <summary>Top view.</summary>
    Top,

    /// <summary>Bottom view.</summary>
    Bottom,

    /// <summary>Isometric front-top-right view.</summary>
    FrontTopRight
}
