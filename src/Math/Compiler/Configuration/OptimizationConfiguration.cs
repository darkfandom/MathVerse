namespace MathVerse.Math.Compiler.Configuration;

public sealed record OptimizationConfiguration
{
    public bool EnableConstantFolding { get; init; } = true;

    public bool EnableDeadCodeElimination { get; init; } = true;

    public bool EnableCommonSubexpressionElimination { get; init; } = true;

    public bool EnableSIMD { get; init; } = true;

    public bool EnableLoopOptimizations { get; init; } = true;

    public bool EnableInlining { get; init; } = true;

    public bool EnableStrengthReduction { get; init; } = true;

    public bool EnablePeepholeOptimization { get; init; } = true;

    public bool EnableAlgebraicSimplification { get; init; } = true;

    public bool EnableInstructionScheduling { get; init; } = true;

    public int MaxInlineDepth { get; init; } = 3;

    public int MaxUnrollFactor { get; init; } = 4;

    public static OptimizationConfiguration Default { get; } = new();

    public static OptimizationConfiguration Aggressive { get; } = new()
    {
        EnableConstantFolding = true,
        EnableDeadCodeElimination = true,
        EnableCommonSubexpressionElimination = true,
        EnableSIMD = true,
        EnableLoopOptimizations = true,
        EnableInlining = true,
        EnableStrengthReduction = true,
        EnablePeepholeOptimization = true,
        EnableAlgebraicSimplification = true,
        EnableInstructionScheduling = true,
        MaxInlineDepth = 5,
        MaxUnrollFactor = 8
    };

    public static OptimizationConfiguration Disabled { get; } = new()
    {
        EnableConstantFolding = false,
        EnableDeadCodeElimination = false,
        EnableCommonSubexpressionElimination = false,
        EnableSIMD = false,
        EnableLoopOptimizations = false,
        EnableInlining = false,
        EnableStrengthReduction = false,
        EnablePeepholeOptimization = false,
        EnableAlgebraicSimplification = false,
        EnableInstructionScheduling = false
    };
}
