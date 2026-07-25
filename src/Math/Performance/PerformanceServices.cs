namespace MathVerse.Math.Performance;

/// <summary>
/// Holds all performance infrastructure service instances.
/// Created once per <see cref="PerformanceEngine"/> and shared across operations.
/// </summary>
public sealed class PerformanceServices
{
    /// <summary>
    /// Initializes all services using the specified options.
    /// </summary>
    /// <param name="options">The performance configuration options.</param>
    public PerformanceServices(PerformanceOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        Interner = new ExpressionInterner();
        Hasher = new CachedExpressionHasher();
        Pool = new ObjectPool<ExpressionPool>();
        EvaluationCache = new EvaluationCache<Expression>(options.EvaluationCacheCapacity);
        RewriteCache = new RewriteCache();
        SimplificationCache = new SimplificationCache();
        TypeCache = new TypeInferenceCache();
        Memoization = new MemoizationEngine();
        Incremental = new IncrementalEngine();
        ParallelScheduler = new EvaluationScheduler();
        Optimizer = new OptimizationPipeline();
        Memory = new MemoryTracker();
        Allocations = new AllocationProfiler();
        Diagnostics = new DiagnosticReporter();
        Benchmarks = new BenchmarkRecorder();
        Logger = new PerformanceLogger(Diagnostics);

        if (!options.EnableDiagnostics)
        {
            Diagnostics.MinimumSeverity = PerformanceWarning.ThreadContention + 1;
        }
    }

    /// <summary>Gets the expression interning service.</summary>
    public ExpressionInterner Interner { get; }

    /// <summary>Gets the expression hashing service.</summary>
    public CachedExpressionHasher Hasher { get; }

    /// <summary>Gets the expression object pool.</summary>
    public ObjectPool<ExpressionPool> Pool { get; }

    /// <summary>Gets the evaluation result cache.</summary>
    public EvaluationCache<Expression> EvaluationCache { get; }

    /// <summary>Gets the rewrite result cache.</summary>
    public RewriteCache RewriteCache { get; }

    /// <summary>Gets the simplification result cache.</summary>
    public SimplificationCache SimplificationCache { get; }

    /// <summary>Gets the type inference cache.</summary>
    public TypeInferenceCache TypeCache { get; }

    /// <summary>Gets the memoization engine.</summary>
    public MemoizationEngine Memoization { get; }

    /// <summary>Gets the incremental computation engine.</summary>
    public IncrementalEngine Incremental { get; }

    /// <summary>Gets the parallel evaluation scheduler.</summary>
    public EvaluationScheduler ParallelScheduler { get; }

    /// <summary>Gets the optimization pipeline.</summary>
    public OptimizationPipeline Optimizer { get; }

    /// <summary>Gets the memory tracker.</summary>
    public MemoryTracker Memory { get; }

    /// <summary>Gets the allocation profiler.</summary>
    public AllocationProfiler Allocations { get; }

    /// <summary>Gets the diagnostic reporter.</summary>
    public DiagnosticReporter Diagnostics { get; }

    /// <summary>Gets the benchmark recorder.</summary>
    public BenchmarkRecorder Benchmarks { get; }

    /// <summary>Gets the structured performance logger.</summary>
    public PerformanceLogger Logger { get; }
}
