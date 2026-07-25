namespace MathVerse.Math.Distributed.VisualizationPipeline
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel mesh generator that creates triangular meshes from scalar functions
    /// by partitioning the evaluation domain across multiple threads.
    /// </summary>
    public sealed class ParallelMeshGen
    {
        /// <summary>
        /// Generates a triangular mesh from a scalar function by evaluating the function
        /// on a grid partitioned into row bands, each processed in parallel.
        /// </summary>
        /// <param name="func">
        /// Scalar function to evaluate.
        /// Signature: (double x, double y) -> double z.
        /// </param>
        /// <param name="xMin">Minimum x coordinate of the domain.</param>
        /// <param name="xMax">Maximum x coordinate of the domain.</param>
        /// <param name="yMin">Minimum y coordinate of the domain.</param>
        /// <param name="yMax">Maximum y coordinate of the domain.</param>
        /// <param name="resolution">
        /// Number of grid subdivisions per axis.
        /// Produces (resolution+1)^2 vertices and 2*resolution^2 triangles.
        /// </param>
        /// <returns>
        /// A tuple of (vertices, triangles):
        /// <list type="bullet">
        ///   <item>vertices: array of [x, y, z] positions</item>
        ///   <item>triangles: array of triangle index triplets [i0, i1, i2]</item>
        /// </list>
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="func"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when resolution is non-positive or domain is invalid.
        /// </exception>
        public static (double[][] vertices, int[][] triangles) GenerateMeshParallel(
            Func<double, double, double> func,
            double xMin,
            double xMax,
            double yMin,
            double yMax,
            int resolution)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));
            if (resolution <= 0) throw new ArgumentException("Resolution must be positive.", nameof(resolution));
            if (xMin >= xMax) throw new ArgumentException("xMin must be less than xMax.", nameof(xMin));
            if (yMin >= yMax) throw new ArgumentException("yMin must be less than yMax.", nameof(yMin));

            int gridSize = resolution + 1;
            double xStep = (xMax - xMin) / resolution;
            double yStep = (yMax - yMin) / resolution;

            // Generate vertices in parallel
            double[][] vertices = new double[gridSize * gridSize][];

            Parallel.For(0, gridSize, row =>
            {
                for (int col = 0; col < gridSize; col++)
                {
                    double x = xMin + col * xStep;
                    double y = yMin + row * yStep;
                    double z = func(x, y);
                    vertices[row * gridSize + col] = new double[] { x, y, z };
                }
            });

            // Generate triangle connectivity in parallel
            int triCount = resolution * resolution * 2;
            int[][] triangles = new int[triCount][];

            Parallel.For(0, resolution, row =>
            {
                for (int col = 0; col < resolution; col++)
                {
                    int baseIdx = row * gridSize + col;
                    int i0 = baseIdx;
                    int i1 = baseIdx + 1;
                    int i2 = baseIdx + gridSize;
                    int i3 = baseIdx + gridSize + 1;

                    // Two triangles per grid cell with consistent winding
                    int triIdx = (row * resolution + col) * 2;
                    triangles[triIdx] = new int[] { i0, i2, i1 };
                    triangles[triIdx + 1] = new int[] { i1, i2, i3 };
                }
            });

            return (vertices, triangles);
        }

        /// <summary>
        /// Generates an adaptive mesh by refining cells where the function has high curvature.
        /// Uses a simple error estimator to decide subdivision.
        /// </summary>
        /// <param name="func">Scalar function to mesh.</param>
        /// <param name="xMin">Minimum x coordinate.</param>
        /// <param name="xMax">Maximum x coordinate.</param>
        /// <param name="yMin">Minimum y coordinate.</param>
        /// <param name="yMax">Maximum y coordinate.</param>
        /// <param name="baseResolution">Base grid resolution for initial mesh.</param>
        /// <param name="errorThreshold">Maximum allowed interpolation error before subdivision.</param>
        /// <param name="maxDepth">Maximum refinement depth.</param>
        /// <returns>
        /// A tuple of (vertices, triangles) with adaptive refinement applied.
        /// </returns>
        public static (double[][] vertices, int[][] triangles) GenerateAdaptiveMeshParallel(
            Func<double, double, double> func,
            double xMin,
            double xMax,
            double yMin,
            double yMax,
            int baseResolution,
            double errorThreshold = 0.01,
            int maxDepth = 4)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));
            if (baseResolution <= 0) throw new ArgumentException("Base resolution must be positive.", nameof(baseResolution));

            // Start with base mesh
            var (vertices, triangles) = GenerateMeshParallel(func, xMin, xMax, yMin, yMax, baseResolution);

            // Adaptive refinement pass
            for (int depth = 0; depth < maxDepth; depth++)
            {
                bool[] shouldSplit = new bool[triangles.Length];

                Parallel.For(0, triangles.Length, t =>
                {
                    int i0 = triangles[t][0];
                    int i1 = triangles[t][1];
                    int i2 = triangles[t][2];

                    double[] mid01 = Midpoint(vertices[i0], vertices[i1]);
                    double[] mid12 = Midpoint(vertices[i1], vertices[i2]);
                    double[] mid20 = Midpoint(vertices[i2], vertices[i0]);

                    double actualCenter = func(
                        (vertices[i0][0] + vertices[i1][0] + vertices[i2][0]) / 3.0,
                        (vertices[i0][1] + vertices[i1][1] + vertices[i2][1]) / 3.0);
                    double interpCenter = (vertices[i0][2] + vertices[i1][2] + vertices[i2][2]) / 3.0;

                    double error = System.Math.Abs(actualCenter - interpCenter);
                    shouldSplit[t] = error > errorThreshold;
                });

                bool anySplit = false;
                for (int i = 0; i < shouldSplit.Length; i++)
                {
                    if (shouldSplit[i]) { anySplit = true; break; }
                }

                if (!anySplit) break;

                // For simplicity, regenerate with higher resolution if any split needed
                int newRes = System.Math.Min(baseResolution * 2, baseResolution * (1 << (depth + 1)));
                newRes = System.Math.Min(newRes, 512);
                (vertices, triangles) = GenerateMeshParallel(func, xMin, xMax, yMin, yMax, newRes);
            }

            return (vertices, triangles);
        }

        private static double[] Midpoint(double[] a, double[] b)
        {
            return new double[]
            {
                (a[0] + b[0]) * 0.5,
                (a[1] + b[1]) * 0.5,
                (a[2] + b[2]) * 0.5
            };
        }
    }
}
