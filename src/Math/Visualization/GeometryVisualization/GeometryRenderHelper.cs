namespace MathVerse.Math.Visualization.GeometryVisualization;

using System.Collections.Immutable;

/// <summary>Provides helper methods for geometry visualization including triangulation,
/// normal computation, and parametric shape generation.</summary>
public static class GeometryRenderHelper
{
    /// <summary>Triangulates a 2D polygon using the ear-clipping algorithm.</summary>
    /// <param name="vertices">The ordered vertices of the polygon (2D: x,y pairs projected to XY plane).</param>
    /// <returns>A list of triangle index triplets representing the triangulation.</returns>
    public static List<(int A, int B, int C)> TriangulatePolygon(double[][] vertices)
    {
        int n = vertices.Length;
        var triangles = new List<(int A, int B, int C)>();
        if (n < 3) return triangles;

        var indices = new List<int>();
        for (int i = 0; i < n; i++) indices.Add(i);

        bool isCCW = PolygonArea(vertices) > 0;
        int remaining = n;

        int failSafe = n * 2;
        int idx = 0;

        while (remaining > 2 && failSafe > 0)
        {
            failSafe--;
            int prev = (idx - 1 + remaining) % remaining;
            int curr = idx % remaining;
            int next = (idx + 1) % remaining;

            int iPrev = indices[prev];
            int iCurr = indices[curr];
            int iNext = indices[next];

            double ax = vertices[iCurr][0] - vertices[iPrev][0];
            double ay = vertices[iCurr][1] - vertices[iPrev][1];
            double bx = vertices[iNext][0] - vertices[iCurr][0];
            double by = vertices[iNext][1] - vertices[iCurr][1];

            double cross = ax * by - ay * bx;
            bool convex = isCCW ? cross > 1e-12 : cross < -1e-12;

            if (convex && !IsEar(vertices, indices, prev, curr, next, remaining))
                convex = false;

            if (convex)
            {
                triangles.Add(isCCW ? (iPrev, iCurr, iNext) : (iPrev, iNext, iCurr));
                indices.RemoveAt(curr);
                remaining--;
                if (idx >= remaining) idx = 0;
            }
            else
            {
                idx++;
                if (idx >= remaining)
                {
                    idx = 0;
                    failSafe--;
                }
            }
        }

        return triangles;
    }

    /// <summary>Computes per-vertex surface normals for a triangle mesh using area-weighted averaging.</summary>
    /// <param name="vertices">The vertex positions (each element is [x, y, z]).</param>
    /// <param name="faces">The triangle face indices (each element is [i0, i1, i2]).</param>
    /// <returns>A parallel array of vertex normals (each element is [nx, ny, nz], normalized).</returns>
    public static double[][] ComputeSurfaceNormals(double[][] vertices, int[][] faces)
    {
        int vertexCount = vertices.Length;
        var normals = new double[vertexCount][];

        for (int i = 0; i < vertexCount; i++)
            normals[i] = [0.0, 0.0, 0.0];

        for (int f = 0; f < faces.Length; f++)
        {
            int i0 = faces[f][0];
            int i1 = faces[f][1];
            int i2 = faces[f][2];

            if (i0 >= vertexCount || i1 >= vertexCount || i2 >= vertexCount) continue;

            double ax = vertices[i1][0] - vertices[i0][0];
            double ay = vertices[i1][1] - vertices[i0][1];
            double az = vertices[i1][2] - vertices[i0][2];

            double bx = vertices[i2][0] - vertices[i0][0];
            double by = vertices[i2][1] - vertices[i0][1];
            double bz = vertices[i2][2] - vertices[i0][2];

            double nx = ay * bz - az * by;
            double ny = az * bx - ax * bz;
            double nz = ax * by - ay * bx;

            normals[i0][0] += nx; normals[i0][1] += ny; normals[i0][2] += nz;
            normals[i1][0] += nx; normals[i1][1] += ny; normals[i1][2] += nz;
            normals[i2][0] += nx; normals[i2][1] += ny; normals[i2][2] += nz;
        }

        for (int i = 0; i < vertexCount; i++)
        {
            double len = System.Math.Sqrt(
                normals[i][0] * normals[i][0] +
                normals[i][1] * normals[i][1] +
                normals[i][2] * normals[i][2]);
            if (len > 1e-15)
            {
                normals[i][0] /= len;
                normals[i][1] /= len;
                normals[i][2] /= len;
            }
            else
            {
                normals[i] = [0.0, 0.0, 1.0];
            }
        }

        return normals;
    }

