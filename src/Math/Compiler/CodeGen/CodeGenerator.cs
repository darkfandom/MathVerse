namespace MathVerse.Math.Compiler.CodeGen;

using MathVerse.Math.Compiler.IR;

/// <summary>
/// Abstract base class for code generators that transform IR into target code strings.
/// </summary>
public abstract class CodeGenerator
{
    /// <summary>
    /// Generates target code for the entire IR module.
    /// </summary>
    /// <param name="module">The IR module to generate code for.</param>
    /// <returns>The generated code as a string.</returns>
    public abstract string Generate(IRModule module);

    /// <summary>
    /// Generates target code for a single IR function.
    /// </summary>
    /// <param name="function">The IR function to generate code for.</param>
    /// <returns>The generated code as a string.</returns>
    public abstract string GenerateFunction(IRFunction function);
}
