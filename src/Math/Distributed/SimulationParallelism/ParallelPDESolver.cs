namespace MathVerse.Math.Distributed.SimulationParallelism
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel PDE solver that evolves a 3D grid over time using a user-supplied
    /// update function, distributing grid-point computations across multiple threads.
    /// </summary>
    public sealed class ParallelPDESolver
    {
        /// <summary>
        /// Solves a PDE on a 3D grid by iteratively applying an update function
        /// across all interior grid points in parallel.
        /// </summary>
        /// <param name="grid">
        /// 3D grid of shape [width, height, depth] representing the PDE field.
        /// Modified in place over time steps.
        /// </param>
        /// <param name="updateFunc">
        /// Update function computing the new value at a given grid point.
        /// Signature: (double[,,] grid, int x, int y, int z) -> double newValue.
        /// </param>
        /// <param name="timeSteps">Number of time steps to evolve the grid.</param>
        /// <returns>
        /// A copy of the final grid state after all time steps have been applied.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="grid"/> or <paramref name="updateFunc"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="timeSteps"/> is negative.</exception>
        public static double[,,] SolveGrid(
            double[,,] grid,
            Func<double[,,], int, int, int, double> updateFunc,
            int timeSteps)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            if (updateFunc == null) throw new ArgumentNullException(nameof(updateFunc));
            if (timeSteps < 0) throw new ArgumentException("Time steps must be non-negative.", nameof(timeSteps));

            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            int depth = grid.GetLength(2);

            double[,,] buffer = new double[width, height, depth];
            System.Array.Copy(grid, buffer, grid.Length);

            for (int t = 0; t < timeSteps; t++)
            {
                double[,,] nextBuffer = new double[width, height, depth];

                // Copy boundary values (Dirichlet boundary conditions)
                System.Array.Copy(buffer, nextBuffer, buffer.Length);

                Parallel.For(1, width - 1, x =>
                {
                    for (int y = 1; y < height - 1; y++)
                    {
                        for (int z = 1; z < depth - 1; z++)
                        {
                            nextBuffer[x, y, z] = updateFunc(buffer, x, y, z);
                        }
                    }
                });

                double[,,] temp = buffer;
                buffer = nextBuffer;
                nextBuffer = temp;
            }

            double[,,] result = new double[width, height, depth];
            System.Array.Copy(buffer, result, buffer.Length);
            return result;
        }
    }
}
