namespace MathVerse.Math.Visualization.Rendering;
using System.Numerics;

/// <summary>Camera for viewing the visualization scene with perspective or orthographic projection.</summary>
public sealed class Camera
{
    /// <summary>Gets or sets the position of the camera in world space.</summary>
    public Vector3 Position { get; set; } = new(0.0f, 0.0f, 5.0f);

    /// <summary>Gets or sets the target point the camera is looking at in world space.</summary>
    public Vector3 Target { get; set; } = Vector3.Zero;

    /// <summary>Gets the up direction vector for the camera.</summary>
    public Vector3 Up { get; init; } = Vector3.UnitY;

    /// <summary>Gets the vertical field of view in degrees.</summary>
    public float FieldOfView { get; init; } = 60.0f;

    /// <summary>Gets the distance to the near clipping plane.</summary>
    public float NearPlane { get; init; } = 0.1f;

    /// <summary>Gets the distance to the far clipping plane.</summary>
    public float FarPlane { get; init; } = 1000.0f;

    /// <summary>Gets the aspect ratio (width divided by height) of the viewport.</summary>
    public float AspectRatio { get; init; } = 16.0f / 9.0f;

    /// <summary>Gets the projection type used by this camera.</summary>
    public ProjectionType Projection { get; init; } = ProjectionType.Perspective;

    /// <summary>Gets the view matrix computed from the camera's position, target, and up vector.</summary>
    public Matrix4x4 ViewMatrix
    {
        get
        {
            Vector3 f = Vector3.Normalize(Target - Position);
            Vector3 s = Vector3.Normalize(Vector3.Cross(f, Up));
            Vector3 u = Vector3.Cross(s, f);

            return new Matrix4x4(
                s.X, s.Y, s.Z, -Vector3.Dot(s, Position),
                u.X, u.Y, u.Z, -Vector3.Dot(u, Position),
                -f.X, -f.Y, -f.Z, Vector3.Dot(f, Position),
                0.0f, 0.0f, 0.0f, 1.0f);
        }
    }

    /// <summary>Gets the projection matrix based on the camera's projection type and parameters.</summary>
    public Matrix4x4 ProjectionMatrix
    {
        get
        {
            if (Projection == ProjectionType.Orthographic)
                return ComputeOrthographic();

            return ComputePerspective();
        }
    }

    /// <summary>Gets the combined view-projection matrix.</summary>
    public Matrix4x4 ViewProjectionMatrix => Matrix4x4.Multiply(ViewMatrix, ProjectionMatrix);

    /// <summary>Gets the view frustum computed from the combined view-projection matrix.</summary>
    public Frustum Frustum => new(ViewProjectionMatrix);

    /// <summary>Repositions the camera on a sphere around the target using azimuth, elevation, and distance.</summary>
    /// <param name="azimuthDeg">The horizontal rotation angle in degrees.</param>
    /// <param name="elevationDeg">The vertical rotation angle in degrees, clamped to avoid gimbal lock.</param>
    /// <param name="distance">The distance from the camera to the target.</param>
    public void Orbit(float azimuthDeg, float elevationDeg, float distance)
    {
        float clampedElevation = System.Math.Clamp(elevationDeg, -89.0f, 89.0f);
        float azRad = azimuthDeg * System.MathF.PI / 180.0f;
        float elRad = clampedElevation * System.MathF.PI / 180.0f;

        float cosEl = System.MathF.Cos(elRad);
        float sinEl = System.MathF.Sin(elRad);
        float sinAz = System.MathF.Sin(azRad);
        float cosAz = System.MathF.Cos(azRad);

        Vector3 offset = new(
            distance * cosEl * sinAz,
            distance * sinEl,
            distance * cosEl * cosAz);

        Position = Target + offset;
    }

    /// <summary>Translates both the position and target by the given offsets in camera-local space.</summary>
    /// <param name="dx">The horizontal pan amount in camera space.</param>
    /// <param name="dy">The vertical pan amount in camera space.</param>
    public void Pan(float dx, float dy)
    {
        Vector3 forward = Vector3.Normalize(Target - Position);
        Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Up));
        Vector3 up = Vector3.Cross(right, forward);

        Vector3 offset = (right * dx) + (up * dy);
        Position += offset;
        Target += offset;
    }

    /// <summary>Moves the camera closer to or farther from the target along the forward direction.</summary>
    /// <param name="factor">The distance to move. Positive values move toward the target; negative values move away.</param>
    public void Zoom(float factor)
    {
        Vector3 forward = Vector3.Normalize(Target - Position);
        Position += forward * factor;
    }

    /// <summary>Computes a perspective projection matrix for row-vector conventions.</summary>
    /// <returns>The perspective projection matrix.</returns>
    private Matrix4x4 ComputePerspective()
    {
        float fovRad = FieldOfView * System.MathF.PI / 180.0f;
        float yScale = 1.0f / System.MathF.Tan(fovRad * 0.5f);
        float xScale = yScale / AspectRatio;
        float zRange = NearPlane - FarPlane;

        return new Matrix4x4(
            xScale, 0.0f, 0.0f, 0.0f,
            0.0f, yScale, 0.0f, 0.0f,
            0.0f, 0.0f, FarPlane / zRange, -1.0f,
            0.0f, 0.0f, (NearPlane * FarPlane) / zRange, 0.0f);
    }

    /// <summary>Computes an orthographic projection matrix for row-vector conventions.</summary>
    /// <returns>The orthographic projection matrix.</returns>
    private Matrix4x4 ComputeOrthographic()
    {
        float halfHeight = 5.0f;
        float halfWidth = halfHeight * AspectRatio;
        float zRange = NearPlane - FarPlane;

        return new Matrix4x4(
            1.0f / halfWidth, 0.0f, 0.0f, 0.0f,
            0.0f, 1.0f / halfHeight, 0.0f, 0.0f,
            0.0f, 0.0f, 1.0f / zRange, 0.0f,
            0.0f, 0.0f, NearPlane / zRange, 1.0f);
    }
}

/// <summary>Enumerates the available camera projection types.</summary>
public enum ProjectionType
{
    /// <summary>Perspective projection with foreshortening based on distance.</summary>
    Perspective,

    /// <summary>Orthographic projection with uniform scaling regardless of distance.</summary>
    Orthographic
}
