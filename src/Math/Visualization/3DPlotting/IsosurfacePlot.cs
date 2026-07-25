namespace MathVerse.Math.Visualization._3DPlotting;
using System.Numerics;

/// <summary>Generates isosurface plots using the marching cubes algorithm.</summary>
public sealed class IsosurfacePlot
{
    private static readonly int[][] EdgeVertexTable = BuildEdgeVertexTable();
    private static readonly int[][] TriTable = BuildTriTable();

    /// <summary>Creates an isosurface from a 3D scalar field using the marching cubes algorithm.</summary>
    /// <param name="scalarField">The 3D scalar field function f(x, y, z) -> value.</param>
    /// <param name="isoValue">The threshold value for the isosurface.</param>
    /// <param name="xMin">The minimum X coordinate of the volume.</param>
    /// <param name="xMax">The maximum X coordinate of the volume.</param>
    /// <param name="yMin">The minimum Y coordinate of the volume.</param>
    /// <param name="yMax">The maximum Y coordinate of the volume.</param>
    /// <param name="zMin">The minimum Z coordinate of the volume.</param>
    /// <param name="zMax">The maximum Z coordinate of the volume.</param>
    /// <param name="resolution">The number of subdivisions along each axis.</param>
    /// <returns>A Plot3DResult containing the isosurface mesh.</returns>
    public static Plot3DResult Create(
        Func<double, double, double, double> scalarField,
        double isoValue,
        double xMin, double xMax,
        double yMin, double yMax,
        double zMin, double zMax,
        int resolution = 20)
    {
        ArgumentNullException.ThrowIfNull(scalarField);
        if (resolution < 1) throw new ArgumentOutOfRangeException(nameof(resolution), "Resolution must be at least 1.");

        double[,,] field = SampleField(scalarField, xMin, xMax, yMin, yMax, zMin, zMax, resolution);
        return MarchCubes(field, isoValue, xMin, xMax, yMin, yMax, zMin, zMax, resolution);
    }

