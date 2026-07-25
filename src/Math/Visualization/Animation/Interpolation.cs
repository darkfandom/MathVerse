namespace MathVerse.Math.Visualization.Animation;

/// <summary>A lightweight 3D vector for animation computations.</summary>
public readonly record struct Vector3
{
    /// <summary>X component.</summary>
    public double X { get; init; }

    /// <summary>Y component.</summary>
    public double Y { get; init; }

    /// <summary>Z component.</summary>
    public double Z { get; init; }

    /// <summary>Creates a new Vector3.</summary>
    /// <param name="x">X component.</param>
    /// <param name="y">Y component.</param>
    /// <param name="z">Z component.</param>
    public Vector3(double x, double y, double z) : this()
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>Zero vector.</summary>
    public static Vector3 Zero => new(0.0, 0.0, 0.0);

    /// <summary>Returns the magnitude of this vector.</summary>
    public double Magnitude => System.Math.Sqrt(X * X + Y * Y + Z * Z);

    /// <summary>Returns a normalized copy of this vector.</summary>
    public Vector3 Normalized
    {
        get
        {
            double mag = Magnitude;
            if (mag < 1e-15) return Zero;
            return new Vector3(X / mag, Y / mag, Z / mag);
        }
    }

    /// <summary>Computes the dot product of two vectors.</summary>
    /// <param name="a">First vector.</param>
    /// <param name="b">Second vector.</param>
    /// <returns>Dot product scalar.</returns>
    public static double Dot(Vector3 a, Vector3 b)
        => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    /// <summary>Computes the cross product of two vectors.</summary>
    /// <param name="a">First vector.</param>
    /// <param name="b">Second vector.</param>
    /// <returns>Cross product vector.</returns>
    public static Vector3 Cross(Vector3 a, Vector3 b)
        => new(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X);

    /// <summary>Linearly interpolates between two vectors.</summary>
    /// <param name="a">Start vector.</param>
    /// <param name="b">End vector.</param>
    /// <param name="t">Interpolation parameter (0-1).</param>
    /// <returns>Interpolated vector.</returns>
    public static Vector3 Lerp(Vector3 a, Vector3 b, double t)
        => new(
            a.X + (b.X - a.X) * t,
            a.Y + (b.Y - a.Y) * t,
            a.Z + (b.Z - a.Z) * t);

    /// <summary>Addition operator.</summary>
    public static Vector3 operator +(Vector3 a, Vector3 b)
        => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    /// <summary>Subtraction operator.</summary>
    public static Vector3 operator -(Vector3 a, Vector3 b)
        => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    /// <summary>Scalar multiplication operator.</summary>
    public static Vector3 operator *(Vector3 v, double s)
        => new(v.X * s, v.Y * s, v.Z * s);

    /// <summary>Scalar multiplication operator.</summary>
    public static Vector3 operator *(double s, Vector3 v)
        => new(v.X * s, v.Y * s, v.Z * s);

    /// <summary>Returns a string representation.</summary>
    public override string ToString() => $"({X:F4}, {Y:F4}, {Z:F4})";
}

/// <summary>Interpolation utilities for animation and visualization.</summary>
public sealed class Interpolation
{
    /// <summary>
    /// Linear interpolation between two scalars.
    /// </summary>
    /// <param name="a">Start value.</param>
    /// <param name="b">End value.</param>
    /// <param name="t">Interpolation parameter (0-1).</param>
    /// <returns>Interpolated value.</returns>
    public static double Lerp(double a, double b, double t)
    {
        return a + (b - a) * System.Math.Clamp(t, 0.0, 1.0);
    }

    /// <summary>
    /// Spherical linear interpolation between two vectors (slerp).
    /// </summary>
    /// <param name="a">Start vector.</param>
    /// <param name="b">End vector.</param>
    /// <param name="t">Interpolation parameter (0-1).</param>
    /// <returns>Interpolated vector along the great circle.</returns>
    public static Vector3 Slerp(Vector3 a, Vector3 b, double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);

        double dot = Vector3.Dot(a, b);
        dot = System.Math.Clamp(dot, -1.0, 1.0);

        double theta = System.Math.Acos(dot);
        if (theta < 1e-6)
            return Vector3.Lerp(a, b, t);

        double sinTheta = System.Math.Sin(theta);
        double wA = System.Math.Sin((1.0 - t) * theta) / sinTheta;
        double wB = System.Math.Sin(t * theta) / sinTheta;

