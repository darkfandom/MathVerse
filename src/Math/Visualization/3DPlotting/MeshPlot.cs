namespace MathVerse.Math.Visualization._3DPlotting;
using System.Numerics;

/// <summary>Generates triangle mesh plots from arbitrary vertex and face data.</summary>
public sealed class MeshPlot
{
    /// <summary>Creates a mesh plot from vertex positions and face indices, with optional per-vertex normals.</summary>
    /// <param name="vertices">The vertex positions as arrays of [x, y, z].</param>
    /// <param name="faces">The face indices, where each inner array contains 3 or more vertex indices.</param>
    /// <param name="normals">Optional per-vertex normals as arrays of [nx, ny, nz]. If null, normals are auto-computed.</param>
    /// <returns>A Plot3DResult containing the triangle mesh.</returns>
    public static Plot3DResult Create(double[][] vertices, int[][] faces, double[][]? normals = null)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        ArgumentNullException.ThrowIfNull(faces);

        List<Vector3> verts = [];
        List<int[]> faceList = [];
        List<Vector3> norms = [];

        foreach (double[] v in vertices)
        {
            if (v.Length < 3) throw new ArgumentException("Each vertex must have at least 3 components.", nameof(vertices));
            verts.Add(new Vector3((float)v[0], (float)v[1], (float)v[2]));
        }

        foreach (int[] f in faces)
        {
            if (f.Length < 3) throw new ArgumentException("Each face must have at least 3 indices.", nameof(faces));
            faceList.Add(f);
        }

        if (normals != null && normals.Length == vertices.Length)
        {
            foreach (double[] n in normals)
            {
                if (n.Length < 3) throw new ArgumentException("Each normal must have at least 3 components.", nameof(normals));
                Vector3 nv = new((float)n[0], (float)n[1], (float)n[2]);
                float len = nv.Length();
                norms.Add(len > 1e-6f ? nv / len : Vector3.UnitY);
            }
        }
        else
        {
            SurfacePlot.ComputeNormalsForFaces(verts, faceList, norms);
        }

        Vector3 bmin = verts.Count > 0 ? verts[0] : Vector3.Zero;
        Vector3 bmax = bmin;
        foreach (Vector3 v in verts)
        {
            bmin = Vector3.Min(bmin, v);
            bmax = Vector3.Max(bmax, v);
        }

        return new Plot3DResult
        {
            Vertices = verts,
            Faces = faceList,
            Normals = norms,
            VertexColors = [],
            Bounds = new Rendering.BoundingBox(bmin, bmax),
            PlotType = Plot3DType.Mesh
        };
    }
}
