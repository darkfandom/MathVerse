namespace MathVerse.Math.Numerics.Optimization;

using System.Collections.Immutable;
using MathVerse.Math.Numerics.LinearAlgebra;

public sealed class NelderMead : IOptimizer
{
    private readonly double _alpha;
    private readonly double _gamma;
    private readonly double _rho;
    private readonly double _sigma;

    public NelderMead(double alpha = 1.0, double gamma = 2.0, double rho = 0.5, double sigma = 0.5)
    {
        _alpha = alpha;
        _gamma = gamma;
        _rho = rho;
        _sigma = sigma;
    }

public OptimizationResult Optimize(Func<Vector, double> f, Vector initialGuess, OptimizationOptions? options = null)
    {
        options ??= OptimizationOptions.Default;
        int n = initialGuess.Size;
        int iterations = 0;
        int functionEvaluations = 0;
        var history = ImmutableArray.CreateBuilder<double>();

        var simplex = new Vector[n + 1];
        var fValues = new double[n + 1];

        simplex[0] = initialGuess;
        fValues[0] = f(initialGuess);
        functionEvaluations = 1;

        double step = options.StepSize;
        for (int i = 1; i <= n; i++)
        {
            var perturbed = initialGuess.ToArray();
            perturbed[i - 1] += step;
            simplex[i] = new Vector(perturbed);
            fValues[i] = f(simplex[i]);
            functionEvaluations++;
        }

        if (options.TrackHistory)
            history.Add(fValues.Min());

        for (int iter = 0; iter < options.MaxIterations; iter++)
        {
            var sortedIndices = fValues.Select((v, i) => new { Value = v, Index = i })
                .OrderBy(x => x.Value).Select(x => x.Index).ToArray();

            double fBest = fValues[sortedIndices[0]];
            double fSecondWorst = fValues[sortedIndices[n - 1]];
            double fWorst = fValues[sortedIndices[n]];
            Vector best = simplex[sortedIndices[0]];
            Vector worst = simplex[sortedIndices[n]];
            Vector secondWorst = simplex[sortedIndices[n - 1]];

            if (options.TrackHistory)
                history.Add(fBest);

            double centroidSize = SimplexSize(simplex);
            if (centroidSize < options.Tolerance || System.Math.Abs(fWorst - fBest) < options.Tolerance)
                return OptimizationResult.ConvergedResult(simplex[sortedIndices[0]], fBest, iterations, functionEvaluations, gradientEvaluations: 0, history.ToImmutable());

            Vector centroid = ComputeCentroid(simplex, sortedIndices, n);
            Vector reflected = Reflect(centroid, worst);
            double fReflected = f(reflected);
            functionEvaluations++;

            if (fReflected < fBest)
            {
                Vector expanded = Expand(centroid, reflected);
                double fExpanded = f(expanded);
                functionEvaluations++;
                if (fExpanded < fReflected)
                {
                    simplex[sortedIndices[n]] = expanded;
                    fValues[sortedIndices[n]] = fExpanded;
                }
                else
                {
                    simplex[sortedIndices[n]] = reflected;
                    fValues[sortedIndices[n]] = fReflected;
                }
            }
            else if (fReflected < fValues[sortedIndices[n - 1]])
            {
                simplex[sortedIndices[n]] = reflected;
                fValues[sortedIndices[n]] = fReflected;
            }
            else
            {
                Vector contracted;
                double fContracted;
                if (fReflected < fWorst)
                {
                    contracted = OutsideContract(centroid, reflected);
                    fContracted = f(contracted);
                    functionEvaluations++;
                    if (fContracted <= fReflected)
                    {
                        simplex[sortedIndices[n]] = contracted;
                        fValues[sortedIndices[n]] = fContracted;
                    }
                    else
                    {
                        ShrinkSimplex(simplex, fValues, f, best);
                        for (int i = 1; i <= n; i++)
                            fValues[i] = f(simplex[i]);
                        functionEvaluations += n;
                    }
                }
                else
                {
                    contracted = InsideContract(centroid, worst);
                    fContracted = f(contracted);
                    functionEvaluations++;
                    if (fContracted < fWorst)
                    {
                        simplex[sortedIndices[n]] = contracted;
                        fValues[sortedIndices[n]] = fContracted;
                    }
                    else
                    {
                        ShrinkSimplex(simplex, fValues, f, best);
                        for (int i = 1; i <= n; i++)
                            fValues[i] = f(simplex[i]);
                        functionEvaluations += n;
                    }
                }
            }

            iterations++;
            if (HasNaN(simplex, fValues))
                return OptimizationResult.NaNDetectedResult(simplex[sortedIndices[0]], fValues[sortedIndices[0]], iterations, functionEvaluations, 0, history.ToImmutable());
        }

        var finalSorted = fValues.Select((v, i) => new { Value = v, Index = i })
            .OrderBy(x => x.Value).Select(x => x.Index).ToArray();
        return OptimizationResult.MaxIterationsResult(simplex[finalSorted[0]], fValues[finalSorted[0]], iterations, functionEvaluations, 0, history.ToImmutable());
    }