        return new Vector3(
            a.X * wA + b.X * wB,
            a.Y * wA + b.Y * wB,
            a.Z * wA + b.Z * wB);
    }

    /// <summary>
    /// Catmull-Rom spline interpolation through four control points.
    /// </summary>
    /// <param name="p0">First control point.</param>
    /// <param name="p1">Second control point (curve starts near here).</param>
    /// <param name="p2">Third control point (curve ends near here).</param>
    /// <param name="p3">Fourth control point.</param>
    /// <param name="t">Interpolation parameter (0-1).</param>
    /// <returns>Interpolated point on the spline.</returns>
    public static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);
        double t2 = t * t;
        double t3 = t2 * t;

        double x = 0.5 * (
            (2.0 * p1.X) +
            (-p0.X + p2.X) * t +
            (2.0 * p0.X - 5.0 * p1.X + 4.0 * p2.X - p3.X) * t2 +
            (-p0.X + 3.0 * p1.X - 3.0 * p2.X + p3.X) * t3);

        double y = 0.5 * (
            (2.0 * p1.Y) +
            (-p0.Y + p2.Y) * t +
            (2.0 * p0.Y - 5.0 * p1.Y + 4.0 * p2.Y - p3.Y) * t2 +
            (-p0.Y + 3.0 * p1.Y - 3.0 * p2.Y + p3.Y) * t3);

        double z = 0.5 * (
            (2.0 * p1.Z) +
            (-p0.Z + p2.Z) * t +
            (2.0 * p0.Z - 5.0 * p1.Z + 4.0 * p2.Z - p3.Z) * t2 +
            (-p0.Z + 3.0 * p1.Z - 3.0 * p2.Z + p3.Z) * t3);

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Hermite spline interpolation with position and tangent control.
    /// </summary>
    /// <param name="p0">Start position.</param>
    /// <param name="m0">Start tangent.</param>
    /// <param name="p1">End position.</param>
    /// <param name="m1">End tangent.</param>
    /// <param name="t">Interpolation parameter (0-1).</param>
    /// <returns>Interpolated point.</returns>
    public static Vector3 Hermite(Vector3 p0, Vector3 m0, Vector3 p1, Vector3 m1, double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);
        double t2 = t * t;
        double t3 = t2 * t;

        double h00 = 2.0 * t3 - 3.0 * t2 + 1.0;
        double h10 = t3 - 2.0 * t2 + t;
        double h01 = -2.0 * t3 + 3.0 * t2;
        double h11 = t3 - t2;

        return new Vector3(
            h00 * p0.X + h10 * m0.X + h01 * p1.X + h11 * m1.X,
            h00 * p0.Y + h10 * m0.Y + h01 * p1.Y + h11 * m1.Y,
            h00 * p0.Z + h10 * m0.Z + h01 * p1.Z + h11 * m1.Z);
    }

    /// <summary>
    /// Multi-point Bézier curve evaluation using de Casteljau's algorithm.
    /// </summary>
    /// <param name="points">Control points defining the Bézier curve.</param>
    /// <param name="t">Interpolation parameter (0-1).</param>
    /// <returns>Point on the Bézier curve.</returns>
    public static double[] Bezier(double[] points, double t)
    {
        if (points == null || points.Length == 0)
            return [];

        if (points.Length == 1)
            return [points[0]];

        t = System.Math.Clamp(t, 0.0, 1.0);
        int n = points.Length;
        var temp = new double[n];
        System.Array.Copy(points, temp, n);

        for (int k = 1; k < n; k++)
        {
            for (int i = 0; i < n - k; i++)
            {
                temp[i] = temp[i] * (1.0 - t) + temp[i + 1] * t;
            }
        }

        return [temp[0]];
    }

    /// <summary>
    /// Multi-dimensional Bézier curve evaluation using de Casteljau's algorithm.
    /// </summary>
    /// <param name="controlPoints">Array of control points, each being a double array.</param>
    /// <param name="t">Interpolation parameter (0-1).</param>
    /// <returns>Interpolated multi-dimensional point.</returns>
    public static double[] BezierMulti(double[][] controlPoints, double t)
    {
        if (controlPoints == null || controlPoints.Length == 0)
            return [];

        t = System.Math.Clamp(t, 0.0, 1.0);
        int n = controlPoints.Length;
        int dims = controlPoints[0].Length;

        double[][] temp = new double[n][];
        for (int i = 0; i < n; i++)
        {
            temp[i] = new double[dims];
            System.Array.Copy(controlPoints[i], temp[i], dims);
        }

        for (int k = 1; k < n; k++)
        {
            for (int i = 0; i < n - k; i++)
            {
                for (int d = 0; d < dims; d++)
                {
                    temp[i][d] = temp[i][d] * (1.0 - t) + temp[i + 1][d] * t;
                }
            }
        }

        return temp[0];
    }
}
