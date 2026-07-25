namespace MathVerse.Math.Compiler.Expressions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

/// <summary>Enumerates optimization strategies for expression evaluation.</summary>
[Flags]
public enum PlannerStrategy
{
    /// <summary>No optimization (evaluate in AST order).</summary>
    None = 0,

    /// <summary>Cache common sub-expressions.</summary>
    CacheSubExpressions = 1,

    /// <summary>Vectorize SIMD-friendly operations.</summary>
    Vectorize = 2,

    /// <summary>Parallelize independent branches.</summary>
    Parallelize = 4,

    /// <summary>Apply all available strategies.</summary>
    All = CacheSubExpressions | Vectorize | Parallelize,
}

/// <summary>Describes how a sub-expression should be evaluated.</summary>
/// <param name="Expression">The sub-expression to evaluate.</param>
/// <param name="ExecutionOrder">The order in which to execute (lower = earlier).</param>
    /// <param name="ShouldCache">Whether to cache the result of this sub-expression.</param>
/// <param name="CanVectorize">Whether this sub-expression is SIMD-vectorizable.</param>
/// <param name="ParallelGroup">ID of the parallel group (0 = sequential).</param>
/// <param name="EstimatedCost">Estimated computational cost.</param>
public sealed record ExpressionPlan(
    ExpressionNode Expression,
    int ExecutionOrder,
    bool ShouldCache,
    bool CanVectorize,
    int ParallelGroup,
    double EstimatedCost);

/// <summary>The full execution plan for an expression.</summary>
/// <param name="Plans">Plans for each unique sub-expression.</param>
/// <param name="TotalEstimatedCost">Total estimated cost.</param>
/// <param name="VectorizationWidth">Suggested SIMD vector width.</param>
/// <param name="MaxParallelism">Maximum parallelism level.</param>
public sealed record ExecutionPlan(
    IReadOnlyList<ExpressionPlan> Plans,
    double TotalEstimatedCost,
    int VectorizationWidth,
    int MaxParallelism);

/// <summary>Plans execution order for expression evaluation, determining caching, vectorization, and parallelization opportunities.</summary>
public sealed class ExpressionPlanner
{
    private readonly Dictionary<ExpressionNode, double> _costCache = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<ExpressionNode, int> _frequencyCache = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<ExpressionNode> _visited = new(ReferenceEqualityComparer.Instance);

    /// <summary>Plans the execution of the given expression with the specified strategy.</summary>
    public ExecutionPlan Plan(ExpressionNode root, PlannerStrategy strategy = PlannerStrategy.All)
    {
        if (root is null) throw new ArgumentNullException(nameof(root));

        _costCache.Clear();
        _frequencyCache.Clear();
        _visited.Clear();

        ComputeCosts(root);
        CountFrequencies(root);

        var plans = new List<ExpressionPlan>();
        int order = 0;
        int parallelGroup = 0;

        PlanNode(root, strategy, ref order, ref parallelGroup, plans);

        double totalCost = plans.Sum(p => p.EstimatedCost);
        int vecWidth = strategy.HasFlag(PlannerStrategy.Vectorize) ? DetermineVectorWidth(root) : 1;
        int maxPar = strategy.HasFlag(PlannerStrategy.Parallelize) ? DetermineMaxParallelism(root) : 1;

        return new ExecutionPlan(plans, totalCost, vecWidth, maxPar);
    }

    /// <summary>Estimates the computational cost of an expression tree.</summary>
    public double EstimateCost(ExpressionNode root)
    {
        if (root is null) throw new ArgumentNullException(nameof(root));
        _costCache.Clear();
        ComputeCosts(root);
        return _costCache.TryGetValue(root, out double cost) ? cost : 0;
    }

    /// <summary>Finds all common sub-expressions (expressions that appear more than once).</summary>
    public IReadOnlyList<ExpressionNode> FindCommonSubExpressions(ExpressionNode root)
    {
        if (root is null) throw new ArgumentNullException(nameof(root));

        _frequencyCache.Clear();
        CountFrequencies(root);

        return _frequencyCache
            .Where(kv => kv.Value > 1 && kv.Key is not (NumberNode or VariableNode))
            .Select(kv => kv.Key)
            .ToArray();
    }

