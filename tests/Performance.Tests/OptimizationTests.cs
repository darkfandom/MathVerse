using MathVerse.Math.Performance.Optimization;
using MathVerse.Math.Performance.Optimization.Passes;

namespace MathVerse.Performance.Tests;

public sealed class OptimizationTests
{
    [Fact]
    public void OptimizationStage_FlagValues_AreCorrect()
    {
        ((int)OptimizationStage.None).Should().Be(0);
        ((int)OptimizationStage.Canonicalization).Should().Be(1);
        ((int)OptimizationStage.ConstantFolding).Should().Be(2);
        ((int)OptimizationStage.CommonSubexpressionElimination).Should().Be(4);
        ((int)OptimizationStage.DeadExpressionElimination).Should().Be(8);
        ((int)OptimizationStage.AlgebraicOptimization).Should().Be(16);
        ((int)OptimizationStage.CacheOptimization).Should().Be(32);
        ((int)OptimizationStage.All).Should().Be(63);
    }

    [Fact]
    public void OptimizationStage_CombinedFlags_CombineCorrectly()
    {
        var combined = OptimizationStage.ConstantFolding | OptimizationStage.AlgebraicOptimization;
        combined.Should().HaveFlag(OptimizationStage.ConstantFolding);
        combined.Should().HaveFlag(OptimizationStage.AlgebraicOptimization);
        combined.Should().NotHaveFlag(OptimizationStage.Canonicalization);
    }

    [Fact]
    public void OptimizationContext_InitialProperties()
    {
        var input = Expr.Literal(42.0);
        var ctx = new OptimizationContext(input, OptimizationStage.ConstantFolding, 0);
        ctx.Input.Should().BeSameAs(input);
        ctx.Stage.Should().Be(OptimizationStage.ConstantFolding);
        ctx.PassNumber.Should().Be(0);
        ctx.HasChanges.Should().BeFalse();
    }

    [Fact]
    public void OptimizationContext_MarkChanged_SetsFlag()
    {
        var ctx = new OptimizationContext(Expr.Literal(1), OptimizationStage.Canonicalization, 1);
        ctx.HasChanges.Should().BeFalse();
        ctx.MarkChanged();
        ctx.HasChanges.Should().BeTrue();
    }

    [Fact]
    public void OptimizationContext_Replace_DifferentNodes_MarksChanged()
    {
        var old_expr = Expr.Literal(1.0);
        var new_expr = Expr.Literal(2.0);
        var ctx = new OptimizationContext(old_expr, OptimizationStage.ConstantFolding, 0);
        var result = ctx.Replace(old_expr, new_expr);
        result.Should().BeSameAs(new_expr);
        ctx.HasChanges.Should().BeTrue();
    }

    [Fact]
    public void OptimizationContext_Replace_SameReference_NoChange()
    {
        var expr = Expr.Literal(1.0);
        var ctx = new OptimizationContext(expr, OptimizationStage.ConstantFolding, 0);
        var result = ctx.Replace(expr, expr);
        result.Should().BeSameAs(expr);
        ctx.HasChanges.Should().BeFalse();
    }

    [Fact]
    public void OptimizationResult_StaticUnchanged()
    {
        var input = Expr.Add(Expr.Literal(1), Expr.Literal(2));
        var result = OptimizationResult.Unchanged(input, OptimizationStage.Canonicalization);
        result.Output.Should().BeSameAs(input);
        result.Stage.Should().Be(OptimizationStage.Canonicalization);
        result.Duration.Should().Be(TimeSpan.Zero);
        result.NodesRemoved.Should().Be(0);
        result.NodesSimplified.Should().Be(0);
        result.HasChanges.Should().BeFalse();
    }

    [Fact]
    public void OptimizationResult_Constructor_AllProperties()
    {
        var output = Expr.Literal(42);
        var result = new OptimizationResult(
            output,
            OptimizationStage.ConstantFolding,
            TimeSpan.FromMilliseconds(5),
            3,
            2,
            true);
        result.Output.Should().BeSameAs(output);
        result.Stage.Should().Be(OptimizationStage.ConstantFolding);
        result.Duration.Should().Be(TimeSpan.FromMilliseconds(5));
        result.NodesRemoved.Should().Be(3);
        result.NodesSimplified.Should().Be(2);
        result.HasChanges.Should().BeTrue();
    }

