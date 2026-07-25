namespace MathVerse.Math.HPC.Core;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using MathVerse.Math.HPC.Diagnostics;

/// <summary>
/// Container for all HPC subsystem services. Lazy-initialized on first access.
/// </summary>
public sealed class HpcServices : IDisposable
{
    private readonly HpcRegistry _registry;
    private readonly ConcurrentDictionary<Type, object> _instances;
    private readonly object _lock;
    private bool _disposed;

    public HpcServices(HpcRegistry registry)
    {
        _registry = registry;
        _instances = new ConcurrentDictionary<Type, object>();
        _lock = new object();

        RegisterCoreServices();
    }

    /// <summary>
    /// Gets the static analyzer service.
    /// </summary>
    public IStaticAnalyzer StaticAnalyzer => GetOrCreate<IStaticAnalyzer>();

    /// <summary>
    /// Gets the symbolic executor service.
    /// </summary>
    public ISymbolicExecutor SymbolicExecutor => GetOrCreate<ISymbolicExecutor>();

    /// <summary>
    /// Gets the constraint solver service.
    /// </summary>
    public IConstraintSolver ConstraintSolver => GetOrCreate<IConstraintSolver>();

    /// <summary>
    /// Gets the kernel optimizer service.
    /// </summary>
    public IKernelOptimizer KernelOptimizer => GetOrCreate<IKernelOptimizer>();

    /// <summary>
    /// Gets the vectorizer service.
    /// </summary>
    public IVectorizer Vectorizer => GetOrCreate<IVectorizer>();

    /// <summary>
    /// Gets the parallel scheduler service.
    /// </summary>
    public IParallelScheduler ParallelScheduler => GetOrCreate<IParallelScheduler>();

    /// <summary>
    /// Gets the memory manager service.
    /// </summary>
    public IMemoryManager MemoryManager => GetOrCreate<IMemoryManager>();

    /// <summary>
    /// Gets the cache manager service.
    /// </summary>
    public ICacheManager CacheManager => GetOrCreate<ICacheManager>();

    /// <summary>
    /// Gets the kernel fusion service.
    /// </summary>
    public IKernelFusion KernelFusion => GetOrCreate<IKernelFusion>();

    /// <summary>
    /// Gets the runtime scheduler service.
    /// </summary>
    public IRuntimeScheduler RuntimeScheduler => GetOrCreate<IRuntimeScheduler>();

    /// <summary>
    /// Gets the CPU executor service.
    /// </summary>
    public ICpuExecutor CpuExecutor => GetOrCreate<ICpuExecutor>();

    /// <summary>
    /// Gets the GPU executor service.
    /// </summary>
    public IGpuExecutor GpuExecutor => GetOrCreate<IGpuExecutor>();

    /// <summary>
    /// Gets the kernel compiler service.
    /// </summary>
    public IKernelCompiler KernelCompiler => GetOrCreate<IKernelCompiler>();

    /// <summary>
    /// Gets the graph executor service.
    /// </summary>
    public IGraphExecutor GraphExecutor => GetOrCreate<IGraphExecutor>();

    /// <summary>
    /// Gets the memory system service.
    /// </summary>
    public IMemorySystem MemorySystem => GetOrCreate<IMemorySystem>();

    /// <summary>
    /// Gets the cache system service.
    /// </summary>
    public ICacheSystem CacheSystem => GetOrCreate<ICacheSystem>();

    /// <summary>
    /// Gets the distributed executor service.
    /// </summary>
    public IDistributedExecutor DistributedExecutor => GetOrCreate<IDistributedExecutor>();

    /// <summary>
    /// Gets the numerical executor service.
    /// </summary>
    public INumericalExecutor NumericalExecutor => GetOrCreate<INumericalExecutor>();

    /// <summary>
    /// Gets the AI executor service.
    /// </summary>
    public IAiExecutor AiExecutor => GetOrCreate<IAiExecutor>();

