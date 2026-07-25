namespace MathVerse.Math.Geometry.Cameras;

using Geometry3D;
using Transformations;

/// <summary>An orthographic projection camera.</summary>
public sealed record OrthographicCamera : Camera
{
    /// <summary>Half-width of the orthographic view.</summary>
    public double HalfWidth { get; init; } = 10.0;

    /// <summary>Half-height of the orthographic view.</summary>
    public double HalfHeight { get; init; } = 10.0;

    /// <summary>Computes the orthographic view matrix.</summary>
    /// <returns>The view transformation matrix.</returns>
    public override Transform3D GetViewMatrix() => Transform3D.LookAt(Position, Target, Up);

    /// <summary>Computes the orthographic projection matrix.</summary>
    /// <returns>The orthographic projection matrix.</returns>
    public override Transform3D GetProjectionMatrix()
    {
        double range = FarPlane - NearPlane;

        double[][] m = new double[4][];
        for (int i = 0; i < 4; i++) { m[i] = new double[4]; m[i][i] = 1.0; }
        m[0][0] = 1.0 / HalfWidth;
        m[1][1] = 1.0 / HalfHeight;
        m[2][2] = -2.0 / range;
        m[2][3] = -(FarPlane + NearPlane) / range;

        return Transform3D.FromRowMajor(m);
    }
}
