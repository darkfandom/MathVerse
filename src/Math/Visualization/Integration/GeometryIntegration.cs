namespace MathVerse.Math.Visualization.Integration;
using System.Numerics;
using System.Collections.Generic;

/// <summary>Integrates with Geometry and Geometry.Advanced modules for geometry rendering.</summary>
public sealed class GeometryIntegration
{
    /// <summary>Creates a 2D circle visualization.</summary>
    /// <param name="center">The circle center.</param>
    /// <param name="radius">The circle radius.</param>
    /// <param name="segmentCount">The number of line segments to approximate the circle.</param>
    /// <returns>Line segments representing the circle.</returns>
    public static List<(Vector2 Start, Vector2 End)> CreateCircle2D(Vector2 center, double radius, int segmentCount = 64)
    {
        var segments = new List<(Vector2, Vector2)>();

        for (int i = 0; i < segmentCount; i++)
        {
            double angle0 = 2.0 * System.Math.PI * i / segmentCount;
            double angle1 = 2.0 * System.Math.PI * (i + 1) / segmentCount;

            Vector2 p0 = new Vector2(
                center.X + (float)(radius * System.Math.Cos(angle0)),
                center.Y + (float)(radius * System.Math.Sin(angle0)));
            Vector2 p1 = new Vector2(
                center.X + (float)(radius * System.Math.Cos(angle1)),
                center.Y + (float)(radius * System.Math.Sin(angle1)));

            segments.Add((p0, p1));
        }

        return segments;
    }

    /// <summary>Creates a 2D ellipse visualization.</summary>
    /// <param name="center">The ellipse center.</param>
    /// <param name="radiusX">The X radius.</param>
    /// <param name="radiusY">The Y radius.</param>
    /// <param name="rotation">The rotation angle in radians.</param>
    /// <param name="segmentCount">The number of segments.</param>
    /// <returns>Line segments representing the ellipse.</returns>
    public static List<(Vector2 Start, Vector2 End)> CreateEllipse2D(
        Vector2 center, double radiusX, double radiusY, double rotation, int segmentCount = 64)
    {
        var segments = new List<(Vector2, Vector2)>();
        float cosR = (float)System.Math.Cos(rotation);
        float sinR = (float)System.Math.Sin(rotation);

        for (int i = 0; i < segmentCount; i++)
        {
            double angle0 = 2.0 * System.Math.PI * i / segmentCount;
            double angle1 = 2.0 * System.Math.PI * (i + 1) / segmentCount;

            float localX0 = (float)(radiusX * System.Math.Cos(angle0));
            float localY0 = (float)(radiusY * System.Math.Sin(angle0));
            float x0 = center.X + localX0 * cosR - localY0 * sinR;
            float y0 = center.Y + localX0 * sinR + localY0 * cosR;

            float localX1 = (float)(radiusX * System.Math.Cos(angle1));
            float localY1 = (float)(radiusY * System.Math.Sin(angle1));
            float x1 = center.X + localX1 * cosR - localY1 * sinR;
            float y1 = center.Y + localX1 * sinR + localY1 * cosR;

            segments.Add((new Vector2(x0, y0), new Vector2(x1, y1)));
        }

        return segments;
    }

    /// <summary>Creates a regular polygon visualization.</summary>
    /// <param name="center">The polygon center.</param>
    /// <param name="radius">The circumscribed radius.</param>
    /// <param name="sides">The number of sides.</param>
    /// <param name="rotation">The initial rotation angle.</param>
    /// <returns>Line segments representing the polygon.</returns>
    public static List<(Vector2 Start, Vector2 End)> CreatePolygon2D(Vector2 center, double radius, int sides, double rotation = 0)
    {
        var segments = new List<(Vector2, Vector2)>();

        if (sides < 3)
            return segments;

        var vertices = new List<Vector2>();
        for (int i = 0; i < sides; i++)
        {
            double angle = 2.0 * System.Math.PI * i / sides + rotation;
            vertices.Add(new Vector2(
                center.X + (float)(radius * System.Math.Cos(angle)),
                center.Y + (float)(radius * System.Math.Sin(angle))));
        }

        for (int i = 0; i < sides; i++)
        {
            segments.Add((vertices[i], vertices[(i + 1) % sides]));
        }

        return segments;
    }

