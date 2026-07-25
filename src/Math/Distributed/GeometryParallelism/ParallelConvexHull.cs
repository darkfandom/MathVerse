namespace MathVerse.Math.Distributed.GeometryParallelism
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel convex hull computation using a divide-and-conquer strategy.
    /// Partitions points into subsets, computes local hulls, and merges them.
    /// </summary>
    public sealed class ParallelConvexHull
    {
        /// <summary>
        /// Computes the convex hull of a set of 2D points using parallel divide-and-conquer.
        /// Points are partitioned into subsets, each subset's hull is computed independently,
        /// and the partial hulls are merged into the final result.
        /// </summary>
        /// <param name="points">Array of 2D points, each represented as [x, y].</param>
        /// <returns>
        /// Array of 2D points forming the convex hull in counter-clockwise order.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="points"/> has fewer than 3 points or contains invalid points.
        /// </exception>
        public static double[][] Compute(double[][] points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (points.Length < 3)
                throw new ArgumentException("At least 3 points are required for a convex hull.", nameof(points));

            // Sort points by x-coordinate for deterministic partitioning
            double[][] sorted = new double[points.Length][];
            for (int i = 0; i < points.Length; i++)
            {
                sorted[i] = new double[] { points[i][0], points[i][1] };
            }

            System.Array.Sort(sorted, (a, b) =>
            {
                int cmp = a[0].CompareTo(b[0]);
                return cmp != 0 ? cmp : a[1].CompareTo(b[1]);
            });

            int processorCount = System.Environment.ProcessorCount;
            int chunkSize = System.Math.Max(3, sorted.Length / processorCount);
            int chunkCount = (sorted.Length + chunkSize - 1) / chunkSize;

            if (chunkCount < 2)
            {
                return ComputeSequential(sorted);
            }

            double[][][] localHulls = new double[chunkCount][][];

            // Compute local hulls in parallel
            Parallel.For(0, chunkCount, c =>
            {
                int start = c * chunkSize;
                int end = System.Math.Min(start + chunkSize, sorted.Length);
                int count = end - start;

                double[][] chunk = new double[count][];
                for (int i = 0; i < count; i++)
                {
                    chunk[i] = sorted[start + i];
                }

                localHulls[c] = ComputeSequential(chunk);
            });

            // Merge hulls sequentially (hull merging is inherently sequential for correctness)
            double[][] merged = localHulls[0];
            for (int c = 1; c < chunkCount; c++)
            {
                merged = MergeHulls(merged, localHulls[c]);
            }

            return merged;
        }

        /// <summary>
        /// Computes convex hull sequentially using Andrew's monotone chain algorithm.
        /// </summary>
        private static double[][] ComputeSequential(double[][] points)
        {
            if (points.Length < 3)
            {
                double[][] result = new double[points.Length][];
                for (int i = 0; i < points.Length; i++)
                {
                    result[i] = new double[] { points[i][0], points[i][1] };
                }
                return result;
            }

            int n = points.Length;
            List<double[]> hull = new List<double[]>(n + 1);

            // Lower hull
            for (int i = 0; i < n; i++)
            {
                while (hull.Count >= 2 && Cross(hull[hull.Count - 2], hull[hull.Count - 1], points[i]) <= 0)
                {
                    hull.RemoveAt(hull.Count - 1);
                }
                hull.Add(points[i]);
            }

            // Upper hull
            int lowerCount = hull.Count + 1;
            for (int i = n - 2; i >= 0; i--)
            {
                while (hull.Count >= lowerCount && Cross(hull[hull.Count - 2], hull[hull.Count - 1], points[i]) <= 0)
                {
                    hull.RemoveAt(hull.Count - 1);
                }
                hull.Add(points[i]);
            }

            // Remove last point (duplicate of first)
            if (hull.Count > 1) hull.RemoveAt(hull.Count - 1);

            double[][] output = new double[hull.Count][];
            for (int i = 0; i < hull.Count; i++)
            {
                output[i] = new double[] { hull[i][0], hull[i][1] };
            }
            return output;
        }

        /// <summary>
        /// Merges two convex hulls into one using a standard hull merge algorithm.
        /// </summary>
        private static double[][] MergeHulls(double[][] hullA, double[][] hullB)
        {
            double[][] combined = new double[hullA.Length + hullB.Length][];
            for (int i = 0; i < hullA.Length; i++)
            {
                combined[i] = hullA[i];
            }
            for (int i = 0; i < hullB.Length; i++)
            {
                combined[hullA.Length + i] = hullB[i];
            }

            return ComputeSequential(combined);
        }

        /// <summary>
        /// Computes the cross product of vectors OA and OB where O is the origin relative to points.
        /// </summary>
        private static double Cross(double[] o, double[] a, double[] b)
        {
            return (a[0] - o[0]) * (b[1] - o[1]) - (a[1] - o[1]) * (b[0] - o[0]);
        }
    }
}
