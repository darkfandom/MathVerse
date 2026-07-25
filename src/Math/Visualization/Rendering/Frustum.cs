namespace MathVerse.Math.Visualization.Rendering;
using System.Numerics;

/// <summary>View frustum for culling operations, extracted from a view-projection matrix.</summary>
public sealed class Frustum
{
    private readonly Plane[] _planes = new Plane[6];

    /// <summary>The left frustum plane index.</summary>
    public const int Left = 0;

    /// <summary>The right frustum plane index.</summary>
    public const int Right = 1;

    /// <summary>The bottom frustum plane index.</summary>
    public const int Bottom = 2;

    /// <summary>The top frustum plane index.</summary>
    public const int Top = 3;

    /// <summary>The near frustum plane index.</summary>
    public const int Near = 4;

    /// <summary>The far frustum plane index.</summary>
    public const int Far = 5;

    /// <summary>Initializes a new instance of the <see cref="Frustum"/> class by extracting six frustum planes from a combined view-projection matrix.</summary>
    /// <param name="viewProjection">The combined view-projection matrix in row-vector convention.</param>
    public Frustum(Matrix4x4 viewProjection)
    {
        ExtractPlanes(viewProjection, _planes);
    }

    /// <summary>Gets the six frustum planes in order: left, right, bottom, top, near, far.</summary>
    public ReadOnlySpan<Plane> Planes => _planes;

    /// <summary>Tests whether a point is inside or on the frustum boundary.</summary>
    /// <param name="point">The world-space point to test.</param>
    /// <returns><c>true</c> if the point is inside the frustum; otherwise <c>false</c>.</returns>
    public bool ContainsPoint(Vector3 point)
    {
        for (int i = 0; i < 6; i++)
        {
            float dot = Vector3.Dot(_planes[i].Normal, point) + _planes[i].D;
            if (dot < 0.0f)
                return false;
        }
        return true;
    }

    /// <summary>Tests whether a sphere is fully or partially inside the frustum.</summary>
    /// <param name="center">The center of the sphere in world space.</param>
    /// <param name="radius">The radius of the sphere.</param>
    /// <returns><c>true</c> if the sphere intersects or is inside the frustum; otherwise <c>false</c>.</returns>
    public bool ContainsSphere(Vector3 center, float radius)
    {
        for (int i = 0; i < 6; i++)
        {
            float distance = Vector3.Dot(_planes[i].Normal, center) + _planes[i].D;
            if (distance < -radius)
                return false;
        }
        return true;
    }

    /// <summary>Tests whether an axis-aligned bounding box is fully or partially inside the frustum.</summary>
    /// <param name="box">The bounding box to test.</param>
    /// <returns><c>true</c> if the box intersects or is inside the frustum; otherwise <c>false</c>.</returns>
    public bool ContainsBox(BoundingBox box)
    {
        for (int i = 0; i < 6; i++)
        {
            Vector3 positiveVertex = box.Min;

            if (_planes[i].Normal.X >= 0.0f)
                positiveVertex.X = box.Max.X;
            if (_planes[i].Normal.Y >= 0.0f)
                positiveVertex.Y = box.Max.Y;
            if (_planes[i].Normal.Z >= 0.0f)
                positiveVertex.Z = box.Max.Z;

            float dot = Vector3.Dot(_planes[i].Normal, positiveVertex) + _planes[i].D;
            if (dot < 0.0f)
                return false;
        }
        return true;
    }

    /// <summary>Performs a full containment test on a bounding box, returning whether it is inside, outside, or intersecting the frustum.</summary>
    /// <param name="bounds">The bounding box to test.</param>
    /// <returns>A <see cref="CullingResult"/> indicating the containment state.</returns>
    public CullingResult Cull(BoundingBox bounds)
    {
        bool allInside = true;

        for (int i = 0; i < 6; i++)
        {
            Vector3 positiveVertex = bounds.Min;
            Vector3 negativeVertex = bounds.Max;

            if (_planes[i].Normal.X >= 0.0f)
            {
                positiveVertex.X = bounds.Max.X;
                negativeVertex.X = bounds.Min.X;
            }
            else
            {
                positiveVertex.X = bounds.Min.X;
                negativeVertex.X = bounds.Max.X;
            }

            if (_planes[i].Normal.Y >= 0.0f)
            {
                positiveVertex.Y = bounds.Max.Y;
                negativeVertex.Y = bounds.Min.Y;
            }
            else
            {
                positiveVertex.Y = bounds.Min.Y;
                negativeVertex.Y = bounds.Max.Y;
            }

            if (_planes[i].Normal.Z >= 0.0f)
            {
                positiveVertex.Z = bounds.Max.Z;
                negativeVertex.Z = bounds.Min.Z;
            }
            else
            {
                positiveVertex.Z = bounds.Min.Z;
                negativeVertex.Z = bounds.Max.Z;
            }

            float positiveDot = Vector3.Dot(_planes[i].Normal, positiveVertex) + _planes[i].D;
            if (positiveDot < 0.0f)
                return CullingResult.Outside;

            float negativeDot = Vector3.Dot(_planes[i].Normal, negativeVertex) + _planes[i].D;
            if (negativeDot < 0.0f)
                allInside = false;
        }

        return allInside ? CullingResult.Inside : CullingResult.Intersecting;
    }

    /// <summary>Extracts and normalizes the six frustum planes from a combined view-projection matrix using the Gribb-Hartmann method.</summary>
    /// <param name="vp">The combined view-projection matrix.</param>
    /// <param name="planes">The array to store the extracted planes.</param>
    private static void ExtractPlanes(Matrix4x4 vp, Plane[] planes)
    {
        planes[Left] = NormalizePlane(new Plane(
            vp.M14 + vp.M11,
            vp.M24 + vp.M21,
            vp.M34 + vp.M31,
            vp.M44 + vp.M41));

        planes[Right] = NormalizePlane(new Plane(
            vp.M14 - vp.M11,
            vp.M24 - vp.M21,
            vp.M34 - vp.M31,
            vp.M44 - vp.M41));

        planes[Bottom] = NormalizePlane(new Plane(
            vp.M14 + vp.M12,
            vp.M24 + vp.M22,
            vp.M34 + vp.M32,
            vp.M44 + vp.M42));

        planes[Top] = NormalizePlane(new Plane(
            vp.M14 - vp.M12,
            vp.M24 - vp.M22,
            vp.M34 - vp.M32,
            vp.M44 - vp.M42));

        planes[Near] = NormalizePlane(new Plane(
            vp.M13,
            vp.M23,
            vp.M33,
            vp.M43));

        planes[Far] = NormalizePlane(new Plane(
            vp.M14 - vp.M13,
            vp.M24 - vp.M23,
            vp.M34 - vp.M33,
            vp.M44 - vp.M43));
    }

    /// <summary>Normalizes a plane so that the normal vector has unit length.</summary>
    /// <param name="plane">The plane to normalize.</param>
    /// <returns>The normalized plane.</returns>
    private static Plane NormalizePlane(Plane plane)
    {
        float length = plane.Normal.Length();
        if (length < 1e-8f)
            return plane;

        float invLength = 1.0f / length;
        return new Plane(
            plane.Normal * invLength,
            plane.D * invLength);
    }
}

/// <summary>Enumerates the possible results of a frustum culling test.</summary>
public enum CullingResult
{
    /// <summary>The object is entirely inside the frustum.</summary>
    Inside,

    /// <summary>The object is entirely outside the frustum.</summary>
    Outside,

    /// <summary>The object partially intersects the frustum boundary.</summary>
    Intersecting
}
