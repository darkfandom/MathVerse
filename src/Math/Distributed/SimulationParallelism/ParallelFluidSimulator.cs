namespace MathVerse.Math.Distributed.SimulationParallelism
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel fluid field simulator using Jacobi iteration for solving
    /// diffusion and pressure equations on a 3D grid.
    /// </summary>
    public sealed class ParallelFluidSimulator
    {
        /// <summary>
        /// Simulates fluid diffusion on a 3D field using parallel Jacobi iteration.
        /// The field is a 3D array where the first two dimensions represent spatial coordinates
        /// and the third dimension holds multiple scalar fields (e.g., density, velocity components).
        /// </summary>
        /// <param name="field">
        /// 3D field array of shape [width, height, components].
        /// Modified in place with the diffusion result.
        /// </param>
        /// <param name="iterations">Number of Jacobi iterations to perform (default: 10).</param>
        /// <param name="diffusionRate">Diffusion coefficient controlling spread rate (default: 0.01).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="field"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when iterations is negative or diffusion rate is non-positive.
        /// </exception>
        public static void Simulate(double[,,] field, int iterations = 10, double diffusionRate = 0.01)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            if (iterations < 0) throw new ArgumentException("Iterations must be non-negative.", nameof(iterations));
            if (diffusionRate <= 0.0) throw new ArgumentException("Diffusion rate must be positive.", nameof(diffusionRate));

            int width = field.GetLength(0);
            int height = field.GetLength(1);
            int components = field.GetLength(2);

            if (width < 2 || height < 2) return;

            double[,,] buffer = new double[width, height, components];
            System.Array.Copy(field, buffer, field.Length);

            double alpha = diffusionRate;
            double beta = 4.0 + alpha;

            for (int iter = 0; iter < iterations; iter++)
            {
                Parallel.For(1, width - 1, x =>
                {
                    for (int y = 1; y < height - 1; y++)
                    {
                        for (int c = 0; c < components; c++)
                        {
                            double left = buffer[x - 1, y, c];
                            double right = buffer[x + 1, y, c];
                            double up = buffer[x, y - 1, c];
                            double down = buffer[x, y + 1, c];
                            double center = buffer[x, y, c];

                            field[x, y, c] = (left + right + up + down + alpha * center) / beta;
                        }
                    }
                });

                // Copy field back to buffer for next iteration
                System.Array.Copy(field, buffer, field.Length);

                // Enforce boundary conditions
                ApplyBoundaryConditions(field, width, height, components);
            }
        }

        /// <summary>
        /// Applies fixed boundary conditions to the field edges.
        /// </summary>
        private static void ApplyBoundaryConditions(double[,,] field, int width, int height, int components)
        {
            for (int c = 0; c < components; c++)
            {
                // Top and bottom edges
                for (int x = 0; x < width; x++)
                {
                    field[x, 0, c] = 0.0;
                    field[x, height - 1, c] = 0.0;
                }

                // Left and right edges
                for (int y = 0; y < height; y++)
                {
                    field[0, y, c] = 0.0;
                    field[width - 1, y, c] = 0.0;
                }
            }
        }
    }
}
