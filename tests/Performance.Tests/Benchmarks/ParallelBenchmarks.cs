using BenchmarkDotNet.Attributes;
using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;

namespace MathVerse.Performance.Tests.Benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ParallelBenchmarks
{
    private EvaluationScheduler _scheduler = null!;
    private WorkScheduler _workScheduler = null!;
    private Expression[] _expressions = null!;
    private IReadOnlyList<Expression> _expressionList = null!;
    private int[] _partitionInput = null!;
    private Func<Expression, int> _hashEvaluator = null!;
    private Func<Expression, Expression> _simplifyEvaluator = null!;

    [Params(100, 1000)]
    public int WorkItemCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _scheduler = new EvaluationScheduler();
        _workScheduler = new WorkScheduler();

        _expressions = new Expression[WorkItemCount];
        for (var i = 0; i < WorkItemCount; i++)
            _expressions[i] = Expr.Add(Expr.Multiply(Expr.Variable("x"), Expr.Literal(i)), Expr.Sin(Expr.Variable("y")));
        _expressionList = _expressions;

        _partitionInput = Enumerable.Range(0, WorkItemCount).ToArray();
        _hashEvaluator = static e => e.GetHashCode();
        _simplifyEvaluator = static e => e;
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Sequential")]
    public long Sequential_EvaluateAll()
    {
        var total = 0L;
        for (var i = 0; i < _expressionList.Count; i++)
            total += _hashEvaluator(_expressionList[i]);
        return total;
    }

    [Benchmark]
    [BenchmarkCategory("Sequential")]
    public long Sequential_EvaluateWithIndex()
    {
        var total = 0L;
        for (var i = 0; i < _expressionList.Count; i++)
            total += i + _hashEvaluator(_expressionList[i]);
        return total;
    }

    [Benchmark]
    [BenchmarkCategory("Parallel")]
    public IReadOnlyList<int> Parallel_EvaluateAll()
    {
        return _scheduler.EvaluateAll(_expressionList, _hashEvaluator,
            new ParallelEvaluationOptions(4, false, CancellationToken.None, null));
    }

    [Benchmark]
    [BenchmarkCategory("Parallel")]
    public IReadOnlyList<int> Parallel_EvaluateAll_Deterministic()
    {
        return _scheduler.EvaluateAll(_expressionList, _hashEvaluator,
            new ParallelEvaluationOptions(4, true, CancellationToken.None, null));
    }

    [Benchmark]
    [BenchmarkCategory("Parallel")]
    public IReadOnlyList<int> Parallel_EvaluateAll_MaxParallelism()
    {
        return _scheduler.EvaluateAll(_expressionList, _hashEvaluator,
            new ParallelEvaluationOptions(Environment.ProcessorCount, false, CancellationToken.None, null));
    }

    [Benchmark]
    [BenchmarkCategory("Parallel")]
    public IReadOnlyList<Expression> Parallel_EvaluateAll_Simplify()
    {
        return _scheduler.EvaluateAll(_expressionList, _simplifyEvaluator,
            new ParallelEvaluationOptions(4, false, CancellationToken.None, null));
    }

    [Benchmark]
    [BenchmarkCategory("Partitioning")]
    public IReadOnlyList<IReadOnlyList<int>> Partition_4Parts()
    {
        return TaskPartitioner.Partition(_partitionInput, 4);
    }

    [Benchmark]
    [BenchmarkCategory("Partitioning")]
    public IReadOnlyList<IReadOnlyList<int>> Partition_BySize()
    {
        return TaskPartitioner.PartitionBySize(_partitionInput, 50);
    }

    [Benchmark]
    [BenchmarkCategory("Partitioning")]
    public IReadOnlyList<IReadOnlyList<int>> Partition_ManySmall()
    {
        return TaskPartitioner.Partition(_partitionInput, _partitionInput.Length / 2);
    }

    [Benchmark]
    [BenchmarkCategory("WorkScheduler")]
    public void WorkScheduler_EnqueueDequeue()
    {
        _workScheduler.Clear();
        for (var i = 0; i < WorkItemCount; i++)
            _workScheduler.Enqueue(i % 16, static () => { });
        while (_workScheduler.Dequeue() is not null) { }
    }

    [Benchmark]
    [BenchmarkCategory("WorkScheduler")]
    public void WorkScheduler_PriorityOrder()
    {
        _workScheduler.Clear();
        for (var i = 0; i < WorkItemCount; i++)
            _workScheduler.Enqueue(i % 16, static () => { });
        var dequeued = 0;
        while (_workScheduler.Dequeue() is not null)
            dequeued++;
        _ = dequeued;
    }

    [Benchmark]
    [BenchmarkCategory("WorkScheduler")]
    public int WorkScheduler_MixedPriorities()
    {
        _workScheduler.Clear();
        for (var i = 0; i < WorkItemCount; i++)
        {
            var priority = i % 4;
            var captured = i;
            _workScheduler.Enqueue(priority, () => { var _ = captured; });
        }
        var count = _workScheduler.Count;
        _workScheduler.Clear();
        return count;
    }

    [Benchmark]
    [BenchmarkCategory("SingleEval")]
    public int SingleEval_Direct()
    {
        var result = 0;
        for (var i = 0; i < WorkItemCount; i++)
            result += _scheduler.EvaluateSingle(_expressionList[i], _hashEvaluator);
        return result;
    }
}
