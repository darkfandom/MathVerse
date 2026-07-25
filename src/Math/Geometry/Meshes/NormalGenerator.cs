using System.Collections.Immutable;
using MathVerse.Math.Geometry.Geometry3D;

namespace MathVerse.Math.Geometry.Meshes;

/// <summary>Provides static methods for computing mesh normals and tangents.</summary>
public static class NormalGenerator
{
    /// <summary>Computes per-vertex normals weighted by the area of adjacent triangles.</summary>
    /// <param name="mesh">The input mesh.</param>
    /// <returns>An immutable array of area-weighted vertex normals, one per vertex.</returns>
    public static ImmutableArray<Vector3D> ComputeVertexNormals(TriangleMesh mesh)
    {
        Vector3D[] normals = new Vector3D[mesh.VertexCount];

        for (int i = 0; i < mesh.Faces.Length; i++)
        {
            TriangleFace f = mesh.Faces[i];
            Point3D a = mesh.Vertices[f.V0].Position;
            Point3D b = mesh.Vertices[f.V1].Position;
            Point3D c = mesh.Vertices[f.V2].Position;

            Vector3D ab = new(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
            Vector3D ac = new(c.X - a.X, c.Y - a.Y, c.Z - a.Z);
            Vector3D cross = ab.Cross(ac);
            double area = cross.Length * 0.5;
            Vector3D faceNormal = cross.Normalize();

            Vector3D weighted = faceNormal.Scale(area);
            normals[f.V0] = normals[f.V0].Add(weighted);
            normals[f.V1] = normals[f.V1].Add(weighted);
            normals[f.V2] = normals[f.V2].Add(weighted);
        }

        for (int i = 0; i < normals.Length; i++)
            normals[i] = normals[i].Normalize();

        return ImmutableArray.Create(normals);
    }

    /// <summary>Computes the face normal for each triangle.</summary>
    /// <param name="mesh">The input mesh.</param>
    /// <returns>An immutable array of unit face normals, one per face.</returns>
    public static ImmutableArray<Vector3D> ComputeFaceNormals(TriangleMesh mesh)
    {
        ImmutableArray<Vector3D>.Builder builder = ImmutableArray.CreateBuilder<Vector3D>(mesh.Faces.Length);

        for (int i = 0; i < mesh.Faces.Length; i++)
        {
            TriangleFace f = mesh.Faces[i];
            Point3D a = mesh.Vertices[f.V0].Position;
            Point3D b = mesh.Vertices[f.V1].Position;
            Point3D c = mesh.Vertices[f.V2].Position;

            Vector3D ab = new(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
            Vector3D ac = new(c.X - a.X, c.Y - a.Y, c.Z - a.Z);
            builder.Add(ab.Cross(ac).Normalize());
        }

        return builder.ToImmutable();
    }

    /// <summary>Computes per-vertex normals weighted by the angle subtended at each vertex.</summary>
    /// <param name="mesh">The input mesh.</param>
    /// <returns>An immutable array of angle-weighted vertex normals, one per vertex.</returns>
    public static ImmutableArray<Vector3D> ComputeSmoothNormals(TriangleMesh mesh)
    {
        Vector3D[] normals = new Vector3D[mesh.VertexCount];

        for (int i = 0; i < mesh.Faces.Length; i++)
        {
            TriangleFace f = mesh.Faces[i];
            Point3D pa = mesh.Vertices[f.V0].Position;
            Point3D pb = mesh.Vertices[f.V1].Position;
            Point3D pc = mesh.Vertices[f.V2].Position;

            Vector3D ab = new(pb.X - pa.X, pb.Y - pa.Y, pb.Z - pa.Z);
            Vector3D ac = new(pc.X - pa.X, pc.Y - pa.Y, pc.Z - pa.Z);
            Vector3D bc = new(pc.X - pb.X, pc.Y - pb.Y, pc.Z - pb.Z);

            Vector3D cross = ab.Cross(ac);
            Vector3D faceNormal = cross.Normalize();

            double angleA = ab.AngleTo(ac.Negate());
            double angleB = new Vector3D(-ab.X, -ab.Y, -ab.Z).AngleTo(bc);
            double angleC = new Vector3D(-ac.X, -ac.Y, -ac.Z).AngleTo(new Vector3D(-bc.X, -bc.Y, -bc.Z));

            normals[f.V0] = normals[f.V0].Add(faceNormal.Scale(angleA));
            normals[f.V1] = normals[f.V1].Add(faceNormal.Scale(angleB));
            normals[f.V2] = normals[f.V2].Add(faceNormal.Scale(angleC));
        }

        for (int i = 0; i < normals.Length; i++)
            normals[i] = normals[i].Normalize();

        return ImmutableArray.Create(normals);
    }

    /// <summary>Computes tangent vectors for each vertex using UV gradients.</summary>
    /// <param name="mesh">The input mesh.</param>
    /// <returns>An immutable array of tangent vectors, one per vertex.</returns>
    public static ImmutableArray<Vector3D> ComputeTangents(TriangleMesh mesh)
    {
        Vector3D[] tangents = new Vector3D[mesh.VertexCount];

        for (int i = 0; i < mesh.Faces.Length; i++)
        {
            TriangleFace f = mesh.Faces[i];
            Vertex v0 = mesh.Vertices[f.V0];
            Vertex v1 = mesh.Vertices[f.V1];
            Vertex v2 = mesh.Vertices[f.V2];

            Point3D p0 = v0.Position;
            Point3D p1 = v1.Position;
            Point3D p2 = v2.Position;

            double duv0x = v1.UV.U - v0.UV.U;
            double duv0y = v1.UV.V - v0.UV.V;
            double duv1x = v2.UV.U - v0.UV.U;
            double duv1y = v2.UV.V - v0.UV.V;

            Vector3D edge1 = new(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D edge2 = new(p2.X - p0.X, p2.Y - p0.Y, p2.Z - p0.Z);

            double denom = duv0x * duv1y - duv1x * duv0y;
            if (System.Math.Abs(denom) < 1e-30)
                continue;

            double invDenom = 1.0 / denom;
            Vector3D t = edge1.Scale(duv1y * invDenom).Add(edge2.Scale(-duv0y * invDenom));

            tangents[f.V0] = tangents[f.V0].Add(t);
            tangents[f.V1] = tangents[f.V1].Add(t);
            tangents[f.V2] = tangents[f.V2].Add(t);
        }

        for (int i = 0; i < tangents.Length; i++)
            tangents[i] = tangents[i].Normalize();

        return ImmutableArray.Create(tangents);
    }
}
