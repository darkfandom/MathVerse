using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry2D;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Advanced.Curves;

/// <summary>
/// Represents a local orthonormal coordinate frame (Frenet-Serret frame) at a point along a space curve.
/// </summary>
/// <param name="Position">The position of the frame origin on the curve.</param>
/// <param name="Tangent">The unit tangent vector T, pointing in the direction of increasing curve parameter.</param>
/// <param name="Normal">The unit principal normal vector N, pointing toward the center of curvature.</param>
/// <param name="Binormal">The unit binormal vector B, computed as T × N.</param>
public readonly record struct FrenetFrame(Point3D Position, Vector3D Tangent, Vector3D Normal, Vector3D Binormal);

/// <summary>
/// Provides methods for computing Frenet-Serret frames along polyline curves using finite differences and Gram-Schmidt orthonormalization.
/// </summary>
public static class FrenetFrameComputer
{
    /// <summary>
    /// Computes the Frenet frame at a specific vertex of a polyline curve. The tangent is computed via central finite differences,
    /// the normal via the derivative of the tangent, and the binormal as T × N. Gram-Schmidt orthonormalization ensures frame validity.
    /// </summary>
    /// <param name="curve">The polyline curve.</param>
    /// <param name="index">The index of the vertex at which to compute the frame. Must be in [0, curve.Length - 1].</param>
    /// <returns>A <see cref="FrenetFrame"/> representing the local coordinate frame at the specified vertex.</returns>
    /// <exception cref="ArgumentException">Thrown when the curve has fewer than 2 points or the index is out of range.</exception>
    public static FrenetFrame Compute(ImmutableArray<Point3D> curve, int index)
    {
        if (curve.Length < 2)
            throw new ArgumentException("Curve must have at least 2 points.", nameof(curve));
        if (index < 0 || index >= curve.Length)
            throw new ArgumentException($"Index must be in [0, {curve.Length - 1}].", nameof(index));

        Vector3D tangent = ComputeTangent(curve, index);

        Vector3D normal = ComputeNormalVector(curve, index, tangent);

        Orthonormalize(ref tangent, ref normal);

        Vector3D binormal = Cross(tangent, normal);
        double bLen = System.Math.Sqrt(binormal.X * binormal.X + binormal.Y * binormal.Y + binormal.Z * binormal.Z);
        if (bLen > 1e-15)
            binormal = new Vector3D(binormal.X / bLen, binormal.Y / bLen, binormal.Z / bLen);

        return new FrenetFrame(curve[index], tangent, normal, binormal);
    }

    /// <summary>
    /// Computes Frenet frames for all vertices of a polyline curve.
    /// </summary>
    /// <param name="curve">The polyline curve.</param>
    /// <returns>An immutable array of <see cref="FrenetFrame"/> for each vertex, in order.</returns>
    /// <exception cref="ArgumentException">Thrown when the curve has fewer than 2 points.</exception>
    public static ImmutableArray<FrenetFrame> ComputeAll(ImmutableArray<Point3D> curve)
    {
        if (curve.Length < 2)
            throw new ArgumentException("Curve must have at least 2 points.", nameof(curve));

        var builder = ImmutableArray.CreateBuilder<FrenetFrame>(curve.Length);

        for (int i = 0; i < curve.Length; i++)
        {
            builder.Add(Compute(curve, i));
        }

        return builder.MoveToImmutable();
    }

    /// <summary>
    /// Applies Gram-Schmidt orthonormalization to ensure the tangent and normal vectors are unit-length and orthogonal.
    /// The tangent is normalized first, then the normal is orthogonalized against the tangent and normalized.
    /// </summary>
    /// <param name="tangent">Reference to the tangent vector. Will be normalized on output.</param>
    /// <param name="normal">Reference to the normal vector. Will be orthogonalized against tangent and normalized on output.</param>
    public static void Orthonormalize(ref Vector3D tangent, ref Vector3D normal)
    {
        double tLen = System.Math.Sqrt(tangent.X * tangent.X + tangent.Y * tangent.Y + tangent.Z * tangent.Z);
        if (tLen < 1e-15)
        {
            tangent = new Vector3D(1, 0, 0);
            tLen = 1.0;
        }
        tangent = new Vector3D(tangent.X / tLen, tangent.Y / tLen, tangent.Z / tLen);

        double dot = normal.X * tangent.X + normal.Y * tangent.Y + normal.Z * tangent.Z;
        Vector3D ortho = new Vector3D(
            normal.X - dot * tangent.X,
            normal.Y - dot * tangent.Y,
            normal.Z - dot * tangent.Z);

        double nLen = System.Math.Sqrt(ortho.X * ortho.X + ortho.Y * ortho.Y + ortho.Z * ortho.Z);
        if (nLen < 1e-15)
        {
            Vector3D arb = System.Math.Abs(tangent.X) < 0.9
                ? new Vector3D(1, 0, 0)
                : new Vector3D(0, 1, 0);
            ortho = new Vector3D(
                arb.X - (arb.X * tangent.X + arb.Y * tangent.Y + arb.Z * tangent.Z) * tangent.X,
                arb.Y - (arb.X * tangent.X + arb.Y * tangent.Y + arb.Z * tangent.Z) * tangent.Y,
                arb.Z - (arb.X * tangent.X + arb.Y * tangent.Y + arb.Z * tangent.Z) * tangent.Z);
            nLen = System.Math.Sqrt(ortho.X * ortho.X + ortho.Y * ortho.Y + ortho.Z * ortho.Z);
        }

        normal = new Vector3D(ortho.X / nLen, ortho.Y / nLen, ortho.Z / nLen);
    }

