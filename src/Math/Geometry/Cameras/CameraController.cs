namespace MathVerse.Math.Geometry.Cameras;

using Geometry3D;
using Transformations;

/// <summary>Provides interactive control over a camera's position and orientation.</summary>
public sealed class CameraController
{
    private Camera _camera;
    private readonly Point3D _initialPosition;
    private readonly Point3D _initialTarget;
    private readonly Vector3D _initialUp;

    /// <summary>Initializes a new camera controller.</summary>
    /// <param name="camera">The camera to control.</param>
    public CameraController(Camera camera)
    {
        _camera = camera;
        _initialPosition = camera.Position;
        _initialTarget = camera.Target;
        _initialUp = camera.Up;
    }

    /// <summary>Gets or sets the controlled camera.</summary>
    public Camera Camera
    {
        get => _camera;
        set => _camera = value;
    }

    /// <summary>Moves the camera forward along its look direction.</summary>
    /// <param name="distance">The distance to move.</param>
    public void MoveForward(double distance)
    {
        Vector3D forward = _camera.Forward;
        _camera = _camera with
        {
            Position = new Point3D(
                _camera.Position.X + forward.X * distance,
                _camera.Position.Y + forward.Y * distance,
                _camera.Position.Z + forward.Z * distance),
            Target = new Point3D(
                _camera.Target.X + forward.X * distance,
                _camera.Target.Y + forward.Y * distance,
                _camera.Target.Z + forward.Z * distance)
        };
    }

    /// <summary>Moves the camera right along its right direction.</summary>
    /// <param name="distance">The distance to move.</param>
    public void MoveRight(double distance)
    {
        Vector3D right = _camera.Right;
        _camera = _camera with
        {
            Position = new Point3D(
                _camera.Position.X + right.X * distance,
                _camera.Position.Y + right.Y * distance,
                _camera.Position.Z + right.Z * distance),
            Target = new Point3D(
                _camera.Target.X + right.X * distance,
                _camera.Target.Y + right.Y * distance,
                _camera.Target.Z + right.Z * distance)
        };
    }

    /// <summary>Moves the camera up along its up direction.</summary>
    /// <param name="distance">The distance to move.</param>
    public void MoveUp(double distance)
    {
        Vector3D up = _camera.Up;
        _camera = _camera with
        {
            Position = new Point3D(
                _camera.Position.X + up.X * distance,
                _camera.Position.Y + up.Y * distance,
                _camera.Position.Z + up.Z * distance),
            Target = new Point3D(
                _camera.Target.X + up.X * distance,
                _camera.Target.Y + up.Y * distance,
                _camera.Target.Z + up.Z * distance)
        };
    }

    /// <summary>Rotates the camera by the given yaw and pitch angles.</summary>
    /// <param name="yawRadians">The yaw angle (rotation around the up axis) in radians.</param>
    /// <param name="pitchRadians">The pitch angle (rotation around the right axis) in radians.</param>
    public void Rotate(double yawRadians, double pitchRadians)
    {
        if (System.Math.Abs(yawRadians) < 1e-15 && System.Math.Abs(pitchRadians) < 1e-15)
            return;
        
        Vector3D forward = _camera.Forward;
        Vector3D up = _camera.Up;

        if (yawRadians != 0.0)
        {
            Transform3D yawRotation = Transform3D.RotationAxis(up, yawRadians);
            forward = yawRotation.TransformVector(forward);
        }

        if (pitchRadians != 0.0)
        {
            Vector3D right = forward.Cross(up).Normalize();
            Transform3D pitchRotation = Transform3D.RotationAxis(right, pitchRadians);
            forward = pitchRotation.TransformVector(forward);
        }

        _camera = _camera with
        {
            Target = new Point3D(
                _camera.Position.X + forward.X,
                _camera.Position.Y + forward.Y,
                _camera.Position.Z + forward.Z)
        };
    }

    /// <summary>Points the camera at the specified target position.</summary>
    /// <param name="target">The target point to look at.</param>
    public void LookAt(Point3D target)
    {
        _camera = _camera with { Target = target };
    }

    /// <summary>Resets the camera to its initial position, target, and up vector.</summary>
    public void Reset()
    {
        _camera = _camera with
        {
            Position = _initialPosition,
            Target = _initialTarget,
            Up = _initialUp
        };
    }
}
