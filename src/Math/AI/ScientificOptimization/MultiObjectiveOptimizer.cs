namespace MathVerse.Math.AI.ScientificOptimization;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Multi-objective optimizer using an NSGA-II-like evolutionary approach
/// to approximate the Pareto front of a set of conflicting objectives.
/// </summary>
public sealed class MultiObjectiveOptimizer
{
    private readonly Random _rng;

    /// <summary>Initializes a new instance with the default random seed.</summary>
    public MultiObjectiveOptimizer()
    {
        _rng = new Random(42);
    }

    /// <summary>Initializes a new instance with the specified random seed.</summary>
    public MultiObjectiveOptimizer(int seed)
    {
        _rng = new Random(seed);
    }

    /// <summary>
    /// Finds an approximation of the Pareto front for the given multi-objective problem.
    /// </summary>
    /// <param name="objectives">Array of objective functions to minimize.</param>
    /// <param name="initial">Initial guess for variable ranges (used as bounds).</param>
    /// <param name="numVariables">Number of decision variables.</param>
    /// <param name="iterations">Number of generations to evolve.</param>
    /// <param name="populationSize">Number of candidate solutions per generation.</param>
    /// <returns>A <see cref="ParetoFront"/> containing the non-dominated solutions.</returns>
    public ParetoFront FindParetoFront(
        Func<double[], double>[] objectives,
        double[] initial,
        int numVariables,
        int iterations = 100,
        int populationSize = 50)
    {
        double[] lowerBounds = new double[numVariables];
        double[] upperBounds = new double[numVariables];
        for (int i = 0; i < numVariables; i++)
        {
            double val = i < initial.Length ? initial[i] : 1.0;
            lowerBounds[i] = val - System.Math.Abs(val) - 1.0;
            upperBounds[i] = val + System.Math.Abs(val) + 1.0;
        }

        List<double[]> population = InitializePopulation(populationSize, numVariables, lowerBounds, upperBounds);
        List<double[]> objValues = EvaluatePopulation(population, objectives);

        for (int gen = 0; gen < iterations; gen++)
        {
            List<double[]> offspring = CreateOffspring(population, numVariables, lowerBounds, upperBounds);
            List<double[]> offspringObj = EvaluatePopulation(offspring, objectives);

            List<double[]> combined = new List<double[]>(population);
            combined.AddRange(offspring);
            List<double[]> combinedObj = new List<double[]>(objValues);
            combinedObj.AddRange(offspringObj);

            List<int> selected = EnvironmentalSelection(combinedObj, populationSize);

            population = new List<double[]>(populationSize);
            objValues = new List<double[]>(populationSize);
            for (int i = 0; i < selected.Count && i < populationSize; i++)
            {
                population.Add(combined[selected[i]]);
                objValues.Add(combinedObj[selected[i]]);
            }
        }

        List<int> frontIndices = ExtractFrontIndices(objValues);
        List<double[]> paretoSet = new List<double[]>(frontIndices.Count);
        List<double[]> paretoObjValues = new List<double[]>(frontIndices.Count);
        double[] paretoCrowding = ComputeCrowdingOnFront(frontIndices, objValues);

        for (int i = 0; i < frontIndices.Count; i++)
        {
            paretoSet.Add(population[frontIndices[i]]);
            paretoObjValues.Add(objValues[frontIndices[i]]);
        }

        return new ParetoFront
        {
            Solutions = paretoSet,
            ObjectiveValues = paretoObjValues,
            CrowdingDistances = paretoCrowding
        };
    }

    /// <summary>
    /// Checks whether solution <paramref name="a"/> Pareto-dominates solution <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First solution's objective values.</param>
    /// <param name="b">Second solution's objective values.</param>
    /// <returns>True if <paramref name="a"/> dominates <paramref name="b"/>.</returns>
    public static bool Dominates(double[] a, double[] b)
    {
        bool atLeastOneBetter = false;
        for (int i = 0; i < a.Length; i++)
        {
            if (i >= b.Length)
                break;
            if (a[i] > b[i])
                return false;
            if (a[i] < b[i])
                atLeastOneBetter = true;
        }
        return atLeastOneBetter;
    }

