namespace MathVerse.Math.Compiler.Configuration;

public sealed record CompilerConfiguration
{
    public OptimizationLevel OptimizationLevel { get; init; } = OptimizationLevel.Basic;

    public bool VectorizationEnabled { get; init; } = true;

    public bool ParallelizationEnabled { get; init; } = true;

    public CompilationTargetType TargetPlatform { get; init; } = CompilationTargetType.Generic;

    public int MaxParallelism { get; init; } = Environment.ProcessorCount;

    public bool CacheEnabled { get; init; } = true;

    public int MaxCacheSize { get; init; } = 1024;

    public bool EnableConstantFolding { get; init; } = true;

    public bool EnableDeadCodeElimination { get; init; } = true;

    public bool EnableCommonSubexpressionElimination { get; init; } = true;

    public bool EnableSIMD { get; init; } = true;

    public bool EnableLoopOptimizations { get; init; } = true;

    public bool EnableInlining { get; init; } = true;

    public int MaxInlineSize { get; init; } = 64;

    public bool EnableKernelFusion { get; init; } = true;

    public bool EnableMemoryOptimization { get; init; } = true;

    public bool EnableAutomaticDifferentiation { get; init; } = true;

    public bool EnableProfiling { get; init; }

    public int MaxRecursionDepth { get; init; } = 256;

    public int MaxExpressionDepth { get; init; } = 128;

    public static CompilerConfiguration Default { get; } = new();
}
