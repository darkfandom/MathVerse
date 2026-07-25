namespace MathVerse.Math.AI.Probability;

using System;
using System.Collections.Generic;

/// <summary>Bayesian optimization using Gaussian process surrogate and expected improvement acquisition function.</summary>
public sealed class BayesianOptimization
{
    private readonly GaussianProcess _gp;
    private readonly Random _rng;

    /// <summary>Initializes a new Bayesian optimization instance.</summary>
    /// <param name="seed">Random seed for reproducibility. Use -1 for non-deterministic.</param>
    public BayesianOptimization(int seed = -1)
    {
        _gp = new GaussianProcess();
        _rng = seed >= 0 ? new Random(seed) : new Random();
    }

    /// <summary>Optimizes a black-box function using Bayesian optimization with expected improvement.</summary>
    /// <param name="objective">Objective function to minimize.</param>
    /// <param name="initialPoints">Initial evaluation points.</param>
    /// <param name="iterations">Number of optimization iterations.</param>
    /// <returns>Tuple of (best point found, best objective value).</returns>
    public (double[] BestPoint, double BestValue) Optimize(Func<double[], double> objective, double[][] initialPoints, int iterations = 20)
    {
        if (objective == null)
            throw new ArgumentNullException(nameof(objective));
        if (initialPoints == null || initialPoints.Length == 0)
            throw new ArgumentException("Initial points cannot be null or empty.", nameof(initialPoints));
        if (iterations < 0)
            throw new ArgumentException("Iterations must be non-negative.", nameof(iterations));

        List<double[]> Xevaluated = new();
        List<double> yEvaluated = new();

        foreach (double[] point in initialPoints)
        {
            Xevaluated.Add(point);
            yEvaluated.Add(objective(point));
        }

        for (int iter = 0; iter < iterations; iter++)
        {
            double[][] Xtrain = Xevaluated.ToArray();
            double[] ytrain = yEvaluated.ToArray();

            _gp.Fit(Xtrain, ytrain, 1.0, 1.0);

            double[] nextPoint = SelectNextPoint(Xtrain, ytrain);
            double nextValue = objective(nextPoint);

            Xevaluated.Add(nextPoint);
            yEvaluated.Add(nextValue);
        }

        double bestVal = double.MaxValue;
        double[] bestPt = Xevaluated[0];
        for (int i = 0; i < yEvaluated.Count; i++)
        {
            if (yEvaluated[i] < bestVal)
            {
                bestVal = yEvaluated[i];
                bestPt = Xevaluated[i];
            }
        }

        return (bestPt, bestVal);
    }

    /// <summary>Computes the expected improvement at a given point.</summary>
    /// <param name="x">Point to evaluate.</param>
    /// <param name="bestY">Best observed objective value so far.</param>
    /// <returns>Expected improvement value.</returns>
    public double ExpectedImprovement(double[] x, double bestY)
    {
        if (x == null)
            throw new ArgumentNullException(nameof(x));

        var (means, variances) = _gp.Predict(new[] { x });
        double mu = means[0];
        double sigma = System.Math.Sqrt(variances[0] + 1e-10);

        double improvement = bestY - mu;
        double z = improvement / sigma;

        double phi = GaussianCDF(z);
        double pdf = GaussianPDF(z);

        return improvement * phi + sigma * pdf;
    }

    private double[] SelectNextPoint(double[][] Xtrain, double[] ytrain)
    {
        const int numCandidates = 100;
        const int numRestarts = 5;
        int dim = Xtrain[0].Length;

        double bestY = double.MaxValue;
        for (int i = 0; i < ytrain.Length; i++)
        {
            if (ytrain[i] < bestY)
                bestY = ytrain[i];
        }

        double[] bestCandidate = Xtrain[0];
        double bestEI = double.MinValue;

        for (int r = 0; r < numRestarts; r++)
        {
            double[] candidate = new double[dim];
            for (int d = 0; d < dim; d++)
                candidate[d] = _rng.NextDouble() * 10.0 - 5.0;

            double candidateEI = ExpectedImprovement(candidate, bestY);
            if (candidateEI > bestEI)
            {
                bestEI = candidateEI;
                bestCandidate = candidate;
            }

            for (int iter = 0; iter < 20; iter++)
            {
                double stepSize = 0.5 * System.Math.Exp(-0.1 * iter);
                double[] neighbor = new double[dim];
                for (int d = 0; d < dim; d++)
                {
                    double u1 = _rng.NextDouble();
                    double u2 = _rng.NextDouble();
                    double z = System.Math.Sqrt(-2.0 * System.Math.Log(u1 + 1e-300)) * System.Math.Cos(2.0 * System.Math.PI * u2);
                    neighbor[d] = candidate[d] + stepSize * z;
                }

                double neighborEI = ExpectedImprovement(neighbor, bestY);
                if (neighborEI > bestEI)
                {
                    bestEI = neighborEI;
                    bestCandidate = neighbor;
                }
                candidate = neighbor;
            }
        }

        for (int c = 0; c < numCandidates; c++)
        {
            double[] candidate = new double[dim];
            for (int d = 0; d < dim; d++)
                candidate[d] = _rng.NextDouble() * 10.0 - 5.0;

            double ei = ExpectedImprovement(candidate, bestY);
            if (ei > bestEI)
            {
                bestEI = ei;
                bestCandidate = candidate;
            }
        }

        return bestCandidate;
    }

    private static double GaussianCDF(double x)
    {
        double t = 1.0 / (1.0 + 0.2316419 * System.Math.Abs(x));
        double d = 0.3989422804014327;
        double p = d * System.Math.Exp(-x * x / 2.0) *
            (t * (0.3193815 + t * (-0.3565638 + t * (1.781478 + t * (-1.8212560 + t * 1.3302744)))));

        return x > 0.0 ? 1.0 - p : p;
    }

    private static double GaussianPDF(double x)
    {
        return 0.3989422804014327 * System.Math.Exp(-x * x / 2.0);
    }
}
