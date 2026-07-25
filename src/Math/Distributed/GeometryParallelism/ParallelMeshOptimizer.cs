namespace MathVerse.Math.Distributed.GeometryParallelism
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel mesh optimizer that improves mesh quality by updating vertex positions
    /// concurrently using Laplacian smoothing and related techniques.
    /// </summary>
    public sealed class ParallelMeshOptimizer
    {
        /// <summary>
        /// Optimizes mesh quality by performing parallel Laplacian smoothing on vertex positions.
        /// Each vertex is moved toward the centroid of its neighbors, with the update
        /// parallelized across all vertices.
        /// </summary>
        /// <param name="vertices">
        /// Array of vertex positions. Each element is a coordinate vector [x, y, z].
        /// Modified in place.
        /// </param>
        /// <param name="faces">
        /// Array of face definitions. Each element is an array of vertex indices forming a face.
        /// </param>
        /// <param name="iterations">Number of smoothing iterations to perform (default: 10).</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="vertices"/> or <paramref name="faces"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when vertices or faces are empty, or iterations is negative.
        /// </exception>
        public static void Optimize(double[][] vertices, int[][] faces, int iterations = 10)
        {
            if (vertices == null) throw new ArgumentNullException(nameof(vertices));
            if (faces == null) throw new ArgumentNullException(nameof(faces));
            if (vertices.Length == 0) throw new ArgumentException("Vertices must not be empty.", nameof(vertices));
            if (faces.Length == 0) throw new ArgumentException("Faces must not be empty.", nameof(faces));
            if (iterations < 0) throw new ArgumentException("Iterations must be non-negative.", nameof(iterations));

            int vertexCount = vertices.Length;
            int dim = vertices[0].Length;

            // Build adjacency: for each vertex, store neighbor vertex indices
            int[][] neighbors = BuildAdjacency(vertexCount, faces);

            double[][] buffer = new double[vertexCount][];
            for (int i = 0; i < vertexCount; i++)
            {
                buffer[i] = new double[dim];
                System.Array.Copy(vertices[i], buffer[i], dim);
            }

            double relaxationFactor = 0.5;

            for (int iter = 0; iter < iterations; iter++)
            {
                double[][] newPositions = new double[vertexCount][];
                for (int i = 0; i < vertexCount; i++)
                {
                    newPositions[i] = new double[dim];
                }

                Parallel.For(0, vertexCount, v =>
                {
                    int[] nbrs = neighbors[v];
                    if (nbrs.Length == 0)
                    {
                        System.Array.Copy(buffer[v], newPositions[v], dim);
                        return;
                    }

                    // Compute centroid of neighbors
                    double[] centroid = new double[dim];
                    for (int n = 0; n < nbrs.Length; n++)
                    {
                        for (int d = 0; d < dim; d++)
                        {
                            centroid[d] += buffer[nbrs[n]][d];
                        }
                    }

                    double invCount = 1.0 / nbrs.Length;
                    for (int d = 0; d < dim; d++)
                    {
                        centroid[d] *= invCount;
                    }

                    // Relax toward centroid
                    for (int d = 0; d < dim; d++)
                    {
                        newPositions[v][d] = buffer[v][d] + relaxationFactor * (centroid[d] - buffer[v][d]);
                    }
                });

                double[][] temp = buffer;
                buffer = newPositions;
                newPositions = temp;
            }

            // Copy results back
            for (int i = 0; i < vertexCount; i++)
            {
                System.Array.Copy(buffer[i], vertices[i], dim);
            }
        }

        /// <summary>
        /// Builds vertex adjacency list from face definitions.
        /// </summary>
        private static int[][] BuildAdjacency(int vertexCount, int[][] faces)
        {
            bool[][] adjMatrix = new bool[vertexCount][];
            for (int i = 0; i < vertexCount; i++)
            {
                adjMatrix[i] = new bool[vertexCount];
            }

            for (int f = 0; f < faces.Length; f++)
            {
                int faceVerts = faces[f].Length;
                for (int i = 0; i < faceVerts; i++)
                {
                    int v0 = faces[f][i];
                    int v1 = faces[f][(i + 1) % faceVerts];

                    if (v0 >= 0 && v0 < vertexCount && v1 >= 0 && v1 < vertexCount)
                    {
                        adjMatrix[v0][v1] = true;
                        adjMatrix[v1][v0] = true;
                    }
                }
            }

            int[][] result = new int[vertexCount][];
            for (int i = 0; i < vertexCount; i++)
            {
                List<int> nbrs = new List<int>();
                for (int j = 0; j < vertexCount; j++)
                {
                    if (adjMatrix[i][j]) nbrs.Add(j);
                }
                result[i] = nbrs.ToArray();
            }

            return result;
        }
    }
}
