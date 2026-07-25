namespace MathVerse.Math.Distributed.SimulationParallelism
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel Monte Carlo simulator that runs multiple independent stochastic particle
    /// simulations concurrently to estimate statistical quantities.
    /// </summary>
    public sealed class ParallelMonteCarloSimulator
    {
        /// <summary>
        /// Runs Monte Carlo simulations in parallel across multiple particles.
        /// Each particle follows an independent stochastic trajectory, and the final
        /// states are collected and returned.
        /// </summary>
        /// <param name="stepFunc">
        /// Single-step transition function for each particle.
        /// Signature: (double[] state, double randomSeed, double dt) -> double[] newState.
        /// The randomSeed parameter provides per-particle deterministic randomness.
        /// </param>
        /// <param name="initialState">Initial state vector for all particles.</param>
        /// <param name="numParticles">Number of independent particles to simulate.</param>
        /// <param name="steps">Number of time steps per particle.</param>
        /// <param name="dt">Time step size.</param>
        /// <returns>
        /// Array of final states, one per particle. Each element is a state vector.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stepFunc"/> or <paramref name="initialState"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="numParticles"/> or <paramref name="steps"/> is negative,
        /// or <paramref name="initialState"/> is empty.
        /// </exception>
        public static double[][] Simulate(
            Func<double[], double, double, double[]> stepFunc,
            double[] initialState,
            int numParticles,
            int steps,
            double dt)
        {
            if (stepFunc == null) throw new ArgumentNullException(nameof(stepFunc));
            if (initialState == null) throw new ArgumentNullException(nameof(initialState));
            if (initialState.Length == 0) throw new ArgumentException("Initial state must not be empty.", nameof(initialState));
            if (numParticles < 0) throw new ArgumentException("Number of particles must be non-negative.", nameof(numParticles));
            if (steps < 0) throw new ArgumentException("Steps must be non-negative.", nameof(steps));

            double[][] finalStates = new double[numParticles][];
            int stateDim = initialState.Length;

            Parallel.For(0, numParticles, p =>
            {
                Random rng = new Random(p * 31 + 7);
                double[] currentState = new double[stateDim];
                System.Array.Copy(initialState, currentState, stateDim);

                for (int step = 0; step < steps; step++)
                {
                    double seed = rng.NextDouble();
                    double[] newState = stepFunc(currentState, seed, dt);
                    System.Array.Copy(newState, currentState, stateDim);
                }

                finalStates[p] = new double[stateDim];
                System.Array.Copy(currentState, finalStates[p], stateDim);
            });

            return finalStates;
        }
    }
}
