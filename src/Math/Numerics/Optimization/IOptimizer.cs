namespace MathVerse.Math.Numerics.Optimization;

using System.Collections.Immutable;
using MathVerse.Math.Numerics.LinearAlgebra;

public interface IOptimizer
{
    OptimizationResult Optimize(Func<Vector, double> f, Vector initialGuess, OptimizationOptions? options = null);
    OptimizationResult OptimizeConstrained(Func<Vector, double> f, Vector initialGuess, ImmutableArray<Constraint> constraints, OptimizationOptions? options = null);
}