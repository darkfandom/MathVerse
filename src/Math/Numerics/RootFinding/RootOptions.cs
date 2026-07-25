namespace MathVerse.Math.Numerics.RootFinding;

using System.Collections.Immutable;

public sealed record RootOptions
{
    public double Tolerance { get; init; } = 1e-12;
    public int MaxIterations { get; init; } = 100;
    public bool TrackHistory { get; init; } = false;
    public bool RequireBracket { get; init; } = false;

    public static RootOptions Default { get; } = new();
}