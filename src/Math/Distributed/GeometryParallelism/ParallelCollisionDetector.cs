namespace MathVerse.Math.Distributed.GeometryParallelism
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel broad-phase collision detector using spatial hashing to efficiently
    /// identify overlapping pairs among a set of spheres.
    /// </summary>
    public sealed class ParallelCollisionDetector
    {
        /// <summary>
        /// Detects all colliding pairs among a set of spheres using parallel spatial hashing.
        /// Pairs are found by hashing spheres into grid cells and checking within cells
        /// and neighboring cells in parallel.
        /// </summary>
        /// <param name="positions">Array of sphere center positions [x, y, z].</param>
        /// <param name="radii">Array of sphere radii, one per sphere.</param>
        /// <returns>
        /// Array of collision pairs, where each pair is [indexA, indexB] with indexA &lt; indexB.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when either argument is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when arrays have different lengths or are empty.
        /// </exception>
        public static int[][] DetectCollisions(double[][] positions, double[] radii)
        {
            if (positions == null) throw new ArgumentNullException(nameof(positions));
            if (radii == null) throw new ArgumentNullException(nameof(radii));
            if (positions.Length == 0) throw new ArgumentException("Positions must not be empty.", nameof(positions));
            if (positions.Length != radii.Length)
                throw new ArgumentException("Positions and radii must have the same length.");

            int count = positions.Length;

            // Find maximum radius for cell size
            double maxRadius = 0.0;
            for (int i = 0; i < count; i++)
            {
                if (radii[i] > maxRadius) maxRadius = radii[i];
            }

            double cellSize = maxRadius * 2.0;
            if (cellSize < 1e-10) cellSize = 1.0;

            // Build spatial hash
            Dictionary<(int, int, int), List<int>> grid = new Dictionary<(int, int, int), List<int>>();

            for (int i = 0; i < count; i++)
            {
                var cell = GetCell(positions[i], cellSize);
                if (!grid.TryGetValue(cell, out List<int>? list))
                {
                    list = new List<int>();
                    grid[cell] = list;
                }
                list.Add(i);
            }

            // Collect all potential pairs from grid (cell + neighbors)
            ConcurrentBag<(int, int)> collisionPairs = new ConcurrentBag<(int, int)>();

            List<(int, int, int)> cellKeys = new List<(int, int, int)>(grid.Keys);

            Parallel.ForEach(cellKeys, cell =>
            {
                if (!grid.TryGetValue(cell, out List<int>? cellObjects)) return;

                // Check all 27 neighboring cells
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dz = -1; dz <= 1; dz++)
                        {
                            var neighbor = (cell.Item1 + dx, cell.Item2 + dy, cell.Item3 + dz);

                            if (!grid.TryGetValue(neighbor, out List<int>? neighborObjects)) continue;

                            for (int i = 0; i < cellObjects.Count; i++)
                            {
                                int a = cellObjects[i];

                                // Only check objects in neighbor cells or same cell with higher index
                                for (int j = 0; j < neighborObjects.Count; j++)
                                {
                                    int b = neighborObjects[j];

                                    if (dx == 0 && dy == 0 && dz == 0 && b <= a) continue;

                                    double distSq = DistanceSquared(positions[a], positions[b]);
                                    double combinedRadius = radii[a] + radii[b];

                                    if (distSq <= combinedRadius * combinedRadius)
                                    {
                                        int minIdx = System.Math.Min(a, b);
                                        int maxIdx = System.Math.Max(a, b);
                                        collisionPairs.Add((minIdx, maxIdx));
                                    }
                                }
                            }
                        }
                    }
                }
            });

            // Deduplicate results
            HashSet<(int, int)> unique = new HashSet<(int, int)>(collisionPairs);
            int[][] result = new int[unique.Count][];
            int idx = 0;
            foreach (var pair in unique)
            {
                result[idx++] = new int[] { pair.Item1, pair.Item2 };
            }

            return result;
        }

        /// <summary>
        /// Computes the spatial hash cell for a position.
        /// </summary>
        private static (int, int, int) GetCell(double[] position, double cellSize)
        {
            int cx = (int)System.Math.Floor(position[0] / cellSize);
            int cy = (int)System.Math.Floor(position[1] / cellSize);
            int cz = (int)System.Math.Floor(position[2] / cellSize);
            return (cx, cy, cz);
        }

        /// <summary>
        /// Computes the squared distance between two 3D points.
        /// </summary>
        private static double DistanceSquared(double[] a, double[] b)
        {
            double dx = a[0] - b[0];
            double dy = a[1] - b[1];
            double dz = a[2] - b[2];
            return dx * dx + dy * dy + dz * dz;
        }
    }
}