    /// <summary>Creates a 3D sphere mesh.</summary>
    /// <param name="center">The sphere center.</param>
    /// <param name="radius">The sphere radius.</param>
    /// <param name="latSegments">The number of latitude segments.</param>
    /// <param name="lonSegments">The number of longitude segments.</param>
    /// <returns>Vertices and faces for the sphere mesh.</returns>
    public static (List<Vector3> Vertices, List<List<int>> Faces) CreateSphere3D(
        Vector3 center, float radius, int latSegments = 16, int lonSegments = 32)
    {
        var vertices = new List<Vector3>();
        var faces = new List<List<int>>();

        for (int lat = 0; lat <= latSegments; lat++)
        {
            double theta = System.Math.PI * lat / latSegments;
            float sinTheta = (float)System.Math.Sin(theta);
            float cosTheta = (float)System.Math.Cos(theta);

            for (int lon = 0; lon <= lonSegments; lon++)
            {
                double phi = 2.0 * System.Math.PI * lon / lonSegments;
                float sinPhi = (float)System.Math.Sin(phi);
                float cosPhi = (float)System.Math.Cos(phi);

                float x = center.X + radius * sinTheta * cosPhi;
                float y = center.Y + radius * cosTheta;
                float z = center.Z + radius * sinTheta * sinPhi;

                vertices.Add(new Vector3(x, y, z));
            }
        }

        for (int lat = 0; lat < latSegments; lat++)
        {
            for (int lon = 0; lon < lonSegments; lon++)
            {
                int v00 = lat * (lonSegments + 1) + lon;
                int v10 = (lat + 1) * (lonSegments + 1) + lon;
                int v01 = lat * (lonSegments + 1) + lon + 1;
                int v11 = (lat + 1) * (lonSegments + 1) + lon + 1;

                faces.Add(new List<int> { v00, v10, v01 });
                faces.Add(new List<int> { v10, v11, v01 });
            }
        }

        return (vertices, faces);
    }

    /// <summary>Creates a 3D cylinder mesh.</summary>
    /// <param name="baseCenter">The base center.</param>
    /// <param name="radius">The cylinder radius.</param>
    /// <param name="height">The cylinder height.</param>
    /// <param name="segments">The number of circumference segments.</param>
    /// <returns>Vertices and faces for the cylinder mesh.</returns>
    public static (List<Vector3> Vertices, List<List<int>> Faces) CreateCylinder3D(
        Vector3 baseCenter, float radius, float height, int segments = 32)
    {
        var vertices = new List<Vector3>();
        var faces = new List<List<int>>();

        for (int i = 0; i <= segments; i++)
        {
            double angle = 2.0 * System.Math.PI * i / segments;
            float x = baseCenter.X + radius * (float)System.Math.Cos(angle);
            float z = baseCenter.Z + radius * (float)System.Math.Sin(angle);

            vertices.Add(new Vector3(x, baseCenter.Y, z));
            vertices.Add(new Vector3(x, baseCenter.Y + height, z));
        }

        int centerBottom = vertices.Count;
        vertices.Add(baseCenter);
        int centerTop = vertices.Count;
        vertices.Add(new Vector3(baseCenter.X, baseCenter.Y + height, baseCenter.Z));

        for (int i = 0; i < segments; i++)
        {
            int b0 = i * 2;
            int t0 = i * 2 + 1;
            int b1 = (i + 1) * 2;
            int t1 = (i + 1) * 2 + 1;

            faces.Add(new List<int> { b0, b1, t0 });
            faces.Add(new List<int> { b1, t1, t0 });

            faces.Add(new List<int> { centerBottom, b1, b0 });
            faces.Add(new List<int> { centerTop, t0, t1 });
        }

        return (vertices, faces);
    }