    /// <summary>
    /// Computes the tangent vector at a vertex using central finite differences, or forward/backward differences at endpoints.
    /// </summary>
    private static Vector3D ComputeTangent(ImmutableArray<Point3D> curve, int index)
    {
        if (index == 0)
        {
            return new Vector3D(
                curve[1].X - curve[0].X,
                curve[1].Y - curve[0].Y,
                curve[1].Z - curve[0].Z);
        }

        if (index == curve.Length - 1)
        {
            int last = curve.Length - 1;
            return new Vector3D(
                curve[last].X - curve[last - 1].X,
                curve[last].Y - curve[last - 1].Y,
                curve[last].Z - curve[last - 1].Z);
        }

        return new Vector3D(
            curve[index + 1].X - curve[index - 1].X,
            curve[index + 1].Y - curve[index - 1].Y,
            curve[index + 1].Z - curve[index - 1].Z);
    }

    /// <summary>
    /// Computes the normal vector at a vertex via the derivative of the tangent vector (second-order finite differences).
    /// Falls back to a cross-product with a reference vector at endpoints.
    /// </summary>
    private static Vector3D ComputeNormalVector(ImmutableArray<Point3D> curve, int index, Vector3D tangent)
    {
        if (index > 0 && index < curve.Length - 1)
        {
            Vector3D tPrev = ComputeTangent(curve, index - 1);
            Vector3D tNext = ComputeTangent(curve, index + 1);

            return new Vector3D(
                tNext.X - tPrev.X,
                tNext.Y - tPrev.Y,
                tNext.Z - tPrev.Z);
        }

        if (index == 0 && curve.Length > 2)
        {
            Vector3D t0 = ComputeTangent(curve, 0);
            Vector3D t1 = ComputeTangent(curve, 1);
            Vector3D d = new Vector3D(t1.X - t0.X, t1.Y - t0.Y, t1.Z - t0.Z);
            double dLen = System.Math.Sqrt(d.X * d.X + d.Y * d.Y + d.Z * d.Z);
            if (dLen > 1e-15)
                return d;

            Vector3D refVec = System.Math.Abs(tangent.X) < 0.9
                ? new Vector3D(1, 0, 0)
                : new Vector3D(0, 1, 0);
            return Cross(tangent, refVec);
        }

        if (index == curve.Length - 1 && curve.Length > 2)
        {
            Vector3D tLast = ComputeTangent(curve, curve.Length - 1);
            Vector3D tPrev = ComputeTangent(curve, curve.Length - 2);
            Vector3D d = new Vector3D(tLast.X - tPrev.X, tLast.Y - tPrev.Y, tLast.Z - tPrev.Z);
            double dLen = System.Math.Sqrt(d.X * d.X + d.Y * d.Y + d.Z * d.Z);
            if (dLen > 1e-15)
                return d;

            Vector3D refVec = System.Math.Abs(tangent.X) < 0.9
                ? new Vector3D(1, 0, 0)
                : new Vector3D(0, 1, 0);
            return Cross(tangent, refVec);
        }

        Vector3D rv = System.Math.Abs(tangent.X) < 0.9
            ? new Vector3D(1, 0, 0)
            : new Vector3D(0, 1, 0);
        return Cross(tangent, rv);
    }

    /// <summary>
    /// Computes the cross product of two vectors.
    /// </summary>
    private static Vector3D Cross(Vector3D a, Vector3D b)
    {
        return new Vector3D(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X);
    }
}
