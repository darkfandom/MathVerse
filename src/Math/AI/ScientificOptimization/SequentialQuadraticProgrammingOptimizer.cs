namespace MathVerse.Math.AI.ScientificOptimization;
using MathVerse.Math.AI.Optimization;
using System;
using System.Collections.Immutable;
using System.Diagnostics;

/// <summary>
/// Sequential Quadratic Programming optimizer with BFGS Hessian approximation.
/// Solves constrained nonlinear optimization by iteratively solving quadratic subproblems
/// with linearized constraints and performing line search with a merit function.
/// </summary>
public sealed class SequentialQuadraticProgrammingOptimizer
{
    /// <summary>Initializes a new instance.</summary>
    public SequentialQuadraticProgrammingOptimizer()
    {
    }

    /// <summary>
    /// Optimizes the objective subject to equality and inequality constraints using SQP.
    /// </summary>
    /// <param name="objective">The objective function to minimize.</param>
    /// <param name="equalityConstraints">Equality constraint functions h_i(x) = 0.</param>
    /// <param name="inequalityConstraints">Inequality constraint functions g_i(x) &lt;= 0.</param>
    /// <param name="initial">Initial guess for the parameter vector.</param>
    /// <returns>An <see cref="OptimizationResult"/> with the solution.</returns>
    public OptimizationResult Optimize(
        Func<double[], double> objective,
        Func<double[], double>[] equalityConstraints,
        Func<double[], double>[] inequalityConstraints,
        double[] initial)
    {
        Stopwatch sw = Stopwatch.StartNew();
        int n = initial.Length;
        int me = equalityConstraints.Length;
        int mi = inequalityConstraints.Length;
        int maxIterations = 200;
        double convergenceTol = 1e-6;
        double meritPenalty = 10.0;

        double[] x = new double[n];
        Array.Copy(initial, x, System.Math.Min(n, initial.Length));

        double[][] hessian = new double[n][];
        for (int i = 0; i < n; i++)
        {
            hessian[i] = new double[n];
            hessian[i][i] = 1.0;
        }

        double bestMerit = double.MaxValue;
        double[] bestX = new double[n];
        Array.Copy(x, bestX, n);

        for (int iter = 0; iter < maxIterations; iter++)
        {
            double[] gradF = ComputeGradient(objective, x);

            double[] hVals = new double[me];
            double[][] jacH = new double[me][];
            for (int i = 0; i < me; i++)
            {
                hVals[i] = equalityConstraints[i](x);
                jacH[i] = ComputeGradient(equalityConstraints[i], x);
            }

            double[] gVals = new double[mi];
            double[][] jacG = new double[mi][];
            for (int i = 0; i < mi; i++)
            {
                gVals[i] = inequalityConstraints[i](x);
                jacG[i] = ComputeGradient(inequalityConstraints[i], x);
            }

            double[] searchDir = SolveQPSubproblem(
                gradF, hessian, jacH, hVals, jacG, gVals, n, me, mi);

            double dirNorm = 0.0;
            for (int i = 0; i < n; i++)
            {
                dirNorm += searchDir[i] * searchDir[i];
            }
            dirNorm = System.Math.Sqrt(dirNorm);

            if (dirNorm < convergenceTol)
            {
                double maxViolation = ComputeMaxViolation(equalityConstraints, inequalityConstraints, x);
                bool feasible = maxViolation < convergenceTol;
                sw.Stop();
                return new OptimizationResult
                {
                    Success = feasible,
                    BestParameters = bestX,
                    BestValue = objective(bestX),
                    IterationsExecuted = iter + 1,
                    Converged = true,
                    ElapsedTime = sw.Elapsed,
                    Metrics = ImmutableDictionary<string, double>.Empty
                        .Add("constraintViolation", maxViolation)
                };
            }

            Func<double[], double> meritFn = z =>
                MeritFunction(objective, equalityConstraints, inequalityConstraints, z, meritPenalty);
            double alpha = LineSearch(x, searchDir, meritFn);

            double[] xNew = new double[n];
            for (int i = 0; i < n; i++)
            {
                xNew[i] = x[i] + alpha * searchDir[i];
            }

            double[] s = new double[n];
            double[] y = new double[n];
            for (int i = 0; i < n; i++)
            {
                s[i] = xNew[i] - x[i];
            }

            double[] gradFNew = ComputeGradient(objective, xNew);
            for (int i = 0; i < n; i++)
            {
                y[i] = gradFNew[i] - gradF[i];
            }

            BFGSUpdate(hessian, s, y, n);

            double newMerit = meritFn(xNew);
            if (newMerit < bestMerit)
            {
                bestMerit = newMerit;
                Array.Copy(xNew, bestX, n);
            }

            x = xNew;
        }

        double finalViolation = ComputeMaxViolation(equalityConstraints, inequalityConstraints, x);
        sw.Stop();
        return new OptimizationResult
        {
            Success = finalViolation < convergenceTol,
            BestParameters = bestX,
            BestValue = objective(bestX),
            IterationsExecuted = maxIterations,
            Converged = finalViolation < convergenceTol,
            ElapsedTime = sw.Elapsed,
            Metrics = ImmutableDictionary<string, double>.Empty
                .Add("constraintViolation", finalViolation)
        };
    }

    private double MeritFunction(
        Func<double[], double> objective,
        Func<double[], double>[] eqConstraints,
        Func<double[], double>[] ineqConstraints,
        double[] x,
        double penalty)
    {
        double f = objective(x);
        double violation = 0.0;
        for (int i = 0; i < eqConstraints.Length; i++)
        {
            double h = eqConstraints[i](x);
            violation += h * h;
        }
        for (int i = 0; i < ineqConstraints.Length; i++)
        {
            double g = ineqConstraints[i](x);
            if (g > 0.0)
            {
                violation += g * g;
            }
        }
        return f + penalty * violation;
    }