    /// <summary>
    /// Gets the geometry executor service.
    /// </summary>
    public IGeometryExecutor GeometryExecutor => GetOrCreate<IGeometryExecutor>();

    /// <summary>
    /// Gets the simulation executor service.
    /// </summary>
    public ISimulationExecutor SimulationExecutor => GetOrCreate<ISimulationExecutor>();

    /// <summary>
    /// Gets the quantum executor service.
    /// </summary>
    public IQuantumExecutor QuantumExecutor => GetOrCreate<IQuantumExecutor>();

    /// <summary>
    /// Gets or creates a service instance of the specified type.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <returns>The service instance.</returns>
    public T GetOrCreate<T>()
        where T : class
    {
        return (T)_instances.GetOrAdd(typeof(T), t => CreateInstance<T>());
    }

    /// <summary>
    /// Tries to get a registered service instance.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <param name="instance">The service instance.</param>
    /// <returns>True if found; otherwise, false.</returns>
    public bool TryGet<T>(out T? instance)
        where T : class
    {
        if (_instances.TryGetValue(typeof(T), out var existing))
        {
            instance = (T)existing;
            return true;
        }

        if (_registry.TryResolve<T>(out instance))
        {
            _instances.TryAdd(typeof(T), instance!);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Resolves a service or returns a default implementation.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
/// <returns>The service instance.</returns>
    public T ResolveOrDefault<T>()
        where T : class
    {
        if (TryGet<T>(out var instance) && instance != null)
        {
            return instance;
        }

        return GetDefaultImplementation<T>();
    }

    /// <summary>
    /// Registers a custom service implementation.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <param name="instance">The instance to register.</param>
    public void RegisterInstance<T>(T instance)
        where T : class
    {
        _registry.RegisterInstance(instance);
        _instances.AddOrUpdate(typeof(T), instance, (_, _) => instance);
    }

    /// <summary>
    /// Registers a service factory.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <param name="factory">The factory function.</param>
    public void RegisterFactory<T>(Func<T> factory)
        where T : class
    {
        _registry.Register(factory);
    }

private void RegisterCoreServices()
        {
            // Register default implementations
            _registry.Register<IStaticAnalyzer>(() => new DefaultStaticAnalyzer());
            _registry.Register<ISymbolicExecutor>(() => new DefaultSymbolicExecutor());
            _registry.Register<IConstraintSolver>(() => new DefaultConstraintSolver());
            _registry.Register<IKernelOptimizer>(() => new DefaultKernelOptimizer());
            _registry.Register<IVectorizer>(() => new DefaultVectorizer());
            _registry.Register<IParallelScheduler>(() => new DefaultParallelScheduler());
            _registry.Register<IMemoryManager>(() => new DefaultMemoryManager());
            _registry.Register<ICacheManager>(() => new DefaultCacheManager());
            _registry.Register<IKernelFusion>(() => new DefaultKernelFusion());
            _registry.Register<IRuntimeScheduler>(() => new DefaultRuntimeScheduler());
            _registry.Register<ICpuExecutor>(() => new DefaultCpuExecutor());
            _registry.Register<IGpuExecutor>(() => new DefaultGpuExecutor());
            _registry.Register<IKernelCompiler>(() => new DefaultKernelCompiler());
            _registry.Register<IGraphExecutor>(() => new DefaultGraphExecutor());
            _registry.Register<IMemorySystem>(() => new DefaultMemorySystem());
            _registry.Register<ICacheSystem>(() => new DefaultCacheSystem());
            _registry.Register<IDistributedExecutor>(() => new DefaultDistributedExecutor());
            _registry.Register<INumericalExecutor>(() => new DefaultNumericalExecutor());
            _registry.Register<IAiExecutor>(() => new DefaultAiExecutor());
            _registry.Register<IGeometryExecutor>(() => new DefaultGeometryExecutor());
            _registry.Register<ISimulationExecutor>(() => new DefaultSimulationExecutor());
            _registry.Register<IQuantumExecutor>(() => new DefaultQuantumExecutor());
        }

private T CreateInstance<T>()
        where T : class
    {
        // Try to resolve from registry first
        if (_registry.TryResolve(out T? instance) && instance != null)
        {
            return instance;
        }

        // Create default implementation
        return GetDefaultImplementation<T>();
    }

    private T GetDefaultImplementation<T>()
        where T : class
    {
        var typeName = typeof(T).Name;
        var defaultTypeName = $"MathVerse.Math.HPC.Services.{typeName.TrimStart('I')}";

        var assembly = Assembly.GetExecutingAssembly();
        var defaultType = assembly.GetType(defaultTypeName)
            ?? assembly.GetTypes().FirstOrDefault(t => t.Name == typeName.TrimStart('I') && t.GetInterfaces().Contains(typeof(T)));

        if (defaultType != null)
        {
            return (T)Activator.CreateInstance(defaultType)!;
        }

        // Return a stub implementation
        return CreateStub<T>();
    }

private T CreateStub<T>()
        where T : class
    {
        // System.Runtime.Remoting not available in .NET Core
        // Return null - real implementations should be registered instead
        return null!;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var instance in _instances.Values)
        {
            if (instance is IDisposable disposable)
            {
                try { disposable.Dispose(); } catch { }
            }
        }

_instances.Clear();
    }

    // StubInvocationHandler removed - System.Runtime.Remoting not available in .NET Core
    // Use interface-based abstractions instead
}

/// <summary>
/// Static analyzer interface.
/// </summary>
public interface IStaticAnalyzer
{
    Task<AnalysisResult> AnalyzeAsync(object kernel, HpcContext context, CancellationToken cancellationToken);
}

/// <summary>
/// Symbolic executor interface.
/// </summary>
public interface ISymbolicExecutor
{
    Task<SymbolicResult> ExecuteAsync(object expression, HpcContext context, CancellationToken cancellationToken);
}

/// <summary>
/// Constraint solver interface.
/// </summary>
public interface IConstraintSolver
{
    Task<ConstraintSolution> SolveAsync(object constraints, HpcContext context, CancellationToken cancellationToken);
}

/// <summary>
/// Kernel optimizer interface.
/// </summary>
public interface IKernelOptimizer
{
    Task<IOptimizedKernel> OptimizeAsync(object kernel, HpcContext context, CancellationToken cancellationToken);
    Task<IOptimizedKernel> AutoTuneAsync(object kernel, HpcContext context, CancellationToken cancellationToken);
}

/// <summary>
/// Vectorizer interface.
/// </summary>
public interface IVectorizer
{
    Task<IVectorizedCode> VectorizeAsync(object code, HpcContext context, CancellationToken cancellationToken);
}

/// <summary>
/// Parallel scheduler interface.
/// </summary>
public interface IParallelScheduler
{
    Task<IParallelPlan> ScheduleAsync(object taskGraph, HpcContext context, CancellationToken cancellationToken);
}

/// <summary>
/// Memory manager interface.
/// </summary>
public interface IMemoryManager
{
    Task ManageAsync(object kernel, HpcContext context, CancellationToken cancellationToken);
    Task<MemoryAllocation> AllocateAsync(long size, HpcContext context, CancellationToken cancellationToken);
    Task FreeAsync(MemoryAllocation allocation, CancellationToken cancellationToken);
}

/// <summary>
/// Cache manager interface.
/// </summary>
public interface ICacheManager
{
    Task ManageAsync(HpcContext context, CancellationToken cancellationToken);
    Task<object?> GetAsync(string key, CancellationToken cancellationToken);
    Task SetAsync(string key, object value, CancellationToken cancellationToken);
}

/// <summary>
/// Kernel fusion interface.
/// </summary>
public interface IKernelFusion
{
    Task<IOptimizedKernel> FuseAsync(IReadOnlyList<object> kernels, HpcContext context, CancellationToken cancellationToken);
}

/// <summary>
/// Runtime scheduler interface.
/// </summary>
public interface IRuntimeScheduler
{
    Task<IExecutionResult> ScheduleAsync(object workload, HpcContext context, CancellationToken cancellationToken);
}

/// <summary>
/// CPU executor interface.
/// </summary>
public interface ICpuExecutor
{
    Task<IExecutionResult> ExecuteAsync(object kernel, HpcContext context, CancellationToken cancellationToken);
}

/// <summary>
/// GPU executor interface.
/// </summary>
public interface IGpuExecutor
{
    Task<IExecutionResult> ExecuteAsync(object kernel, HpcContext context, CancellationToken cancellationToken);
}

/// <summary>
/// Kernel compiler interface.
/// </summary>
public interface IKernelCompiler
{
    Task<ICompiledModule> CompileAsync(object kernel, HpcContext context, CancellationToken cancellationToken);
}

/// <summary>
/// Graph executor interface.
/// </summary>
public interface IGraphExecutor
{
    Task<IExecutionResult> ExecuteAsync(object graph, HpcContext context, CancellationToken cancellationToken);
}

/// <summary>
/// Memory system interface.
/// </summary>
public interface IMemorySystem
{
    Task<MemoryAllocation> AllocateAsync(long size, CancellationToken cancellationToken);
    Task FreeAsync(MemoryAllocation allocation, CancellationToken cancellationToken);
}

/// <summary>
/// Cache system interface.
/// </summary>
public interface ICacheSystem
{
    Task<object?> GetAsync(string key, CancellationToken cancellationToken);
    Task SetAsync(string key, object value, CancellationToken cancellationToken);
    Task InvalidateAsync(string pattern, CancellationToken cancellationToken);
}

/// <summary>
/// Distributed executor interface.
/// </summary>
public interface IDistributedExecutor
{
    Task<IExecutionResult> ExecuteAsync(object kernel, HpcContext context, CancellationToken cancellationToken);
}

/// <summary>
/// Numerical executor interface.
/// </summary>
public interface INumericalExecutor
{
    Task<IExecutionResult> ExecuteAsync(object computation, HpcContext context, CancellationToken cancellationToken);
}

/// <summary>
/// AI executor interface.
/// </summary>
public interface IAiExecutor
{
    Task<IExecutionResult> ExecuteAsync(object model, HpcContext context, CancellationToken cancellationToken);
}

/// <summary>
/// Geometry executor interface.
/// </summary>
public interface IGeometryExecutor
{
    Task<IExecutionResult> ExecuteAsync(object geometry, HpcContext context, CancellationToken cancellationToken);
}

/// <summary>
/// Simulation executor interface.
/// </summary>
public interface ISimulationExecutor
{
    Task<IExecutionResult> ExecuteAsync(object simulation, HpcContext context, CancellationToken cancellationToken);
}

/// <summary>
/// Quantum executor interface.
/// </summary>
public interface IQuantumExecutor
{
    Task<IExecutionResult> ExecuteAsync(object circuit, HpcContext context, CancellationToken cancellationToken);
}

// Result types
public sealed record AnalysisResult(string Summary, IReadOnlyDictionary<string, object> Metrics);
public sealed record SymbolicResult(object Expression, IReadOnlyDictionary<string, object> Variables);
public sealed record ConstraintSolution(bool Satisfiable, IReadOnlyDictionary<string, object> Assignments);
public sealed record MemoryAllocation(IntPtr Pointer, long Size, bool Pinned);
public sealed record ExecutionResult(string ExecutionId, object? ReturnValue, TimeSpan ExecutionTime, long PeakMemoryBytes, IReadOnlyDictionary<string, object> Metadata) : IExecutionResult
{
}

// Default implementations (stubs)
public sealed class DefaultStaticAnalyzer : IStaticAnalyzer
{
    public Task<AnalysisResult> AnalyzeAsync(object kernel, HpcContext context, CancellationToken cancellationToken)
        => Task.FromResult(new AnalysisResult("Analyzed", new Dictionary<string, object> { ["nodes"] = 100 }));
}

public sealed class DefaultSymbolicExecutor : ISymbolicExecutor
{
    public Task<SymbolicResult> ExecuteAsync(object expression, HpcContext context, CancellationToken cancellationToken)
        => Task.FromResult(new SymbolicResult(expression, new Dictionary<string, object>().ToImmutableDictionary()));
}

public sealed class DefaultConstraintSolver : IConstraintSolver
{
    public Task<ConstraintSolution> SolveAsync(object constraints, HpcContext context, CancellationToken cancellationToken)
        => Task.FromResult(new ConstraintSolution(true, new Dictionary<string, object>().ToImmutableDictionary()));
}

public sealed class DefaultKernelOptimizer : IKernelOptimizer
{
    public Task<IOptimizedKernel> OptimizeAsync(object kernel, HpcContext context, CancellationToken cancellationToken)
        => Task.FromResult<IOptimizedKernel>(new OptimizedKernel("opt-1", kernel, kernel, Array.Empty<string>(), 1.5, new Dictionary<string, object>().ToImmutableDictionary()));

