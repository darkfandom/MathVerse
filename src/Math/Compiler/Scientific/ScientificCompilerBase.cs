namespace MathVerse.Math.Compiler.Scientific;

using IRModule = MathVerse.Math.Compiler.IR.IRModule;
using ComputationGraph = MathVerse.Math.Compiler.Graph.ComputationGraph;

/// <summary>Abstract base class for all scientific compilers.
/// Each subclass lowers high-level math domain operations into optimized IR or computation graphs.</summary>
public abstract class ScientificCompilerBase
{
    /// <summary>Gets the domain name that this compiler handles (e.g. "CAS", "Numerics", "Geometry").</summary>
    public abstract string DomainName { get; }

    /// <summary>Compiles a domain-specific expression string into an <see cref="IRModule"/>.</summary>
    /// <param name="expression">The expression to compile.</param>
    /// <returns>An IR module representing the compiled expression.</returns>
    public abstract IRModule Compile(string expression);

    /// <summary>Builds a <see cref="ComputationGraph"/> from the given expression.</summary>
    /// <param name="expression">The expression to build a graph for.</param>
    /// <returns>A computation graph representing the expression.</returns>
    public abstract ComputationGraph BuildGraph(string expression);
}
