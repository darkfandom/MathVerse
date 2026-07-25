namespace MathVerse.Math.Compiler.Memory;

using System;
using System.Collections.Generic;
using System.Text;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Represents an annotation on an IR value indicating its preferred allocation strategy.
/// </summary>
public sealed class AllocationAnnotation
{
    /// <summary>The IR value this annotation applies to.</summary>
    public IRValue Value { get; }

    /// <summary>Whether this allocation should be placed on the stack.</summary>
    public bool PreferStack { get; }

    /// <summary>Whether this allocation should use a memory pool.</summary>
    public bool PreferPool { get; }

    /// <summary>Whether this allocation can be performed in-place (aliased with input).</summary>
    public bool AllowInPlace { get; }

    /// <summary>
    /// Initializes a new allocation annotation.
    /// </summary>
    public AllocationAnnotation(IRValue value, bool preferStack, bool preferPool, bool allowInPlace)
    {
        Value = value;
        PreferStack = preferStack;
        PreferPool = preferPool;
        AllowInPlace = allowInPlace;
    }
}

/// <summary>
/// Optimizes allocation patterns in an IR module: choosing stack vs heap allocation,
/// identifying pooling opportunities, and detecting in-place operation candidates.
/// </summary>
public sealed class AllocationOptimizer
{
    private const int StackAllocationThreshold = 256;
    private readonly LifetimeAnalyzer _lifetimeAnalyzer = new();

    /// <summary>
    /// Analyzes an IR module and applies allocation annotations to optimize memory usage.
    /// Returns a new module with annotated values.
    /// </summary>
    /// <param name="module">The IR module to optimize.</param>
    /// <returns>The same IR module with allocation annotations added to metadata.</returns>
    public IRModule Optimize(IRModule module)
    {
        ArgumentNullException.ThrowIfNull(module);

        for (var f = 0; f < module.Functions.Count; f++)
        {
            var function = module.Functions[f];
            OptimizeFunction(function, module);
        }

        return module;
    }

    /// <summary>
    /// Analyzes a function and returns allocation annotations for all values.
    /// </summary>
    /// <param name="function">The function to analyze.</param>
    /// <returns>A list of allocation annotations.</returns>
    public IReadOnlyList<AllocationAnnotation> AnalyzeFunction(IRFunction function)
    {
        ArgumentNullException.ThrowIfNull(function);

        var lifetimes = _lifetimeAnalyzer.Analyze(function);
        var annotations = new List<AllocationAnnotation>();

        foreach (var kvp in lifetimes)
        {
            var value = kvp.Key;
            var lifetime = kvp.Value;

            if (value.IsConstant) continue;
            if (value.Type == IRType.Void) continue;

            var size = IRTypeHelper.SizeInBytes(value.Type);
            var preferStack = ShouldStackAllocate(size, lifetime);
            var preferPool = ShouldPool(value.Type);
            var allowInPlace = CanBeInPlace(value, function);

            annotations.Add(new AllocationAnnotation(value, preferStack, preferPool, allowInPlace));
        }

        return annotations;
    }

    private void OptimizeFunction(IRFunction function, IRModule module)
    {
        var annotations = AnalyzeFunction(function);

        for (var i = 0; i < annotations.Count; i++)
        {
            var annotation = annotations[i];
            var key = $"alloc_{function.Name}_{annotation.Value.Name}";

            var flags = new StringBuilder();
            if (annotation.PreferStack) flags.Append("stack;");
            if (annotation.PreferPool) flags.Append("pool;");
            if (annotation.AllowInPlace) flags.Append("inplace;");

            module.SetMetadata(key, flags.ToString());
        }
    }

    private static bool ShouldStackAllocate(int size, LifetimeRange lifetime)
    {
        if (size > StackAllocationThreshold)
            return false;

        // Short-lived values are good stack candidates
        var duration = lifetime.Duration;
        if (duration < 50)
            return true;

        // Small, short-lived allocations
        if (size <= 64 && duration < 200)
            return true;

        return false;
    }

    private static bool ShouldPool(IRType type)
    {
        // Pool allocations for medium-sized, frequently-used types
        return type switch
        {
            IRType.Vector => true,
            IRType.Tensor => true,
            _ => false
        };
    }

    private static bool CanBeInPlace(IRValue value, IRFunction function)
    {
        // Check if this value is used only once as an operand to a binary op
        // and its defining instruction is also a binary op (potential in-place transform)
        var useCount = 0;
        var lastUser = (IRInstruction?)null;

        for (var b = 0; b < function.Blocks.Count; b++)
        {
            var block = function.Blocks[b];
            for (var i = 0; i < block.Instructions.Count; i++)
            {
                var inst = block.Instructions[i];
                for (var j = 0; j < inst.Operands.Count; j++)
                {
                    if (inst.Operands[j].Id == value.Id)
                    {
                        useCount++;
                        lastUser = inst;
                    }
                }
            }
        }

        if (useCount != 1 || lastUser == null)
            return false;

        // In-place is possible if the only user is a binary op of the same type
        return lastUser.OpCode is IROpCode.Add or IROpCode.Sub or IROpCode.Mul or IROpCode.Div;
    }
}
