namespace MathVerse.Math.Compiler.Runtime;

using System;
using System.Collections.Concurrent;

/// <summary>
/// Dynamically adjusts optimization strategy based on runtime profiling data. Functions that are
/// called rarely are skipped for optimization, while hot functions receive aggressive optimization.
/// </summary>
public sealed class AdaptiveOptimizer
{
    private readonly ConcurrentDictionary<string, OptimizationStrategy> _strategyCache = new();
    private readonly HotPathAnalyzer _analyzer = new();
    private int _defaultStrategy;

    /// <summary>
    /// The default strategy applied to functions with no profiling data.
    /// Defaults to <see cref="OptimizationStrategy.StandardOptimization"/>.
    /// </summary>
    public OptimizationStrategy DefaultStrategy
    {
        get => (OptimizationStrategy)Volatile.Read(ref _defaultStrategy);
        set => Volatile.Write(ref _defaultStrategy, (int)value);
    }

    /// <summary>
    /// Initializes a new adaptive optimizer with standard default strategy.
    /// </summary>
    public AdaptiveOptimizer()
    {
        _defaultStrategy = (int)OptimizationStrategy.StandardOptimization;
    }

    /// <summary>
    /// Initializes a new adaptive optimizer with a specified default strategy.
    /// </summary>
    /// <param name="defaultStrategy">The strategy to use for functions with no profiling data.</param>
    public AdaptiveOptimizer(OptimizationStrategy defaultStrategy)
    {
        _defaultStrategy = (int)defaultStrategy;
    }

    /// <summary>
    /// Gets the recommended optimization strategy for a function based on the latest profiling data.
    /// </summary>
    /// <param name="functionName">The function to get a strategy for.</param>
    /// <returns>The recommended optimization strategy.</returns>
    public OptimizationStrategy GetStrategy(string functionName)
    {
        ArgumentNullException.ThrowIfNull(functionName);

        if (_strategyCache.TryGetValue(functionName, out var cached))
            return cached;

        return DefaultStrategy;
    }

    /// <summary>
    /// Updates the optimization strategies for all functions based on the provided profiler data.
    /// This recalculates strategies and caches them.
    /// </summary>
    /// <param name="profiler">The profiler containing execution data.</param>
    public void UpdateStrategies(ExecutionProfiler profiler)
    {
        ArgumentNullException.ThrowIfNull(profiler);

        var analyses = _analyzer.Analyze(profiler);

        foreach (var analysis in analyses)
        {
            var strategy = DetermineStrategy(analysis);
            _strategyCache[analysis.FunctionName] = strategy;
        }
    }

    /// <summary>
    /// Updates the strategy for a single function based on a specific analysis.
    /// </summary>
    /// <param name="functionName">The function name.</param>
    /// <param name="analysis">The hot path analysis for this function.</param>
    public void UpdateStrategy(string functionName, HotPathAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(functionName);
        ArgumentNullException.ThrowIfNull(analysis);

        _strategyCache[functionName] = DetermineStrategy(analysis);
    }

    /// <summary>
    /// Forces a specific strategy for a function, overriding automatic detection.
    /// </summary>
    /// <param name="functionName">The function name.</param>
    /// <param name="strategy">The strategy to apply.</param>
    public void OverrideStrategy(string functionName, OptimizationStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(functionName);
        _strategyCache[functionName] = strategy;
    }

    /// <summary>
    /// Clears all cached strategies, resetting to default behavior.
    /// </summary>
    public void Reset()
    {
        _strategyCache.Clear();
    }

    private static OptimizationStrategy DetermineStrategy(HotPathAnalysis analysis)
    {
        if (analysis.CallCount == 0)
            return OptimizationStrategy.SkipOptimization;

        if (analysis.PercentageOfTotal > 50.0 || analysis.CallCount > 10000)
            return OptimizationStrategy.FullOptimization;

        if (analysis.IsHot)
            return OptimizationStrategy.AggressiveOptimization;

        if (analysis.PercentageOfTotal > 20.0 || analysis.CallCount > 1000)
            return OptimizationStrategy.StandardOptimization;

        if (analysis.PercentageOfTotal > 5.0 || analysis.CallCount > 100)
            return OptimizationStrategy.BasicOptimization;

        return OptimizationStrategy.SkipOptimization;
    }
}
