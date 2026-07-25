namespace MathVerse.Math.Distributed.GeometryParallelism
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel Voronoi diagram computation that assigns grid cells to their nearest
    /// seed point concurrently using parallel nearest-site queries.
    /// </summary>
    public sealed class ParallelVoronoi
    {
        /// <summary>
        /// Computes a Voronoi diagram on a 2D grid by assigning each grid cell to
        /// its nearest seed point. The grid is partitioned into row bands and
        /// processed in parallel.
        /// </summary>
        /// <param name="seeds">Array of 2D seed points [x, y].</param>
        /// <param name="gridWidth">Width of the output grid (number of cells in x).</param>
        /// <param name="gridHeight">Height of the output grid (number of cells in y).</param>
        /// <param name="xMin">Minimum x coordinate of the grid domain.</param>
        /// <param name="xMax">Maximum x coordinate of the grid domain.</param>
        /// <param name="yMin">Minimum y coordinate of the grid domain.</param>
        /// <param name="yMax">Maximum y coordinate of the grid domain.</param>
        /// <returns>
        /// 2D array of shape [gridHeight, gridWidth] where each cell contains
        /// the index of the nearest seed point.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="seeds"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when seeds is empty or grid dimensions are non-positive.
        /// </exception>
        public static int[,] Compute(
            double[][] seeds,
            int gridWidth,
            int gridHeight,
            double xMin,
            double xMax,
            double yMin,
            double yMax)
        {
            if (seeds == null) throw new ArgumentNullException(nameof(seeds));
            if (seeds.Length == 0) throw new ArgumentException("Seeds must not be empty.", nameof(seeds));
            if (gridWidth <= 0) throw new ArgumentException("Grid width must be positive.", nameof(gridWidth));
            if (gridHeight <= 0) throw new ArgumentException("Grid height must be positive.", nameof(gridHeight));

            int seedCount = seeds.Length;
            int[,] assignments = new int[gridHeight, gridWidth];

            double xRange = xMax - xMin;
            double yRange = yMax - yMin;

            Parallel.For(0, gridHeight, row =>
            {
                double py = yMin + (row + 0.5) * yRange / gridHeight;

                for (int col = 0; col < gridWidth; col++)
                {
                    double px = xMin + (col + 0.5) * xRange / gridWidth;

                    double minDistSq = double.MaxValue;
                    int nearestSeed = 0;

                    for (int s = 0; s < seedCount; s++)
                    {
                        double dx = px - seeds[s][0];
                        double dy = py - seeds[s][1];
                        double distSq = dx * dx + dy * dy;

                        if (distSq < minDistSq)
                        {
                            minDistSq = distSq;
                            nearestSeed = s;
                        }
                    }

                    assignments[row, col] = nearestSeed;
                }
            });

            return assignments;
        }

        /// <summary>
        /// Computes Voronoi cell boundaries by extracting edges where adjacent grid cells
        /// belong to different seed points. Boundary detection is parallelized by rows.
        /// </summary>
        /// <param name="assignments">Grid assignments from <see cref="Compute"/>.</param>
        /// <returns>
        /// List of boundary edge segments, each represented as [x1, y1, x2, y2].
        /// </returns>
        public static double[][] ExtractBoundaries(int[,] assignments)
        {
            if (assignments == null) throw new ArgumentNullException(nameof(assignments));

            int height = assignments.GetLength(0);
            int width = assignments.GetLength(1);

            List<double[]>[] rowEdges = new List<double[]>[height];

            Parallel.For(0, height, row =>
            {
                rowEdges[row] = new List<double[]>();

                for (int col = 0; col < width; col++)
                {
                    int current = assignments[row, col];

                    // Check right neighbor
                    if (col + 1 < width && assignments[row, col + 1] != current)
                    {
                        double x = col + 1.0;
                        rowEdges[row].Add(new double[] { x, row, x, row + 1.0 });
                    }

                    // Check bottom neighbor
                    if (row + 1 < height && assignments[row + 1, col] != current)
                    {
                        double y = row + 1.0;
                        rowEdges[row].Add(new double[] { col, y, col + 1.0, y });
                    }
                }
            });

            List<double[]> allEdges = new List<double[]>();
            for (int row = 0; row < height; row++)
            {
                allEdges.AddRange(rowEdges[row]);
            }

            return allEdges.ToArray();
        }
    }
}
