namespace MathVerse.Math.AI.ScientificOptimization;
using MathVerse.Math.AI.Optimization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

/// <summary>
/// Optimizer using the exterior penalty function method with increasing penalty parameter.
/// Converts a constrained problem into a sequence of unconstrained problems by adding
/// a penalty term P(x) = f(x) + rho * sum(max(0, g_i(x))^2).
/// </summary>
public sealed class PenaltyFunctionOptimizer
{
    private readonly Random _rng;

    /// <summary>Initializes a new instance with the default random seed.</summary>
    public PenaltyFunctionOptimizer()
    {
        _rng = new Random(42);
    }

    /// <summary>Initializes a new instance with the specified random seed.</summary>
    public PenaltyFunctionOptimizer(int seed)
    {
        _rng = new Random(seed);
    }

    /// <summary>
    /// Optimizes the objective subject to inequality constraints using the exterior penalty method.
    /// Each constraint g_i(x) should be satisfied when g_i(x) &lt;= 0.
    /// </summary>
    /// <param name="objective">The objective function to minimize.</param>
    /// <param name="inequalityConstraints">Inequality constraint functions g_i(x) &lt;= 0.</param>
    /// <param name="initial">Initial guess for the parameter vector.</param>
    /// <returns>An <see cref="OptimizationResult"/> with the solution.</returns>
    public OptimizationResult Optimize(
        Func<double[], double> objective,
        Func<double[], double>[] inequalityConstraints,
        double[] initial)
    {
        Stopwatch sw = Stopwatch.StartNew();
        int n = initial.Length;
        int outerIterations = 20;
        int innerIterations = 200;
        double rho = 1.0;
        double rhoFactor = 10.0;
        double stepSize = 0.01;
        double convergenceTolerance = 1e-8;

        double[] x = new double[n];
        Array.Copy(initial, x, System.Math.Min(n, initial.Length));

        double bestValue = double.MaxValue;
        double[] bestX = new double[n];
        Array.Copy(x, bestX, n);

        for (int outer = 0; outer < outerIterations; outer++)
        {
            for (int inner = 0; inner < innerIterations; inner++)
            {
                Func<double[], double> penalized = CreatePenalizedObjective(objective, inequalityConstraints, rho);
                double[] gradient = ComputeGradient(penalized, x);

                double gradNorm = 0.0;
                for (int i = 0; i < n; i++)
                {
                    gradNorm += gradient[i] * gradient[i];
                }
                gradNorm = System.Math.Sqrt(gradNorm);

                if (gradNorm < convergenceTolerance)
                    break;

                for (int i = 0; i < n; i++)
                {
                    x[i] -= stepSize * gradient[i];
                }
            }

            double currentPenalty = PenalizedObjective(objective, inequalityConstraints, x, rho);
            if (currentPenalty < bestValue)
            {
                bestValue = currentPenalty;
                Array.Copy(x, bestX, n);
            }

            rho *= rhoFactor;
        }

        double finalObjective = objective(bestX);
        double totalViolation = ComputeTotalViolation(inequalityConstraints, bestX);
        bool converged = totalViolation < 1e-4;

        sw.Stop();
        return new OptimizationResult
        {
            Success = converged,
            BestParameters = bestX,
            BestValue = finalObjective,
            IterationsExecuted = outerIterations,
            Converged = converged,
            ElapsedTime = sw.Elapsed,
            Metrics = ImmutableDictionary<string, double>.Empty
                .Add("totalViolation", totalViolation)
                .Add("finalPenalty", rho)
        };
    }

    private double PenalizedObjective(
        Func<double[], double> objective,
        Func<double[], double>[] constraints,
        double[] x,
        double rho)
    {
        double f = objective(x);
        double penalty = 0.0;
        for (int i = 0; i < constraints.Length; i++)
        {
            double g = constraints[i](x);
            if (g > 0.0)
            {
                penalty += g * g;
            }
        }
        return f + rho * penalty;
    }

    private Func<double[], double> CreatePenalizedObjective(
        Func<double[], double> objective,
        Func<double[], double>[] constraints,
        double rho)
    {
        return x => PenalizedObjective(objective, constraints, x, rho);
    }

    private double[] ComputeGradient(Func<double[], double> f, double[] x)
    {
        int n = x.Length;
        double[] gradient = new double[n];
        double h = 1e-7;
        double f0 = f(x);

        for (int i = 0; i < n; i++)
        {
            double[] xPert = new double[n];
            Array.Copy(x, xPert, n);
            xPert[i] += h;
            gradient[i] = (f(xPert) - f0) / h;
        }
        return gradient;
    }

    private double ComputeTotalViolation(Func<double[], double>[] constraints, double[] x)
    {
        double total = 0.0;
        for (int i = 0; i < constraints.Length; i++)
        {
            double g = constraints[i](x);
            if (g > 0.0)
            {
                total += g;
            }
        }
        return total;
    }
}
