namespace MathVerse.Math.Visualization.Interaction;
using System.Numerics;

/// <summary>Provides 3D rotation interaction functionality.</summary>
public sealed class RotateTool
{
    private const double DefaultSensitivity = 0.5;
    private const double MaxElevation = System.Math.PI / 2.0 - 0.01;

    /// <summary>Computes the rotation deltas from a mouse drag.</summary>
    /// <param name="start">The start position of the drag.</param>
    /// <param name="end">The end position of the drag.</param>
    /// <param name="sensitivity">The rotation sensitivity factor.</param>
    /// <returns>The azimuth and elevation deltas in radians.</returns>
    public static (double azimuthDelta, double elevationDelta) ComputeRotation(Vector2 start, Vector2 end, double sensitivity = 0.5)
    {
        double dx = end.X - start.X;
        double dy = end.Y - start.Y;

        double azimuthDelta = dx * sensitivity * System.Math.PI / 180.0;
        double elevationDelta = -dy * sensitivity * System.Math.PI / 180.0;

        return (azimuthDelta, elevationDelta);
    }

    /// <summary>Applies rotation deltas to current angles with clamping.</summary>
    /// <param name="currentAzimuth">The current azimuth angle in radians.</param>
    /// <param name="currentElevation">The current elevation angle in radians.</param>
    /// <param name="azimuthDelta">The azimuth delta to apply.</param>
    /// <param name="elevationDelta">The elevation delta to apply.</param>
    /// <returns>The new azimuth and elevation angles.</returns>
    public static (double azimuth, double elevation) ApplyRotation(
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

    /// <summary>Computes a rotation quaternion from azimuth and elevation.</summary>
    /// <param name="azimuth">The azimuth angle in radians.</param>
    /// <param name="elevation">The elevation angle in radians.</param>
    /// <returns>The rotation quaternion.</returns>
    public static Quaternion ComputeRotationQuaternion(double azimuth, double elevation)
    {
        Quaternion yaw = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)azimuth);
        Quaternion pitch = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)elevation);

        return yaw * pitch;
    }

    /// <summary>Computes the camera position from rotation angles and distance.</summary>
    /// <param name="azimuth">The azimuth angle in radians.</param>
    /// <param name="elevation">The elevation angle in radians.</param>
    /// <param name="distance">The distance from the target.</param>
    /// <param name="target">The target point to orbit around.</param>
    /// <returns>The camera position in world coordinates.</returns>
    public static Vector3 ComputeCameraPosition(double azimuth, double elevation, double distance, Vector3 target)
    {
        float x = (float)(distance * System.Math.Cos(elevation) * System.Math.Sin(azimuth));
        float y = (float)(distance * System.Math.Sin(elevation));
        float z = (float)(distance * System.Math.Cos(elevation) * System.Math.Cos(azimuth));

        return target + new Vector3(x, y, z);
    }

    /// <summary>Computes a trackball rotation from screen drag.</summary>
    /// <param name="start">The start position on screen.</param>
    /// <param name="end">The end position on screen.</param>
    /// <param name="radius">The trackball radius.</param>
    /// <returns>The rotation quaternion.</returns>
    public static Quaternion ComputeTrackballRotation(Vector2 start, Vector2 end, double radius)
    {
        Vector3 p1 = MapToSphere(start, radius);
        Vector3 p2 = MapToSphere(end, radius);

        Vector3 axis = Vector3.Cross(p1, p2);
        float dot = Vector3.Dot(p1, p2);
        dot = System.Math.Max(-1.0f, System.Math.Min(1.0f, dot));

        float angle = (float)System.Math.Acos(dot);

        if (axis.LengthSquared() < 1e-10f)
            return Quaternion.Identity;

        axis = Vector3.Normalize(axis);
        return Quaternion.CreateFromAxisAngle(axis, angle);
    }

    /// <summary>Interpolates between two rotation states.</summary>
    /// <param name="from">The starting rotation quaternion.</param>
    /// <param name="to">The target rotation quaternion.</param>
    /// <param name="t">Interpolation parameter (0-1).</param>
    /// <returns>The interpolated rotation quaternion.</returns>
    public static Quaternion InterpolateRotation(Quaternion from, Quaternion to, double t)
    {
        t = System.Math.Max(0.0, System.Math.Min(1.0, t));
        return Quaternion.Slerp(from, to, (float)t);
    }

    /// <summary>Computes the view matrix from camera parameters.</summary>
    /// <param name="position">The camera position.</param>
    /// <param name="target">The look-at target.</param>
    /// <param name="up">The up vector.</param>
    /// <returns>The view matrix.</returns>
    public static Matrix4x4 ComputeViewMatrix(Vector3 position, Vector3 target, Vector3 up)
    {
        Vector3 forward = Vector3.Normalize(target - position);
        Vector3 right = Vector3.Normalize(Vector3.Cross(up, forward));
        Vector3 actualUp = Vector3.Cross(forward, right);

        return new Matrix4x4(
            right.X, actualUp.X, forward.X, 0,
            right.Y, actualUp.Y, forward.Y, 0,
            right.Z, actualUp.Z, forward.Z, 0,
            -Vector3.Dot(right, position),
            -Vector3.Dot(actualUp, position),
            -Vector3.Dot(forward, position),
            1
        );
    }

    private static Vector3 MapToSphere(Vector2 point, double radius)
    {
        float x = point.X / (float)radius;
        float y = -point.Y / (float)radius;

        float r2 = x * x + y * y;

        if (r2 > 1.0f)
        {
            float s = 1.0f / (float)System.Math.Sqrt(r2);
            return new Vector3(x * s, y * s, 0);
        }

        float z = (float)System.Math.Sqrt(1.0 - r2);
        return new Vector3(x, y, z);
    }
}
