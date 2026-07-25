using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;
using MathVerse.Math.Geometry.Meshes;
using MathVerse.Math.Geometry.Surfaces;

namespace MathVerse.Math.Geometry.Tessellation;

/// <summary>Provides static methods for tessellating parametric surfaces into triangle meshes.</summary>
public static class SurfaceTessellator
{
    /// <summary>Tessellates a Bezier surface into a triangle mesh.</summary>
    /// <param name="surface">The Bezier surface to tessellate.</param>
    /// <param name="uRes">The number of subdivisions in the U direction.</param>
    /// <param name="vRes">The number of subdivisions in the V direction.</param>
    /// <returns>A triangle mesh approximating the surface.</returns>
    public static TriangleMesh Tessellate(BezierSurface surface, int uRes, int vRes)
    {
        return TessellateParametric(surface.PointAt, 0.0, 1.0, 0.0, 1.0, uRes, vRes);
    }

    /// <summary>Tessellates a B-spline surface into a triangle mesh.</summary>
    /// <param name="surface">The B-spline surface to tessellate.</param>
    /// <param name="uRes">The number of subdivisions in the U direction.</param>
    /// <param name="vRes">The number of subdivisions in the V direction.</param>
    /// <returns>A triangle mesh approximating the surface.</returns>
    public static TriangleMesh Tessellate(BSplineSurface surface, int uRes, int vRes)
    {
        return TessellateParametric(surface.PointAt, 0.0, 1.0, 0.0, 1.0, uRes, vRes);
    }

    /// <summary>Tessellates a sphere into a triangle mesh using latitude-longitude subdivision.</summary>
    /// <param name="sphere">The sphere to tessellate.</param>
    /// <param name="latitudeDiv">The number of latitude divisions.</param>
    /// <param name="longitudeDiv">The number of longitude divisions.</param>
    /// <returns>A triangle mesh approximating the sphere.</returns>
    public static TriangleMesh TessellateSphere(Sphere3D sphere, int latitudeDiv, int longitudeDiv)
    {
        MeshBuilder builder = new();

        double phiStep = System.Math.PI / System.Math.Max(latitudeDiv, 1);
        double thetaStep = 2.0 * System.Math.PI / System.Math.Max(longitudeDiv, 1);

        for (int i = 0; i <= latitudeDiv; i++)
        {
            double phi = i * phiStep;
            double sinPhi = System.Math.Sin(phi);
            double cosPhi = System.Math.Cos(phi);

            for (int j = 0; j <= longitudeDiv; j++)
            {
                double theta = j * thetaStep;
                double sinTheta = System.Math.Sin(theta);
                double cosTheta = System.Math.Cos(theta);

                double nx = sinPhi * cosTheta;
                double ny = cosPhi;
                double nz = sinPhi * sinTheta;

                Point3D pos = new Point3D(
                    sphere.Center.X + sphere.Radius * nx,
                    sphere.Center.Y + sphere.Radius * ny,
                    sphere.Center.Z + sphere.Radius * nz);

                builder.AddVertex(new Vertex(pos, new Vector3D(nx, ny, nz), (j / (double)longitudeDiv, i / (double)latitudeDiv)));
            }
        }

        for (int i = 0; i < latitudeDiv; i++)
        {
            for (int j = 0; j < longitudeDiv; j++)
            {
                int topLeft = i * (longitudeDiv + 1) + j;
                int topRight = topLeft + 1;
                int bottomLeft = topLeft + (longitudeDiv + 1);
                int bottomRight = bottomLeft + 1;

                if (i != 0)
                    builder.AddTriangle(topLeft, bottomLeft, topRight);

                if (i != latitudeDiv - 1)
                    builder.AddTriangle(topRight, bottomLeft, bottomRight);
            }
        }

        return builder.Build();
    }

