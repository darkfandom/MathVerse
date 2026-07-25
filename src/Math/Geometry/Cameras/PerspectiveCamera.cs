namespace MathVerse.Math.Geometry.Cameras;

using Geometry3D;
using Transformations;

/// <summary>A perspective projection camera.</summary>
public sealed record PerspectiveCamera : Camera
{
    /// <summary>Computes the perspective view matrix.</summary>
    /// <returns>The view transformation matrix.</returns>
    public override Transform3D GetViewMatrix() => Transform3D.LookAt(Position, Target, Up);

    /// <summary>Computes the perspective projection matrix.</summary>
    /// <returns>The perspective projection matrix.</returns>
    public override Transform3D GetProjectionMatrix()
    {
        double fovRad = FieldOfView * System.Math.PI / 180.0;
        double tanHalfFov = System.Math.Tan(fovRad / 2.0);
        double range = FarPlane - NearPlane;

        double[][] m = new double[4][];
        for (int i = 0; i < 4; i++) { m[i] = new double[4]; m[i][i] = 1.0; }
        m[0][0] = 1.0 / (AspectRatio * tanHalfFov);
        m[1][1] = 1.0 / tanHalfFov;
        m[2][2] = -(FarPlane + NearPlane) / range;
        m[2][3] = -2.0 * FarPlane * NearPlane / range;
        m[3][2] = -1.0;
        m[3][3] = 0.0;

        return Transform3D.FromRowMajor(m);
    }
}
