namespace MathVerse.Math.AI.Core;

using System.Collections.Immutable;

/// <summary>Configuration options for the AI engine.</summary>
public sealed class AIOptions
{
    /// <summary>Maximum number of training epochs.</summary>
    public int MaxEpochs { get; init; } = 1000;

    /// <summary>Learning rate for gradient-based optimization.</summary>
    public double LearningRate { get; init; } = 0.01;

    /// <summary>Tolerance below which training is considered converged.</summary>
    public double ConvergenceTolerance { get; init; } = 1e-6;

    /// <summary>Seed for reproducible pseudo-random number generation.</summary>
    public int RandomSeed { get; init; } = 42;

    /// <summary>Maximum degree of parallelism for concurrent operations.</summary>
    public int MaxConcurrency { get; init; } = System.Environment.ProcessorCount;

    /// <summary>Whether to cache intermediate computation results.</summary>
    public bool EnableCaching { get; init; } = true;

    /// <summary>Whether to collect detailed diagnostic information during execution.</summary>
    public bool EnableDiagnostics { get; init; } = true;

    /// <summary>Maximum number of entries retained in the computation cache.</summary>
    public int MaxCacheSize { get; init; } = 1024;

    /// <summary>Arbitrary key-value metadata attached to this options instance.</summary>
    public ImmutableDictionary<string, string> Metadata { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>Returns a default <see cref="AIOptions"/> instance.</summary>
    public static AIOptions Default => new();

    /// <summary>Creates a shallow copy with overridden values.</summary>
    /// <param name="maxEpochs">New maximum epoch count, or <c>null</c> to keep current.</param>
    /// <param name="learningRate">New learning rate, or <c>null</c> to keep current.</param>
    /// <param name="convergenceTolerance">New tolerance, or <c>null</c> to keep current.</param>
    /// <param name="randomSeed">New seed, or <c>null</c> to keep current.</param>
    /// <returns>A new <see cref="AIOptions"/> instance with the specified overrides.</returns>
    public AIOptions WithOverrides(
        int? maxEpochs = null,
        double? learningRate = null,
        double? convergenceTolerance = null,
        int? randomSeed = null) =>
        new()
        {
            MaxEpochs = maxEpochs ?? MaxEpochs,
            LearningRate = learningRate ?? LearningRate,
            ConvergenceTolerance = convergenceTolerance ?? ConvergenceTolerance,
            RandomSeed = randomSeed ?? RandomSeed,
            MaxConcurrency = MaxConcurrency,
            EnableCaching = EnableCaching,
            EnableDiagnostics = EnableDiagnostics,
            MaxCacheSize = MaxCacheSize,
            Metadata = Metadata,
        };
}
