namespace MathVerse.Math.Compiler.Diagnostics;

using System;

/// <summary>Records the result of a single optimization pass, including what was applied and what changed.</summary>
public sealed class OptimizationRecord
{
    /// <summary>The name of the optimization pass.</summary>
    public string PassName { get; }
    /// <summary>Instruction count before the pass.</summary>
    public int BeforeInstructionCount { get; }
    /// <summary>Instruction count after the pass.</summary>
    public int AfterInstructionCount { get; }
    /// <summary>The time taken by the optimization pass.</summary>
    public TimeSpan Duration { get; }
    /// <summary>Whether the pass made any changes.</summary>
    public bool Changed { get; }

    /// <summary>Initializes a new instance of the <see cref="OptimizationRecord"/> class.</summary>
    public OptimizationRecord(string passName, int beforeCount, int afterCount, TimeSpan duration)
    {
        PassName = passName ?? throw new ArgumentNullException(nameof(passName));
        BeforeInstructionCount = beforeCount >= 0 ? beforeCount : throw new ArgumentOutOfRangeException(nameof(beforeCount));
        AfterInstructionCount = afterCount >= 0 ? afterCount : throw new ArgumentOutOfRangeException(nameof(afterCount));
        Duration = duration;
        Changed = beforeCount != afterCount;
    }
}
