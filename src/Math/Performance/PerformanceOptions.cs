namespace MathVerse.Math.Performance;

/// <summary>
/// Configuration options for the MathVerse performance infrastructure.
/// </summary>
public sealed class PerformanceOptions
{
    /// <summary>
    /// Gets the default performance options with sensible defaults.
    /// </summary>
    public static PerformanceOptions Default { get; } = new();

    /// <summary>
    /// Initializes a new instance of <see cref="PerformanceOptions"/> with default values.
    /// </summary>
    public PerformanceOptions()
    {
        InterningCapacity = 4096;
        EvaluationCacheCapacity = 1024;
        RewriteCacheCapacity = 1024;
        EnableIncrementalEvaluation = true;
        EnableParallelExecution = false;
        MaxDegreeOfParallelism = Environment.ProcessorCount;
        EnableDiagnostics = true;
        EnableOptimization = true;
        EnabledOptimizationStages = OptimizationStage.All;
    }

    /// <summary>
    /// Gets or sets the maximum number of interned expressions.
    /// </summary>
    public int InterningCapacity { get; set; }

    /// <summary>
    /// Gets or sets the capacity of the evaluation cache.
    /// </summary>
    public int EvaluationCacheCapacity { get; set; }

    /// <summary>
    /// Gets or sets the capacity of the rewrite cache.
    /// </summary>
    public int RewriteCacheCapacity { get; set; }

    /// <summary>
    /// Gets or sets whether incremental evaluation is enabled.
    /// </summary>
    public bool EnableIncrementalEvaluation { get; set; }

    /// <summary>
    /// Gets or sets whether parallel execution is enabled.
    /// </summary>
    public bool EnableParallelExecution { get; set; }

    /// <summary>
    /// Gets or sets the maximum degree of parallelism.
    /// A value of -1 uses <see cref="Environment.ProcessorCount"/>.
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; }

    /// <summary>
    /// Gets or sets whether performance diagnostics are enabled.
    /// </summary>
    public bool EnableDiagnostics { get; set; }

    /// <summary>
    /// Gets or sets whether the optimization pipeline is enabled.
    /// </summary>
    public bool EnableOptimization { get; set; }

    /// <summary>
    /// Gets or sets the optimization stages to enable.
    /// </summary>
    public OptimizationStage EnabledOptimizationStages { get; set; }
}
