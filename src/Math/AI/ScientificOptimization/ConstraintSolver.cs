namespace MathVerse.Math.AI.ScientificOptimization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

/// <summary>
/// Solves constrained optimization problems using the augmented Lagrangian method,
/// which combines Lagrange multipliers with a penalty function to enforce constraints.
/// </summary>
public sealed class ConstraintSolver
{
    private readonly Random _rng;

    /// <summary>Initializes a new instance with the default random seed.</summary>
    public ConstraintSolver()
    {
        _rng = new Random(42);
    }

    /// <summary>Initializes a new instance with the specified random seed.</summary>
    public ConstraintSolver(int seed)
    {
        _rng = new Random(seed);
    }

    /// <summary>
    /// Solves a constrained optimization problem using the augmented Lagrangian method.
    /// Constraints are specified as equality functions that should evaluate to zero at feasibility.
    /// </summary>
    /// <param name="objective">The objective function to minimize.</param>
    /// <param name="constraints">Equality constraint functions g(x) = 0.</param>
    /// <param name="initial">Initial guess for the parameter vector.</param>
    /// <param name="iterations">Maximum number of outer iterations.</param>
    /// <param name="initialPenalty">Initial penalty parameter value.</param>
    /// <returns>A <see cref="ConstraintSolverResult"/> with the solution.</returns>
    public ConstraintSolverResult Solve(
        Func<double[], double> objective,
        Func<double[], bool>[] constraints,
        double[] initial,
        int iterations = 1000,
        double initialPenalty = 1.0)
    {
        Stopwatch sw = Stopwatch.StartNew();
        int n = initial.Length;
        int m = constraints.Length;

        double[] x = new double[n];
        Array.Copy(initial, x, System.Math.Min(n, initial.Length));

        double[] lambda = new double[m];
        double rho = initialPenalty;

        double stepSize = 0.01;
        int maxInnerIterations = 50;

        for (int outer = 0; outer < iterations; outer++)
        {
            for (int inner = 0; inner < maxInnerIterations; inner++)
            {
                double[] gradient = ComputeAugmentedLagrangianGradient(x, lambda, rho, objective, constraints);

                double normSq = 0.0;
                for (int i = 0; i < n; i++)
                {
                    normSq += gradient[i] * gradient[i];
                }
                if (System.Math.Sqrt(normSq) < 1e-10)
                    break;

                double[] xNew = new double[n];
                for (int i = 0; i < n; i++)
                {
                    xNew[i] = x[i] - stepSize * gradient[i];
                }
                x = xNew;
            }

            double[] violations = new double[m];
            for (int i = 0; i < m; i++)
            {
                violations[i] = ConstraintViolation(x, constraints[i]);
            }

            for (int i = 0; i < m; i++)
            {
                lambda[i] = lambda[i] + rho * violations[i];
            }

            rho *= 2.0;

            double maxViolation = 0.0;
            for (int i = 0; i < m; i++)
            {
                double absV = System.Math.Abs(violations[i]);
                if (absV > maxViolation)
                    maxViolation = absV;
            }
            if (maxViolation < 1e-6)
            {
                sw.Stop();
                return new ConstraintSolverResult
                {
                    Success = true,
                    BestParameters = x,
                    BestValue = objective(x),
                    LagrangeMultipliers = lambda,
                    ConstraintViolations = violations,
                    IterationsExecuted = outer + 1,
                    IsFeasible = true,
                    ElapsedTime = sw.Elapsed,
                    Metrics = ImmutableDictionary<string, double>.Empty
                        .Add("finalPenalty", rho)
                        .Add("maxViolation", maxViolation)
                };
            }
        }

        double[] finalViolations = new double[m];
        for (int i = 0; i < m; i++)
        {
            finalViolations[i] = ConstraintViolation(x, constraints[i]);
        }
        double finalMaxViolation = 0.0;
        for (int i = 0; i < m; i++)
        {
            double absV = System.Math.Abs(finalViolations[i]);
            if (absV > finalMaxViolation)
                finalMaxViolation = absV;
        }

        sw.Stop();
        return new ConstraintSolverResult
        {
            Success = finalMaxViolation < 1e-4,
            BestParameters = x,
            BestValue = objective(x),
            LagrangeMultipliers = lambda,
            ConstraintViolations = finalViolations,
            IterationsExecuted = iterations,
            IsFeasible = finalMaxViolation < 1e-4,
            ElapsedTime = sw.Elapsed,
            Metrics = ImmutableDictionary<string, double>.Empty
                .Add("finalPenalty", rho)
                .Add("maxViolation", finalMaxViolation)
        };
    }

    private double ConstraintViolation(double[] x, Func<double[], bool> constraint)
    {
        return constraint(x) ? 0.0 : 1.0;
    }

    private double[] ComputeAugmentedLagrangianGradient(
        double[] x,
        double[] lambda,
        double rho,
        Func<double[], double> objective,
        Func<double[], bool>[] constraints)
    {
        int n = x.Length;
        int m = constraints.Length;
        double[] gradient = new double[n];
        double h = 1e-7;

        double f0 = objective(x);
        for (int i = 0; i < n; i++)
        {
            double[] xPert = new double[n];
            Array.Copy(x, xPert, n);
            xPert[i] += h;
            gradient[i] = (objective(xPert) - f0) / h;
        }

        for (int c = 0; c < m; c++)
        {
            double violation = ConstraintViolation(x, constraints[c]);
            double penaltyContrib = 2.0 * rho * violation;
            double[] xPert = new double[n];
            for (int i = 0; i < n; i++)
            {
                Array.Copy(x, xPert, n);
                xPert[i] += h;
                double violPert = ConstraintViolation(xPert, constraints[c]);
                gradient[i] += penaltyContrib * (violPert - violation) / h;
                gradient[i] += lambda[c] * (violPert - violation) / h;
            }
        }

        return gradient;
    }
}
