namespace MathVerse.Math.Distributed.SimulationParallelism
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel rigid body simulator that updates positions and orientations of multiple
    /// rigid bodies simultaneously, distributing force/torque computations across threads.
    /// </summary>
    public sealed class ParallelRigidBodySimulator
    {
        /// <summary>
        /// Simulates rigid body evolution over time with parallel force and orientation updates.
        /// Uses semi-implicit Euler integration for positions and orientation updates.
        /// </summary>
        /// <param name="positions">Array of rigid body positions (center of mass coordinates).</param>
        /// <param name="orientations">Array of rigid body orientations (Euler angles or rotation vectors).</param>
        /// <param name="forceFunc">
        /// Function that computes the force/torque on a body given its position and orientation.
        /// Signature: (int bodyIndex, double[] position, double[] orientation) -> double[] forceAndTorque.
        /// Note: the Func takes (int, double[], double[]) and returns double[].
        /// </param>
        /// <param name="dt">Time step size for integration.</param>
        /// <param name="steps">Number of simulation steps to perform.</param>
        /// <returns>Final positions of all rigid bodies after simulation.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when positions and orientations have different lengths or arrays are empty.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public static double[][] Simulate(
            double[][] positions,
            double[][] orientations,
            Func<int, double[], double[], double[]> forceFunc,
            double dt,
            int steps)
        {
            if (positions == null) throw new ArgumentNullException(nameof(positions));
            if (orientations == null) throw new ArgumentNullException(nameof(orientations));
            if (forceFunc == null) throw new ArgumentNullException(nameof(forceFunc));
            if (positions.Length == 0) throw new ArgumentException("Positions array must not be empty.", nameof(positions));
            if (positions.Length != orientations.Length)
                throw new ArgumentException("Positions and orientations must have the same length.");
            if (steps < 0) throw new ArgumentException("Steps must be non-negative.", nameof(steps));

            int bodyCount = positions.Length;

            double[][] resultPositions = new double[bodyCount][];
            double[][] resultOrientations = new double[bodyCount][];

            for (int i = 0; i < bodyCount; i++)
            {
                resultPositions[i] = new double[positions[i].Length];
                resultOrientations[i] = new double[orientations[i].Length];
                System.Array.Copy(positions[i], resultPositions[i], positions[i].Length);
                System.Array.Copy(orientations[i], resultOrientations[i], orientations[i].Length);
            }

            double[][] forces = new double[bodyCount][];

            for (int step = 0; step < steps; step++)
            {
                Parallel.For(0, bodyCount, i =>
                {
                    forces[i] = forceFunc(i, resultPositions[i], resultOrientations[i]);
                });

                Parallel.For(0, bodyCount, i =>
                {
                    int posDims = resultPositions[i].Length;
                    int oriDims = resultOrientations[i].Length;

                    // Split force/torque: first posDims entries are force, remaining are torque
                    for (int d = 0; d < posDims; d++)
                    {
                        resultPositions[i][d] += forces[i][d] * dt;
                    }

                    for (int d = 0; d < oriDims; d++)
                    {
                        int forceIdx = posDims + d;
                        if (forceIdx < forces[i].Length)
                        {
                            resultOrientations[i][d] += forces[i][forceIdx] * dt;
                        }
                    }

                    // Normalize orientations to prevent drift (mod 2*PI)
                    for (int d = 0; d < oriDims; d++)
                    {
                        resultOrientations[i][d] = resultOrientations[i][d]
                            - System.Math.Floor(resultOrientations[i][d] / (2.0 * System.Math.PI)) * (2.0 * System.Math.PI);
                    }
                });
            }

            return resultPositions;
        }
    }
}