    public OptimizationResult OptimizeConstrained(Func<Vector, double> f, Vector initialGuess, ImmutableArray<Constraint> constraints, OptimizationOptions? options = null)
    {
        var penaltyFunc = CreatePenaltyFunction(f, constraints, 1e4);
        return Optimize(penaltyFunc, initialGuess, options);
    }

    private static Vector ComputeCentroid(Vector[] simplex, int[] sortedIndices, int n)
    {
        var centroid = new double[simplex[0].Size];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < centroid.Length; j++)
                centroid[j] += simplex[sortedIndices[i]][j];
        }
        for (int j = 0; j < centroid.Length; j++)
            centroid[j] /= n;
        return new Vector(centroid);
    }

    private static Vector Reflect(Vector centroid, Vector worst)
    {
        return centroid.Add(centroid.Subtract(worst).Scale(1.0));
    }

    private static Vector Expand(Vector centroid, Vector reflected)
    {
        return centroid.Add(centroid.Subtract(reflected).Scale(2.0));
    }

    private static Vector OutsideContract(Vector centroid, Vector reflected)
    {
        return centroid.Add(centroid.Subtract(reflected).Scale(0.5));
    }

    private static Vector InsideContract(Vector centroid, Vector worst)
    {
        return centroid.Subtract(centroid.Subtract(worst).Scale(0.5));
    }

    private static void ShrinkSimplex(Vector[] simplex, double[] fValues, Func<Vector, double> f, Vector best)
    {
        for (int i = 1; i < simplex.Length; i++)
        {
            simplex[i] = best.Add(simplex[i].Subtract(best).Scale(0.5));
            fValues[i] = f(simplex[i]);
        }
    }

    private static double SimplexSize(Vector[] simplex)
    {
        double maxDist = 0;
        for (int i = 0; i < simplex.Length; i++)
        {
            for (int j = i + 1; j < simplex.Length; j++)
            {
                double dist = simplex[i].Subtract(simplex[j]).Norm();
                if (dist > maxDist) maxDist = dist;
            }
        }
        return maxDist;
    }

    private static Func<Vector, double> CreatePenaltyFunction(Func<Vector, double> f, ImmutableArray<Constraint> constraints, double penalty)
    {
        return x =>
        {
            double penaltyTerm = 0.0;
            foreach (var c in constraints)
            {
                double v = c.Evaluate(x);
                penaltyTerm += c.Type switch
                {
                    ConstraintType.Equality => v * v,
                    ConstraintType.InequalityLess => System.Math.Max(0, v) * System.Math.Max(0, v),
                    ConstraintType.InequalityGreater => System.Math.Max(0, -v) * System.Math.Max(0, -v),
                    _ => 0
                };
            }
            return f(x) + penalty * penaltyTerm;
        };
    }

    private static bool HasNaN(Vector[] simplex, double[] fValues)
    {
        foreach (var v in simplex)
            foreach (double val in v.Values)
                if (double.IsNaN(val) || double.IsInfinity(val))
                    return true;
        foreach (double fv in fValues)
            if (double.IsNaN(fv) || double.IsInfinity(fv))
                return true;
        return false;
    }
}