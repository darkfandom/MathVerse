using BenchmarkDotNet.Attributes;
using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;
using MathVerse.Math.Performance.Optimization.Passes;

namespace MathVerse.Performance.Tests.Benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class OptimizationBenchmarks
{
    private ConstantFoldingPass _constantFolding = null!;
    private AlgebraicSimplificationPass _algebraicSimplify = null!;
    private CommonSubexpressionEliminationPass _cse = null!;
    private DeadExpressionEliminationPass _deadElimination = null!;
    private CanonicalizationPass _canonicalization = null!;
    private OptimizationPipeline _fullPipeline = null!;

    private Expression _foldableExpr = null!;
    private Expression _algebraicExpr = null!;
    private Expression _cseExpr = null!;
    private Expression _complexExpr = null!;
    private Expression _wideExpr = null!;

    [Params(5, 20)]
    public int TreeSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _constantFolding = new ConstantFoldingPass();
        _algebraicSimplify = new AlgebraicSimplificationPass();
        _cse = new CommonSubexpressionEliminationPass();
        _deadElimination = new DeadExpressionEliminationPass();
        _canonicalization = new CanonicalizationPass();

        _foldableExpr = Expr.Add(
            Expr.Multiply(Expr.Literal(2.0), Expr.Literal(3.0)),
            Expr.Pow(Expr.Literal(4.0), Expr.Literal(2.0)));

        _algebraicExpr = Expr.Add(
            Expr.Multiply(Expr.Variable("x"), Expr.Literal(1.0)),
            Expr.Multiply(Expr.Variable("y"), Expr.Literal(0.0)));

        var shared = Expr.Add(Expr.Variable("x"), Expr.Literal(1.0));
        _cseExpr = Expr.Add(
            Expr.Multiply(shared, shared),
            Expr.Add(shared, shared));

        Expression complex = Expr.Literal(0.0);
        for (var i = 0; i < TreeSize; i++)
            complex = Expr.Add(
                Expr.Multiply(Expr.Variable($"x{i}"), Expr.Literal(i + 1)),
                Expr.Pow(Expr.Variable($"y{i}"), Expr.Literal(2.0)));
        _complexExpr = complex;

        Expression wide = Expr.Literal(0.0);
        for (var i = 0; i < TreeSize; i++)
            wide = Expr.Add(wide, Expr.Add(
                Expr.Multiply(Expr.Literal(i), Expr.Literal(i + 1)),
                Expr.Literal(0.0)));
        _wideExpr = wide;