    [Fact]
    public void OptimizationStatistics_Record_Accumulates()
    {
        var stats = new OptimizationStatistics();
        var r1 = new OptimizationResult(Expr.Literal(1), OptimizationStage.ConstantFolding, TimeSpan.FromMilliseconds(1), 2, 1, true);
        var r2 = new OptimizationResult(Expr.Literal(2), OptimizationStage.Canonicalization, TimeSpan.FromMilliseconds(3), 0, 1, false);
        stats.Record(r1);
        stats.Record(r2);
        stats.TotalPasses.Should().Be(2);
        stats.TotalNodesRemoved.Should().Be(2);
        stats.TotalNodesSimplified.Should().Be(2);
        stats.TotalDuration.Should().Be(TimeSpan.FromMilliseconds(4));
        stats.StagesExecuted.Should().HaveCount(2);
    }

    [Fact]
    public void OptimizationStatistics_Record_Null_Throws()
    {
        var stats = new OptimizationStatistics();
        Action act = () => stats.Record(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void OptimizationStatistics_Record_DuplicateStages_NotDuplicated()
    {
        var stats = new OptimizationStatistics();
        var r1 = new OptimizationResult(Expr.Literal(1), OptimizationStage.ConstantFolding, TimeSpan.Zero, 0, 0, false);
        var r2 = new OptimizationResult(Expr.Literal(2), OptimizationStage.ConstantFolding, TimeSpan.Zero, 1, 0, true);
        stats.Record(r1);
        stats.Record(r2);
        stats.StagesExecuted.Should().HaveCount(1);
        stats.StagesExecuted[0].Should().Be(OptimizationStage.ConstantFolding);
    }

    [Fact]
    public void OptimizationStatistics_ToString_ContainsValues()
    {
        var stats = new OptimizationStatistics();
        var r = new OptimizationResult(Expr.Literal(1), OptimizationStage.ConstantFolding, TimeSpan.FromMilliseconds(10), 5, 3, true);
        stats.Record(r);
        var str = stats.ToString();
        str.Should().Contain("Passes=1");
        str.Should().Contain("NodesRemoved=5");
        str.Should().Contain("NodesSimplified=3");
    }

    [Fact]
    public void ConstantFoldingPass_Add_FoldsCorrectly()
    {
        var pass = new ConstantFoldingPass();
        var input = Expr.Add(Expr.Literal(3), Expr.Literal(4));
        var ctx = new OptimizationContext(input, OptimizationStage.ConstantFolding, 0);
        var result = pass.Optimize(input, ctx);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(7.0);
        ctx.HasChanges.Should().BeTrue();
    }

    [Fact]
    public void ConstantFoldingPass_Subtract_FoldsCorrectly()
    {
        var pass = new ConstantFoldingPass();
        var input = Expr.Subtract(Expr.Literal(10), Expr.Literal(3));
        var ctx = new OptimizationContext(input, OptimizationStage.ConstantFolding, 0);
        var result = pass.Optimize(input, ctx);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(7.0);
    }

    [Fact]
    public void ConstantFoldingPass_Multiply_FoldsCorrectly()
    {
        var pass = new ConstantFoldingPass();
        var input = Expr.Multiply(Expr.Literal(6), Expr.Literal(7));
        var ctx = new OptimizationContext(input, OptimizationStage.ConstantFolding, 0);
        var result = pass.Optimize(input, ctx);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(42.0);
    }

    [Fact]
    public void ConstantFoldingPass_Divide_FoldsCorrectly()
    {
        var pass = new ConstantFoldingPass();
        var input = Expr.Divide(Expr.Literal(20), Expr.Literal(4));
        var ctx = new OptimizationContext(input, OptimizationStage.ConstantFolding, 0);
        var result = pass.Optimize(input, ctx);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(5.0);
    }

    [Fact]
    public void ConstantFoldingPass_Power_FoldsCorrectly()
    {
        var pass = new ConstantFoldingPass();
        var input = Expr.Pow(Expr.Literal(2), Expr.Literal(3));
        var ctx = new OptimizationContext(input, OptimizationStage.ConstantFolding, 0);
        var result = pass.Optimize(input, ctx);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(8.0);
    }

    [Fact]
    public void ConstantFoldingPass_Modulo_FoldsCorrectly()
    {
        var pass = new ConstantFoldingPass();
        var input = Expr.Modulo(Expr.Literal(10), Expr.Literal(3));
        var ctx = new OptimizationContext(input, OptimizationStage.ConstantFolding, 0);
        var result = pass.Optimize(input, ctx);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(1.0);
    }

    [Fact]
    public void ConstantFoldingPass_Negate_FoldsCorrectly()
    {
        var pass = new ConstantFoldingPass();
        var input = Expr.Negate(Expr.Literal(5));
        var ctx = new OptimizationContext(input, OptimizationStage.ConstantFolding, 0);
        var result = pass.Optimize(input, ctx);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(-5.0);
    }

    [Fact]
    public void ConstantFoldingPass_NoLiteralChildren_NoChange()
    {
        var pass = new ConstantFoldingPass();
        var x = Expr.Variable("x");
        var input = Expr.Add(x, Expr.Literal(1));
        var ctx = new OptimizationContext(input, OptimizationStage.ConstantFolding, 0);
        var result = pass.Optimize(input, ctx);
        result.Should().BeOfType<BinaryExpression>();
        ctx.HasChanges.Should().BeFalse();
    }

    [Fact]
    public void ConstantFoldingPass_NestedConstants_FoldRecursively()
    {
        var pass = new ConstantFoldingPass();
        var inner = Expr.Add(Expr.Literal(2), Expr.Literal(3));
        var input = Expr.Multiply(inner, Expr.Literal(4));
        var ctx = new OptimizationContext(input, OptimizationStage.ConstantFolding, 0);
        var result = pass.Optimize(input, ctx);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(20.0);
    }

    [Fact]
    public void AlgebraicSimplificationPass_AddZero()
    {
        var pass = new AlgebraicSimplificationPass();
        var x = Expr.Variable("x");
        var input = Expr.Add(x, Expr.Literal(0));
        var ctx = new OptimizationContext(input, OptimizationStage.AlgebraicOptimization, 0);
        var result = pass.Optimize(input, ctx);
        result.Should().BeSameAs(x);
        ctx.HasChanges.Should().BeTrue();
    }

    [Fact]
    public void AlgebraicSimplificationPass_ZeroAdd()
    {
        var pass = new AlgebraicSimplificationPass();
        var x = Expr.Variable("x");
        var input = Expr.Add(Expr.Literal(0), x);
        var ctx = new OptimizationContext(input, OptimizationStage.AlgebraicOptimization, 0);
        var result = pass.Optimize(input, ctx);
        result.Should().BeSameAs(x);
    }

    [Fact]
    public void AlgebraicSimplificationPass_MultiplyOne()
    {
        var pass = new AlgebraicSimplificationPass();
        var x = Expr.Variable("x");
        var input = Expr.Multiply(x, Expr.Literal(1));
        var ctx = new OptimizationContext(input, OptimizationStage.AlgebraicOptimization, 0);
        var result = pass.Optimize(input, ctx);
        result.Should().BeSameAs(x);
        ctx.HasChanges.Should().BeTrue();
    }

    [Fact]
    public void AlgebraicSimplificationPass_MultiplyZero()
    {
        var pass = new AlgebraicSimplificationPass();
        var x = Expr.Variable("x");
        var input = Expr.Multiply(x, Expr.Literal(0));
        var ctx = new OptimizationContext(input, OptimizationStage.AlgebraicOptimization, 0);
        var result = pass.Optimize(input, ctx);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(0);
    }

    [Fact]
    public void AlgebraicSimplificationPass_PowerZero()
    {
        var pass = new AlgebraicSimplificationPass();
        var x = Expr.Variable("x");
        var input = Expr.Pow(x, Expr.Literal(0));
        var ctx = new OptimizationContext(input, OptimizationStage.AlgebraicOptimization, 0);
        var result = pass.Optimize(input, ctx);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(1);
    }

    [Fact]
    public void AlgebraicSimplificationPass_PowerOne()
    {
        var pass = new AlgebraicSimplificationPass();
        var x = Expr.Variable("x");
        var input = Expr.Pow(x, Expr.Literal(1));
        var ctx = new OptimizationContext(input, OptimizationStage.AlgebraicOptimization, 0);
        var result = pass.Optimize(input, ctx);
        result.Should().BeSameAs(x);
    }

    [Fact]
    public void AlgebraicSimplificationPass_SubtractSelf()
    {
        var pass = new AlgebraicSimplificationPass();
        var x = Expr.Variable("x");
        var input = Expr.Subtract(x, x);
        var ctx = new OptimizationContext(input, OptimizationStage.AlgebraicOptimization, 0);
        var result = pass.Optimize(input, ctx);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(0);
    }

    [Fact]
    public void AlgebraicSimplificationPass_DivideSelf()
    {
        var pass = new AlgebraicSimplificationPass();
        var x = Expr.Variable("x");
        var input = Expr.Divide(x, x);
        var ctx = new OptimizationContext(input, OptimizationStage.AlgebraicOptimization, 0);
        var result = pass.Optimize(input, ctx);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(1);
    }

    [Fact]
    public void CanonicalizationPass_NegateLiteral_FoldsToLiteral()
    {
        var pass = new CanonicalizationPass();
        var input = Expr.Negate(Expr.Literal(5));
        var ctx = new OptimizationContext(input, OptimizationStage.Canonicalization, 0);
        var result = pass.Optimize(input, ctx);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(-5);
        ctx.HasChanges.Should().BeTrue();
    }

    [Fact]
    public void CanonicalizationPass_LeafExpression_Unchanged()
    {
        var pass = new CanonicalizationPass();
        var input = Expr.Literal(42);
        var ctx = new OptimizationContext(input, OptimizationStage.Canonicalization, 0);
        var result = pass.Optimize(input, ctx);
        result.Should().BeSameAs(input);
        ctx.HasChanges.Should().BeFalse();
    }

    [Fact]
    public void CanonicalizationPass_NonCommutative_PreservesOrder()
    {
        var pass = new CanonicalizationPass();
        var input = Expr.Subtract(Expr.Variable("a"), Expr.Variable("b"));
        var ctx = new OptimizationContext(input, OptimizationStage.Canonicalization, 0);
        var result = pass.Optimize(input, ctx);
        result.Should().BeOfType<BinaryExpression>();
    }

    [Fact]
    public void CommonSubexpressionEliminationPass_DuplicateSubexpr_ReusesFirst()
    {
        var pass = new CommonSubexpressionEliminationPass();
        var sub = Expr.Add(Expr.Literal(1), Expr.Literal(2));
        var input = Expr.Multiply(sub, sub);
        var ctx = new OptimizationContext(input, OptimizationStage.CommonSubexpressionElimination, 0);
        var result = pass.Optimize(input, ctx);
        result.Should().BeOfType<BinaryExpression>();
    }

    [Fact]
    public void CommonSubexpressionEliminationPass_NoDuplicates_NoChange()
    {
        var pass = new CommonSubexpressionEliminationPass();
        var a = Expr.Add(Expr.Literal(1), Expr.Literal(2));
        var b = Expr.Add(Expr.Literal(3), Expr.Literal(4));
        var input = Expr.Multiply(a, b);
        var ctx = new OptimizationContext(input, OptimizationStage.CommonSubexpressionElimination, 0);
        pass.Optimize(input, ctx);
    }

    [Fact]
    public void DeadExpressionEliminationPass_SimpleExpression_NoChange()
    {
        var pass = new DeadExpressionEliminationPass();
        var input = Expr.Literal(42);
        var ctx = new OptimizationContext(input, OptimizationStage.DeadExpressionElimination, 0);
        var result = pass.Optimize(input, ctx);
        result.Should().BeSameAs(input);
    }

    [Fact]
    public void OptimizationPipeline_EmptyPipeline_ReturnsInput()
    {
        var pipeline = new OptimizationPipeline();
        var input = Expr.Add(Expr.Literal(1), Expr.Literal(2));
        var result = pipeline.Optimize(input);
        result.Should().Be(input);
    }

    [Fact]
    public void OptimizationPipeline_AddPass_NullThrows()
    {
        var pipeline = new OptimizationPipeline();
        Action act = () => pipeline.AddPass(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void OptimizationPipeline_WithConstantFolding_Folds()
    {
        var pipeline = new OptimizationPipeline();
        pipeline.AddPass(new ConstantFoldingPass());
        var input = Expr.Add(Expr.Literal(3), Expr.Literal(4));
        var result = pipeline.Optimize(input);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(7);
    }

    [Fact]
    public void OptimizationPipeline_StageFilter_OnlyRunsMatchingStages()
    {
        var pipeline = new OptimizationPipeline();
        pipeline.AddPass(new ConstantFoldingPass());
        pipeline.AddPass(new AlgebraicSimplificationPass());
        var input = Expr.Add(Expr.Literal(3), Expr.Literal(0));
        var result = pipeline.Optimize(input, OptimizationStage.ConstantFolding);
        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(3);
    }

    [Fact]
    public void OptimizationPipeline_ClearPasses_EmptiesList()
    {
        var pipeline = new OptimizationPipeline();
        pipeline.AddPass(new ConstantFoldingPass());
        pipeline.AddPass(new AlgebraicSimplificationPass());
        pipeline.Passes.Should().HaveCount(2);
        pipeline.ClearPasses();
        pipeline.Passes.Should().BeEmpty();
    }

    [Fact]
    public void OptimizationPipeline_Statistics_AreRecorded()
    {
        var pipeline = new OptimizationPipeline();
        pipeline.AddPass(new ConstantFoldingPass());
        var input = Expr.Add(Expr.Literal(1), Expr.Literal(2));
        pipeline.Optimize(input);
        pipeline.Statistics.TotalPasses.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public void OptimizationPipeline_MultiplePasses_OrderedByStage()
    {
        var pipeline = new OptimizationPipeline();
        pipeline.AddPass(new AlgebraicSimplificationPass());
        pipeline.AddPass(new ConstantFoldingPass());
        var passNames = pipeline.Passes.Select(p => p.Name).ToList();
        passNames.Should().ContainInOrder("ConstantFolding", "AlgebraicSimplification");
    }

    [Fact]
    public void OptimizationPipeline_Optimize_NullInput_Throws()
    {
        var pipeline = new OptimizationPipeline();
        Action act = () => pipeline.Optimize(null!);
        act.Should().NotThrow();
    }

    [Fact]
    public void ConstantFoldingPass_Properties()
    {
        var pass = new ConstantFoldingPass();
        pass.Name.Should().Be("ConstantFolding");
        pass.Stage.Should().Be(OptimizationStage.ConstantFolding);
        pass.Order.Should().Be(0);
    }

    [Fact]
    public void AlgebraicSimplificationPass_Properties()
    {
        var pass = new AlgebraicSimplificationPass();
        pass.Name.Should().Be("AlgebraicSimplification");
        pass.Stage.Should().Be(OptimizationStage.AlgebraicOptimization);
        pass.Order.Should().Be(0);
    }

    [Fact]
    public void CommonSubexpressionEliminationPass_Properties()
    {
        var pass = new CommonSubexpressionEliminationPass();
        pass.Name.Should().Be("CommonSubexpressionElimination");
        pass.Stage.Should().Be(OptimizationStage.CommonSubexpressionElimination);
        pass.Order.Should().Be(0);
    }

    [Fact]
    public void DeadExpressionEliminationPass_Properties()
    {
        var pass = new DeadExpressionEliminationPass();
        pass.Name.Should().Be("DeadExpressionElimination");
        pass.Stage.Should().Be(OptimizationStage.DeadExpressionElimination);
        pass.Order.Should().Be(0);
    }

    [Fact]
    public void CanonicalizationPass_Properties()
    {
        var pass = new CanonicalizationPass();
        pass.Name.Should().Be("Canonicalization");
        pass.Stage.Should().Be(OptimizationStage.Canonicalization);
        pass.Order.Should().Be(0);
    }
}
