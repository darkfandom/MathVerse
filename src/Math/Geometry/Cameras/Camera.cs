namespace MathVerse.Math.Geometry.Cameras;

using Geometry3D;
using Transformations;

/// <summary>Abstract base class for cameras.</summary>
public abstract record Camera
{
    /// <summary>Camera field of view in degrees (for perspective).</summary>
    public double FieldOfView { get; init; } = 60.0;

    /// <summary>Aspect ratio (width / height).</summary>
    public double AspectRatio { get; init; } = 1.0;

    /// <summary>Near clipping plane distance.</summary>
    public double NearPlane { get; init; } = 0.1;

    /// <summary>Far clipping plane distance.</summary>
    public double FarPlane { get; init; } = 1000.0;

    /// <summary>Camera position.</summary>
    public Point3D Position { get; init; } = new(0, 0, 5);

    /// <summary>Camera look-at target.</summary>
    public Point3D Target { get; init; } = Point3D.Origin;

    /// <summary>Camera up vector.</summary>
    public Vector3D Up { get; init; } = Vector3D.UnitY;

    /// <summary>Computes the view matrix.</summary>
    /// <returns>The view transformation matrix.</returns>
    public abstract Transform3D GetViewMatrix();

    /// <summary>Computes the projection matrix.</summary>
    /// <returns>The projection transformation matrix.</returns>
    public abstract Transform3D GetProjectionMatrix();

    /// <summary>Forward direction (normalized).</summary>
    public Vector3D Forward => new Vector3D(Target.X - Position.X, Target.Y - Position.Y, Target.Z - Position.Z).Normalize();

    /// <summary>Right direction (normalized).</summary>
    public Vector3D Right => Forward.Cross(Up).Normalize();
}
