using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Transformations;

/// <summary>Represents a rotation quaternion with components (X, Y, Z, W), where W is the scalar part.</summary>
public readonly record struct QuaternionRotation(double X, double Y, double Z, double W)
{
    /// <summary>Gets the X component (i).</summary>
    public double X { get; } = X;

    /// <summary>Gets the Y component (j).</summary>
    public double Y { get; } = Y;

    /// <summary>Gets the Z component (k).</summary>
    public double Z { get; } = Z;

    /// <summary>Gets the W component (scalar).</summary>
    public double W { get; } = W;

    /// <summary>Gets the identity quaternion (no rotation).</summary>
    public static QuaternionRotation Identity => new(0, 0, 0, 1);

    /// <summary>Gets the length of this quaternion.</summary>
    public double Length => System.Math.Sqrt(X * X + Y * Y + Z * Z + W * W);

    /// <summary>Gets the squared length of this quaternion.</summary>
    public double LengthSquared => X * X + Y * Y + Z * Z + W * W;

    /// <summary>Creates a quaternion from an axis and an angle.</summary>
    /// <param name="axis">The axis of rotation (will be normalized).</param>
    /// <param name="angle">The rotation angle in radians.</param>
    /// <returns>The resulting quaternion.</returns>
    public static QuaternionRotation FromAxisAngle(Vector3D axis, double angle)
    {
        Vector3D n = axis.Normalize();
        double halfAngle = angle * 0.5;
        double s = System.Math.Sin(halfAngle);
        return new QuaternionRotation(n.X * s, n.Y * s, n.Z * s, System.Math.Cos(halfAngle));
    }

    /// <summary>Creates a quaternion from Euler angles using the ZXY convention.</summary>
    /// <param name="yaw">The yaw angle (rotation about Y) in radians.</param>
    /// <param name="pitch">The pitch angle (rotation about X) in radians.</param>
    /// <param name="roll">The roll angle (rotation about Z) in radians.</param>
    /// <returns>The resulting quaternion representing Rz * Rx * Ry.</returns>
    public static QuaternionRotation FromEuler(double yaw, double pitch, double roll)
    {
        QuaternionRotation qy = FromAxisAngle(Vector3D.UnitY, yaw);
        QuaternionRotation qx = FromAxisAngle(Vector3D.UnitX, pitch);
        QuaternionRotation qz = FromAxisAngle(Vector3D.UnitZ, roll);
        return qz.Multiply(qx).Multiply(qy);
    }

    /// <summary>Extracts a quaternion from a rotation matrix.</summary>
    /// <param name="m">The rotation matrix (only the upper-left 3x3 is used).</param>
    /// <returns>The resulting quaternion.</returns>
    public static QuaternionRotation FromRotationMatrix(Transform3D m)
    {
        double trace = m[0, 0] + m[1, 1] + m[2, 2];

        if (trace > 0)
        {
            double s = 0.5 / System.Math.Sqrt(trace + 1.0);
            return new QuaternionRotation(
                (m[2, 1] - m[1, 2]) * s,
                (m[0, 2] - m[2, 0]) * s,
                (m[1, 0] - m[0, 1]) * s,
                0.25 / s);
        }

        if (m[0, 0] > m[1, 1] && m[0, 0] > m[2, 2])
        {
            double s = 2.0 * System.Math.Sqrt(1.0 + m[0, 0] - m[1, 1] - m[2, 2]);
            return new QuaternionRotation(
                0.25 * s,
                (m[0, 1] + m[1, 0]) / s,
                (m[0, 2] + m[2, 0]) / s,
                (m[2, 1] - m[1, 2]) / s);
        }

        if (m[1, 1] > m[2, 2])
        {
            double s = 2.0 * System.Math.Sqrt(1.0 + m[1, 1] - m[0, 0] - m[2, 2]);
            return new QuaternionRotation(
                (m[0, 1] + m[1, 0]) / s,
                0.25 * s,
                (m[1, 2] + m[2, 1]) / s,
                (m[0, 2] - m[2, 0]) / s);
        }

        {
            double s = 2.0 * System.Math.Sqrt(1.0 + m[2, 2] - m[0, 0] - m[1, 1]);
            return new QuaternionRotation(
                (m[0, 2] + m[2, 0]) / s,
                (m[1, 2] + m[2, 1]) / s,
                0.25 * s,
                (m[1, 0] - m[0, 1]) / s);
        }
    }

    /// <summary>Returns a normalized copy of this quaternion.</summary>
    /// <returns>A unit quaternion, or <see cref="Identity"/> if the length is near zero.</returns>
    public QuaternionRotation Normalize()
    {
        double len = Length;
        if (len < 1e-15) return Identity;
        double inv = 1.0 / len;
        return new QuaternionRotation(X * inv, Y * inv, Z * inv, W * inv);
    }

    /// <summary>Returns the conjugate of this quaternion.</summary>
    /// <returns>The conjugate quaternion (-X, -Y, -Z, W).</returns>
    public QuaternionRotation Conjugate() => new(-X, -Y, -Z, W);

    /// <summary>Returns the inverse of this quaternion.</summary>
    /// <returns>The inverse quaternion.</returns>
    public QuaternionRotation Inverse()
    {
        double lenSq = LengthSquared;
        if (lenSq < 1e-30) return Identity;
        double inv = 1.0 / lenSq;
        return new QuaternionRotation(-X * inv, -Y * inv, -Z * inv, W * inv);
    }

    /// <summary>Rotates a vector by this quaternion.</summary>
    /// <param name="v">The vector to rotate.</param>
    /// <returns>The rotated vector.</returns>
    public Vector3D Rotate(Vector3D v)
    {
        QuaternionRotation qv = new(v.X, v.Y, v.Z, 0);
        QuaternionRotation result = this.Multiply(qv).Multiply(Conjugate());
        return new Vector3D(result.X, result.Y, result.Z);
    }

    /// <summary>Converts this quaternion to a <see cref="Transform3D"/> rotation matrix.</summary>
    /// <returns>The equivalent 4x4 rotation transformation.</returns>
    public Transform3D ToTransform()
    {
        double xx = X * X, yy = Y * Y, zz = Z * Z;
        double xy = X * Y, xz = X * Z, yz = Y * Z;
        double wx = W * X, wy = W * Y, wz = W * Z;

        return new Transform3D(ImmutableArray.Create(
            ImmutableArray.Create(1.0 - 2.0 * (yy + zz), 2.0 * (xy - wz), 2.0 * (xz + wy), 0.0),
            ImmutableArray.Create(2.0 * (xy + wz), 1.0 - 2.0 * (xx + zz), 2.0 * (yz - wx), 0.0),
            ImmutableArray.Create(2.0 * (xz - wy), 2.0 * (yz + wx), 1.0 - 2.0 * (xx + yy), 0.0),
            ImmutableArray.Create(0.0, 0.0, 0.0, 1.0)));
    }

    /// <summary>Multiplies this quaternion by another (this * other).</summary>
    /// <param name="other">The other quaternion.</param>
    /// <returns>The product quaternion.</returns>
    public QuaternionRotation Multiply(QuaternionRotation other) => new(
        W * other.X + X * other.W + Y * other.Z - Z * other.Y,
        W * other.Y - X * other.Z + Y * other.W + Z * other.X,
        W * other.Z + X * other.Y - Y * other.X + Z * other.W,
        W * other.W - X * other.X - Y * other.Y - Z * other.Z);

    /// <summary>Performs spherical linear interpolation between this quaternion and a target.</summary>
    /// <param name="target">The target quaternion.</param>
    /// <param name="t">The interpolation parameter (0 = this, 1 = target).</param>
    /// <returns>The interpolated quaternion.</returns>
    public QuaternionRotation Slerp(QuaternionRotation target, double t)
    {
        double dot = X * target.X + Y * target.Y + Z * target.Z + W * target.W;

        QuaternionRotation adjustedTarget = target;
        if (dot < 0)
        {
            dot = -dot;
            adjustedTarget = new QuaternionRotation(-target.X, -target.Y, -target.Z, -target.W);
        }

        if (dot > 0.9995)
        {
            double oneMinusT = 1.0 - t;
            return new QuaternionRotation(
                oneMinusT * X + t * adjustedTarget.X,
                oneMinusT * Y + t * adjustedTarget.Y,
                oneMinusT * Z + t * adjustedTarget.Z,
                oneMinusT * W + t * adjustedTarget.W).Normalize();
        }

        double theta = System.Math.Acos(System.Math.Min(dot, 1.0));
        double sinTheta = System.Math.Sin(theta);
        double a = System.Math.Sin((1.0 - t) * theta) / sinTheta;
        double b = System.Math.Sin(t * theta) / sinTheta;

        return new QuaternionRotation(
            a * X + b * adjustedTarget.X,
            a * Y + b * adjustedTarget.Y,
            a * Z + b * adjustedTarget.Z,
            a * W + b * adjustedTarget.W);
    }

    /// <summary>Multiplies two quaternions.</summary>
    /// <param name="a">The left quaternion.</param>
    /// <param name="b">The right quaternion.</param>
    /// <returns>The product a * b.</returns>
    public static QuaternionRotation operator *(QuaternionRotation a, QuaternionRotation b) => a.Multiply(b);

    /// <summary>Returns a string representation of this quaternion.</summary>
    public override string ToString() => $"QuaternionRotation({X:F6}, {Y:F6}, {Z:F6}, {W:F6})";
}
