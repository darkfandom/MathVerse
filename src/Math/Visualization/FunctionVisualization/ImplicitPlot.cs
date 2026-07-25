namespace MathVerse.Math.Visualization.FunctionVisualization;
using System.Numerics;

/// <summary>Generates implicit equation plots f(x, y) = 0 using the marching squares algorithm.</summary>
public sealed class ImplicitPlot
{
    private static readonly int[][] EdgeTable = BuildEdgeTable();
    private static readonly int[][] LineTable = BuildLineTable();

    /// <summary>Creates an implicit curve plot for the equation f(x, y) = 0.</summary>
    /// <param name="implicitFunc">The implicit function f(x, y) where the zero level set defines the curve.</param>
    /// <param name="xMin">The minimum X value of the plotting range.</param>
    /// <param name="xMax">The maximum X value of the plotting range.</param>
    /// <param name="yMin">The minimum Y value of the plotting range.</param>
    /// <param name="yMax">The maximum Y value of the plotting range.</param>
    /// <param name="resolution">The number of subdivisions along each axis.</param>
    /// <returns>A list of line segments, where every two consecutive points form a segment.</returns>
    public static List<Vector2> Create(
        Func<double, double, double> implicitFunc,
        double xMin, double xMax,
        double yMin, double yMax,
        int resolution = 100)
    {
        ArgumentNullException.ThrowIfNull(implicitFunc);
        if (resolution < 2) throw new ArgumentOutOfRangeException(nameof(resolution), "Resolution must be at least 2.");

        double[,] field = new double[resolution + 1, resolution + 1];
        double xStep = (xMax - xMin) / resolution;
        double yStep = (yMax - yMin) / resolution;

        for (int j = 0; j <= resolution; j++)
        {
            for (int i = 0; i <= resolution; i++)
            {
                double x = xMin + i * xStep;
                double y = yMin + j * yStep;
                double val = implicitFunc(x, y);
                field[j, i] = double.IsNaN(val) || double.IsInfinity(val) ? 0.0 : val;
            }
        }

        List<Vector2> segments = [];

        for (int j = 0; j < resolution; j++)
        {
            for (int i = 0; i < resolution; i++)
            {
                double x0 = xMin + i * xStep;
                double x1 = xMin + (i + 1) * xStep;
                double y0 = yMin + j * yStep;
                double y1 = yMin + (j + 1) * yStep;

                double[] cornerValues =
                [
                    field[j, i],
                    field[j, i + 1],
                    field[j + 1, i + 1],
                    field[j + 1, i]
                ];

                int cubeIndex = 0;
                for (int c = 0; c < 4; c++)
                {
                    if (cornerValues[c] > 0)
                        cubeIndex |= 1 << c;
                }

                if (cubeIndex == 0 || cubeIndex == 15) continue;

                Vector2[] cornerPositions =
                [
                    new Vector2((float)x0, (float)y0),
                    new Vector2((float)x1, (float)y0),
                    new Vector2((float)x1, (float)y1),
                    new Vector2((float)x0, (float)y1)
                ];

                Vector2[] edgePoints = new Vector2[4];
                int[][] edgePairs = [[0, 1], [1, 2], [2, 3], [3, 0]];

                for (int e = 0; e < 4; e++)
                {
                    int v0 = edgePairs[e][0];
                    int v1 = edgePairs[e][1];
                    double denom = cornerValues[v1] - cornerValues[v0];
                    float t = (denom != 0.0)
                        ? (float)(-cornerValues[v0] / denom)
                        : 0.5f;
                    t = System.Math.Clamp(t, 0f, 1f);
                    edgePoints[e] = Vector2.Lerp(cornerPositions[v0], cornerPositions[v1], t);
                }

                int[] lines = LineTable[cubeIndex];
                for (int l = 0; l < lines.Length; l += 2)
                {
                    segments.Add(edgePoints[lines[l]]);
                    segments.Add(edgePoints[lines[l + 1]]);
                }
            }
        }

        return segments;
    }

    private static int[][] BuildEdgeTable()
    {
        int[][] table = new int[16][];
        table[0] = [];
        table[1] = [0, 3];
        table[2] = [0, 1];
        table[3] = [1, 3];
        table[4] = [1, 2];
        table[5] = [0, 1, 2, 3];
        table[6] = [0, 2];
        table[7] = [2, 3];
        table[8] = [2, 3];
        table[9] = [0, 2];
        table[10] = [0, 3, 1, 2];
        table[11] = [1, 2];
        table[12] = [1, 3];
        table[13] = [0, 1];
        table[14] = [0, 3];
        table[15] = [];
        return table;
    }

    private static int[][] BuildLineTable()
    {
        int[][] table = new int[16][];
        table[0] = [];
        table[1] = [0, 3];
        table[2] = [0, 1];
        table[3] = [1, 3];
        table[4] = [1, 2];
        table[5] = [0, 3, 1, 2];
        table[6] = [0, 2];
        table[7] = [2, 3];
        table[8] = [2, 3];
        table[9] = [0, 2];
        table[10] = [0, 1, 2, 3];
        table[11] = [1, 2];
        table[12] = [1, 3];
        table[13] = [0, 1];
        table[14] = [0, 3];
        table[15] = [];
        return table;
    }
}
