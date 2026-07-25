namespace MathVerse.Math.Distributed.GeometryParallelism
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel Delaunay triangulation using a divide-and-conquer approach.
    /// Partitions points spatially, computes local triangulations in parallel,
    /// and merges them into the final triangulation.
    /// </summary>
    public sealed class ParallelDelaunay
    {
        /// <summary>
        /// Computes the Delaunay triangulation of a set of 2D points using parallel
        /// divide-and-conquer. Points are partitioned by x-coordinate into subsets,
        /// triangulated independently, and merged.
        /// </summary>
        /// <param name="points">Array of 2D points, each represented as [x, y].</param>
        /// <returns>
        /// Array of triangles, where each triangle is [i0, i1, i2] indexing into the input points.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when fewer than 3 points are provided.
        /// </exception>
        public static int[][] Compute(double[][] points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (points.Length < 3)
                throw new ArgumentException("At least 3 points are required for Delaunay triangulation.", nameof(points));

            int n = points.Length;

            // Create index array sorted by x-coordinate
            int[] indices = new int[n];
            for (int i = 0; i < n; i++) indices[i] = i;

            System.Array.Sort(indices, (a, b) =>
            {
                int cmp = points[a][0].CompareTo(points[b][0]);
                return cmp != 0 ? cmp : points[a][1].CompareTo(points[b][1]);
            });

            int processorCount = System.Environment.ProcessorCount;
            int chunkSize = System.Math.Max(3, n / processorCount);
            int chunkCount = (n + chunkSize - 1) / chunkSize;

            if (chunkCount < 2)
            {
                return TriangulateChunk(points, indices, 0, n);
            }

            // Compute local triangulations in parallel
            int[][][] localTriangles = new int[chunkCount][][];

            Parallel.For(0, chunkCount, c =>
            {
                int start = c * chunkSize;
                int end = System.Math.Min(start + chunkSize, n);
                localTriangles[c] = TriangulateChunk(points, indices, start, end);
            });

            // Merge all local triangulations
            List<int[]> mergedTriangles = new List<int[]>();
            for (int c = 0; c < chunkCount; c++)
            {
                for (int t = 0; t < localTriangles[c].Length; t++)
                {
                    mergedTriangles.Add(localTriangles[c][t]);
                }
            }

            // Remove duplicate triangles
            HashSet<string> uniqueTriangles = new HashSet<string>();
            List<int[]> result = new List<int[]>();

            for (int i = 0; i < mergedTriangles.Count; i++)
            {
                int[] tri = mergedTriangles[i];
                string key = TriangleKey(tri);
                if (uniqueTriangles.Add(key))
                {
                    result.Add(tri);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Triangulates a chunk of points using the Bowyer-Watson algorithm.
        /// </summary>
        private static int[][] TriangulateChunk(double[][] points, int[] sortedIndices, int start, int end)
        {
            int count = end - start;
            if (count < 3) return Array.Empty<int[]>();

            // Extract local points
            double[][] localPoints = new double[count][];
            int[] localIndexMap = new int[count];
            for (int i = 0; i < count; i++)
            {
                int globalIdx = sortedIndices[start + i];
                localPoints[i] = new double[] { points[globalIdx][0], points[globalIdx][1] };
                localIndexMap[i] = globalIdx;
            }

            // Bowyer-Watson insertion
            List<int[]> triangles = new List<int[]>();

            // Find bounding triangle
            double minX = double.MaxValue, maxX = double.MinValue;
            double minY = double.MaxValue, maxY = double.MinValue;
            for (int i = 0; i < count; i++)
            {
                if (localPoints[i][0] < minX) minX = localPoints[i][0];
                if (localPoints[i][0] > maxX) maxX = localPoints[i][0];
                if (localPoints[i][1] < minY) minY = localPoints[i][1];
                if (localPoints[i][1] > maxY) maxY = localPoints[i][1];
            }

            double dx = maxX - minX;
            double dy = maxY - minY;
            double margin = System.Math.Max(dx, dy) * 10.0;

            // Super-triangle vertices (virtual indices -1, -2, -3 mapped to local points array)
            List<double[]> augmentedPoints = new List<double[]>(localPoints);
            augmentedPoints.Add(new double[] { minX - margin, minY - margin });
            augmentedPoints.Add(new double[] { minX + dx * 0.5, maxY + margin });
            augmentedPoints.Add(new double[] { maxX + margin, minY - margin });

            int super0 = count;
            int super1 = count + 1;
            int super2 = count + 2;

            triangles.Add(new int[] { super0, super1, super2 });

            for (int p = 0; p < count; p++)
            {
                double[] point = localPoints[p];
                List<int[]> badTriangles = new List<int[]>();

                for (int t = triangles.Count - 1; t >= 0; t--)
                {
                    if (InCircumcircle(point, augmentedPoints[triangles[t][0]],
                        augmentedPoints[triangles[t][1]], augmentedPoints[triangles[t][2]]))
                    {
                        badTriangles.Add(triangles[t]);
                        triangles.RemoveAt(t);
                    }
                }

                // Find boundary of the polygonal hole
                List<(int, int)> polygon = new List<(int, int)>();
                foreach (var tri in badTriangles)
                {
                    for (int e = 0; e < 3; e++)
                    {
                        int v0 = tri[e];
                        int v1 = tri[(e + 1) % 3];
                        bool shared = false;

                        foreach (var other in badTriangles)
                        {
                            if (other == tri) continue;
                            for (int oe = 0; oe < 3; oe++)
                            {
                                if ((other[oe] == v1 && other[(oe + 1) % 3] == v0))
                                {
                                    shared = true;
                                    break;
                                }
                            }
                            if (shared) break;
                        }

                        if (!shared)
                        {
                            polygon.Add((v0, v1));
                        }
                    }
                }

                // Re-triangulate the hole
                foreach (var edge in polygon)
                {
                    triangles.Add(new int[] { edge.Item1, edge.Item2, p });
                }
            }

            // Remove triangles referencing super-triangle vertices
            List<int[]> filteredTriangles = new List<int[]>();
            foreach (var tri in triangles)
            {
                if (tri[0] < count && tri[1] < count && tri[2] < count)
                {
                    filteredTriangles.Add(new int[]
                    {
                        localIndexMap[tri[0]],
                        localIndexMap[tri[1]],
                        localIndexMap[tri[2]]
                    });
                }
            }

            return filteredTriangles.ToArray();
        }

        /// <summary>
        /// Tests whether a point lies inside the circumcircle of a triangle.
        /// </summary>
        private static bool InCircumcircle(double[] p, double[] a, double[] b, double[] c)
        {
            double ax = a[0] - p[0];
            double ay = a[1] - p[1];
            double bx = b[0] - p[0];
            double by = b[1] - p[1];
            double cx = c[0] - p[0];
            double cy = c[1] - p[1];

            double det = (ax * ax + ay * ay) * (bx * cy - cx * by)
                        - (bx * bx + by * by) * (ax * cy - cx * ay)
                        + (cx * cx + cy * cy) * (ax * by - bx * ay);

            return det > 1e-10;
        }

        /// <summary>
        /// Generates a unique key for a triangle (sorted vertex indices).
        /// </summary>
        private static string TriangleKey(int[] tri)
        {
            int a = tri[0], b = tri[1], c = tri[2];
            if (a > b) { int t = a; a = b; b = t; }
            if (a > c) { int t = a; a = c; c = t; }
            if (b > c) { int t = b; b = c; c = t; }
            return $"{a},{b},{c}";
        }
    }
}
