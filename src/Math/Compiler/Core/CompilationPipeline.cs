namespace MathVerse.Math.Compiler.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using MathVerse.Math.Compiler.IR;
using MathVerse.Math.Compiler.Optimizations;

public sealed class CompilationPipeline
{
    private readonly List<ICompilationPass> _passes = new();
    private readonly CompilationTarget _target;

    public CompilationPipeline(CompilationTarget target)
    {
        _target = target;
        InitializeDefaultPasses();
    }

    public CompilationPipeline AddPass(ICompilationPass pass)
    {
        _passes.Add(pass);
        return this;
    }

    public IRModule Run(IRModule module)
    {
        foreach (var pass in _passes)
            module = pass.Run(module);
        return module;
    }

    public IReadOnlyList<ICompilationPass> Passes => _passes.AsReadOnly();

    private void InitializeDefaultPasses()
    {
        _passes.Add(new ParsePass(_target));
        _passes.Add(new LowerPass());
        _passes.Add(new OptimizePass());
        _passes.Add(new VectorizePass());
        _passes.Add(new CodegenPass());
    }
}

internal sealed class ParsePass : ICompilationPass
{
    public string Name => "Parse";
    private readonly CompilationTarget _target;

    public ParsePass(CompilationTarget target) { _target = target; }

    public IRModule Run(IRModule module) => module;
}

internal sealed class LowerPass : ICompilationPass
{
    public string Name => "Lower";

    public IRModule Run(IRModule module) => module;
}

internal sealed class OptimizePass : ICompilationPass
{
    public string Name => "Optimize";

    public IRModule Run(IRModule module)
    {
        var optimizer = CompilerOptimizer.CreateDefault();
        return optimizer.Optimize(module, Configuration.OptimizationLevel.Basic);
    }
}

internal sealed class VectorizePass : ICompilationPass
{
    public string Name => "Vectorize";

    public IRModule Run(IRModule module)
    {
        var vectorizer = new Vectorization.Vectorizer();
        return vectorizer.Vectorize(module);
    }
}

internal sealed class CodegenPass : ICompilationPass
{
    public string Name => "Codegen";

    public IRModule Run(IRModule module) => module;
}