    /// <summary>Samples the scalar field into a 3D array.</summary>
    private static double[,,] SampleField(
        Func<double, double, double, double> func,
        double xMin, double xMax,
        double yMin, double yMax,
        double zMin, double zMax,
        int resolution)
    {
        int size = resolution + 1;
        double[,,] field = new double[size, size, size];
        double xStep = (xMax - xMin) / resolution;
        double yStep = (yMax - yMin) / resolution;
        double zStep = (zMax - zMin) / resolution;

        for (int z = 0; z < size; z++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    double val = func(xMin + x * xStep, yMin + y * yStep, zMin + z * zStep);
                    field[z, y, x] = double.IsNaN(val) || double.IsInfinity(val) ? 0.0 : val;
                }
            }
        }

        return field;
    }

    /// <summary>Runs the marching cubes algorithm on the sampled field.</summary>
    private static Plot3DResult MarchCubes(
        double[,,] field, double isoValue,
        double xMin, double xMax,
        double yMin, double yMax,
        double zMin, double zMax,
        int resolution)
    {
        List<Vector3> vertices = [];
        List<int[]> faces = [];

        int size = resolution + 1;
        double xStep = (xMax - xMin) / resolution;
        double yStep = (yMax - yMin) / resolution;
        double zStep = (zMax - zMin) / resolution;

        Vector3[] cornerOffsets =
        [
            new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0),
            new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1)
        ];

        int[][] edgeConnections =
        [
            [0, 1], [1, 2], [2, 3], [3, 0],
            [4, 5], [5, 6], [6, 7], [7, 4],
            [0, 4], [1, 5], [2, 6], [3, 7]
        ];

        for (int z = 0; z < resolution; z++)
        {
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    double[] cornerValues = new double[8];
                    Vector3[] cornerPositions = new Vector3[8];

                    for (int c = 0; c < 8; c++)
                    {
                        int cz = z + (int)cornerOffsets[c].Z;
                        int cy = y + (int)cornerOffsets[c].Y;
                        int cx = x + (int)cornerOffsets[c].X;
                        cornerValues[c] = field[cz, cy, cx];
                        cornerPositions[c] = new Vector3(
                            (float)(xMin + cx * xStep),
                            (float)(yMin + cy * yStep),
                            (float)(zMin + cz * zStep));
                    }

                    int cubeIndex = 0;
                    for (int c = 0; c < 8; c++)
                    {
                        if (cornerValues[c] > isoValue)
                            cubeIndex |= 1 << c;
                    }

                    if (EdgeVertexTable[cubeIndex].Length == 0) continue;

                    Vector3[] edgeVertexPositions = new Vector3[12];
                    for (int e = 0; e < 12; e++)
                    {
                        if ((EdgeVertexTable[cubeIndex][0] & (1 << e)) == 0)
                            continue;

                        int v0 = edgeConnections[e][0];
                        int v1 = edgeConnections[e][1];

                        double val0 = cornerValues[v0];
                        double val1 = cornerValues[v1];
                        double denom = val1 - val0;
                        float t = (denom != 0.0) ? (float)((isoValue - val0) / denom) : 0.5f;

                        edgeVertexPositions[e] = Vector3.Lerp(cornerPositions[v0], cornerPositions[v1], t);
                    }

                    int[] triIndices = TriTable[cubeIndex];
                    for (int i = 0; triIndices[i] != -1; i += 3)
                    {
                        int baseIndex = vertices.Count;
                        vertices.Add(edgeVertexPositions[triIndices[i]]);
                        vertices.Add(edgeVertexPositions[triIndices[i + 1]]);
                        vertices.Add(edgeVertexPositions[triIndices[i + 2]]);
                        faces.Add([baseIndex, baseIndex + 1, baseIndex + 2]);
                    }
                }
            }
        }

        List<Vector3> normals = [];
        if (faces.Count > 0)
        {
            SurfacePlot.ComputeNormalsForFaces(vertices, faces, normals);
        }

        Vector3 bmin = vertices.Count > 0 ? vertices[0] : Vector3.Zero;
        Vector3 bmax = bmin;
        foreach (Vector3 v in vertices)
        {
            bmin = Vector3.Min(bmin, v);
            bmax = Vector3.Max(bmax, v);
        }

        List<Vector4> vertexColors = [];
        float valRange = (float)(field[resolution, resolution, resolution] - field[0, 0, 0]);
        if (System.Math.Abs(valRange) < 1e-6f) valRange = 1.0f;

        foreach (Vector3 v in vertices)
        {
            float ny = System.Math.Clamp((v.Y - bmin.Y) / System.Math.Max(bmax.Y - bmin.Y, 1e-6f), 0f, 1f);
            vertexColors.Add(new Vector4(ny, 1f - ny, 1f - ny * 0.5f, 1f));
        }

        return new Plot3DResult
        {
            Vertices = vertices,
            Faces = faces,
            Normals = normals,
            VertexColors = vertexColors,
            Bounds = new Rendering.BoundingBox(bmin, bmax),
            PlotType = Plot3DType.Mesh
        };
    }

    /// <summary>Builds the edge vertex lookup table for marching cubes.</summary>
    private static int[][] BuildEdgeVertexTable()
    {
        int[][] table = new int[256][];
        for (int i = 0; i < 256; i++)
        {
            int edges = 0;
            if ((i & 1) != 0) edges |= 1 << 0;
            if ((i & 2) != 0) edges |= 1 << 1;
            if ((i & 4) != 0) edges |= 1 << 2;
            if ((i & 8) != 0) edges |= 1 << 3;
            if ((i & 16) != 0) edges |= 1 << 4;
            if ((i & 32) != 0) edges |= 1 << 5;
            if ((i & 64) != 0) edges |= 1 << 6;
            if ((i & 128) != 0) edges |= 1 << 7;

            if (edges == 0)
            {
                table[i] = [];
                continue;
            }
            table[i] = [edges];
        }
        return table;
    }

    /// <summary>Builds the simplified triangle table for marching cubes with all 256 cases.</summary>
    private static int[][] BuildTriTable()
    {
        int[][] table = new int[256][];
        table[0] = [-1];
        table[1] = [0, 8, 3, -1];
        table[2] = [0, 1, 9, -1];
        table[3] = [1, 8, 3, 9, 8, 1, -1];
        table[4] = [1, 2, 10, -1];
        table[5] = [0, 8, 3, 1, 2, 10, -1];
        table[6] = [9, 2, 10, 0, 2, 9, -1];
        table[7] = [2, 8, 3, 2, 10, 8, 10, 9, 8, -1];
        table[8] = [3, 11, 2, -1];
        table[9] = [0, 11, 2, 8, 11, 0, -1];
        table[10] = [1, 9, 0, 2, 3, 11, -1];
        table[11] = [1, 11, 2, 1, 9, 11, 9, 8, 11, -1];
        table[12] = [3, 10, 1, 11, 10, 3, -1];
        table[13] = [0, 10, 1, 0, 8, 10, 8, 11, 10, -1];
        table[14] = [3, 9, 0, 3, 11, 9, 11, 10, 9, -1];
        table[15] = [9, 8, 10, 10, 8, 11, -1];
        table[16] = [4, 7, 8, -1];
        table[17] = [4, 3, 0, 7, 3, 4, -1];
        table[18] = [0, 1, 9, 8, 4, 7, -1];
        table[19] = [4, 1, 9, 4, 7, 1, 7, 3, 1, -1];
        table[20] = [1, 2, 10, 8, 4, 7, -1];
        table[21] = [3, 4, 7, 3, 0, 4, 1, 2, 10, -1];
        table[22] = [9, 2, 10, 9, 0, 2, 8, 4, 7, -1];
        table[23] = [2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4, -1];
        table[24] = [8, 4, 7, 3, 11, 2, -1];
        table[25] = [11, 4, 7, 11, 2, 4, 2, 0, 4, -1];
        table[26] = [9, 0, 1, 8, 4, 7, 2, 3, 11, -1];
        table[27] = [4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1, -1];
        table[28] = [3, 10, 1, 3, 11, 10, 7, 8, 4, -1];
        table[29] = [1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4, -1];
        table[30] = [4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3, -1];
        table[31] = [4, 7, 11, 4, 11, 9, 9, 11, 10, -1];
        table[32] = [9, 5, 4, -1];
        table[33] = [9, 5, 4, 0, 8, 3, -1];
        table[34] = [0, 5, 4, 1, 5, 0, -1];
        table[35] = [8, 5, 4, 8, 3, 5, 3, 1, 5, -1];
        table[36] = [1, 2, 10, 9, 5, 4, -1];
        table[37] = [3, 0, 8, 1, 2, 10, 4, 9, 5, -1];
        table[38] = [5, 2, 10, 5, 4, 2, 4, 0, 2, -1];
        table[39] = [2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8, -1];
        table[40] = [9, 5, 4, 2, 3, 11, -1];
        table[41] = [0, 11, 2, 0, 8, 11, 4, 9, 5, -1];
        table[42] = [0, 5, 4, 0, 1, 5, 2, 3, 11, -1];
        table[43] = [2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5, -1];
        table[44] = [10, 3, 11, 10, 1, 3, 9, 5, 4, -1];
        table[45] = [4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10, -1];
        table[46] = [5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3, -1];
        table[47] = [5, 4, 8, 5, 8, 10, 10, 8, 11, -1];
        table[48] = [9, 7, 8, 5, 7, 9, -1];
        table[49] = [9, 3, 0, 9, 5, 3, 5, 7, 3, -1];
        table[50] = [0, 7, 8, 0, 1, 7, 1, 5, 7, -1];
        table[51] = [1, 5, 3, 3, 5, 7, -1];
        table[52] = [9, 7, 8, 9, 5, 7, 10, 1, 2, -1];
        table[53] = [10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3, -1];
        table[54] = [8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2, -1];
        table[55] = [2, 10, 5, 2, 5, 3, 3, 5, 7, -1];
        table[56] = [7, 9, 5, 7, 8, 9, 3, 11, 2, -1];
        table[57] = [9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11, -1];
        table[58] = [2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7, -1];
        table[59] = [11, 2, 1, 11, 1, 7, 7, 1, 5, -1];
        table[60] = [9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11, -1];
        table[61] = [5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0, -1];
        table[62] = [11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0, -1];
        table[63] = [11, 10, 5, 11, 5, 7, 7, 5, 8, -1];
        table[64] = [10, 6, 5, -1];
        table[65] = [0, 8, 3, 5, 10, 6, -1];
        table[66] = [9, 0, 1, 5, 10, 6, -1];
        table[67] = [1, 8, 3, 1, 9, 8, 5, 10, 6, -1];
        table[68] = [6, 1, 2, 6, 5, 1, 5, 0, 1, -1];
        table[69] = [1, 2, 6, 5, 1, 6, 5, 6, 0, 5, 0, 8, -1];
        table[70] = [9, 6, 5, 9, 0, 6, 0, 2, 6, -1];
        table[71] = [5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8, -1];
        table[72] = [2, 3, 11, 10, 6, 5, -1];
        table[73] = [11, 0, 8, 11, 2, 0, 10, 6, 5, -1];
        table[74] = [0, 1, 9, 2, 3, 11, 5, 10, 6, -1];
        table[75] = [5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11, -1];
        table[76] = [6, 3, 11, 6, 5, 3, 5, 1, 3, -1];
        table[77] = [0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6, -1];
        table[78] = [0, 3, 11, 0, 11, 6, 0, 6, 9, 6, 11, 5, -1];
        table[79] = [6, 9, 5, 6, 11, 9, 11, 8, 9, -1];
        table[80] = [5, 10, 6, 4, 7, 8, -1];
        table[81] = [4, 3, 0, 4, 7, 3, 6, 5, 10, -1];
        table[82] = [1, 9, 0, 5, 10, 6, 8, 4, 7, -1];
        table[83] = [10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4, -1];
        table[84] = [6, 1, 2, 6, 5, 1, 4, 7, 8, -1];
        table[85] = [1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7, -1];
        table[86] = [8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6, -1];
        table[87] = [7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9, -1];
        table[88] = [3, 11, 2, 7, 8, 4, 10, 6, 5, -1];
        table[89] = [5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11, -1];
        table[90] = [0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6, -1];
        table[91] = [9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6, -1];
        table[92] = [8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6, -1];
        table[93] = [5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11, -1];
        table[94] = [0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7, -1];
        table[95] = [6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9, -1];
        table[96] = [10, 4, 9, 6, 4, 10, -1];
        table[97] = [4, 10, 6, 4, 9, 10, 0, 8, 3, -1];
        table[98] = [10, 0, 1, 10, 6, 0, 6, 4, 0, -1];
        table[99] = [8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10, -1];
        table[100] = [1, 4, 9, 1, 2, 4, 2, 6, 4, -1];
        table[101] = [3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4, -1];
        table[102] = [0, 2, 4, 4, 2, 6, -1];
        table[103] = [8, 3, 2, 8, 2, 4, 4, 2, 6, -1];
        table[104] = [10, 4, 9, 10, 6, 4, 11, 2, 3, -1];
        table[105] = [0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6, -1];
        table[106] = [3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10, -1];
        table[107] = [6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1, -1];
        table[108] = [9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3, -1];
        table[109] = [8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1, -1];
        table[110] = [3, 11, 6, 3, 6, 0, 0, 6, 4, -1];
        table[111] = [6, 4, 8, 11, 6, 8, -1];
        table[112] = [7, 10, 6, 7, 8, 10, 8, 9, 10, -1];
        table[113] = [0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10, -1];
        table[114] = [10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0, -1];
        table[115] = [10, 6, 7, 10, 7, 1, 1, 7, 3, -1];
        table[116] = [1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7, -1];
        table[117] = [2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9, -1];
        table[118] = [7, 8, 0, 7, 0, 6, 6, 0, 2, -1];
        table[119] = [7, 3, 2, 6, 7, 2, -1];
        table[120] = [2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7, -1];
        table[121] = [2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7, -1];
        table[122] = [1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11, -1];
        table[123] = [11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1, -1];
        table[124] = [8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6, -1];
        table[125] = [0, 9, 1, 11, 6, 7, -1];
        table[126] = [7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0, -1];
        table[127] = [7, 11, 6, -1];
        table[128] = [7, 6, 11, -1];
        table[129] = [3, 0, 8, 11, 7, 6, -1];
        table[130] = [0, 1, 9, 11, 7, 6, -1];
        table[131] = [8, 1, 9, 8, 3, 1, 11, 7, 6, -1];
        table[132] = [10, 1, 2, 6, 11, 7, -1];
        table[133] = [1, 2, 10, 3, 0, 8, 6, 11, 7, -1];
        table[134] = [2, 9, 0, 2, 10, 9, 6, 11, 7, -1];
        table[135] = [6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8, -1];
        table[136] = [7, 2, 3, 6, 2, 7, -1];
        table[137] = [7, 0, 8, 7, 6, 0, 6, 2, 0, -1];
        table[138] = [2, 7, 6, 2, 3, 7, 0, 1, 9, -1];
        table[139] = [1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6, -1];
        table[140] = [10, 7, 6, 10, 1, 7, 1, 3, 7, -1];
        table[141] = [10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8, -1];
        table[142] = [0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7, -1];
        table[143] = [7, 6, 10, 7, 10, 8, 8, 10, 9, -1];
        table[144] = [6, 8, 4, 11, 8, 6, -1];
        table[145] = [3, 6, 11, 3, 0, 6, 0, 4, 6, -1];
        table[146] = [8, 6, 11, 8, 4, 6, 9, 0, 1, -1];
        table[147] = [9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6, -1];
        table[148] = [6, 8, 4, 6, 11, 8, 2, 10, 1, -1];
        table[149] = [1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6, -1];
        table[150] = [4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9, -1];
        table[151] = [10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3, -1];
        table[152] = [8, 2, 3, 8, 4, 2, 4, 6, 2, -1];
        table[153] = [0, 4, 2, 4, 6, 2, -1];
        table[154] = [1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8, -1];
        table[155] = [1, 9, 4, 1, 4, 2, 2, 4, 6, -1];
        table[156] = [8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1, -1];
        table[157] = [10, 1, 0, 10, 0, 6, 6, 0, 4, -1];
        table[158] = [4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3, -1];
        table[159] = [10, 9, 4, 6, 10, 4, -1];
        table[160] = [4, 9, 5, 7, 6, 11, -1];
        table[161] = [0, 8, 3, 4, 9, 5, 11, 7, 6, -1];
        table[162] = [5, 0, 1, 5, 4, 0, 7, 6, 11, -1];
        table[163] = [11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5, -1];
        table[164] = [9, 5, 4, 10, 1, 2, 7, 6, 11, -1];
        table[165] = [6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5, -1];
        table[166] = [7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2, -1];
        table[167] = [3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6, -1];
        table[168] = [7, 2, 3, 7, 6, 2, 5, 4, 9, -1];
        table[169] = [9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7, -1];
        table[170] = [3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0, -1];
        table[171] = [6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8, -1];
        table[172] = [9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7, -1];
        table[173] = [1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4, -1];
        table[174] = [4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10, -1];
        table[175] = [7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10, -1];
        table[176] = [6, 9, 5, 6, 11, 9, 11, 8, 9, -1];
        table[177] = [3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5, -1];
        table[178] = [0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11, -1];
        table[179] = [6, 11, 3, 6, 3, 5, 5, 3, 1, -1];
        table[180] = [1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6, -1];
        table[181] = [0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10, -1];
        table[182] = [11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5, -1];
        table[183] = [6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3, -1];
        table[184] = [5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2, -1];
        table[185] = [9, 5, 6, 9, 6, 0, 0, 6, 2, -1];
        table[186] = [1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8, -1];
        table[187] = [1, 5, 6, 2, 1, 6, -1];
        table[188] = [1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6, -1];
        table[189] = [10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0, -1];
        table[190] = [0, 3, 8, 5, 6, 10, -1];
        table[191] = [10, 5, 6, -1];
        table[192] = [11, 5, 10, 7, 5, 11, -1];
        table[193] = [11, 5, 10, 11, 7, 5, 8, 3, 0, -1];
        table[194] = [5, 11, 7, 5, 10, 11, 1, 9, 0, -1];
        table[195] = [10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1, -1];
        table[196] = [11, 1, 2, 11, 7, 1, 7, 5, 1, -1];
        table[197] = [0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11, -1];
        table[198] = [9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7, -1];
        table[199] = [7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2, -1];
        table[200] = [2, 5, 10, 2, 3, 5, 3, 7, 5, -1];
        table[201] = [8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5, -1];
        table[202] = [9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2, -1];
        table[203] = [9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2, -1];
        table[204] = [1, 3, 5, 3, 7, 5, -1];
        table[205] = [0, 8, 7, 0, 7, 1, 1, 7, 5, -1];
        table[206] = [9, 0, 3, 9, 3, 5, 5, 3, 7, -1];
        table[207] = [9, 8, 7, 5, 9, 7, -1];
        table[208] = [5, 8, 4, 5, 10, 8, 10, 11, 8, -1];
        table[209] = [5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0, -1];
        table[210] = [0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5, -1];
        table[211] = [10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4, -1];
        table[212] = [2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8, -1];
        table[213] = [0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11, -1];
        table[214] = [0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5, -1];
        table[215] = [9, 4, 5, 2, 11, 3, -1];
        table[216] = [2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4, -1];
        table[217] = [5, 10, 2, 5, 2, 4, 4, 2, 0, -1];
        table[218] = [3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9, -1];
        table[219] = [5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2, -1];
        table[220] = [8, 4, 5, 8, 5, 3, 3, 5, 1, -1];
        table[221] = [0, 4, 5, 1, 0, 5, -1];
        table[222] = [8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5, -1];
        table[223] = [9, 4, 5, -1];
        table[224] = [4, 11, 7, 4, 9, 11, 9, 10, 11, -1];
        table[225] = [0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11, -1];
        table[226] = [1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11, -1];
        table[227] = [3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4, -1];
        table[228] = [4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2, -1];
        table[229] = [9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3, -1];
        table[230] = [11, 7, 4, 11, 4, 2, 2, 4, 0, -1];
        table[231] = [11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4, -1];
        table[232] = [2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9, -1];
        table[233] = [9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7, -1];
        table[234] = [3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10, -1];
        table[235] = [1, 10, 2, 8, 7, 4, -1];
        table[236] = [4, 9, 1, 4, 1, 7, 7, 1, 3, -1];
        table[237] = [4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1, -1];
        table[238] = [4, 0, 3, 7, 4, 3, -1];
        table[239] = [4, 8, 7, -1];
        table[240] = [9, 10, 8, 10, 11, 8, -1];
        table[241] = [3, 0, 9, 3, 9, 11, 11, 9, 10, -1];
        table[242] = [0, 1, 10, 0, 10, 8, 8, 10, 11, -1];
        table[243] = [3, 1, 10, 11, 3, 10, -1];
        table[244] = [1, 2, 11, 1, 11, 9, 9, 11, 8, -1];
        table[245] = [3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9, -1];
        table[246] = [0, 2, 11, 8, 0, 11, -1];
        table[247] = [3, 2, 11, -1];
        table[248] = [2, 3, 8, 2, 8, 10, 10, 8, 9, -1];
        table[249] = [9, 10, 2, 0, 9, 2, -1];
        table[250] = [2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8, -1];
        table[251] = [1, 10, 2, -1];
        table[252] = [1, 3, 8, 9, 1, 8, -1];
        table[253] = [0, 9, 1, -1];
        table[254] = [0, 3, 8, -1];
        table[255] = [-1];

        return table;
    }
}