    /// <summary>Identifies SIMD-vectorizable sub-expressions.</summary>
    public IReadOnlyList<ExpressionNode> FindVectorizableSubExpressions(ExpressionNode root)
    {
        if (root is null) throw new ArgumentNullException(nameof(root));

        var result = new List<ExpressionNode>();
        FindVectorizableImpl(root, result);
        return result;
    }

    /// <summary>Identifies independent branches that can execute in parallel.</summary>
    public IReadOnlyList<IReadOnlyList<ExpressionNode>> FindParallelBranches(ExpressionNode root)
    {
        if (root is null) throw new ArgumentNullException(nameof(root));

        var result = new List<IReadOnlyList<ExpressionNode>>();
        FindParallelBranchesImpl(root, result);
        return result;
    }

    private void PlanNode(ExpressionNode node, PlannerStrategy strategy, ref int order, ref int parallelGroup, List<ExpressionPlan> plans)
    {
        if (node is NumberNode or VariableNode)
        {
            double cost = GetCost(node);
            bool shouldCache = strategy.HasFlag(PlannerStrategy.CacheSubExpressions) &&
                               _frequencyCache.TryGetValue(node, out int freq) && freq > 1;
            plans.Add(new ExpressionPlan(node, order++, shouldCache, false, 0, cost));
            return;
        }

        if (node is BinaryOpNode bin)
        {
            bool leftIndependent = IsIndependentBranch(bin.Left, bin.Right);
            bool rightIndependent = IsIndependentBranch(bin.Right, bin.Left);

            if (strategy.HasFlag(PlannerStrategy.Parallelize) && leftIndependent && rightIndependent)
            {
                int pg = ++parallelGroup;
                PlanNode(bin.Left, strategy, ref order, ref parallelGroup, plans);
                PlanNode(bin.Right, strategy, ref order, ref parallelGroup, plans);
                double cost = GetCost(node);
                bool canVec = strategy.HasFlag(PlannerStrategy.Vectorize) && IsVectorizableOp(bin.Op);
                plans.Add(new ExpressionPlan(node, order++, false, canVec, 0, cost));
            }
            else
            {
                PlanNode(bin.Left, strategy, ref order, ref parallelGroup, plans);
                PlanNode(bin.Right, strategy, ref order, ref parallelGroup, plans);
                double cost = GetCost(node);
                bool canVec = strategy.HasFlag(PlannerStrategy.Vectorize) && IsVectorizableOp(bin.Op);
                bool shouldCache = strategy.HasFlag(PlannerStrategy.CacheSubExpressions) &&
                                   _frequencyCache.TryGetValue(node, out int freq) && freq > 1;
                plans.Add(new ExpressionPlan(node, order++, shouldCache, canVec, 0, cost));
            }
        }
        else if (node is UnaryOpNode unary)
        {
            PlanNode(unary.Operand, strategy, ref order, ref parallelGroup, plans);
            double cost = GetCost(node);
            plans.Add(new ExpressionPlan(node, order++, false, false, 0, cost));
        }
        else if (node is FunctionNode func)
        {
            foreach (var arg in func.Arguments)
                PlanNode(arg, strategy, ref order, ref parallelGroup, plans);
            double cost = GetCost(node);
            plans.Add(new ExpressionPlan(node, order++, false, false, 0, cost));
        }
    }

    private void ComputeCosts(ExpressionNode node)
    {
        if (_costCache.ContainsKey(node)) return;

        double cost = node switch
        {
            NumberNode => 0.1,
            VariableNode => 0.1,
            BinaryOpNode bin => GetCost(bin.Left) + GetCost(bin.Right) + GetBinaryCost(bin.Op),
            UnaryOpNode unary => GetCost(unary.Operand) + 0.5,
            FunctionNode func => func.Arguments.Sum(GetCost) + GetFunctionCost(func.FunctionName),
            _ => 1.0,
        };

        _costCache[node] = cost;
    }

    private double GetCost(ExpressionNode node)
    {
        ComputeCosts(node);
        return _costCache.TryGetValue(node, out double cost) ? cost : 1.0;
    }