        _fullPipeline = new OptimizationPipeline();
        _fullPipeline.AddPass(new CanonicalizationPass());
        _fullPipeline.AddPass(new ConstantFoldingPass());
        _fullPipeline.AddPass(new CommonSubexpressionEliminationPass());
        _fullPipeline.AddPass(new DeadExpressionEliminationPass());
        _fullPipeline.AddPass(new AlgebraicSimplificationPass());
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("IndividualPasses")]
    public Expression ConstantFolding_Small()
    {
        var ctx = new OptimizationContext(_foldableExpr, OptimizationStage.ConstantFolding, 0);
        return _constantFolding.Optimize(_foldableExpr, ctx);
    }

    [Benchmark]
    [BenchmarkCategory("IndividualPasses")]
    public Expression ConstantFolding_Complex()
    {
        var ctx = new OptimizationContext(_complexExpr, OptimizationStage.ConstantFolding, 0);
        return _constantFolding.Optimize(_complexExpr, ctx);
    }

    [Benchmark]
    [BenchmarkCategory("IndividualPasses")]
    public Expression AlgebraicSimplify_Small()
    {
        var ctx = new OptimizationContext(_algebraicExpr, OptimizationStage.AlgebraicOptimization, 0);
        return _algebraicSimplify.Optimize(_algebraicExpr, ctx);
    }

    [Benchmark]
    [BenchmarkCategory("IndividualPasses")]
    public Expression AlgebraicSimplify_Complex()
    {
        var ctx = new OptimizationContext(_complexExpr, OptimizationStage.AlgebraicOptimization, 0);
        return _algebraicSimplify.Optimize(_complexExpr, ctx);
    }

    [Benchmark]
    [BenchmarkCategory("IndividualPasses")]
    public Expression CSE_Small()
    {
        var ctx = new OptimizationContext(_cseExpr, OptimizationStage.CommonSubexpressionElimination, 0);
        return _cse.Optimize(_cseExpr, ctx);
    }

    [Benchmark]
    [BenchmarkCategory("IndividualPasses")]
    public Expression CSE_Complex()
    {
        var ctx = new OptimizationContext(_complexExpr, OptimizationStage.CommonSubexpressionElimination, 0);
        return _cse.Optimize(_complexExpr, ctx);
    }

    [Benchmark]
    [BenchmarkCategory("IndividualPasses")]
    public Expression DeadElimination_Small()
    {
        var ctx = new OptimizationContext(_cseExpr, OptimizationStage.DeadExpressionElimination, 0);
        return _deadElimination.Optimize(_cseExpr, ctx);
    }

    [Benchmark]
    [BenchmarkCategory("IndividualPasses")]
    public Expression DeadElimination_Complex()
    {
        var ctx = new OptimizationContext(_complexExpr, OptimizationStage.DeadExpressionElimination, 0);
        return _deadElimination.Optimize(_complexExpr, ctx);
    }

    [Benchmark]
    [BenchmarkCategory("IndividualPasses")]
    public Expression Canonicalization_Small()
    {
        var ctx = new OptimizationContext(_algebraicExpr, OptimizationStage.Canonicalization, 0);
        return _canonicalization.Optimize(_algebraicExpr, ctx);
    }

    [Benchmark]
    [BenchmarkCategory("IndividualPasses")]
    public Expression Canonicalization_Complex()
    {
        var ctx = new OptimizationContext(_complexExpr, OptimizationStage.Canonicalization, 0);
        return _canonicalization.Optimize(_complexExpr, ctx);
    }

    [Benchmark]
    [BenchmarkCategory("Pipeline")]
    public Expression FullPipeline_Small()
    {
        return _fullPipeline.Optimize(_foldableExpr);
    }

    [Benchmark]
    [BenchmarkCategory("Pipeline")]
    public Expression FullPipeline_Complex()
    {
        return _fullPipeline.Optimize(_complexExpr);
    }

    [Benchmark]
    [BenchmarkCategory("Pipeline")]
    public Expression FullPipeline_Wide()
    {
        return _fullPipeline.Optimize(_wideExpr);
    }

    [Benchmark]
    [BenchmarkCategory("Pipeline")]
    public Expression Pipeline_WithStageFilter()
    {
        return _fullPipeline.Optimize(_complexExpr, OptimizationStage.ConstantFolding | OptimizationStage.AlgebraicOptimization);
    }

    [Benchmark]
    [BenchmarkCategory("Pipeline")]
    public OptimizationStatistics Pipeline_GetStatistics()
    {
        _fullPipeline.ClearPasses();
        _fullPipeline.AddPass(new ConstantFoldingPass());
        _fullPipeline.AddPass(new AlgebraicSimplificationPass());
        _fullPipeline.AddPass(new CommonSubexpressionEliminationPass());
        for (var i = 0; i < 10; i++)
            _fullPipeline.Optimize(_complexExpr);
        return _fullPipeline.Statistics;
    }

    [Benchmark]
    [BenchmarkCategory("Context")]
    public void OptimizationContext_MarkChanged()
    {
        var ctx = new OptimizationContext(_complexExpr, OptimizationStage.ConstantFolding, 0);
        ctx.MarkChanged();
        _ = ctx.HasChanges;
    }

    [Benchmark]
    [BenchmarkCategory("Context")]
    public Expression OptimizationContext_Replace()
    {
        var ctx = new OptimizationContext(_complexExpr, OptimizationStage.ConstantFolding, 0);
        return ctx.Replace(_foldableExpr, Expr.Literal(42.0));
    }

    [Benchmark]
    [BenchmarkCategory("Repeated")]
    public Expression Pipeline_RepeatedOptimization()
    {
        var pipeline = new OptimizationPipeline();
        pipeline.AddPass(new ConstantFoldingPass());
        pipeline.AddPass(new AlgebraicSimplificationPass());
        Expression current = _wideExpr;
        for (var i = 0; i < 5; i++)
            current = pipeline.Optimize(current);
        return current;
    }
}
