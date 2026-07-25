using BenchmarkDotNet.Attributes;
using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;

namespace MathVerse.Performance.Tests.Benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class IncrementalBenchmarks
{
    private DependencyTracker _dependencyTracker = null!;
    private IncrementalEngine _incrementalEngine = null!;
    private IncrementalEvaluator _evaluator = null!;
    private InvalidationGraph _invalidationGraph = null!;
    private Expression _simpleExpr = null!;
    private Expression _complexExpr = null!;
    private int[] _nodeIds = null!;

    [Params(50, 500)]
    public int NodeCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _dependencyTracker = new DependencyTracker();
        _incrementalEngine = new IncrementalEngine();
        _evaluator = new IncrementalEvaluator();
        _invalidationGraph = new InvalidationGraph();
        _invalidationGraph.SetTracker(_dependencyTracker);

        _simpleExpr = Expr.Add(Expr.Variable("x"), Expr.Literal(1.0));
        _complexExpr = Expr.Add(
            Expr.Multiply(Expr.Variable("x"), Expr.Pow(Expr.Variable("y"), Expr.Literal(3.0))),
            Expr.Sin(Expr.Variable("z")));

        _nodeIds = new int[NodeCount];
        for (var i = 0; i < NodeCount; i++)
            _nodeIds[i] = _dependencyTracker.AddNode($"node_{i}");
        for (var i = 1; i < NodeCount; i++)
            _dependencyTracker.AddDependency(_nodeIds[i], _nodeIds[i - 1]);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("DependencyTracking")]
    public int DependencyTracker_AddNodes()
    {
        var tracker = new DependencyTracker();
        for (var i = 0; i < NodeCount; i++)
            tracker.AddNode($"n{i}");
        return tracker.NodeCount;
    }

    [Benchmark]
    [BenchmarkCategory("DependencyTracking")]
    public int DependencyTracker_AddNodesWithEdges()
    {
        var tracker = new DependencyTracker();
        var ids = new int[NodeCount];
        for (var i = 0; i < NodeCount; i++)
            ids[i] = tracker.AddNode($"n{i}");
        for (var i = 1; i < NodeCount; i++)
            tracker.AddDependency(ids[i], ids[i - 1]);
        return tracker.NodeCount;
    }

    [Benchmark]
    [BenchmarkCategory("DependencyTracking")]
    public IReadOnlyList<int> DependencyTracker_GetDirtyNodes()
    {
        _dependencyTracker.MarkDirty(_nodeIds[0]);
        return _dependencyTracker.GetDirtyNodes();
    }

    [Benchmark]
    [BenchmarkCategory("DependencyTracking")]
    public void DependencyTracker_MarkDirty()
    {
        _dependencyTracker.MarkAllClean();
        _dependencyTracker.MarkDirty(_nodeIds[0]);
    }

    [Benchmark]
    [BenchmarkCategory("DependencyTracking")]
    public void DependencyTracker_RemoveNode()
    {
        var tracker = new DependencyTracker();
        var ids = new int[NodeCount];
        for (var i = 0; i < NodeCount; i++)
            ids[i] = tracker.AddNode($"n{i}");
        for (var i = 1; i < NodeCount; i++)
            tracker.AddDependency(ids[i], ids[i - 1]);
        for (var i = 0; i < NodeCount; i++)
            tracker.RemoveNode(ids[i]);
    }

    [Benchmark]
    [BenchmarkCategory("Invalidation")]
    public ChangeSet InvalidationGraph_Propagate_Linear()
    {
        _dependencyTracker.MarkAllClean();
        _dependencyTracker.MarkDirty(_nodeIds[0]);
        var dirtyNodes = _dependencyTracker.GetDirtyNodes();
        var changeSet = new ChangeSet(new HashSet<int>(dirtyNodes), new HashSet<int>(dirtyNodes));
        return _invalidationGraph.Propagate(changeSet);
    }

    [Benchmark]
    [BenchmarkCategory("Invalidation")]
    public ChangeSet InvalidationGraph_Propagate_Middle()
    {
        _dependencyTracker.MarkAllClean();
        var midIndex = NodeCount / 2;
        _dependencyTracker.MarkDirty(_nodeIds[midIndex]);
        var dirtyNodes = _dependencyTracker.GetDirtyNodes();
        var changeSet = new ChangeSet(new HashSet<int>(dirtyNodes), new HashSet<int>(dirtyNodes));
        return _invalidationGraph.Propagate(changeSet);
    }

    [Benchmark]
    [BenchmarkCategory("Invalidation")]
    public void InvalidationGraph_MultipleDirtyNodes()
    {
        _dependencyTracker.MarkAllClean();
        for (var i = 0; i < Math.Min(10, NodeCount); i++)
            _dependencyTracker.MarkDirty(_nodeIds[i]);
        var dirtyNodes = _dependencyTracker.GetDirtyNodes();
        var changeSet = new ChangeSet(new HashSet<int>(dirtyNodes), new HashSet<int>(dirtyNodes));
        _invalidationGraph.Propagate(changeSet);
    }

    [Benchmark]
    [BenchmarkCategory("Evaluation")]
    public Expression IncrementalEngine_Evaluate_Simple()
    {
        _incrementalEngine.Reset();
        return _incrementalEngine.Evaluate(_simpleExpr);
    }

    [Benchmark]
    [BenchmarkCategory("Evaluation")]
    public Expression IncrementalEngine_Evaluate_Complex()
    {
        _incrementalEngine.Reset();
        return _incrementalEngine.Evaluate(_complexExpr);
    }

    [Benchmark]
    [BenchmarkCategory("Evaluation")]
    public Expression IncrementalEngine_EvaluateAndInvalidate()
    {
        _incrementalEngine.Reset();
        _incrementalEngine.Evaluate(_complexExpr);
        _incrementalEngine.Invalidate(_complexExpr);
        return _incrementalEngine.Evaluate(_complexExpr);
    }

    [Benchmark]
    [BenchmarkCategory("Evaluation")]
    public Expression Evaluator_RepeatedEvaluation()
    {
        var evaluator = new IncrementalEvaluator();
        Expression result = null!;
        for (var i = 0; i < 10; i++)
            result = evaluator.Evaluate(_complexExpr);
        return result;
    }

    [Benchmark]
    [BenchmarkCategory("ChangeSet")]
    public void ChangeSet_Empty()
    {
        var cs = ChangeSet.Empty;
        _ = cs.HasChanges;
    }

    [Benchmark]
    [BenchmarkCategory("ChangeSet")]
    public ChangeSet ChangeSet_Merge()
    {
        var set1 = new ChangeSet(new HashSet<int> { 1, 2 }, new HashSet<int> { 1, 2, 3 });
        var set2 = new ChangeSet(new HashSet<int> { 4, 5 }, new HashSet<int> { 4, 5, 6 });
        return set1.Merge(set2);
    }

    [Benchmark]
    [BenchmarkCategory("Engine")]
    public void IncrementalEngine_Update()
    {
        _incrementalEngine.Reset();
        _incrementalEngine.Evaluate(_complexExpr);
        _dependencyTracker.MarkDirty(_nodeIds[0]);
        _incrementalEngine.Update();
    }

    [Benchmark]
    [BenchmarkCategory("Engine")]
    public void IncrementalEngine_MultipleInvalidate()
    {
        _incrementalEngine.Reset();
        var exprs = new Expression[10];
        for (var i = 0; i < 10; i++)
        {
            exprs[i] = Expr.Add(Expr.Variable($"x{i}"), Expr.Literal(i));
            _incrementalEngine.Evaluate(exprs[i]);
        }
        for (var i = 0; i < 10; i++)
            _incrementalEngine.Invalidate(exprs[i]);
    }
}
