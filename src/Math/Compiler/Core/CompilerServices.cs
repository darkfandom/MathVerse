namespace MathVerse.Math.Compiler.Core;

using MathVerse.Math.Compiler.Caching;
using MathVerse.Math.Compiler.CodeGen;
using MathVerse.Math.Compiler.Diagnostics;
using MathVerse.Math.Compiler.Optimizations;

public sealed class CompilerServices
{
    public CompilerOptimizer Optimizer { get; }
    public CodeGenerator CodeGenerator { get; }
    public KernelGenerator KernelGenerator { get; }
    public Vectorization.Vectorizer Vectorizer { get; }
    public CompilationCache Cache { get; }
    public CompilerDiagnostics Diagnostics { get; }
    public Runtime.ExecutionProfiler Profiler { get; }
    public CompilerRegistry Registry { get; }

    public CompilerServices()
    {
        Optimizer = CompilerOptimizer.CreateDefault();
        CodeGenerator = new CodeGen.PseudoAssemblyGenerator();
        KernelGenerator = new KernelGenerator();
        Vectorizer = new Vectorization.Vectorizer();
        Cache = new CompilationCache();
        Diagnostics = new CompilerDiagnostics();
        Profiler = new Runtime.ExecutionProfiler();
        Registry = new CompilerRegistry();
    }

    public CompilerServices(CompilerServices other)
    {
        Optimizer = other.Optimizer;
        CodeGenerator = other.CodeGenerator;
        KernelGenerator = other.KernelGenerator;
        Vectorizer = other.Vectorizer;
        Cache = other.Cache;
        Diagnostics = other.Diagnostics;
        Profiler = other.Profiler;
        Registry = other.Registry;
    }
}