    /// <summary>Generates points along a circle in 3D space.</summary>
    /// <param name="cx">Center X coordinate.</param>
    /// <param name="cy">Center Y coordinate.</param>
    /// <param name="cz">Center Z coordinate.</param>
    /// <param name="radius">The circle radius.</param>
    /// <param name="segments">Number of segments (default 64).</param>
    /// <returns>A tuple of X, Y, Z coordinate arrays.</returns>
    public static (ImmutableArray<double> X, ImmutableArray<double> Y, ImmutableArray<double> Z)
        GenerateCirclePoints(double cx, double cy, double cz, double radius, int segments = 64)
    {
        var xBuilder = ImmutableArray.CreateBuilder<double>(segments + 1);
        var yBuilder = ImmutableArray.CreateBuilder<double>(segments + 1);
        var zBuilder = ImmutableArray.CreateBuilder<double>(segments + 1);

        for (int i = 0; i <= segments; i++)
        {
            double angle = 2.0 * System.Math.PI * i / segments;
            xBuilder.Add(cx + radius * System.Math.Cos(angle));
            yBuilder.Add(cy + radius * System.Math.Sin(angle));
            zBuilder.Add(cz);
        }

        return (xBuilder.ToImmutable(), yBuilder.ToImmutable(), zBuilder.ToImmutable());
    }

    /// <summary>Generates points on a sphere surface using latitude-longitude sampling.</summary>
    /// <param name="cx">Center X coordinate.</param>
    /// <param name="cy">Center Y coordinate.</param>
    /// <param name="cz">Center Z coordinate.</param>
    /// <param name="radius">The sphere radius.</param>
    /// <param name="latSegments">Number of latitude segments (default 16).</param>
    /// <param name="lonSegments">Number of longitude segments (default 32).</param>
    /// <returns>A tuple of vertex arrays and face index arrays.</returns>
    public static (double[][] Vertices, int[][] Faces) GenerateSpherePoints(
        double cx, double cy, double cz, double radius,
        int latSegments = 16, int lonSegments = 32)
    {
        int vertexCount = (latSegments + 1) * (lonSegments + 1);
        var vertices = new double[vertexCount][];

        int vi = 0;
        for (int lat = 0; lat <= latSegments; lat++)
        {
            double theta = System.Math.PI * lat / latSegments;
            double sinTheta = System.Math.Sin(theta);
            double cosTheta = System.Math.Cos(theta);

            for (int lon = 0; lon <= lonSegments; lon++)
            {
                double phi = 2.0 * System.Math.PI * lon / lonSegments;
                double sinPhi = System.Math.Sin(phi);
                double cosPhi = System.Math.Cos(phi);

                double x = cx + radius * sinTheta * cosPhi;
                double y = cy + radius * cosTheta;
                double z = cz + radius * sinTheta * sinPhi;

                vertices[vi++] = [x, y, z];
            }
        }

        int faceCount = latSegments * lonSegments * 2;
        var faces = new int[faceCount][];
        int fi = 0;

        for (int lat = 0; lat < latSegments; lat++)
        {
            for (int lon = 0; lon < lonSegments; lon++)
            {
                int current = lat * (lonSegments + 1) + lon;
                int next = current + lonSegments + 1;

                faces[fi++] = [current, next, current + 1];
                faces[fi++] = [current + 1, next, next + 1];
            }
        }

        return (vertices, faces);
    }

    private static bool IsEar(double[][] vertices, List<int> indices, int prev, int curr, int next, int count)
    {
        int iPrev = indices[prev];
        int iCurr = indices[curr];
        int iNext = indices[next];

        for (int i = 0; i < count; i++)
        {
            if (i == prev || i == curr || i == next) continue;
            int iPt = indices[i];
            if (PointInTriangle(
                vertices[iPt][0], vertices[iPt][1],
                vertices[iPrev][0], vertices[iPrev][1],
                vertices[iCurr][0], vertices[iCurr][1],
                vertices[iNext][0], vertices[iNext][1]))
                return false;
        }

        return true;
    }

    private static bool PointInTriangle(double px, double py,
        double ax, double ay, double bx, double by, double cx, double cy)
    {
        double d1 = (px - bx) * (ay - by) - (ax - bx) * (py - by);
        double d2 = (px - cx) * (by - cy) - (bx - cx) * (py - cy);
        double d3 = (px - ax) * (cy - ay) - (cx - ax) * (py - ay);

        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(hasNeg && hasPos);
    }

    private static double PolygonArea(double[][] vertices)
    {
        double area = 0;
        int n = vertices.Length;
        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            area += vertices[i][0] * vertices[j][1];
            area -= vertices[j][0] * vertices[i][1];
        }
        return area * 0.5;
    }
}
