namespace MathVerse.Math.Distributed.GeometryParallelism
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel spatial query engine that performs range queries and nearest-neighbor searches
    /// on point sets using parallel partitioning strategies.
    /// </summary>
    public sealed class ParallelSpatialQuery
    {
        /// <summary>
        /// Performs a parallel range query to find all points within a given radius of a center point.
        /// The point set is partitioned across threads, and each thread checks its subset
        /// independently.
        /// </summary>
        /// <param name="points">Array of points to query against.</param>
        /// <param name="center">Center point of the query sphere.</param>
        /// <param name="radius">Radius of the query sphere.</param>
        /// <returns>
        /// Array of indices of points that fall within the query radius.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when points is empty or radius is negative.
        /// </exception>
        public static int[] RangeQuery(double[][] points, double[] center, double radius)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (center == null) throw new ArgumentNullException(nameof(center));
            if (points.Length == 0) throw new ArgumentException("Points must not be empty.", nameof(points));
            if (radius < 0) throw new ArgumentException("Radius must be non-negative.", nameof(radius));

            double radiusSq = radius * radius;
            int dims = center.Length;

            ConcurrentBag<int> results = new ConcurrentBag<int>();

            // Partition points across threads
            int batchSize = System.Math.Max(1, points.Length / System.Environment.ProcessorCount);

            Parallel.For(0, points.Length, i =>
            {
                double distSq = 0.0;
                for (int d = 0; d < dims && d < points[i].Length; d++)
                {
                    double diff = points[i][d] - center[d];
                    distSq += diff * diff;
                }

                if (distSq <= radiusSq)
                {
                    results.Add(i);
                }
            });

            int[] resultArray = results.ToArray();
            System.Array.Sort(resultArray);
            return resultArray;
        }

        /// <summary>
        /// Performs a parallel k-nearest-neighbor query. Partitions the point set and
        /// finds the k closest points to the query center.
        /// </summary>
        /// <param name="points">Array of points to search.</param>
        /// <param name="center">Query point.</param>
        /// <param name="k">Number of nearest neighbors to return.</param>
        /// <returns>
        /// Array of indices of the k nearest points, sorted by distance (nearest first).
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when points is empty, k is non-positive, or k exceeds point count.
        /// </exception>
        public static int[] KNearestNeighbors(double[][] points, double[] center, int k)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (center == null) throw new ArgumentNullException(nameof(center));
            if (points.Length == 0) throw new ArgumentException("Points must not be empty.", nameof(points));
            if (k <= 0) throw new ArgumentException("k must be positive.", nameof(k));
            if (k > points.Length) throw new ArgumentException("k cannot exceed the number of points.", nameof(k));

            int dims = center.Length;
            int count = points.Length;

            // Compute distances in parallel
            double[] distances = new double[count];

            Parallel.For(0, count, i =>
            {
                double distSq = 0.0;
                for (int d = 0; d < dims && d < points[i].Length; d++)
                {
                    double diff = points[i][d] - center[d];
                    distSq += diff * diff;
                }
                distances[i] = distSq;
            });

            // Build index array and partial sort for top-k
            int[] indices = new int[count];
            for (int i = 0; i < count; i++) indices[i] = i;

            // Use a simple selection approach: find k smallest
            for (int i = 0; i < k; i++)
            {
                int minIdx = i;
                for (int j = i + 1; j < count; j++)
                {
                    if (distances[indices[j]] < distances[indices[minIdx]])
                    {
                        minIdx = j;
                    }
                }

                int temp = indices[i];
                indices[i] = indices[minIdx];
                indices[minIdx] = temp;
            }

            int[] result = new int[k];
            System.Array.Copy(indices, result, k);
            return result;
        }

        /// <summary>
        /// Performs parallel axis-aligned bounding box queries across multiple query volumes.
        /// Each query is processed independently in parallel.
        /// </summary>
        /// <param name="points">Array of points to query.</param>
        /// <param name="queryMin">Lower corner of each query box.</param>
        /// <param name="queryMax">Upper corner of each query box.</param>
        /// <returns>
        /// Array of index arrays, one per query, containing points within each bounding box.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public static int[][] BatchBoxQuery(double[][] points, double[][] queryMin, double[][] queryMax)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (queryMin == null) throw new ArgumentNullException(nameof(queryMin));
            if (queryMax == null) throw new ArgumentNullException(nameof(queryMax));

            int queryCount = queryMin.Length;
            int dims = queryMin[0].Length;
            int pointCount = points.Length;

            int[][] results = new int[queryCount][];

            Parallel.For(0, queryCount, q =>
            {
                List<int> found = new List<int>();

                for (int i = 0; i < pointCount; i++)
                {
                    bool inside = true;
                    for (int d = 0; d < dims; d++)
                    {
                        if (points[i][d] < queryMin[q][d] || points[i][d] > queryMax[q][d])
                        {
                            inside = false;
                            break;
                        }
                    }
                    if (inside) found.Add(i);
                }

                results[q] = found.ToArray();
            });

            return results;
        }
    }
}
