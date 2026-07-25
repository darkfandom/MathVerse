namespace MathVerse.Math.Compiler.Configuration;

using System;

public sealed class CompilerConfigurationBuilder
{
    private OptimizationLevel _optimizationLevel = OptimizationLevel.Basic;
    private bool _vectorizationEnabled = true;
    private bool _parallelizationEnabled = true;
    private CompilationTargetType _targetPlatform = CompilationTargetType.Generic;
    private int _maxParallelism = Environment.ProcessorCount;
    private bool _cacheEnabled = true;
    private int _maxCacheSize = 1024;
    private bool _enableConstantFolding = true;
    private bool _enableDeadCodeElimination = true;
    private bool _enableCSE = true;
    private bool _enableSIMD = true;
    private bool _enableLoopOptimizations = true;
    private bool _enableInlining = true;
    private int _maxInlineSize = 64;
    private bool _enableKernelFusion = true;
    private bool _enableMemoryOptimization = true;
    private bool _enableAutomaticDifferentiation = true;
    private bool _enableProfiling;
    private int _maxRecursionDepth = 256;
    private int _maxExpressionDepth = 128;

    public CompilerConfigurationBuilder SetOptimizationLevel(OptimizationLevel level)
    {
        _optimizationLevel = level;
        return this;
    }

    public CompilerConfigurationBuilder EnableVectorization(bool enabled = true)
    {
        _vectorizationEnabled = enabled;
        return this;
    }

    public CompilerConfigurationBuilder EnableParallelization(bool enabled = true)
    {
        _parallelizationEnabled = enabled;
        return this;
    }

    public CompilerConfigurationBuilder SetTargetPlatform(CompilationTargetType target)
    {
        _targetPlatform = target;
        return this;
    }

    public CompilerConfigurationBuilder SetMaxParallelism(int max)
    {
        _maxParallelism = Math.Max(1, max);
        return this;
    }

    public CompilerConfigurationBuilder EnableCaching(bool enabled = true)
    {
        _cacheEnabled = enabled;
        return this;
    }

    public CompilerConfigurationBuilder SetMaxCacheSize(int size)
    {
        _maxCacheSize = Math.Max(0, size);
        return this;
    }

    public CompilerConfigurationBuilder EnableConstantFolding(bool enabled = true)
    {
        _enableConstantFolding = enabled;
        return this;
    }

    public CompilerConfigurationBuilder EnableDeadCodeElimination(bool enabled = true)
    {
        _enableDeadCodeElimination = enabled;
        return this;
    }

    public CompilerConfigurationBuilder EnableCSE(bool enabled = true)
    {
        _enableCSE = enabled;
        return this;
    }

    public CompilerConfigurationBuilder EnableSIMD(bool enabled = true)
    {
        _enableSIMD = enabled;
        return this;
    }

    public CompilerConfigurationBuilder EnableLoopOptimizations(bool enabled = true)
    {
        _enableLoopOptimizations = enabled;
        return this;
    }

    public CompilerConfigurationBuilder EnableInlining(bool enabled = true)
    {
        _enableInlining = enabled;
        return this;
    }

    public CompilerConfigurationBuilder SetMaxInlineSize(int size)
    {
        _maxInlineSize = Math.Max(1, size);
        return this;
    }

    public CompilerConfigurationBuilder EnableKernelFusion(bool enabled = true)
    {
        _enableKernelFusion = enabled;
        return this;
    }

    public CompilerConfigurationBuilder EnableMemoryOptimization(bool enabled = true)
    {
        _enableMemoryOptimization = enabled;
        return this;
    }

    public CompilerConfigurationBuilder EnableAutomaticDifferentiation(bool enabled = true)
    {
        _enableAutomaticDifferentiation = enabled;
        return this;
    }

    public CompilerConfigurationBuilder EnableProfiling(bool enabled = true)
    {
        _enableProfiling = enabled;
        return this;
    }

    public CompilerConfigurationBuilder SetMaxRecursionDepth(int depth)
    {
        _maxRecursionDepth = Math.Max(1, depth);
        return this;
    }

    public CompilerConfigurationBuilder SetMaxExpressionDepth(int depth)
    {
        _maxExpressionDepth = Math.Max(1, depth);
        return this;
    }

    public CompilerConfiguration Build()
    {
        return new CompilerConfiguration
        {
            OptimizationLevel = _optimizationLevel,
            VectorizationEnabled = _vectorizationEnabled,
            ParallelizationEnabled = _parallelizationEnabled,
            TargetPlatform = _targetPlatform,
            MaxParallelism = _maxParallelism,
            CacheEnabled = _cacheEnabled,
            MaxCacheSize = _maxCacheSize,
            EnableConstantFolding = _enableConstantFolding,
            EnableDeadCodeElimination = _enableDeadCodeElimination,
            EnableCommonSubexpressionElimination = _enableCSE,
            EnableSIMD = _enableSIMD,
            EnableLoopOptimizations = _enableLoopOptimizations,
            EnableInlining = _enableInlining,
            MaxInlineSize = _maxInlineSize,
            EnableKernelFusion = _enableKernelFusion,
            EnableMemoryOptimization = _enableMemoryOptimization,
            EnableAutomaticDifferentiation = _enableAutomaticDifferentiation,
            EnableProfiling = _enableProfiling,
            MaxRecursionDepth = _maxRecursionDepth,
            MaxExpressionDepth = _maxExpressionDepth
        };
    }
}
