namespace MathVerse.Math.Compiler.Tensor;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Compiles tensor operations into optimized execution plans.
/// Supports matmul, elementwise ops, reductions, reshaping, and transposing.</summary>
public sealed class TensorCompiler
{
    private readonly TensorOptimizer _optimizer;
    private readonly TensorScheduler _scheduler;

    /// <summary>Initializes a new instance of the <see cref="TensorCompiler"/> class.</summary>
    public TensorCompiler()
    {
        _optimizer = new TensorOptimizer();
        _scheduler = new TensorScheduler();
    }

    /// <summary>Compiles a tensor expression into an optimized execution plan.</summary>
    /// <param name="expr">The root tensor expression to compile.</param>
    /// <returns>A <see cref="TensorPlan"/> containing the graph, schedule, and cost estimates.</returns>
    public TensorPlan Compile(TensorExpression expr)
    {
        if (expr is null) throw new ArgumentNullException(nameof(expr));

        var graph = BuildGraph(expr);
        var optimizedGraph = _optimizer.Optimize(graph);
        var schedule = _scheduler.Schedule(optimizedGraph);
        var memory = EstimateMemoryCost(optimizedGraph);

        return new TensorPlan(optimizedGraph, schedule, schedule.EstimatedFLOPs, memory);
    }

    /// <summary>Compiles a set of tensor expressions into a single plan.</summary>
    public TensorPlan CompileAll(IReadOnlyList<TensorExpression> expressions)
    {
        if (expressions is null) throw new ArgumentNullException(nameof(expressions));

        var combinedGraph = new TensorGraph();
        foreach (var expr in expressions)
        {
            BuildGraphInto(expr, combinedGraph);
        }

        var optimizedGraph = _optimizer.Optimize(combinedGraph);
        var schedule = _scheduler.Schedule(optimizedGraph);
        var memory = EstimateMemoryCost(optimizedGraph);

        return new TensorPlan(optimizedGraph, schedule, schedule.EstimatedFLOPs, memory);
    }

    private static TensorGraph BuildGraph(TensorExpression expr)
    {
        var graph = new TensorGraph();
        BuildGraphInto(expr, graph);
        return graph;
    }

    private static void BuildGraphInto(TensorExpression expr, TensorGraph graph)
    {
        var visited = new HashSet<TensorExpression>(new ReferenceEqualityComparer());

        void Visit(TensorExpression e)
        {
            if (!visited.Add(e)) return;
            foreach (var input in e.Inputs)
                Visit(input);
            graph.AddOperation(e.OpType, e.Inputs, e.Shape, e.Label);
        }

        Visit(expr);
    }

    private static long EstimateMemoryCost(TensorGraph graph)
    {
        long totalBytes = 0;
        foreach (var op in graph.Operations)
        {
            var elements = 1;
            foreach (var d in op.Shape)
                elements *= d;
            totalBytes += elements * 8L;
        }
        return totalBytes;
    }
}
