namespace MathVerse.Math.Distributed.NumericalParallelism
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Parallel function evaluation for optimization algorithms.
    /// </summary>
    public sealed class ParallelOptimization
    {
        /// <summary>
        /// Evaluates an objective function over a population in parallel.
        /// </summary>
        /// <param name="objective">Objective function to minimize.</param>
        /// <param name="population">Array of candidate solutions.</param>
        /// <returns>Array of fitness values.</returns>
        public double[] EvaluatePopulation(Func<double[], double> objective, double[][] population)
        {
            if (objective == null)
                throw new ArgumentNullException(nameof(objective));
            if (population == null)
                throw new ArgumentNullException(nameof(population));

            var fitness = new double[population.Length];

            Parallel.For(0, population.Length, i =>
            {
                fitness[i] = objective(population[i]);
            });

            return fitness;
        }

        /// <summary>
        /// Evaluates multiple candidate solutions in parallel.
        /// </summary>
        /// <param name="objective">Objective function.</param>
        /// <param name="candidates">Candidate solution vectors.</param>
        /// <returns>Array of objective values.</returns>
        public double[] EvaluateCandidates(Func<double[], double> objective, double[][] candidates)
        {
            return EvaluatePopulation(objective, candidates);
        }

        /// <summary>
        /// Parallel differential evolution mutation step.
        /// </summary>
        /// <param name="population">Current population.</param>
        /// <param name="fitness">Current fitness values.</param>
        /// <param name="f">Mutation factor.</param>
        /// <param name="cr">Crossover rate.</param>
        /// <returns>Mutated trial vectors.</returns>
        public double[][] MutateParallel(double[][] population, double[] fitness, double f, double cr)
        {
            int popSize = population.Length;
            int dim = population[0].Length;
            var trials = new double[popSize][];

            Parallel.For(0, popSize, i =>
            {
                var rng = new Random(i * 31 + DateTime.Now.Millisecond);
                int a, b, c;
                do { a = rng.Next(popSize); } while (a == i);
                do { b = rng.Next(popSize); } while (b == i || b == a);
                do { c = rng.Next(popSize); } while (c == i || c == a || c == b);

                trials[i] = new double[dim];
                int jrand = rng.Next(dim);

                for (int j = 0; j < dim; j++)
                {
                    if (rng.NextDouble() < cr || j == jrand)
                    {
                        trials[i][j] = population[a][j] + f * (population[b][j] - population[c][j]);
                    }
                    else
                    {
                        trials[i][j] = population[i][j];
                    }
                }
            });

            return trials;
        }

        /// <summary>
        /// Parallel selection step for optimization.
        /// </summary>
        /// <param name="population">Current population.</param>
        /// <param name="fitness">Current fitness values.</param>
        /// <param name="trials">Trial vectors.</param>
        /// <param name="trialFitness">Trial fitness values.</param>
        public void SelectParallel(double[][] population, double[] fitness, double[][] trials, double[] trialFitness)
        {
            Parallel.For(0, population.Length, i =>
            {
                if (trialFitness[i] < fitness[i])
                {
                    Array.Copy(trials[i], population[i], population[i].Length);
                    fitness[i] = trialFitness[i];
                }
            });
        }
    }
}