    /// <summary>Tessellates a cylinder into a triangle mesh using radial and height divisions.</summary>
    /// <param name="cylinder">The cylinder to tessellate.</param>
    /// <param name="radialDiv">The number of radial divisions around the axis.</param>
    /// <param name="heightDiv">The number of height divisions.</param>
    /// <returns>A triangle mesh approximating the cylinder.</returns>
    public static TriangleMesh TessellateCylinder(Cylinder3D cylinder, int radialDiv, int heightDiv)
    {
        MeshBuilder builder = new();
        double halfH = cylinder.Height * 0.5;
        double thetaStep = 2.0 * System.Math.PI / System.Math.Max(radialDiv, 1);
        double heightStep = cylinder.Height / System.Math.Max(heightDiv, 1);

        for (int i = 0; i <= heightDiv; i++)
        {
            double y = cylinder.Center.Y - halfH + i * heightStep;
            double v = (double)i / System.Math.Max(heightDiv, 1);

            for (int j = 0; j <= radialDiv; j++)
            {
                double theta = j * thetaStep;
                double cosT = System.Math.Cos(theta);
                double sinT = System.Math.Sin(theta);

                Point3D pos = new Point3D(
                    cylinder.Center.X + cylinder.Radius * cosT,
                    y,
                    cylinder.Center.Z + cylinder.Radius * sinT);

                builder.AddVertex(new Vertex(pos, new Vector3D(cosT, 0, sinT), ((double)j / radialDiv, v)));
            }
        }

        for (int i = 0; i < heightDiv; i++)
        {
            for (int j = 0; j < radialDiv; j++)
            {
                int topLeft = i * (radialDiv + 1) + j;
                int topRight = topLeft + 1;
                int bottomLeft = topLeft + (radialDiv + 1);
                int bottomRight = bottomLeft + 1;

                builder.AddTriangle(topLeft, bottomLeft, topRight);
                builder.AddTriangle(topRight, bottomLeft, bottomRight);
            }
        }

        int topCenter = builder.AddVertex(new Vertex(
            new Point3D(cylinder.Center.X, cylinder.Center.Y - halfH, cylinder.Center.Z),
            new Vector3D(0, -1, 0),
            (0.5, 0.5)));

        int bottomCenter = builder.AddVertex(new Vertex(
            new Point3D(cylinder.Center.X, cylinder.Center.Y + halfH, cylinder.Center.Z),
            new Vector3D(0, 1, 0),
            (0.5, 0.5)));

        for (int j = 0; j < radialDiv; j++)
        {
            int current = j;
            int next = j + 1;
            builder.AddTriangle(topCenter, next, current);

            int rowOffset = heightDiv * (radialDiv + 1);
            builder.AddTriangle(bottomCenter, rowOffset + current, rowOffset + next);
        }

        return builder.Build();
    }

    private static TriangleMesh TessellateParametric(
        Func<double, double, Point3D> evaluate,
        double uMin,
        double uMax,
        double vMin,
        double vMax,
        int uRes,
        int vRes)
    {
        MeshBuilder builder = new();
        double uStep = (uMax - uMin) / System.Math.Max(uRes, 1);
        double vStep = (vMax - vMin) / System.Math.Max(vRes, 1);

        for (int j = 0; j <= vRes; j++)
        {
            double v = vMin + j * vStep;
            for (int i = 0; i <= uRes; i++)
            {
                double u = uMin + i * uStep;
                Point3D pos = evaluate(u, v);
                builder.AddVertex(new Vertex(pos, Vector3D.Zero, (u, v)));
            }
        }

        for (int j = 0; j < vRes; j++)
        {
            for (int i = 0; i < uRes; i++)
            {
                int topLeft = j * (uRes + 1) + i;
                int topRight = topLeft + 1;
                int bottomLeft = topLeft + (uRes + 1);
                int bottomRight = bottomLeft + 1;

                builder.AddTriangle(topLeft, bottomLeft, topRight);
                builder.AddTriangle(topRight, bottomLeft, bottomRight);
            }
        }

        return builder.Build();
    }
}
