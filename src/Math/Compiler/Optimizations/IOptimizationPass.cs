namespace MathVerse.Math.Compiler.Optimizations;

using MathVerse.Math.Compiler.IR;

/// <summary>
/// Defines a standalone optimization pass that transforms an IR module.
/// Each implementation must be side-effect-free and may be run multiple times.
/// </summary>
public interface IOptimizationPass
{
    /// <summary>
    /// Gets the human-readable name of this optimization pass.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Applies the optimization to the given IR module and returns the transformed module.
    /// </summary>
    /// <param name="module">The IR module to optimize.</param>
    /// <returns>A new or modified IR module with the optimization applied.</returns>
    IRModule Optimize(IRModule module);
}