    private void CountFrequencies(ExpressionNode node)
    {
        if (!_frequencyCache.TryAdd(node, 1))
        {
            _frequencyCache[node]++;
            return;
        }

        switch (node)
        {
            case BinaryOpNode bin:
                CountFrequencies(bin.Left);
                CountFrequencies(bin.Right);
                break;
            case UnaryOpNode unary:
                CountFrequencies(unary.Operand);
                break;
            case FunctionNode func:
                foreach (var arg in func.Arguments)
                    CountFrequencies(arg);
                break;
        }
    }

    private static double GetBinaryCost(BinaryOperator op) =>
        op switch
        {
            BinaryOperator.Add or BinaryOperator.Subtract => 0.5,
            BinaryOperator.Multiply or BinaryOperator.Divide => 1.0,
            BinaryOperator.Power => 5.0,
            _ => 1.0,
        };

    private static double GetFunctionCost(string name) =>
        name.ToLowerInvariant() switch
        {
            "sin" or "cos" or "tan" => 2.0,
            "asin" or "acos" or "atan" => 3.0,
            "ln" or "log" => 2.0,
            "exp" => 2.0,
            "sqrt" => 1.5,
            "abs" => 0.5,
            "ceil" or "floor" => 0.5,
            _ => 2.0,
        };

    private static bool IsVectorizableOp(BinaryOperator op) =>
        op is BinaryOperator.Add or BinaryOperator.Subtract or BinaryOperator.Multiply;

    private int DetermineVectorWidth(ExpressionNode root)
    {
        var vecNodes = FindVectorizableSubExpressions(root);
        return vecNodes.Count >= 4 ? Vector<double>.Count : vecNodes.Count >= 2 ? 2 : 1;
    }

    private int DetermineMaxParallelism(ExpressionNode root)
    {
        var branches = FindParallelBranches(root);
        return branches.Count > 0 ? branches.Max(b => b.Count) : 1;
    }

    private bool IsIndependentBranch(ExpressionNode a, ExpressionNode b)
    {
        var varsA = CollectVariables(a);
        var varsB = CollectVariables(b);
        return !varsA.Overlaps(varsB);
    }

    private HashSet<string> CollectVariables(ExpressionNode node)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        CollectVariablesImpl(node, result);
        return result;
    }

    private void CollectVariablesImpl(ExpressionNode node, HashSet<string> vars)
    {
        switch (node)
        {
            case VariableNode v:
                vars.Add(v.Name);
                break;
            case BinaryOpNode bin:
                CollectVariablesImpl(bin.Left, vars);
                CollectVariablesImpl(bin.Right, vars);
                break;
            case UnaryOpNode unary:
                CollectVariablesImpl(unary.Operand, vars);
                break;
            case FunctionNode func:
                foreach (var arg in func.Arguments)
                    CollectVariablesImpl(arg, vars);
                break;
        }
    }

    private void FindVectorizableImpl(ExpressionNode node, List<ExpressionNode> result)
    {
        if (node is BinaryOpNode bin && IsVectorizableOp(bin.Op))
        {
            result.Add(node);
            FindVectorizableImpl(bin.Left, result);
            FindVectorizableImpl(bin.Right, result);
        }
        else if (node is BinaryOpNode bin2)
        {
            FindVectorizableImpl(bin2.Left, result);
            FindVectorizableImpl(bin2.Right, result);
        }
        else if (node is UnaryOpNode unary)
        {
            FindVectorizableImpl(unary.Operand, result);
        }
        else if (node is FunctionNode func)
        {
            foreach (var arg in func.Arguments)
                FindVectorizableImpl(arg, result);
        }
    }

    private void FindParallelBranchesImpl(ExpressionNode node, List<IReadOnlyList<ExpressionNode>> result)
    {
        if (node is BinaryOpNode bin)
        {
            if (IsIndependentBranch(bin.Left, bin.Right))
            {
                result.Add([bin.Left, bin.Right]);
            }
            FindParallelBranchesImpl(bin.Left, result);
            FindParallelBranchesImpl(bin.Right, result);
        }
    }

    private sealed class ReferenceEqualityComparer : IEqualityComparer<ExpressionNode>
    {
        public static readonly ReferenceEqualityComparer Instance = new();
        public bool Equals(ExpressionNode? x, ExpressionNode? y) => ReferenceEquals(x, y);
        public int GetHashCode(ExpressionNode obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}
