namespace MathVerse.Math.Distributed.AIParallelism
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel search engine that evaluates a population of candidate solutions
    /// concurrently for evolutionary and population-based optimization algorithms.
    /// </summary>
    public sealed class ParallelSearchEngine
    {
        /// <summary>
        /// Searches for the minimum of an objective function using a parallel population-based
        /// search strategy. Evaluates fitness of all candidates in parallel each iteration,
        /// then applies selection, crossover, and mutation to evolve the population.
        /// </summary>
        /// <param name="objective">
        /// Objective function to minimize.
        /// Signature: (double[] candidate) -> double fitness (lower is better).
        /// </param>
        /// <param name="dimensions">Dimensionality of the search space.</param>
        /// <param name="populationSize">Number of candidate solutions (default: 100).</param>
        /// <param name="iterations">Number of evolutionary iterations (default: 50).</param>
        /// <returns>
        /// The best candidate solution found, as a coordinate vector.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="objective"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="dimensions"/> or other parameters are invalid.
        /// </exception>
        public static double[] Search(
            Func<double[], double> objective,
            int dimensions,
            int populationSize = 100,
            int iterations = 50)
        {
            if (objective == null) throw new ArgumentNullException(nameof(objective));
            if (dimensions <= 0) throw new ArgumentException("Dimensions must be positive.", nameof(dimensions));
            if (populationSize <= 0) throw new ArgumentException("Population size must be positive.", nameof(populationSize));
            if (iterations < 0) throw new ArgumentException("Iterations must be non-negative.", nameof(iterations));

            Random rng = new Random(42);

            // Initialize population randomly in [-10, 10]
            double[][] population = new double[populationSize][];
            double[] fitness = new double[populationSize];

            for (int i = 0; i < populationSize; i++)
            {
                population[i] = new double[dimensions];
                for (int d = 0; d < dimensions; d++)
                {
                    population[i][d] = rng.NextDouble() * 20.0 - 10.0;
                }
            }

            // Evaluate initial population in parallel
            Parallel.For(0, populationSize, i =>
            {
                fitness[i] = objective(population[i]);
            });

            for (int iter = 0; iter < iterations; iter++)
            {
                // Find best for elitism
                int bestIdx = 0;
                for (int i = 1; i < populationSize; i++)
                {
                    if (fitness[i] < fitness[bestIdx]) bestIdx = i;
                }

                double[][] newPopulation = new double[populationSize][];

                // Keep the best candidate (elitism)
                newPopulation[0] = new double[dimensions];
                System.Array.Copy(population[bestIdx], newPopulation[0], dimensions);

                // Generate new candidates via crossover and mutation
                for (int i = 1; i < populationSize; i++)
                {
                    int parent1 = TournamentSelect(fitness, populationSize, rng);
                    int parent2 = TournamentSelect(fitness, populationSize, rng);

                    newPopulation[i] = Crossover(population[parent1], population[parent2], dimensions, rng);
                    Mutate(newPopulation[i], dimensions, rng);
                }

                population = newPopulation;

                // Evaluate new population in parallel
                Parallel.For(0, populationSize, i =>
                {
                    fitness[i] = objective(population[i]);
                });
            }

            // Find global best
            int globalBest = 0;
            for (int i = 1; i < populationSize; i++)
            {
                if (fitness[i] < fitness[globalBest]) globalBest = i;
            }

            double[] result = new double[dimensions];
            System.Array.Copy(population[globalBest], result, dimensions);
            return result;
        }

        /// <summary>
        /// Performs tournament selection to choose a parent from the population.
        /// </summary>
        private static int TournamentSelect(double[] fitness, int populationSize, Random rng)
        {
            int tournamentSize = System.Math.Max(2, populationSize / 10);
            int best = rng.Next(populationSize);

            for (int t = 1; t < tournamentSize; t++)
            {
                int candidate = rng.Next(populationSize);
                if (fitness[candidate] < fitness[best])
                {
                    best = candidate;
                }
            }

            return best;
        }

        /// <summary>
        /// Performs uniform crossover between two parent solutions.
        /// </summary>
        private static double[] Crossover(double[] parent1, double[] parent2, int dimensions, Random rng)
        {
            double[] child = new double[dimensions];
            double crossoverRate = 0.7;

            for (int d = 0; d < dimensions; d++)
            {
                child[d] = rng.NextDouble() < crossoverRate
                    ? parent1[d]
                    : parent2[d];
            }

            return child;
        }

        /// <summary>
        /// Applies Gaussian mutation to a candidate solution.
        /// </summary>
        private static void Mutate(double[] candidate, int dimensions, Random rng)
        {
            double mutationRate = 1.0 / dimensions;
            double mutationStrength = 0.5;

            for (int d = 0; d < dimensions; d++)
            {
                if (rng.NextDouble() < mutationRate)
                {
                    // Box-Muller transform for Gaussian noise
                    double u1 = rng.NextDouble();
                    double u2 = rng.NextDouble();
                    double noise = System.Math.Sqrt(-2.0 * System.Math.Log(u1 + 1e-300))
                        * System.Math.Cos(2.0 * System.Math.PI * u2);
                    candidate[d] += mutationStrength * noise;
                }
            }
        }
    }
}