    /// <summary>Creates a 3D torus mesh.</summary>
    /// <param name="center">The torus center.</param>
    /// <param name="majorRadius">The major radius.</param>
    /// <param name="minorRadius">The minor radius.</param>
    /// <param name="majorSegments">The number of major segments.</param>
    /// <param name="minorSegments">The number of minor segments.</param>
    /// <returns>Vertices and faces for the torus mesh.</returns>
    public static (List<Vector3> Vertices, List<List<int>> Faces) CreateTorus3D(
        Vector3 center, float majorRadius, float minorRadius,
        int majorSegments = 32, int minorSegments = 16)
    {
        var vertices = new List<Vector3>();
        var faces = new List<List<int>>();

        for (int i = 0; i <= majorSegments; i++)
        {
            double u = 2.0 * System.Math.PI * i / majorSegments;
            float cosU = (float)System.Math.Cos(u);
            float sinU = (float)System.Math.Sin(u);

            for (int j = 0; j <= minorSegments; j++)
            {
                double v = 2.0 * System.Math.PI * j / minorSegments;
                float cosV = (float)System.Math.Cos(v);
                float sinV = (float)System.Math.Sin(v);

                float x = center.X + (majorRadius + minorRadius * cosV) * cosU;
                float y = center.Y + minorRadius * sinV;
                float z = center.Z + (majorRadius + minorRadius * cosV) * sinU;

                vertices.Add(new Vector3(x, y, z));
            }
        }

        for (int i = 0; i < majorSegments; i++)
        {
            for (int j = 0; j < minorSegments; j++)
            {
                int v00 = i * (minorSegments + 1) + j;
                int v10 = (i + 1) * (minorSegments + 1) + j;
                int v01 = i * (minorSegments + 1) + j + 1;
                int v11 = (i + 1) * (minorSegments + 1) + j + 1;

                faces.Add(new List<int> { v00, v10, v01 });
                faces.Add(new List<int> { v10, v11, v01 });
            }
        }

        return (vertices, faces);
    }

    /// <summary>Creates a 2D parametric curve visualization.</summary>
    /// <param name="xFunc">The X(t) function.</param>
    /// <param name="yFunc">The Y(t) function.</param>
    /// <param name="tMin">The minimum parameter value.</param>
    /// <param name="tMax">The maximum parameter value.</param>
    /// <param name="segments">The number of line segments.</param>
    /// <returns>Line segments for the parametric curve.</returns>
    public static List<(Vector2 Start, Vector2 End)> CreateParametricCurve2D(
        System.Func<double, double> xFunc, System.Func<double, double> yFunc,
        double tMin, double tMax, int segments = 200)
    {
        var points = new List<Vector2>();
        double step = (tMax - tMin) / segments;

        for (int i = 0; i <= segments; i++)
        {
            double t = tMin + i * step;
            points.Add(new Vector2((float)xFunc(t), (float)yFunc(t)));
        }

        var segments2 = new List<(Vector2, Vector2)>();
        for (int i = 0; i < points.Count - 1; i++)
        {
            segments2.Add((points[i], points[i + 1]));
        }

        return segments2;
    }

    /// <summary>Transforms a list of 3D vertices by a matrix.</summary>
    /// <param name="vertices">The vertices to transform.</param>
    /// <param name="transform">The transformation matrix.</param>
    /// <returns>The transformed vertices.</returns>
    public static List<Vector3> TransformVertices(List<Vector3> vertices, Matrix4x4 transform)
    {
        var result = new List<Vector3>(vertices.Count);

        foreach (var v in vertices)
        {
            result.Add(Vector3.Transform(v, transform));
        }

        return result;
    }

    /// <summary>Computes the face normals for a mesh.</summary>
    /// <param name="vertices">The mesh vertices.</param>
    /// <param name="faces">The mesh faces.</param>
    /// <returns>The face normals.</returns>
    public static List<Vector3> ComputeFaceNormals(List<Vector3> vertices, List<List<int>> faces)
    {
        var normals = new List<Vector3>();

        foreach (var face in faces)
        {
            if (face == null || face.Count < 3)
            {
                normals.Add(Vector3.UnitY);
                continue;
            }

            Vector3 v0 = vertices[face[0]];
            Vector3 v1 = vertices[face[1]];
            Vector3 v2 = vertices[face[2]];

            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;
            Vector3 normal = Vector3.Cross(edge1, edge2);

            float length = normal.Length();
            if (length > 1e-6f)
                normal /= length;
            else
                normal = Vector3.UnitY;

            normals.Add(normal);
        }

        return normals;
    }
}