    private double[] SolveQPSubproblem(
        double[] gradF, double[][] hessian,
        double[][] jacH, double[] hVals,
        double[][] jacG, double[] gVals,
        int n, int me, int mi)
    {
        int totalConstraints = me + mi;
        int systemSize = n + totalConstraints;
        double[][] system = new double[systemSize][];
        double[] rhs = new double[systemSize];

        for (int i = 0; i < systemSize; i++)
        {
            system[i] = new double[systemSize];
        }

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                system[i][j] = hessian[i][j];
            }
        }

        for (int i = 0; i < me; i++)
        {
            for (int j = 0; j < n; j++)
            {
                system[i + n][j] = jacH[i][j];
                system[j][i + n] = jacH[i][j];
            }
            rhs[i + n] = -hVals[i];
        }

        for (int i = 0; i < mi; i++)
        {
            for (int j = 0; j < n; j++)
            {
                system[me + i + n][j] = jacG[i][j];
                system[j][me + i + n] = jacG[i][j];
            }
            rhs[me + i + n] = -gVals[i];
        }

        for (int i = 0; i < n; i++)
        {
            rhs[i] = -gradF[i];
        }

        return SolveLinearSystem(system, rhs, systemSize);
    }

    private double[] SolveLinearSystem(double[][] A, double[] b, int size)
    {
        double[][] aug = new double[size][];
        double[] rhs = new double[size];
        for (int i = 0; i < size; i++)
        {
            aug[i] = new double[size];
            Array.Copy(A[i], aug[i], size);
            rhs[i] = b[i];
        }

        for (int col = 0; col < size; col++)
        {
            int maxRow = col;
            double maxVal = System.Math.Abs(aug[col][col]);
            for (int row = col + 1; row < size; row++)
            {
                double absVal = System.Math.Abs(aug[row][col]);
                if (absVal > maxVal)
                {
                    maxVal = absVal;
                    maxRow = row;
                }
            }

            if (maxRow != col)
            {
                double[] tempRow = aug[col];
                aug[col] = aug[maxRow];
                aug[maxRow] = tempRow;
                double tempRhs = rhs[col];
                rhs[col] = rhs[maxRow];
                rhs[maxRow] = tempRhs;
            }

            double pivot = aug[col][col];
            if (System.Math.Abs(pivot) < 1e-14)
                pivot = 1e-10;

            for (int row = col + 1; row < size; row++)
            {
                double factor = aug[row][col] / pivot;
                for (int k = col; k < size; k++)
                {
                    aug[row][k] -= factor * aug[col][k];
                }
                rhs[row] -= factor * rhs[col];
            }
        }

        double[] solution = new double[size];
        for (int i = size - 1; i >= 0; i--)
        {
            double sum = rhs[i];
            for (int j = i + 1; j < size; j++)
            {
                sum -= aug[i][j] * solution[j];
            }
            double diag = aug[i][i];
            if (System.Math.Abs(diag) < 1e-14)
                diag = 1e-10;
            solution[i] = sum / diag;
        }

        return solution;
    }

    private double LineSearch(double[] x, double[] direction, Func<double[], double> meritFunction)
    {
        double alpha = 1.0;
        double c1 = 1e-4;
        double rhoFactor = 0.5;
        int n = x.Length;
        double currentMerit = meritFunction(x);

        double[] gradM = ComputeGradient(meritFunction, x);
        double directionalDeriv = 0.0;
        for (int i = 0; i < n; i++)
        {
            directionalDeriv += gradM[i] * direction[i];
        }

        for (int i = 0; i < 20; i++)
        {
            double[] xNew = new double[n];
            for (int j = 0; j < n; j++)
            {
                xNew[j] = x[j] + alpha * direction[j];
            }

            double newMerit = meritFunction(xNew);
            if (newMerit <= currentMerit + c1 * alpha * directionalDeriv)
                break;

            alpha *= rhoFactor;
        }

        return alpha;
    }

    private void BFGSUpdate(double[][] hessian, double[] s, double[] y, int n)
    {
        double sy = 0.0;
        double yy = 0.0;
        for (int i = 0; i < n; i++)
        {
            sy += s[i] * y[i];
            yy += y[i] * y[i];
        }

        if (System.Math.Abs(sy) < 1e-14)
            return;

        double[] Hs = new double[n];
        for (int i = 0; i < n; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < n; j++)
            {
                sum += hessian[i][j] * s[j];
            }
            Hs[i] = sum;
        }

        double sHs = 0.0;
        for (int i = 0; i < n; i++)
        {
            sHs += s[i] * Hs[i];
        }

        double rho1 = 1.0 / sy;

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                hessian[i][j] = hessian[i][j]
                    - rho1 * (Hs[i] * s[j] + s[i] * Hs[j])
                    + rho1 * rho1 * (1.0 + sHs * rho1) * s[i] * s[j];
            }
        }
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

    private double ComputeMaxViolation(
        Func<double[], double>[] eqConstraints,
        Func<double[], double>[] ineqConstraints,
        double[] x)
    {
        double maxV = 0.0;
        for (int i = 0; i < eqConstraints.Length; i++)
        {
            double v = System.Math.Abs(eqConstraints[i](x));
            if (v > maxV) maxV = v;
        }
        for (int i = 0; i < ineqConstraints.Length; i++)
        {
            double g = ineqConstraints[i](x);
            if (g > maxV) maxV = g;
        }
        return maxV;
    }
}
