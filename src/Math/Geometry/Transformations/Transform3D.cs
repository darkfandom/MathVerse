using System.Collections.Immutable;
using System.Numerics;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Transformations;

/// <summary>Represents an affine 3D transformation stored as a row-major 4x4 matrix.</summary>
/// <remarks>
/// The matrix uses <see cref="ImmutableArray{T}"/> for immutable value-type semantics.
/// Row-major convention: element at row i, column j is stored at Rows[i][j].
/// </remarks>
public readonly record struct Transform3D
{
    /// <summary>The 4 rows of the matrix, each containing 4 elements.</summary>
    internal readonly ImmutableArray<ImmutableArray<double>> Rows;

    internal Transform3D(ImmutableArray<ImmutableArray<double>> rows)
    {
        Rows = rows;
    }

    /// <summary>Gets the element at the specified row and column.</summary>
    /// <param name="row">The row index (0-3).</param>
    /// <param name="col">The column index (0-3).</param>
    /// <returns>The matrix element.</returns>
    public double this[int row, int col]
    {
        get => Rows[row][col];
    }

    /// <summary>Gets the identity transformation.</summary>
    public static Transform3D Identity => new(ImmutableArray.Create(
        ImmutableArray.Create(1.0, 0.0, 0.0, 0.0),
        ImmutableArray.Create(0.0, 1.0, 0.0, 0.0),
        ImmutableArray.Create(0.0, 0.0, 1.0, 0.0),
        ImmutableArray.Create(0.0, 0.0, 0.0, 1.0)));

    /// <summary>Creates a translation transformation.</summary>
    /// <param name="dx">The translation along the X axis.</param>
    /// <param name="dy">The translation along the Y axis.</param>
    /// <param name="dz">The translation along the Z axis.</param>
    /// <returns>The translation transformation.</returns>
    public static Transform3D Translation(double dx, double dy, double dz) => new(ImmutableArray.Create(
        ImmutableArray.Create(1.0, 0.0, 0.0, dx),
        ImmutableArray.Create(0.0, 1.0, 0.0, dy),
        ImmutableArray.Create(0.0, 0.0, 1.0, dz),
        ImmutableArray.Create(0.0, 0.0, 0.0, 1.0)));

    /// <summary>Creates a rotation about the X axis.</summary>
    /// <param name="angleRadians">The rotation angle in radians.</param>
    /// <returns>The rotation transformation.</returns>
    public static Transform3D RotationX(double angleRadians)
    {
        double c = System.Math.Cos(angleRadians);
        double s = System.Math.Sin(angleRadians);
        return new(ImmutableArray.Create(
            ImmutableArray.Create(1.0, 0.0, 0.0, 0.0),
            ImmutableArray.Create(0.0, c, -s, 0.0),
            ImmutableArray.Create(0.0, s, c, 0.0),
            ImmutableArray.Create(0.0, 0.0, 0.0, 1.0)));
    }

    /// <summary>Creates a rotation about the Y axis.</summary>
    /// <param name="angleRadians">The rotation angle in radians.</param>
    /// <returns>The rotation transformation.</returns>
    public static Transform3D RotationY(double angleRadians)
    {
        double c = System.Math.Cos(angleRadians);
        double s = System.Math.Sin(angleRadians);
        return new(ImmutableArray.Create(
            ImmutableArray.Create(c, 0.0, s, 0.0),
            ImmutableArray.Create(0.0, 1.0, 0.0, 0.0),
            ImmutableArray.Create(-s, 0.0, c, 0.0),
            ImmutableArray.Create(0.0, 0.0, 0.0, 1.0)));
    }

    /// <summary>Creates a rotation about the Z axis.</summary>
    /// <param name="angleRadians">The rotation angle in radians.</param>
    /// <returns>The rotation transformation.</returns>
    public static Transform3D RotationZ(double angleRadians)
    {
        double c = System.Math.Cos(angleRadians);
        double s = System.Math.Sin(angleRadians);
        return new(ImmutableArray.Create(
            ImmutableArray.Create(c, -s, 0.0, 0.0),
            ImmutableArray.Create(s, c, 0.0, 0.0),
            ImmutableArray.Create(0.0, 0.0, 1.0, 0.0),
            ImmutableArray.Create(0.0, 0.0, 0.0, 1.0)));
    }

    /// <summary>Creates a rotation about an arbitrary axis using Rodrigues' rotation formula.</summary>
    /// <param name="axis">The axis of rotation (will be normalized).</param>
    /// <param name="angle">The rotation angle in radians.</param>
    /// <returns>The rotation transformation.</returns>
    public static Transform3D RotationAxis(Vector3D axis, double angle)
    {
        Vector3D u = axis.Normalize();
        double ux = u.X, uy = u.Y, uz = u.Z;
        double c = System.Math.Cos(angle);
        double s = System.Math.Sin(angle);
        double t = 1.0 - c;

        return new(ImmutableArray.Create(
            ImmutableArray.Create(
                t * ux * ux + c,
                t * ux * uy - s * uz,
                t * ux * uz + s * uy,
                0.0),
            ImmutableArray.Create(
                t * ux * uy + s * uz,
                t * uy * uy + c,
                t * uy * uz - s * ux,
                0.0),
            ImmutableArray.Create(
                t * ux * uz - s * uy,
                t * uy * uz + s * ux,
                t * uz * uz + c,
                0.0),
            ImmutableArray.Create(0.0, 0.0, 0.0, 1.0)));
    }

    /// <summary>Creates a rotation from Euler angles using the ZXY convention (roll, pitch, yaw).</summary>
    /// <param name="yaw">The yaw angle (rotation about Y) in radians.</param>
    /// <param name="pitch">The pitch angle (rotation about X) in radians.</param>
    /// <param name="roll">The roll angle (rotation about Z) in radians.</param>
    /// <returns>The combined rotation transformation (Rz * Rx * Ry).</returns>
    public static Transform3D RotationEuler(double yaw, double pitch, double roll)
        => RotationZ(roll).Multiply(RotationX(pitch)).Multiply(RotationY(yaw));

    /// <summary>Creates a non-uniform scaling transformation.</summary>
    /// <param name="sx">The scale factor along the X axis.</param>
    /// <param name="sy">The scale factor along the Y axis.</param>
    /// <param name="sz">The scale factor along the Z axis.</param>
    /// <returns>The scaling transformation.</returns>
    public static Transform3D Scaling(double sx, double sy, double sz) => new(ImmutableArray.Create(
        ImmutableArray.Create(sx, 0.0, 0.0, 0.0),
        ImmutableArray.Create(0.0, sy, 0.0, 0.0),
        ImmutableArray.Create(0.0, 0.0, sz, 0.0),
        ImmutableArray.Create(0.0, 0.0, 0.0, 1.0)));

    /// <summary>Creates a uniform scaling transformation.</summary>
    /// <param name="uniform">The uniform scale factor.</param>
    /// <returns>The scaling transformation.</returns>
    public static Transform3D Scaling(double uniform) => Scaling(uniform, uniform, uniform);

    /// <summary>Creates a reflection transformation across the plane perpendicular to the given axis.</summary>
    /// <param name="axis">The normal vector of the reflection plane (will be normalized).</param>
    /// <returns>The reflection transformation.</returns>
    public static Transform3D Reflection(Vector3D axis)
    {
        Vector3D n = axis.Normalize();
        double nx = n.X, ny = n.Y, nz = n.Z;
        return new(ImmutableArray.Create(
            ImmutableArray.Create(1.0 - 2.0 * nx * nx, -2.0 * nx * ny, -2.0 * nx * nz, 0.0),
            ImmutableArray.Create(-2.0 * ny * nx, 1.0 - 2.0 * ny * ny, -2.0 * ny * nz, 0.0),
            ImmutableArray.Create(-2.0 * nz * nx, -2.0 * nz * ny, 1.0 - 2.0 * nz * nz, 0.0),
            ImmutableArray.Create(0.0, 0.0, 0.0, 1.0)));
    }

    /// <summary>Creates a shearing transformation.</summary>
    /// <param name="xy">The shear factor that shifts Y per unit X.</param>
    /// <param name="xz">The shear factor that shifts Z per unit X.</param>
    /// <param name="yx">The shear factor that shifts X per unit Y.</param>
    /// <param name="yz">The shear factor that shifts Z per unit Y.</param>
    /// <param name="zx">The shear factor that shifts X per unit Z.</param>
    /// <param name="zy">The shear factor that shifts Y per unit Z.</param>
    /// <returns>The shearing transformation.</returns>
    public static Transform3D Shearing(double xy, double xz, double yx, double yz, double zx, double zy)
        => new(ImmutableArray.Create(
            ImmutableArray.Create(1.0, yx, zx, 0.0),
            ImmutableArray.Create(xy, 1.0, zy, 0.0),
            ImmutableArray.Create(xz, yz, 1.0, 0.0),
            ImmutableArray.Create(0.0, 0.0, 0.0, 1.0)));

    /// <summary>Creates a view matrix from an eye position looking at a target.</summary>
    /// <param name="eye">The camera position.</param>
    /// <param name="target">The point to look at.</param>
    /// <param name="up">The up direction vector.</param>
    /// <returns>The world-to-view transformation (look-at matrix).</returns>
    public static Transform3D LookAt(Point3D eye, Point3D target, Vector3D up)
    {
        Vector3D forward = new Vector3D(target.X - eye.X, target.Y - eye.Y, target.Z - eye.Z).Normalize();
        Vector3D right = forward.Cross(up).Normalize();
        Vector3D adjustedUp = right.Cross(forward);

        return new(ImmutableArray.Create(
            ImmutableArray.Create(right.X, right.Y, right.Z, -right.Dot(new Vector3D(eye.X, eye.Y, eye.Z))),
            ImmutableArray.Create(adjustedUp.X, adjustedUp.Y, adjustedUp.Z, -adjustedUp.Dot(new Vector3D(eye.X, eye.Y, eye.Z))),
            ImmutableArray.Create(-forward.X, -forward.Y, -forward.Z, forward.Dot(new Vector3D(eye.X, eye.Y, eye.Z))),
            ImmutableArray.Create(0.0, 0.0, 0.0, 1.0)));
    }

    /// <summary>Applies this transformation to a point (uses w=1).</summary>
    /// <param name="p">The point to transform.</param>
    /// <returns>The transformed point.</returns>
    public Point3D TransformPoint(Point3D p)
    {
        double x = Rows[0][0] * p.X + Rows[0][1] * p.Y + Rows[0][2] * p.Z + Rows[0][3];
        double y = Rows[1][0] * p.X + Rows[1][1] * p.Y + Rows[1][2] * p.Z + Rows[1][3];
        double z = Rows[2][0] * p.X + Rows[2][1] * p.Y + Rows[2][2] * p.Z + Rows[2][3];
        return new Point3D(x, y, z);
    }

    /// <summary>Applies this transformation to a vector (uses w=0, ignoring translation).</summary>
    /// <param name="v">The vector to transform.</param>
    /// <returns>The transformed vector.</returns>
    public Vector3D TransformVector(Vector3D v)
    {
        double x = Rows[0][0] * v.X + Rows[0][1] * v.Y + Rows[0][2] * v.Z;
        double y = Rows[1][0] * v.X + Rows[1][1] * v.Y + Rows[1][2] * v.Z;
        double z = Rows[2][0] * v.X + Rows[2][1] * v.Y + Rows[2][2] * v.Z;
        return new Vector3D(x, y, z);
    }

    /// <summary>Transforms a normal vector using the inverse-transpose of this matrix.</summary>
    /// <param name="n">The normal vector to transform.</param>
    /// <returns>The transformed normal vector.</returns>
    public Vector3D TransformNormal(Vector3D n)
    {
        Transform3D inv = Inverse();
        Transform3D invT = inv.Transpose();
        return invT.TransformVector(n);
    }

    /// <summary>Computes the transpose of this matrix.</summary>
    /// <returns>The transposed matrix.</returns>
    public Transform3D Transpose()
    {
        var rows = new ImmutableArray<double>[4];
        for (int j = 0; j < 4; j++)
        {
            rows[j] = ImmutableArray.Create(Rows[0][j], Rows[1][j], Rows[2][j], Rows[3][j]);
        }
        return new Transform3D(rows.ToImmutableArray());
    }

    /// <summary>Multiplies this transformation by another (this * other).</summary>
    /// <param name="other">The other transformation.</param>
    /// <returns>The product transformation.</returns>
    public Transform3D Multiply(Transform3D other)
    {
        var result = new double[4][];
        for (int i = 0; i < 4; i++)
        {
            result[i] = new double[4];
            for (int j = 0; j < 4; j++)
            {
                double sum = 0;
                for (int k = 0; k < 4; k++)
                    sum += Rows[i][k] * other.Rows[k][j];
                result[i][j] = sum;
            }
        }
        return new Transform3D(ImmutableArray.Create(
            result[0].ToImmutableArray(),
            result[1].ToImmutableArray(),
            result[2].ToImmutableArray(),
            result[3].ToImmutableArray()));
    }

    /// <summary>Computes the inverse of this 4x4 matrix using Gauss-Jordan elimination.</summary>
    /// <returns>The inverse matrix.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown when the matrix is singular.</exception>
    public Transform3D Inverse()
    {
        var augmented = new double[4][];
        for (int i = 0; i < 4; i++)
        {
            augmented[i] = new double[8];
            augmented[i][i] = 1.0;
            for (int j = 0; j < 4; j++)
                augmented[i][4 + j] = Rows[i][j];
        }

        for (int col = 0; col < 4; col++)
        {
            int maxRow = col;
            double maxVal = System.Math.Abs(augmented[col][4 + col]);
            for (int row = col + 1; row < 4; row++)
            {
                double val = System.Math.Abs(augmented[row][4 + col]);
                if (val > maxVal)
                {
                    maxVal = val;
                    maxRow = row;
                }
            }

            if (maxVal < 1e-15)
                throw new System.InvalidOperationException("Transform3D matrix is singular and cannot be inverted.");

            if (maxRow != col)
                (augmented[col], augmented[maxRow]) = (augmented[maxRow], augmented[col]);

            double pivot = augmented[col][4 + col];
            for (int j = 0; j < 8; j++)
                augmented[col][j] /= pivot;

            for (int row = 0; row < 4; row++)
            {
                if (row == col) continue;
                double factor = augmented[row][4 + col];
                for (int j = 0; j < 8; j++)
                    augmented[row][j] -= factor * augmented[col][j];
            }
        }

        var resultRows = new ImmutableArray<double>[4];
        for (int i = 0; i < 4; i++)
        {
            resultRows[i] = ImmutableArray.Create(
                augmented[i][0], augmented[i][1], augmented[i][2], augmented[i][3]);
        }
        return new Transform3D(resultRows.ToImmutableArray());
    }

    /// <summary>Computes the determinant of this 4x4 matrix.</summary>
    /// <returns>The determinant value.</returns>
    public double Determinant()
    {
        double a = Rows[0][0], b = Rows[0][1], c = Rows[0][2], d = Rows[0][3];
        double e = Rows[1][0], f = Rows[1][1], g = Rows[1][2], h = Rows[1][3];
        double i = Rows[2][0], j = Rows[2][1], k = Rows[2][2], l = Rows[2][3];
        double m = Rows[3][0], n = Rows[3][1], o = Rows[3][2], p = Rows[3][3];

        double term1 = a * (f * (k * p - l * o) - g * (j * p - l * n) + h * (j * o - k * n));
        double term2 = b * (e * (k * p - l * o) - g * (i * p - l * m) + h * (i * o - k * m));
        double term3 = c * (e * (j * p - l * n) - f * (i * p - l * m) + h * (i * n - j * m));
        double term4 = d * (e * (j * o - k * n) - f * (i * o - k * m) + g * (i * n - j * m));

        return term1 - term2 + term3 - term4;
    }

    /// <summary>Converts this transformation to a <see cref="System.Numerics.Matrix4x4"/>.</summary>
    /// <returns>The equivalent System.Numerics matrix.</returns>
    public Matrix4x4 ToSystemNumerics() => new(
        (float)Rows[0][0], (float)Rows[0][1], (float)Rows[0][2], (float)Rows[0][3],
        (float)Rows[1][0], (float)Rows[1][1], (float)Rows[1][2], (float)Rows[1][3],
        (float)Rows[2][0], (float)Rows[2][1], (float)Rows[2][2], (float)Rows[2][3],
        (float)Rows[3][0], (float)Rows[3][1], (float)Rows[3][2], (float)Rows[3][3]);

    /// <summary>Creates a <see cref="Transform3D"/> from a <see cref="System.Numerics.Matrix4x4"/>.</summary>
    /// <param name="m">The System.Numerics matrix to convert from.</param>
    /// <returns>The equivalent Transform3D.</returns>
    public static Transform3D FromSystemNumerics(Matrix4x4 m) => new(ImmutableArray.Create(
        ImmutableArray.Create((double)m.M11, (double)m.M12, (double)m.M13, (double)m.M14),
        ImmutableArray.Create((double)m.M21, (double)m.M22, (double)m.M23, (double)m.M24),
        ImmutableArray.Create((double)m.M31, (double)m.M32, (double)m.M33, (double)m.M34),
        ImmutableArray.Create((double)m.M41, (double)m.M42, (double)m.M43, (double)m.M44)));

    /// <summary>Creates a Transform3D from a row-major 4x4 array.</summary>
    /// <param name="m">The 4x4 array in row-major order.</param>
    /// <returns>The transformation.</returns>
    public static Transform3D FromRowMajor(double[][] m) => new(ImmutableArray.Create(
        ImmutableArray.Create(m[0][0], m[0][1], m[0][2], m[0][3]),
        ImmutableArray.Create(m[1][0], m[1][1], m[1][2], m[1][3]),
        ImmutableArray.Create(m[2][0], m[2][1], m[2][2], m[2][3]),
        ImmutableArray.Create(m[3][0], m[3][1], m[3][2], m[3][3])));

    /// <summary>Computes the inverse-transpose of the upper-left 3x3 portion, embedded in a 4x4 matrix.</summary>
    /// <returns>The inverse-transpose matrix suitable for transforming normals.</returns>
    public Transform3D InverseTranspose3x3()
    {
        Transform3D inv = Inverse();
        return new Transform3D(ImmutableArray.Create(
            ImmutableArray.Create(inv[0, 0], inv[1, 0], inv[2, 0], inv[3, 0]),
            ImmutableArray.Create(inv[0, 1], inv[1, 1], inv[2, 1], inv[3, 1]),
            ImmutableArray.Create(inv[0, 2], inv[1, 2], inv[2, 2], inv[3, 2]),
            ImmutableArray.Create(0.0, 0.0, 0.0, 1.0)));
    }

    /// <summary>Multiplies two transformations.</summary>
    /// <param name="a">The left transformation.</param>
    /// <param name="b">The right transformation.</param>
    /// <returns>The product a * b.</returns>
    public static Transform3D operator *(Transform3D a, Transform3D b) => a.Multiply(b);

    /// <summary>Applies a transformation to a point.</summary>
    /// <param name="t">The transformation.</param>
    /// <param name="p">The point to transform.</param>
    /// <returns>The transformed point.</returns>
    public static Point3D operator *(Transform3D t, Point3D p) => t.TransformPoint(p);

    /// <summary>Returns a string representation of this transformation.</summary>
    public override string ToString()
        => $"Transform3D([{Rows[0][0]:F4}, {Rows[0][1]:F4}, {Rows[0][2]:F4}, {Rows[0][3]:F4}; " +
           $"{Rows[1][0]:F4}, {Rows[1][1]:F4}, {Rows[1][2]:F4}, {Rows[1][3]:F4}; " +
           $"{Rows[2][0]:F4}, {Rows[2][1]:F4}, {Rows[2][2]:F4}, {Rows[2][3]:F4}; " +
           $"{Rows[3][0]:F4}, {Rows[3][1]:F4}, {Rows[3][2]:F4}, {Rows[3][3]:F4}])";
}