    /// <summary>
    /// Filters a set of solutions to retain only the non-dominated ones.
    /// </summary>
    /// <param name="solutions">Candidate solution vectors.</param>
    /// <param name="objectives">Objective functions to evaluate.</param>
    /// <returns>Non-dominated subset of <paramref name="solutions"/>.</returns>
    public static List<double[]> FilterPareto(List<double[]> solutions, Func<double[], double>[] objectives)
    {
        List<double[]> objValues = new List<double[]>(solutions.Count);
        for (int i = 0; i < solutions.Count; i++)
        {
            double[] vals = new double[objectives.Length];
            for (int j = 0; j < objectives.Length; j++)
            {
                vals[j] = objectives[j](solutions[i]);
            }
            objValues.Add(vals);
        }

        List<double[]> nonDominated = [];
        for (int i = 0; i < solutions.Count; i++)
        {
            bool dominated = false;
            for (int j = 0; j < solutions.Count; j++)
            {
                if (i == j)
                    continue;
                if (Dominates(objValues[j], objValues[i]))
                {
                    dominated = true;
                    break;
                }
            }
            if (!dominated)
            {
                nonDominated.Add(solutions[i]);
            }
        }
        return nonDominated;
    }

    private List<double[]> InitializePopulation(int size, int numVars, double[] lower, double[] upper)
    {
        List<double[]> pop = new List<double[]>(size);
        for (int i = 0; i < size; i++)
        {
            double[] individual = new double[numVars];
            for (int j = 0; j < numVars; j++)
            {
                individual[j] = lower[j] + _rng.NextDouble() * (upper[j] - lower[j]);
            }
            pop.Add(individual);
        }
        return pop;
    }

    private List<double[]> EvaluatePopulation(List<double[]> population, Func<double[], double>[] objectives)
    {
        List<double[]> results = new List<double[]>(population.Count);
        for (int i = 0; i < population.Count; i++)
        {
            double[] vals = new double[objectives.Length];
            for (int j = 0; j < objectives.Length; j++)
            {
                vals[j] = objectives[j](population[i]);
            }
            results.Add(vals);
        }
        return results;
    }

    private List<double[]> CreateOffspring(List<double[]> population, int numVars, double[] lower, double[] upper)
    {
        int popSize = population.Count;
        List<double[]> offspring = new List<double[]>(popSize);

        for (int i = 0; i < popSize; i++)
        {
            int r1 = _rng.Next(popSize);
            int r2 = _rng.Next(popSize);
            while (r2 == r1) r2 = _rng.Next(popSize);
            int r3 = _rng.Next(popSize);
            while (r3 == r1 || r3 == r2) r3 = _rng.Next(popSize);

            double[] child = new double[numVars];
            for (int j = 0; j < numVars; j++)
            {
                if (_rng.NextDouble() < 0.5)
                {
                    child[j] = population[r1][j % population[r1].Length]
                             + 1.5 * (population[r2][j % population[r2].Length]
                                    - population[r3][j % population[r3].Length]);
                }
                else
                {
                    child[j] = population[i][j % population[i].Length];
                }
                child[j] = System.Math.Max(lower[j], System.Math.Min(upper[j], child[j]));
            }
            offspring.Add(child);
        }
        return offspring;
    }

    private static bool DominationCompare(double[] a, double[] b)
    {
        return Dominates(a, b);
    }

