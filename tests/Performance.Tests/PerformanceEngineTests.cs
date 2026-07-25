using MathVerse.Math.Performance;
using MathVerse.Math.Performance.Metrics;
using MathVerse.Math.Performance.Diagnostics;
using MathVerse.Math.Performance.Optimization;

namespace MathVerse.Performance.Tests;

public sealed class PerformanceEngineTests
{
    [Fact]
    public void PerformanceEngine_Create_ReturnsInstance()
    {
        var engine = PerformanceEngine.Create();
        engine.Should().NotBeNull();
        engine.Services.Should().NotBeNull();
    }

    [Fact]
    public void PerformanceEngine_Create_WithOptions()
    {
        var options = new PerformanceOptions { EnableOptimization = false };
        var engine = PerformanceEngine.Create(options);
        engine.Should().NotBeNull();
    }

    [Fact]
    public void PerformanceEngine_Create_NullOptions_Throws()
    {
        Action act = () => PerformanceEngine.Create(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PerformanceEngine_Intern_ReturnsCanonicalInstance()
    {
        var engine = PerformanceEngine.Create();
        var a = Expr.Add(Expr.Literal(1), Expr.Literal(2));
        var b = Expr.Add(Expr.Literal(1), Expr.Literal(2));
        var internedA = engine.Intern(a);
        var internedB = engine.Intern(b);
        internedA.Should().Be(internedB);
    }

    [Fact]
    public void PerformanceEngine_Intern_Null_Throws()
    {
        var engine = PerformanceEngine.Create();
        Action act = () => engine.Intern(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PerformanceEngine_Optimize_FoldsConstants()
    {
        var engine = PerformanceEngine.Create();
        var input = Expr.Add(Expr.Literal(3), Expr.Literal(4));
        var result = engine.Optimize(input);
        result.Should().BeOfType<BinaryExpression>();
    }

    [Fact]
    public void PerformanceEngine_Optimize_Null_Throws()
    {
        var engine = PerformanceEngine.Create();
        Action act = () => engine.Optimize(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PerformanceEngine_Optimize_Disabled_ReturnsInput()
    {
        var options = new PerformanceOptions { EnableOptimization = false };
        var engine = PerformanceEngine.Create(options);
        var input = Expr.Add(Expr.Literal(1), Expr.Literal(2));
        var result = engine.Optimize(input);
        result.Should().BeSameAs(input);
    }

    [Fact]
    public void PerformanceEngine_HashExpression_ReturnsHash()
    {
        var engine = PerformanceEngine.Create();
        var expr = Expr.Add(Expr.Literal(1), Expr.Literal(2));
        var hash = engine.HashExpression(expr);
        hash.Should().NotBe(0);
    }

    [Fact]
    public void PerformanceEngine_HashExpression_SameExpression_SameHash()
    {
        var engine = PerformanceEngine.Create();
        var a = Expr.Add(Expr.Literal(1), Expr.Literal(2));
        var b = Expr.Add(Expr.Literal(1), Expr.Literal(2));
        engine.HashExpression(a).Should().Be(engine.HashExpression(b));
    }

    [Fact]
    public void PerformanceEngine_HashExpression_Null_Throws()
    {
        var engine = PerformanceEngine.Create();
        Action act = () => engine.HashExpression(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PerformanceEngine_InvalidateCaches_DoesNotThrow()
    {
        var engine = PerformanceEngine.Create();
        engine.Intern(Expr.Literal(1));
        engine.Optimize(Expr.Add(Expr.Literal(1), Expr.Literal(2)));
        Action act = () => engine.InvalidateCaches();
        act.Should().NotThrow();
    }

    [Fact]
    public void PerformanceEngine_GetReport_ReturnsReport()
    {
        var engine = PerformanceEngine.Create();
        var report = engine.GetReport();
        report.Should().NotBeNull();
        report.Snapshot.Should().NotBeNull();
    }

    [Fact]
    public void PerformanceEngine_GetReport_AfterOperations_HasData()
    {
        var engine = PerformanceEngine.Create();
        engine.Intern(Expr.Literal(1));
        engine.Optimize(Expr.Add(Expr.Literal(1), Expr.Literal(2)));
        var report = engine.GetReport();
        report.Snapshot.TotalOperations.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void PerformanceOptions_DefaultValues()
    {
        var opts = new PerformanceOptions();
        opts.InterningCapacity.Should().Be(4096);
        opts.EvaluationCacheCapacity.Should().Be(1024);
        opts.RewriteCacheCapacity.Should().Be(1024);
        opts.EnableIncrementalEvaluation.Should().BeTrue();
        opts.EnableParallelExecution.Should().BeFalse();
        opts.EnableDiagnostics.Should().BeTrue();
        opts.EnableOptimization.Should().BeTrue();
        opts.EnabledOptimizationStages.Should().Be(OptimizationStage.All);
    }

    [Fact]
    public void PerformanceOptions_Default_Singleton()
    {
        PerformanceOptions.Default.Should().NotBeNull();
    }

    [Fact]
    public void PerformanceServices_Constructor_CreatesAllServices()
    {
        var options = new PerformanceOptions();
        var services = new PerformanceServices(options);
        services.Interner.Should().NotBeNull();
        services.Hasher.Should().NotBeNull();
        services.Pool.Should().NotBeNull();
        services.EvaluationCache.Should().NotBeNull();
        services.RewriteCache.Should().NotBeNull();
        services.SimplificationCache.Should().NotBeNull();
        services.TypeCache.Should().NotBeNull();
        services.Memoization.Should().NotBeNull();
        services.Incremental.Should().NotBeNull();
        services.ParallelScheduler.Should().NotBeNull();
        services.Optimizer.Should().NotBeNull();
        services.Memory.Should().NotBeNull();
        services.Allocations.Should().NotBeNull();
        services.Diagnostics.Should().NotBeNull();
        services.Benchmarks.Should().NotBeNull();
        services.Logger.Should().NotBeNull();
    }

    [Fact]
    public void PerformanceServices_Constructor_Null_Throws()
    {
        Action act = () => new PerformanceServices(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PerformanceServices_DiagnosticsDisabled_HighSeverity()
    {
        var options = new PerformanceOptions { EnableDiagnostics = false };
        var services = new PerformanceServices(options);
        ((int)services.Diagnostics.MinimumSeverity).Should().BeGreaterThan(0);
    }

    [Fact]
    public void PerformanceConfiguration_DefaultBuild()
    {
        var config = new PerformanceConfiguration();
        var options = config.Build();
        options.Should().NotBeNull();
        options.InterningCapacity.Should().Be(4096);
        options.EnableDiagnostics.Should().BeTrue();
    }

    [Fact]
    public void PerformanceConfiguration_SetInterningCapacity()
    {
        var options = new PerformanceConfiguration()
            .SetInterningCapacity(512)
            .Build();
        options.InterningCapacity.Should().Be(512);
    }

    [Fact]
    public void PerformanceConfiguration_SetInterningCapacity_Zero_Throws()
    {
        var config = new PerformanceConfiguration();
        Action act = () => config.SetInterningCapacity(0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void PerformanceConfiguration_SetCacheCapacity()
    {
        var options = new PerformanceConfiguration()
            .SetCacheCapacity(256)
            .Build();
        options.EvaluationCacheCapacity.Should().Be(256);
        options.RewriteCacheCapacity.Should().Be(256);
    }

    [Fact]
    public void PerformanceConfiguration_SetCacheCapacity_Zero_Throws()
    {
        var config = new PerformanceConfiguration();
        Action act = () => config.SetCacheCapacity(0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void PerformanceConfiguration_EnableParallelism()
    {
        var options = new PerformanceConfiguration()
            .EnableParallelism(8)
            .Build();
        options.EnableParallelExecution.Should().BeTrue();
        options.MaxDegreeOfParallelism.Should().Be(8);
    }

    [Fact]
    public void PerformanceConfiguration_EnableParallelism_DefaultProcessorCount()
    {
        var options = new PerformanceConfiguration()
            .EnableParallelism()
            .Build();
        options.EnableParallelExecution.Should().BeTrue();
        options.MaxDegreeOfParallelism.Should().Be(Environment.ProcessorCount);
    }

    [Fact]
    public void PerformanceConfiguration_EnableOptimizationPipeline()
    {
        var options = new PerformanceConfiguration()
            .EnableOptimizationPipeline(OptimizationStage.ConstantFolding)
            .Build();
        options.EnableOptimization.Should().BeTrue();
        options.EnabledOptimizationStages.Should().Be(OptimizationStage.ConstantFolding);
    }

    [Fact]
    public void PerformanceConfiguration_DisableDiagnostics()
    {
        var options = new PerformanceConfiguration()
            .DisableDiagnostics()
            .Build();
        options.EnableDiagnostics.Should().BeFalse();
    }

    [Fact]
    public void PerformanceConfiguration_UseDefaults_ResetsValues()
    {
        var options = new PerformanceConfiguration()
            .SetInterningCapacity(100)
            .DisableDiagnostics()
            .UseDefaults()
            .Build();
        options.InterningCapacity.Should().Be(4096);
        options.EnableDiagnostics.Should().BeTrue();
    }

    [Fact]
    public void PerformanceConfiguration_FluentChaining()
    {
        var options = new PerformanceConfiguration()
            .SetInterningCapacity(128)
            .SetCacheCapacity(64)
            .EnableParallelism(2)
            .EnableOptimizationPipeline(OptimizationStage.All)
            .DisableDiagnostics()
            .Build();
        options.InterningCapacity.Should().Be(128);
        options.EvaluationCacheCapacity.Should().Be(64);
        options.RewriteCacheCapacity.Should().Be(64);
        options.EnableParallelExecution.Should().BeTrue();
        options.MaxDegreeOfParallelism.Should().Be(2);
        options.EnableOptimization.Should().BeTrue();
        options.EnableDiagnostics.Should().BeFalse();
    }

    [Fact]
    public void PerformanceEngine_FullWorkflow_InternOptimizeHashReport()
    {
        var engine = PerformanceEngine.Create();
        var expr = Expr.Add(Expr.Multiply(Expr.Literal(2), Expr.Literal(3)), Expr.Literal(4));
        var interned = engine.Intern(expr);
        var optimized = engine.Optimize(interned);
        var hash = engine.HashExpression(optimized);
        var report = engine.GetReport();
        hash.Should().NotBe(0);
        report.Should().NotBeNull();
    }

    [Fact]
    public void PerformanceEngine_RepeatedOptimizeConsistency()
    {
        var engine = PerformanceEngine.Create();
        var input = Expr.Add(Expr.Literal(5), Expr.Literal(3));
        var result1 = engine.Optimize(input);
        var result2 = engine.Optimize(input);
        result1.Should().Be(result2);
    }

    [Fact]
    public void PerformanceEngine_Evaluate_ThroughEngine()
    {
        var engine = PerformanceEngine.Create();
        var input = Expr.Literal(42);
        var result = engine.Evaluate(input);
        result.Should().NotBeNull();
    }

    [Fact]
    public void PerformanceEngine_Evaluate_Null_Throws()
    {
        var engine = PerformanceEngine.Create();
        Action act = () => engine.Evaluate(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PerformanceEngine_Optimize_VariousExpressions()
    {
        var engine = PerformanceEngine.Create();
        var add = engine.Optimize(Expr.Add(Expr.Literal(1), Expr.Literal(2)));
        add.Should().BeOfType<BinaryExpression>();
        var mul = engine.Optimize(Expr.Multiply(Expr.Literal(3), Expr.Literal(4)));
        mul.Should().BeOfType<BinaryExpression>();
        var sub = engine.Optimize(Expr.Subtract(Expr.Literal(10), Expr.Literal(3)));
        sub.Should().BeOfType<BinaryExpression>();
    }

    [Fact]
    public void PerformanceEngine_ConcurrentOptimize_ThreadSafe()
    {
        var engine = PerformanceEngine.Create();
        var results = new System.Collections.Concurrent.ConcurrentBag<Expression>();
        Parallel.For(0, 100, i =>
        {
            var expr = Expr.Add(Expr.Literal(i), Expr.Literal(i));
            var result = engine.Optimize(expr);
            results.Add(result);
        });
        results.Should().HaveCount(100);
    }

    [Fact]
    public void PerformanceEngine_GetReport_EmptyEngine()
    {
        var engine = PerformanceEngine.Create();
        var report = engine.GetReport();
        report.Snapshot.TotalOperations.Should().Be(0);
        report.SlowestOperations.Should().BeEmpty();
    }

    [Fact]
    public void PerformanceEngine_InvalidateCaches_ThenIntern()
    {
        var engine = PerformanceEngine.Create();
        engine.Intern(Expr.Literal(1));
        engine.InvalidateCaches();
        var result = engine.Intern(Expr.Literal(1));
        result.Should().NotBeNull();
    }
}
