namespace MathVerse.Math.Distributed.VisualizationPipeline
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel geometry generator that creates surface meshes by partitioning the
    /// evaluation grid across multiple threads.
    /// </summary>
    public sealed class ParallelGeometryGen
    {
        /// <summary>
        /// Generates a 3D surface in parallel by partitioning the grid into row bands
        /// and evaluating the surface function for each row concurrently.
        /// </summary>
        /// <param name="zFunc">
        /// Surface height function.
        /// Signature: (double x, double y) -> double z.
        /// </param>
        /// <param name="xMin">Minimum x coordinate of the domain.</param>
        /// <param name="xMax">Maximum x coordinate of the domain.</param>
        /// <param name="yMin">Minimum y coordinate of the domain.</param>
        /// <param name="yMax">Maximum y coordinate of the domain.</param>
        /// <param name="resolution">
        /// Number of grid subdivisions in each dimension.
        /// Produces a (resolution+1) x (resolution+1) vertex grid.
        /// </param>
        /// <returns>
        /// A tuple of (vertices, indices):
        /// <list type="bullet">
        ///   <item>vertices: array of [x, y, z] positions</item>
        ///   <item>indices: array of triangles, each [i0, i1, i2]</item>
        /// </list>
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="zFunc"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when resolution is non-positive or domain is degenerate.
        /// </exception>
        public static (double[][] vertices, int[][] indices) GenerateSurfaceParallel(
            Func<double, double, double> zFunc,
            double xMin,
            double xMax,
            double yMin,
            double yMax,
            int resolution)
        {
            if (zFunc == null) throw new ArgumentNullException(nameof(zFunc));
            if (resolution <= 0) throw new ArgumentException("Resolution must be positive.", nameof(resolution));
            if (xMin >= xMax) throw new ArgumentException("xMin must be less than xMax.", nameof(xMin));
            if (yMin >= yMax) throw new ArgumentException("yMin must be less than yMax.", nameof(yMin));

            int gridSize = resolution + 1;
            double[][] vertices = new double[gridSize * gridSize][];

            double xStep = (xMax - xMin) / resolution;
            double yStep = (yMax - yMin) / resolution;

            // Evaluate surface function in parallel, partitioned by rows
            Parallel.For(0, gridSize, row =>
            {
                double y = yMin + row * yStep;
                for (int col = 0; col < gridSize; col++)
                {
                    double x = xMin + col * xStep;
                    double z = zFunc(x, y);
                    vertices[row * gridSize + col] = new double[] { x, y, z };
                }
            });

            // Generate triangle indices
            int triCount = resolution * resolution * 2;
            int[][] indices = new int[triCount][];

            Parallel.For(0, resolution, row =>
            {
                for (int col = 0; col < resolution; col++)
                {
                    int i0 = row * gridSize + col;
                    int i1 = i0 + 1;
                    int i2 = i0 + gridSize;
                    int i3 = i2 + 1;

                    indices[(row * resolution + col) * 2] = new int[] { i0, i1, i3 };
                    indices[(row * resolution + col) * 2 + 1] = new int[] { i0, i3, i2 };
                }
            });

            return (vertices, indices);
        }

        /// <summary>
        /// Generates surface normals in parallel for an existing vertex grid.
        /// Each vertex normal is computed as the average of adjacent face normals.
        /// </summary>
        /// <param name="vertices">Array of vertex positions [x, y, z].</param>
        /// <param name="indices">Array of triangle indices [i0, i1, i2].</param>
        /// <returns>Array of unit normal vectors, one per vertex.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public static double[][] GenerateNormals(double[][] vertices, int[][] indices)
        {
            if (vertices == null) throw new ArgumentNullException(nameof(vertices));
            if (indices == null) throw new ArgumentNullException(nameof(indices));

            int vertexCount = vertices.Length;
            double[][] normals = new double[vertexCount][];

            for (int i = 0; i < vertexCount; i++)
            {
                normals[i] = new double[3];
            }

            // Accumulate face normals in parallel
            double[][] faceNormals = new double[indices.Length][];

            Parallel.For(0, indices.Length, f =>
            {
                int i0 = indices[f][0];
                int i1 = indices[f][1];
                int i2 = indices[f][2];

                double[] e1 = Sub(vertices[i1], vertices[i0]);
                double[] e2 = Sub(vertices[i2], vertices[i0]);
                faceNormals[f] = Cross(e1, e2);
            });

            // Accumulate to vertex normals
            for (int f = 0; f < indices.Length; f++)
            {
                for (int v = 0; v < 3; v++)
                {
                    int idx = indices[f][v];
                    normals[idx][0] += faceNormals[f][0];
                    normals[idx][1] += faceNormals[f][1];
                    normals[idx][2] += faceNormals[f][2];
                }
            }

            // Normalize
            Parallel.For(0, vertexCount, i =>
            {
                double len = System.Math.Sqrt(
                    normals[i][0] * normals[i][0] +
                    normals[i][1] * normals[i][1] +
                    normals[i][2] * normals[i][2]);

                if (len > 1e-10)
                {
                    double inv = 1.0 / len;
                    normals[i][0] *= inv;
                    normals[i][1] *= inv;
                    normals[i][2] *= inv;
                }
            });

            return normals;
        }

        private static double[] Sub(double[] a, double[] b)
        {
            return new double[] { a[0] - b[0], a[1] - b[1], a[2] - b[2] };
        }

        private static double[] Cross(double[] a, double[] b)
        {
            return new double[]
            {
                a[1] * b[2] - a[2] * b[1],
                a[2] * b[0] - a[0] * b[2],
                a[0] * b[1] - a[1] * b[0]
            };
        }
    }
}
