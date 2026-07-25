namespace MathVerse.Math.AI.ScientificOptimization;
using MathVerse.Math.AI.Optimization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

/// <summary>
/// Optimizer using the barrier (interior point) function method.
/// Converts constrained optimization into a sequence of unconstrained problems
/// using B(x) = f(x) - mu * sum(ln(-g_i(x))) where g_i(x) &lt; 0 is the feasible region.
/// </summary>
public sealed class InteriorPointOptimizer
{
    private readonly Random _rng;

    /// <summary>Initializes a new instance with the default random seed.</summary>
    public InteriorPointOptimizer()
    {
        _rng = new Random(42);
    }

    /// <summary>Initializes a new instance with the specified random seed.</summary>
    public InteriorPointOptimizer(int seed)
    {
        _rng = new Random(seed);
    }

    /// <summary>
    /// Optimizes the objective subject to inequality constraints using the barrier method.
    /// Each constraint g_i(x) should be negative in the feasible region.
    /// </summary>
    /// <param name="objective">The objective function to minimize.</param>
    /// <param name="inequalityConstraints">Inequality constraint functions where g_i(x) &lt; 0 is feasible.</param>
    /// <param name="initial">Initial guess strictly inside the feasible region.</param>
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
        double mu = 10.0;
        double muFactor = 0.2;
        double stepSize = 0.01;
        double convergenceTolerance = 1e-8;
        double minMu = 1e-10;

        double[] x = new double[n];
        Array.Copy(initial, x, System.Math.Min(n, initial.Length));

        for (int i = 0; i < inequalityConstraints.Length; i++)
        {
            double g = inequalityConstraints[i](x);
            if (g >= 0.0)
            {
                ShiftToFeasible(x, inequalityConstraints, i);
            }
        }

        double bestValue = double.MaxValue;
        double[] bestX = new double[n];
        Array.Copy(x, bestX, n);

        for (int outer = 0; outer < outerIterations; outer++)
        {
            if (mu < minMu)
                break;

            for (int inner = 0; inner < innerIterations; inner++)
            {
                Func<double[], double> barrier = CreateBarrierObjective(objective, inequalityConstraints, mu);
                double[] gradient = ComputeGradient(barrier, x);

                double gradNorm = 0.0;
                for (int i = 0; i < n; i++)
                {
                    gradNorm += gradient[i] * gradient[i];
                }
                gradNorm = System.Math.Sqrt(gradNorm);

                if (gradNorm < convergenceTolerance)
                    break;

                double adaptiveStep = stepSize;
                double[] xNew = new double[n];
                bool feasible = true;
                for (int i = 0; i < n; i++)
                {
                    xNew[i] = x[i] - adaptiveStep * gradient[i];
                }

                for (int c = 0; c < inequalityConstraints.Length; c++)
                {
                    if (inequalityConstraints[c](xNew) >= 0.0)
                    {
                        feasible = false;
                        break;
                    }
                }

                if (feasible)
                {
                    x = xNew;
                }
                else
                {
                    adaptiveStep *= 0.5;
                    for (int attempt = 0; attempt < 20; attempt++)
                    {
                        feasible = true;
                        for (int i = 0; i < n; i++)
                        {
                            xNew[i] = x[i] - adaptiveStep * gradient[i];
                        }
                        for (int c = 0; c < inequalityConstraints.Length; c++)
                        {
                            if (inequalityConstraints[c](xNew) >= 0.0)
                            {
                                feasible = false;
                                break;
                            }
                        }
                        if (feasible)
                        {
                            x = xNew;
                            break;
                        }
                        adaptiveStep *= 0.5;
                    }
                }
            }

            double currentObj = objective(x);
            if (currentObj < bestValue)
            {
                bestValue = currentObj;
                Array.Copy(x, bestX, n);
            }

            mu *= muFactor;
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
                .Add("finalMu", mu)
        };
    }

    private double BarrierObjective(
        Func<double[], double> objective,
        Func<double[], double>[] constraints,
        double[] x,
        double mu)
    {
        double f = objective(x);
        double barrier = 0.0;
        for (int i = 0; i < constraints.Length; i++)
        {
            double g = constraints[i](x);
            if (g >= 0.0)
            {
                g = -1e-10;
            }
            barrier += System.Math.Log(-g);
        }
        return f - mu * barrier;
    }

    private Func<double[], double> CreateBarrierObjective(
        Func<double[], double> objective,
        Func<double[], double>[] constraints,
        double mu)
    {
        return x => BarrierObjective(objective, constraints, x, mu);
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

    private void ShiftToFeasible(double[] x, Func<double[], double>[] constraints, int violatingIndex)
    {
        double h = 1e-5;
        int n = x.Length;
        double[] gradient = new double[n];
        double g0 = constraints[violatingIndex](x);

        for (int i = 0; i < n; i++)
        {
            double[] xPert = new double[n];
            Array.Copy(x, xPert, n);
            xPert[i] += h;
            gradient[i] = (constraints[violatingIndex](xPert) - g0) / h;
        }

        double gradNormSq = 0.0;
        for (int i = 0; i < n; i++)
        {
            gradNormSq += gradient[i] * gradient[i];
        }

        if (gradNormSq > 1e-14)
        {
            double shift = (g0 + 0.1) / gradNormSq;
            for (int i = 0; i < n; i++)
            {
                x[i] -= shift * gradient[i];
            }
        }
    }
}
