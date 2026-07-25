namespace MathVerse.Math.Performance;

/// <summary>
/// Builder for constructing <see cref="PerformanceOptions"/> with a fluent API.
/// </summary>
public sealed class PerformanceConfiguration
{
    private readonly PerformanceOptions _options = new();

    /// <summary>
    /// Resets the configuration to default values and returns this instance.
    /// </summary>
    /// <returns>This <see cref="PerformanceConfiguration"/> for chaining.</returns>
    public PerformanceConfiguration UseDefaults()
    {
        var defaults = new PerformanceOptions();
        _options.InterningCapacity = defaults.InterningCapacity;
        _options.EvaluationCacheCapacity = defaults.EvaluationCacheCapacity;
        _options.RewriteCacheCapacity = defaults.RewriteCacheCapacity;
        _options.EnableIncrementalEvaluation = defaults.EnableIncrementalEvaluation;
        _options.EnableParallelExecution = defaults.EnableParallelExecution;
        _options.MaxDegreeOfParallelism = defaults.MaxDegreeOfParallelism;
        _options.EnableDiagnostics = defaults.EnableDiagnostics;
        _options.EnableOptimization = defaults.EnableOptimization;
        _options.EnabledOptimizationStages = defaults.EnabledOptimizationStages;
        return this;
    }

    /// <summary>
    /// Sets the interning capacity.
    /// </summary>
    /// <param name="capacity">The maximum number of interned expressions.</param>
    /// <returns>This <see cref="PerformanceConfiguration"/> for chaining.</returns>
    public PerformanceConfiguration SetInterningCapacity(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Must be positive.");

        _options.InterningCapacity = capacity;
        return this;
    }

    /// <summary>
    /// Sets the evaluation and rewrite cache capacities.
    /// </summary>
    /// <param name="capacity">The cache capacity.</param>
    /// <returns>This <see cref="PerformanceConfiguration"/> for chaining.</returns>
    public PerformanceConfiguration SetCacheCapacity(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Must be positive.");

        _options.EvaluationCacheCapacity = capacity;
        _options.RewriteCacheCapacity = capacity;
        return this;
    }

    /// <summary>
    /// Enables parallel execution with the specified maximum degree of parallelism.
    /// </summary>
    /// <param name="maxDegree">
    /// The maximum degree of parallelism. Use -1 for <see cref="Environment.ProcessorCount"/>.
    /// </param>
    /// <returns>This <see cref="PerformanceConfiguration"/> for chaining.</returns>
    public PerformanceConfiguration EnableParallelism(int maxDegree = -1)
    {
        _options.EnableParallelExecution = true;
        _options.MaxDegreeOfParallelism = maxDegree > 0 ? maxDegree : Environment.ProcessorCount;
        return this;
    }

    /// <summary>
    /// Enables the optimization pipeline with the specified stages.
    /// </summary>
    /// <param name="stages">The stages to enable. Defaults to <see cref="OptimizationStage.All"/>.</param>
    /// <returns>This <see cref="PerformanceConfiguration"/> for chaining.</returns>
    public PerformanceConfiguration EnableOptimizationPipeline(OptimizationStage stages = OptimizationStage.All)
    {
        _options.EnableOptimization = true;
        _options.EnabledOptimizationStages = stages;
        return this;
    }

    /// <summary>
    /// Disables performance diagnostics.
    /// </summary>
    /// <returns>This <see cref="PerformanceConfiguration"/> for chaining.</returns>
    public PerformanceConfiguration DisableDiagnostics()
    {
        _options.EnableDiagnostics = false;
        return this;
    }

    /// <summary>
    /// Builds and returns the configured <see cref="PerformanceOptions"/>.
    /// </summary>
    /// <returns>An immutable snapshot of the configured options.</returns>
    public PerformanceOptions Build() =>
        new()
        {
            InterningCapacity = _options.InterningCapacity,
            EvaluationCacheCapacity = _options.EvaluationCacheCapacity,
            RewriteCacheCapacity = _options.RewriteCacheCapacity,
            EnableIncrementalEvaluation = _options.EnableIncrementalEvaluation,
            EnableParallelExecution = _options.EnableParallelExecution,
            MaxDegreeOfParallelism = _options.MaxDegreeOfParallelism,
            EnableDiagnostics = _options.EnableDiagnostics,
            EnableOptimization = _options.EnableOptimization,
            EnabledOptimizationStages = _options.EnabledOptimizationStages
        };
}
