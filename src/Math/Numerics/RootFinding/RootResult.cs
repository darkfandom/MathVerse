namespace MathVerse.Math.Numerics.RootFinding;

using System.Collections.Immutable;

public enum RootStatus
{
    Converged,
    MaxIterations,
    NoBracket,
    Singular,
    Diverged
}

public sealed record RootResult
{
    public double Root { get; init; }
    public bool Converged { get; init; }
    public int Iterations { get; init; }
    public double FinalResidual { get; init; }
    public ImmutableArray<double> History { get; init; }
    public RootStatus Status { get; init; }

    public RootResult(double root, bool converged, int iterations, double finalResidual, ImmutableArray<double> history, RootStatus status)
    {
        Root = root;
        Converged = converged;
        Iterations = iterations;
        FinalResidual = finalResidual;
        History = history;
        Status = status;
    }
}