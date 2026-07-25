namespace MathVerse.Math.AI.ScientificOptimization;
using MathVerse.Math.AI.Optimization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

/// <summary>
/// Optimizer using Differential Evolution (DE/rand/1/bin strategy).
/// A population-based stochastic optimizer that uses vector differences for mutation
/// and binomial crossover for exploration.
/// </summary>
public sealed class EvolutionaryOptimizer
{
    private readonly Random _rng;

    /// <summary>Initializes a new instance with the default random seed.</summary>
    public EvolutionaryOptimizer()
    {
        _rng = new Random(42);
    }

    /// <summary>Initializes a new instance with the specified random seed.</summary>
    public EvolutionaryOptimizer(int seed)
    {
        _rng = new Random(seed);
    }

    /// <summary>
    /// Minimizes the objective function using Differential Evolution.
    /// </summary>
    /// <param name="objective">The objective function to minimize.</param>
    /// <param name="numVariables">Number of decision variables.</param>
    /// <param name="populationSize">Population size (must be at least 4).</param>
    /// <param name="iterations">Maximum number of generations.</param>
    /// <param name="F">Mutation scaling factor (typically 0.5–1.0).</param>
    /// <param name="CR">Crossover probability (0–1).</param>
    /// <returns>An <see cref="OptimizationResult"/> with the solution.</returns>
    public OptimizationResult Optimize(
        Func<double[], double> objective,
        int numVariables,
        int populationSize = 50,
        int iterations = 200,
        double F = 0.8,
        double CR = 0.7)
    {
        Stopwatch sw = Stopwatch.StartNew();
        int popSize = System.Math.Max(populationSize, 4);
        double[] lower = new double[numVariables];
        double[] upper = new double[numVariables];
        for (int i = 0; i < numVariables; i++)
        {
            lower[i] = -10.0;
            upper[i] = 10.0;
        }

        double[][] population = new double[popSize][];
        double[] fitness = new double[popSize];

        for (int i = 0; i < popSize; i++)
        {
            population[i] = new double[numVariables];
            for (int j = 0; j < numVariables; j++)
            {
                population[i][j] = lower[j] + _rng.NextDouble() * (upper[j] - lower[j]);
            }
            fitness[i] = objective(population[i]);
        }

        int bestIdx = 0;
        for (int i = 1; i < popSize; i++)
        {
            if (fitness[i] < fitness[bestIdx])
                bestIdx = i;
        }

        double bestFitness = fitness[bestIdx];
        double[] bestParams = new double[numVariables];
        Array.Copy(population[bestIdx], bestParams, numVariables);

        for (int gen = 0; gen < iterations; gen++)
        {
            for (int i = 0; i < popSize; i++)
            {
                int[] indices = SelectDistinct(popSize, i, 3);
                int r1 = indices[0];
                int r2 = indices[1];
                int r3 = indices[2];

                double[] mutant = new double[numVariables];
                for (int j = 0; j < numVariables; j++)
                {
                    mutant[j] = population[r1][j] + F * (population[r2][j] - population[r3][j]);
                    mutant[j] = System.Math.Max(lower[j], System.Math.Min(upper[j], mutant[j]));
                }

                double[] trial = new double[numVariables];
                int jRand = _rng.Next(numVariables);
                for (int j = 0; j < numVariables; j++)
                {
                    if (_rng.NextDouble() < CR || j == jRand)
                    {
                        trial[j] = mutant[j];
                    }
                    else
                    {
                        trial[j] = population[i][j];
                    }
                }

                double trialFitness = objective(trial);
                if (trialFitness <= fitness[i])
                {
                    population[i] = trial;
                    fitness[i] = trialFitness;

                    if (trialFitness < bestFitness)
                    {
                        bestFitness = trialFitness;
                        Array.Copy(trial, bestParams, numVariables);
                    }
                }
            }
        }

        sw.Stop();
        return new OptimizationResult
        {
            Success = true,
            BestParameters = bestParams,
            BestValue = bestFitness,
            IterationsExecuted = iterations,
            Converged = false,
            ElapsedTime = sw.Elapsed,
            Metrics = ImmutableDictionary<string, double>.Empty
                .Add("populationSize", popSize)
                .Add("finalBestFitness", bestFitness)
        };
    }

    private int[] SelectDistinct(int populationSize, int exclude, int count)
    {
        int[] selected = new int[count];
        bool[] used = new bool[populationSize];
        used[exclude] = true;
        int selectedCount = 0;

        while (selectedCount < count)
        {
            int idx = _rng.Next(populationSize);
            if (!used[idx])
            {
                used[idx] = true;
                selected[selectedCount] = idx;
                selectedCount++;
            }
        }

        return selected;
    }
}
