namespace MathVerse.Math.Compiler.Optimizations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Runs a sequence of <see cref="IOptimizationPass"/> instances in order.
/// Supports adding custom passes and provides a default pipeline.
/// </summary>
public sealed class OptimizationPipeline
{
    private readonly List<IOptimizationPass> _passes = new();
    private readonly List<string> _passLog = new();

    /// <summary>
    /// Gets the ordered list of passes in this pipeline.
    /// </summary>
    public IReadOnlyList<IOptimizationPass> Passes => _passes;

    /// <summary>
    /// Gets the log of pass execution results from the last optimization run.
    /// </summary>
    public IReadOnlyList<string> PassLog => _passLog;

    /// <summary>
    /// Creates a default optimization pipeline with standard passes.
    /// Order: ConstantFolding → ConstantPropagation → CSE → DCE → Algebraic → Peephole.
    /// </summary>
    public OptimizationPipeline()
    {
        _passes.Add(new ConstantFolding());
        _passes.Add(new ConstantPropagation());
        _passes.Add(new CommonSubexpressionElimination());
        _passes.Add(new DeadCodeElimination());
        _passes.Add(new AlgebraicOptimizer());
        _passes.Add(new PeepholeOptimizer());
    }

    /// <summary>
    /// Creates an empty optimization pipeline.
    /// </summary>
    /// <param name="useDefaults">If true, populates with the default pipeline passes.</param>
    public OptimizationPipeline(bool useDefaults)
    {
        if (useDefaults)
        {
            _passes.Add(new ConstantFolding());
            _passes.Add(new ConstantPropagation());
            _passes.Add(new CommonSubexpressionElimination());
            _passes.Add(new DeadCodeElimination());
            _passes.Add(new AlgebraicOptimizer());
            _passes.Add(new PeepholeOptimizer());
        }
    }

    /// <summary>
    /// Adds a pass to the end of the pipeline.
    /// </summary>
    /// <param name="pass">The optimization pass to add.</param>
    /// <returns>This pipeline for fluent chaining.</returns>
    public OptimizationPipeline AddPass(IOptimizationPass pass)
    {
        ArgumentNullException.ThrowIfNull(pass);
        _passes.Add(pass);
        return this;
    }

    /// <summary>
    /// Inserts a pass at the specified index in the pipeline.
    /// </summary>
    /// <param name="index">The zero-based index at which to insert the pass.</param>
    /// <param name="pass">The optimization pass to insert.</param>
    /// <returns>This pipeline for fluent chaining.</returns>
    public OptimizationPipeline InsertPass(int index, IOptimizationPass pass)
    {
        ArgumentNullException.ThrowIfNull(pass);
        if (index < 0 || index > _passes.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        _passes.Insert(index, pass);
        return this;
    }

    /// <summary>
    /// Removes a pass by name from the pipeline.
    /// </summary>
    /// <param name="passName">The name of the pass to remove.</param>
    /// <returns>True if the pass was found and removed.</returns>
    public bool RemovePass(string passName)
    {
        var index = _passes.FindIndex(p => p.Name == passName);
        if (index >= 0)
        {
            _passes.RemoveAt(index);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs all optimization passes in sequence on the given IR module.
    /// </summary>
    /// <param name="module">The module to optimize.</param>
    /// <returns>The optimized module.</returns>
    public IRModule Optimize(IRModule module)
    {
        ArgumentNullException.ThrowIfNull(module);
        _passLog.Clear();

        var totalBefore = module.TotalInstructionCount();

        for (var i = 0; i < _passes.Count; i++)
        {
            var pass = _passes[i];
            var instructionsBefore = module.TotalInstructionCount();

            module = pass.Optimize(module);

            var instructionsAfter = module.TotalInstructionCount();
            var eliminated = instructionsBefore - instructionsAfter;
            _passLog.Add($"[{i + 1}/{_passes.Count}] {pass.Name}: {instructionsBefore} → {instructionsAfter} (eliminated {eliminated})");
        }

        var totalAfter = module.TotalInstructionCount();
        _passLog.Add($"Total: {totalBefore} → {totalAfter} instructions (eliminated {totalBefore - totalAfter})");

        return module;
    }

    /// <summary>
    /// Runs the pipeline repeatedly until no more changes occur or the maximum
    /// iteration count is reached.
    /// </summary>
    /// <param name="module">The module to optimize.</param>
    /// <param name="maxIterations">Maximum number of pipeline iterations.</param>
    /// <returns>The optimized module.</returns>
    public IRModule OptimizeUntilFixedPoint(IRModule module, int maxIterations = 10)
    {
        ArgumentNullException.ThrowIfNull(module);
        _passLog.Clear();

        var previousCount = module.TotalInstructionCount();

        for (var iter = 0; iter < maxIterations; iter++)
        {
            _passLog.Add($"--- Iteration {iter + 1} ---");
            module = OptimizeInternal(module);

            var currentCount = module.TotalInstructionCount();
            if (currentCount == previousCount)
            {
                _passLog.Add($"Fixed point reached after {iter + 1} iterations.");
                break;
            }
            previousCount = currentCount;
        }

        return module;
    }

    private IRModule OptimizeInternal(IRModule module)
    {
        var totalBefore = module.TotalInstructionCount();

        for (var i = 0; i < _passes.Count; i++)
        {
            var pass = _passes[i];
            var instructionsBefore = module.TotalInstructionCount();

            module = pass.Optimize(module);

            var instructionsAfter = module.TotalInstructionCount();
            var eliminated = instructionsBefore - instructionsAfter;
            _passLog.Add($"  [{i + 1}] {pass.Name}: {instructionsBefore} → {instructionsAfter} (-{eliminated})");
        }

        var totalAfter = module.TotalInstructionCount();
        _passLog.Add($"  Iteration total: {totalBefore} → {totalAfter} (-{totalBefore - totalAfter})");

        return module;
    }

    /// <summary>
    /// Returns a string representation of the pipeline configuration.
    /// </summary>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("OptimizationPipeline:");
        for (var i = 0; i < _passes.Count; i++)
            sb.AppendLine($"  {i + 1}. {_passes[i].Name}");
        return sb.ToString();
    }
}
