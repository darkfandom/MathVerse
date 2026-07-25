namespace MathVerse.Math.Distributed.SimulationParallelism
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel particle simulator that distributes force computations and state updates
    /// across multiple threads for N-body and particle-based simulations.
    /// </summary>
    public sealed class ParallelParticleSimulator
    {
        /// <summary>
        /// Simulates particle evolution over time with parallel force computation.
        /// Each particle's force is computed independently, then positions and velocities are updated.
        /// </summary>
        /// <param name="positions">Array of particle positions, where each element is a coordinate vector.</param>
        /// <param name="velocities">Array of particle velocities, where each element is a velocity vector.</param>
        /// <param name="forceFunc">
        /// Function that computes the force on a particle given its position and velocity.
        /// Signature: (int particleIndex, double[] position, double[] velocity) -> double[] force.
        /// </param>
        /// <param name="dt">Time step size for integration.</param>
        /// <param name="steps">Number of simulation steps to perform.</param>
        /// <returns>Final positions of all particles after simulation.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when positions and velocities have different lengths, or when arrays are empty.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any argument is null.
        /// </exception>
        public static double[][] Simulate(
            double[][] positions,
            double[][] velocities,
            Func<int, double[], double[], double[]> forceFunc,
            double dt,
            int steps)
        {
            if (positions == null) throw new ArgumentNullException(nameof(positions));
            if (velocities == null) throw new ArgumentNullException(nameof(velocities));
            if (forceFunc == null) throw new ArgumentNullException(nameof(forceFunc));
            if (positions.Length == 0) throw new ArgumentException("Positions array must not be empty.", nameof(positions));
            if (positions.Length != velocities.Length)
                throw new ArgumentException("Positions and velocities must have the same length.");
            if (steps < 0) throw new ArgumentException("Steps must be non-negative.", nameof(steps));

            int particleCount = positions.Length;

            double[][] resultPositions = new double[particleCount][];
            double[][] resultVelocities = new double[particleCount][];

            for (int i = 0; i < particleCount; i++)
            {
                resultPositions[i] = new double[positions[i].Length];
                resultVelocities[i] = new double[velocities[i].Length];
                System.Array.Copy(positions[i], resultPositions[i], positions[i].Length);
                System.Array.Copy(velocities[i], resultVelocities[i], velocities[i].Length);
            }

            double[][] forces = new double[particleCount][];

            for (int step = 0; step < steps; step++)
            {
                Parallel.For(0, particleCount, i =>
                {
                    forces[i] = forceFunc(i, resultPositions[i], resultVelocities[i]);
                });

                Parallel.For(0, particleCount, i =>
                {
                    int dims = resultPositions[i].Length;
                    for (int d = 0; d < dims; d++)
                    {
                        resultVelocities[i][d] += forces[i][d] * dt;
                        resultPositions[i][d] += resultVelocities[i][d] * dt;
                    }
                });
            }

            return resultPositions;
        }
    }
}