    public Task<IOptimizedKernel> AutoTuneAsync(object kernel, HpcContext context, CancellationToken cancellationToken)
        => Task.FromResult<IOptimizedKernel>(new OptimizedKernel("tuned-1", kernel, kernel, Array.Empty<string>(), 2.0, new Dictionary<string, object>().ToImmutableDictionary()));
}

public sealed class DefaultVectorizer : IVectorizer
{
    public Task<IVectorizedCode> VectorizeAsync(object code, HpcContext context, CancellationToken cancellationToken)
        => Task.FromResult<IVectorizedCode>(new VectorizedCode("vec-1", code, code, 256, "AVX2", 4.0, new Dictionary<string, object>().ToImmutableDictionary()));
}

public sealed class DefaultParallelScheduler : IParallelScheduler
{
    public Task<IParallelPlan> ScheduleAsync(object taskGraph, HpcContext context, CancellationToken cancellationToken)
        => Task.FromResult<IParallelPlan>(new ParallelPlan("plan-1", taskGraph, Environment.ProcessorCount, "WorkStealing", 8.0, 0.9, new Dictionary<string, object>().ToImmutableDictionary()));
}

public sealed class DefaultMemoryManager : IMemoryManager
{
    public Task ManageAsync(object kernel, HpcContext context, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task<MemoryAllocation> AllocateAsync(long size, HpcContext context, CancellationToken cancellationToken)
        => Task.FromResult(new MemoryAllocation(IntPtr.Zero, size, false));

    public Task FreeAsync(MemoryAllocation allocation, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

public sealed class DefaultCacheManager : ICacheManager
{
    public Task ManageAsync(HpcContext context, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task<object?> GetAsync(string key, CancellationToken cancellationToken)
        => Task.FromResult<object?>(null);

    public Task SetAsync(string key, object value, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

public sealed class DefaultKernelFusion : IKernelFusion
{
    public Task<IOptimizedKernel> FuseAsync(IReadOnlyList<object> kernels, HpcContext context, CancellationToken cancellationToken)
        => Task.FromResult<IOptimizedKernel>(new OptimizedKernel("fused-1", kernels, kernels, Array.Empty<string>(), 2.5, new Dictionary<string, object>().ToImmutableDictionary()));
}

public sealed class DefaultRuntimeScheduler : IRuntimeScheduler
{
    public Task<IExecutionResult> ScheduleAsync(object workload, HpcContext context, CancellationToken cancellationToken)
        => Task.FromResult<IExecutionResult>(new ExecutionResult("sched-1", null, TimeSpan.Zero, 0, new Dictionary<string, object>().ToImmutableDictionary()));
}

public sealed class DefaultCpuExecutor : ICpuExecutor
{
    public Task<IExecutionResult> ExecuteAsync(object kernel, HpcContext context, CancellationToken cancellationToken)
        => Task.FromResult<IExecutionResult>(new ExecutionResult("cpu-1", kernel, TimeSpan.FromMilliseconds(10), 1024, new Dictionary<string, object>().ToImmutableDictionary()));
}

public sealed class DefaultGpuExecutor : IGpuExecutor
{
    public Task<IExecutionResult> ExecuteAsync(object kernel, HpcContext context, CancellationToken cancellationToken)
        => Task.FromResult<IExecutionResult>(new ExecutionResult("gpu-1", kernel, TimeSpan.FromMilliseconds(5), 2048, new Dictionary<string, object>().ToImmutableDictionary()));
}

public sealed class DefaultKernelCompiler : IKernelCompiler
{
    public Task<ICompiledModule> CompileAsync(object kernel, HpcContext context, CancellationToken cancellationToken)
        => Task.FromResult<ICompiledModule>(new CompiledModule("mod-1", "Native", kernel, new[] { "main" }, new Dictionary<string, object>().ToImmutableDictionary()));
}

public sealed class DefaultGraphExecutor : IGraphExecutor
{
    public Task<IExecutionResult> ExecuteAsync(object graph, HpcContext context, CancellationToken cancellationToken)
        => Task.FromResult<IExecutionResult>(new ExecutionResult("graph-1", graph, TimeSpan.FromMilliseconds(20), 4096, new Dictionary<string, object>().ToImmutableDictionary()));
}

public sealed class DefaultMemorySystem : IMemorySystem
{
    public Task<MemoryAllocation> AllocateAsync(long size, CancellationToken cancellationToken)
        => Task.FromResult(new MemoryAllocation(IntPtr.Zero, size, false));

    public Task FreeAsync(MemoryAllocation allocation, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

public sealed class DefaultCacheSystem : ICacheSystem
{
    public Task<object?> GetAsync(string key, CancellationToken cancellationToken)
        => Task.FromResult<object?>(null);

    public Task SetAsync(string key, object value, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task InvalidateAsync(string pattern, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

public sealed class DefaultDistributedExecutor : IDistributedExecutor
{
    public Task<IExecutionResult> ExecuteAsync(object kernel, HpcContext context, CancellationToken cancellationToken)
        => Task.FromResult<IExecutionResult>(new ExecutionResult("dist-1", kernel, TimeSpan.FromMilliseconds(50), 8192, new Dictionary<string, object>().ToImmutableDictionary()));
}

public sealed class DefaultNumericalExecutor : INumericalExecutor
{
    public Task<IExecutionResult> ExecuteAsync(object computation, HpcContext context, CancellationToken cancellationToken)
        => Task.FromResult<IExecutionResult>(new ExecutionResult("num-1", computation, TimeSpan.FromMilliseconds(15), 1024, new Dictionary<string, object>().ToImmutableDictionary()));
}

public sealed class DefaultAiExecutor : IAiExecutor
{
    public Task<IExecutionResult> ExecuteAsync(object model, HpcContext context, CancellationToken cancellationToken)
        => Task.FromResult<IExecutionResult>(new ExecutionResult("ai-1", model, TimeSpan.FromMilliseconds(100), 50 * 1024 * 1024, new Dictionary<string, object>().ToImmutableDictionary()));
}

public sealed class DefaultGeometryExecutor : IGeometryExecutor
{
    public Task<IExecutionResult> ExecuteAsync(object geometry, HpcContext context, CancellationToken cancellationToken)
        => Task.FromResult<IExecutionResult>(new ExecutionResult("geo-1", geometry, TimeSpan.FromMilliseconds(5), 1024, new Dictionary<string, object>()));
}

public sealed class DefaultSimulationExecutor : ISimulationExecutor
{
    public Task<IExecutionResult> ExecuteAsync(object simulation, HpcContext context, CancellationToken cancellationToken)
        => Task.FromResult<IExecutionResult>(new ExecutionResult("sim-1", simulation, TimeSpan.FromMilliseconds(500), 10 * 1024 * 1024, new Dictionary<string, object>()));
}

public sealed class DefaultQuantumExecutor : IQuantumExecutor
{
    public Task<IExecutionResult> ExecuteAsync(object circuit, HpcContext context, CancellationToken cancellationToken)
        => Task.FromResult<IExecutionResult>(new ExecutionResult("quantum-1", circuit, TimeSpan.FromMilliseconds(200), 1024 * 1024, new Dictionary<string, object>()));
}

// Concrete implementations of interfaces
public sealed class OptimizedKernel : IOptimizedKernel
{
    public OptimizedKernel(string kernelId, object originalKernel, object optimizedKernelIR, IReadOnlyList<string> appliedPasses, double estimatedSpeedup, IReadOnlyDictionary<string, object> metadata)
    {
        KernelId = kernelId;
        OriginalKernel = originalKernel;
        OptimizedKernelIR = optimizedKernelIR;
        AppliedPasses = appliedPasses;
        EstimatedSpeedup = estimatedSpeedup;
        Metadata = metadata;
    }

    public string KernelId { get; }
    public object OriginalKernel { get; }
    public object OptimizedKernelIR { get; }
    public IReadOnlyList<string> AppliedPasses { get; }
    public double EstimatedSpeedup { get; }
    public IReadOnlyDictionary<string, object> Metadata { get; }
}

public sealed class VectorizedCode : IVectorizedCode
{
    public VectorizedCode(string vectorizationId, object originalCode, object vectorizedCodeIR, int vectorWidth, string instructionSet, double estimatedSpeedup, IReadOnlyDictionary<string, object> metadata)
    {
        VectorizationId = vectorizationId;
        OriginalCode = originalCode;
        VectorizedCodeIR = vectorizedCodeIR;
        VectorWidth = vectorWidth;
        InstructionSet = instructionSet;
        EstimatedSpeedup = estimatedSpeedup;
        Metadata = metadata;
    }

    public string VectorizationId { get; }
    public object OriginalCode { get; }
    public object VectorizedCodeIR { get; }
    public int VectorWidth { get; }
    public string InstructionSet { get; }
    public double EstimatedSpeedup { get; }
    public IReadOnlyDictionary<string, object> Metadata { get; }
}

public sealed class ParallelPlan : IParallelPlan
{
    public ParallelPlan(string planId, object taskGraph, int threadCount, string schedulingStrategy, double estimatedParallelism, double loadBalance, IReadOnlyDictionary<string, object> metadata)
    {
        PlanId = planId;
        TaskGraph = taskGraph;
        ThreadCount = threadCount;
        SchedulingStrategy = schedulingStrategy;
        EstimatedParallelism = estimatedParallelism;
        LoadBalance = loadBalance;
        Metadata = metadata;
    }

    public string PlanId { get; }
    public object TaskGraph { get; }
    public int ThreadCount { get; }
    public string SchedulingStrategy { get; }
    public double EstimatedParallelism { get; }
    public double LoadBalance { get; }
    public IReadOnlyDictionary<string, object> Metadata { get; }
}

public sealed class CompiledModule : ICompiledModule
{
    public CompiledModule(string moduleId, string targetPlatform, object compiledArtifact, IReadOnlyList<string> entryPoints, IReadOnlyDictionary<string, object> metadata)
    {
        ModuleId = moduleId;
        TargetPlatform = targetPlatform;
        CompiledArtifact = compiledArtifact;
        EntryPoints = entryPoints;
        Metadata = metadata;
    }

    public string ModuleId { get; }
    public string TargetPlatform { get; }
    public object CompiledArtifact { get; }
    public IReadOnlyList<string> EntryPoints { get; }
    public IReadOnlyDictionary<string, object> Metadata { get; }
}
