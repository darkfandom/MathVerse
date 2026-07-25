namespace MathVerse.Math.Compiler.Vectorization;

using MathVerse.Math.Compiler.IR;

/// <summary>
/// Defines a vectorization pass that converts scalar IR operations into
/// SIMD-vectorized operations where applicable.
/// </summary>
public interface IVectorizationPass
{
    /// <summary>
    /// Gets the human-readable name of this vectorization pass.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Analyzes and vectorizes the given IR module.
    /// </summary>
    /// <param name="module">The IR module to vectorize.</param>
    /// <returns>The vectorized module.</returns>
    IRModule Vectorize(IRModule module);
}
