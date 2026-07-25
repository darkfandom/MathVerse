namespace MathVerse.Math.AI.Optimization;

/// <summary>Genetic optimizer with selection, blend crossover, Gaussian mutation, and elitism.</summary>
public sealed class GeneticOptimizer : IOptimizer
{
    /// <summary>Gets the name of this optimizer.</summary>
    public string Name => "Genetic";

    /// <summary>Runs genetic algorithm optimization.</summary>
    /// <param name="objective">The objective function to minimize.</param>
    /// <param name="initial">The initial parameter vector (used for dimensionality).</param>
    /// <param name="options">Optional optimization options.</param>
    /// <returns>The optimization result.</returns>
    public OptimizationResult Optimize(Func<double[], double> objective, double[] initial, OptimizationOptions? options = null)
    {
        var opts = options ?? OptimizationOptions.Default;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var rng = new Random(opts.RandomSeed);
        int n = initial.Length;
        int popSize = System.Math.Max(30, n * 5);
        int eliteCount = System.Math.Max(2, popSize / 10);
        double crossoverRate = 0.8;
        double mutationRate = 1.0 / n;
        double mutationSigma = 0.1;

        var population = new double[popSize][];
        var fitness = new double[popSize];

        for (int p = 0; p < popSize; p++)
        {
            population[p] = new double[n];
            for (int i = 0; i < n; i++)
            {
                double range = System.Math.Max(System.Math.Abs(initial[i]) * 2.0, 1.0);
                population[p][i] = initial[i] + (rng.NextDouble() * 2.0 - 1.0) * range;
            }
            GradientDescentOptimizer.ClampToBounds(population[p], opts.LowerBounds, opts.UpperBounds);
            fitness[p] = objective(population[p]);
        }

        int bestIdx = 0;
        for (int p = 1; p < popSize; p++)
        {
            if (fitness[p] < fitness[bestIdx]) bestIdx = p;
        }
        double[] globalBest = (double[])population[bestIdx].Clone();
        double globalBestVal = fitness[bestIdx];
        bool converged = false;
        int iter;

        for (iter = 0; iter < opts.MaxIterations; iter++)
        {
            int[] sortedIndices = new int[popSize];
            for (int i = 0; i < popSize; i++) sortedIndices[i] = i;
            for (int i = 0; i < popSize - 1; i++)
            {
                for (int j = i + 1; j < popSize; j++)
                {
                    if (fitness[sortedIndices[j]] < fitness[sortedIndices[i]])
                    {
                        (sortedIndices[i], sortedIndices[j]) = (sortedIndices[j], sortedIndices[i]);
                    }
                }
            }

            var newPop = new double[popSize][];
            for (int e = 0; e < eliteCount; e++)
            {
                newPop[e] = (double[])population[sortedIndices[e]].Clone();
            }

            double totalInvFitness = 0.0;
            for (int p = 0; p < popSize; p++)
            {
                totalInvFitness += 1.0 / (fitness[p] + 1e-10);
            }

            for (int p = eliteCount; p < popSize; p += 2)
            {
                int parent1 = TournamentSelect(population, fitness, popSize, rng);
                int parent2 = TournamentSelect(population, fitness, popSize, rng);

                double[] child1 = (double[])population[parent1].Clone();
                double[] child2 = (double[])population[parent2].Clone();

                if (rng.NextDouble() < crossoverRate)
                {
                    BlendCrossover(child1, child2, n, rng);
                }

                GaussianMutate(child1, mutationRate, mutationSigma, rng);
                GaussianMutate(child2, mutationRate, mutationSigma, rng);

                GradientDescentOptimizer.ClampToBounds(child1, opts.LowerBounds, opts.UpperBounds);
                GradientDescentOptimizer.ClampToBounds(child2, opts.LowerBounds, opts.UpperBounds);

                newPop[p] = child1;
                if (p + 1 < popSize)
                {
                    newPop[p + 1] = child2;
                }
            }

            for (int p = 0; p < popSize; p++)
            {
                population[p] = newPop[p];
                fitness[p] = objective(population[p]);
            }

            for (int p = 0; p < popSize; p++)
            {
                if (fitness[p] < globalBestVal)
                {
                    globalBestVal = fitness[p];
                    globalBest = (double[])population[p].Clone();
                }
            }

            double bestFit = fitness[0];
            double worstFit = fitness[0];
            for (int p = 1; p < popSize; p++)
            {
                if (fitness[p] < bestFit) bestFit = fitness[p];
                if (fitness[p] > worstFit) worstFit = fitness[p];
            }
            if (System.Math.Abs(worstFit - bestFit) < opts.Tolerance)
            {
                converged = true;
                break;
            }
        }

        sw.Stop();
        return new OptimizationResult
        {
            Success = true,
            BestParameters = globalBest,
            BestValue = globalBestVal,
            IterationsExecuted = iter,
            Converged = converged,
            ElapsedTime = sw.Elapsed
        };
    }

    /// <summary>Tournament selection of size 3.</summary>
    /// <param name="population">The population.</param>
    /// <param name="fitness">Fitness values.</param>
    /// <param name="popSize">Population size.</param>
    /// <param name="rng">Random number generator.</param>
    /// <returns>Index of the selected individual.</returns>
    private static int TournamentSelect(double[][] population, double[] fitness, int popSize, Random rng)
    {
        int best = rng.Next(popSize);
        for (int t = 1; t < 3; t++)
        {
            int candidate = rng.Next(popSize);
            if (fitness[candidate] < fitness[best])
            {
                best = candidate;
            }
        }
        return best;
    }

    /// <summary>Blend crossover (BLX-alpha) producing two children.</summary>
    /// <param name="child1">First child to modify in-place.</param>
    /// <param name="child2">Second child to modify in-place.</param>
    /// <param name="n">Dimension.</param>
    /// <param name="rng">Random number generator.</param>
    private static void BlendCrossover(double[] child1, double[] child2, int n, Random rng)
    {
        double alpha = 0.5;
        for (int i = 0; i < n; i++)
        {
            double lo = System.Math.Min(child1[i], child2[i]);
            double hi = System.Math.Max(child1[i], child2[i]);
            double range = hi - lo;
            child1[i] = lo - alpha * range + rng.NextDouble() * (range + 2.0 * alpha * range);
            child2[i] = lo - alpha * range + rng.NextDouble() * (range + 2.0 * alpha * range);
        }
    }

    /// <summary>Gaussian mutation applied to each gene with given probability.</summary>
    /// <param name="individual">The individual to mutate.</param>
    /// <param name="mutationRate">Probability of mutating each gene.</param>
    /// <param name="sigma">Standard deviation of the Gaussian perturbation.</param>
    /// <param name="rng">Random number generator.</param>
    private static void GaussianMutate(double[] individual, double mutationRate, double sigma, Random rng)
    {
        for (int i = 0; i < individual.Length; i++)
        {
            if (rng.NextDouble() < mutationRate)
            {
                double u1 = 1.0 - rng.NextDouble();
                double u2 = 1.0 - rng.NextDouble();
                double z0 = System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Cos(2.0 * System.Math.PI * u2);
                individual[i] += sigma * z0;
            }
        }
    }
}