    private List<int> ExtractFrontIndices(List<double[]> objValues)
    {
        int n = objValues.Count;
        int[] domCount = new int[n];
        List<List<int>> domSet = new List<List<int>>(n);
        for (int i = 0; i < n; i++)
            domSet.Add([]);

        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                if (DominationCompare(objValues[i], objValues[j]))
                {
                    domSet[i].Add(j);
                    domCount[j]++;
                }
                else if (DominationCompare(objValues[j], objValues[i]))
                {
                    domSet[j].Add(i);
                    domCount[i]++;
                }
            }
        }

        List<int> front = [];
        for (int i = 0; i < n; i++)
        {
            if (domCount[i] == 0)
                front.Add(i);
        }

        return front;
    }

    private List<int> EnvironmentalSelection(List<double[]> objValues, int targetSize)
    {
        int n = objValues.Count;
        int[] domCount = new int[n];
        List<List<int>> domSet = new List<List<int>>(n);
        for (int i = 0; i < n; i++)
            domSet.Add([]);

        List<List<int>> fronts = [];

        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                if (DominationCompare(objValues[i], objValues[j]))
                {
                    domSet[i].Add(j);
                    domCount[j]++;
                }
                else if (DominationCompare(objValues[j], objValues[i]))
                {
                    domSet[j].Add(i);
                    domCount[i]++;
                }
            }
        }

        List<int> firstFront = [];
        for (int i = 0; i < n; i++)
        {
            if (domCount[i] == 0)
                firstFront.Add(i);
        }
        fronts.Add(firstFront);

        int fi = 0;
        while (fi < fronts.Count && fronts[fi].Count > 0)
        {
            List<int> next = [];
            for (int p = 0; p < fronts[fi].Count; p++)
            {
                int pNode = fronts[fi][p];
                for (int q = 0; q < domSet[pNode].Count; q++)
                {
                    int qNode = domSet[pNode][q];
                    domCount[qNode]--;
                    if (domCount[qNode] == 0)
                        next.Add(qNode);
                }
            }
            if (next.Count > 0)
                fronts.Add(next);
            fi++;
        }

        List<int> selected = [];
        for (int f = 0; f < fronts.Count; f++)
        {
            if (selected.Count + fronts[f].Count <= targetSize)
            {
                selected.AddRange(fronts[f]);
            }
            else
            {
                int remaining = targetSize - selected.Count;
                if (remaining <= 0)
                    break;

                double[] crowding = ComputeCrowdingOnFront(fronts[f], objValues);
                int[] sortedByCrowding = Enumerable.Range(0, fronts[f].Count)
                    .OrderByDescending(i => i < crowding.Length ? crowding[i] : 0.0)
                    .ToArray();

                for (int i = 0; i < remaining && i < sortedByCrowding.Length; i++)
                {
                    selected.Add(fronts[f][sortedByCrowding[i]]);
                }
            }
        }

        while (selected.Count < targetSize)
            selected.Add(_rng.Next(n));

        return selected.Take(targetSize).ToList();
    }

    private double[] ComputeCrowdingOnFront(List<int> frontIndices, List<double[]> objValues)
    {
        int frontSize = frontIndices.Count;
        double[] distances = new double[frontSize];

        if (frontSize <= 2)
        {
            for (int i = 0; i < frontSize; i++)
                distances[i] = double.PositiveInfinity;
            return distances;
        }

        int numObjectives = objValues[frontIndices[0]].Length;

        for (int m = 0; m < numObjectives; m++)
        {
            int[] sorted = Enumerable.Range(0, frontSize)
                .OrderBy(i => objValues[frontIndices[i]][m])
                .ToArray();

            distances[sorted[0]] = double.PositiveInfinity;
            distances[sorted[frontSize - 1]] = double.PositiveInfinity;

            double minVal = objValues[frontIndices[sorted[0]]][m];
            double maxVal = objValues[frontIndices[sorted[frontSize - 1]]][m];
            double range = maxVal - minVal;

            if (range < 1e-12)
                continue;

            for (int i = 1; i < frontSize - 1; i++)
            {
                double nextVal = objValues[frontIndices[sorted[i + 1]]][m];
                double prevVal = objValues[frontIndices[sorted[i - 1]]][m];
                distances[sorted[i]] += (nextVal - prevVal) / range;
            }
        }

        return distances;
    }
}
