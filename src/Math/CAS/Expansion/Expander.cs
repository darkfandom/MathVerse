namespace MathVerse.Math.CAS.Expansion;

using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;
using System.Collections.Concurrent;
using System.Collections.Immutable;

public sealed class Expander
{
    private static readonly Lazy<Expander> _instance = new(() => new Expander());
    public static Expander Instance => _instance.Value;

    private readonly ConcurrentDictionary<string, ExpansionResult> _cache = new();
    private readonly DistributiveExpander _distributive = new();
    private readonly PolynomialExpander _polynomial = new();

    private Expander() { }

    public ExpansionResult Expand(Expression expr, ExpansionOptions? options = null)
    {
        options ??= ExpansionOptions.Default;

        var cacheKey = $"{expr.NodeId}:{options.GetHashCode()}";
        if (_cache.TryGetValue(cacheKey, out var cached))
            return cached;

        var result = ExpandCore(expr, options);
        _cache.TryAdd(cacheKey, result);
        return result;
    }

    private ExpansionResult ExpandCore(Expression expr, ExpansionOptions options)
    {
        var original = expr;
        var current = expr;
        var steps = new List<string>();

        if (options.DistributeMultiplication)
        {
            var expanded = _distributive.ExpandMulOverAdd(current, steps);
            if (!expanded.Equals(current))
            {
                steps.Add("DistributeMultiplication");
                current = expanded;
            }
        }

        if (options.ExpandPowers)
        {
            var expanded = _polynomial.ExpandPolynomial(current);
            if (!expanded.Equals(current))
            {
                steps.Add("ExpandPowers");
                current = expanded;
            }
        }

        if (options.ExpandFunctions)
        {
            var expanded = _distributive.ExpandFunctionOverAdd(current, steps);
            if (!expanded.Equals(current))
            {
                steps.Add("ExpandFunctions");
                current = expanded;
            }
        }

        if (options.ExpandLogarithms)
        {
            var expanded = ExpandLogarithms(current);
            if (!expanded.Equals(current))
            {
                steps.Add("ExpandLogarithms");
                current = expanded;
            }
        }

        if (options.ExpandTrigonometric)
        {
            var expanded = ExpandTrigonometric(current);
            if (!expanded.Equals(current))
            {
                steps.Add("ExpandTrigonometric");
                current = expanded;
            }
        }

        return new ExpansionResult
        {
            Original = original,
            Expanded = current,
            Steps = steps.Distinct().ToImmutableArray()
        };
    }

    private Expression ExpandLogarithms(Expression expr)
    {
        return expr switch
        {
            FunctionCallExpression f when (f.Name == "log" || f.Name == "ln") && f.Arguments.Count == 1 =>
                ExpandLogArgument(f),
            FunctionCallExpression f when f.Name == "log" && f.Arguments.Count == 2 =>
                ExpandLogArgument(new FunctionCallExpression(f.Name, f.Arguments)),
            BinaryExpression b => new BinaryExpression(b.Operator, ExpandLogarithms(b.Left), ExpandLogarithms(b.Right)),
            UnaryExpression u => new UnaryExpression(u.Operator, ExpandLogarithms(u.Operand)),
            _ => expr
        };
    }

    private Expression ExpandLogArgument(FunctionCallExpression expr)
    {
        var arg = expr.Arguments[0];

        if (arg is BinaryExpression b)
        {
            if (b.Operator.Equals(MathOperator.Multiply))
            {
                return Expr.Add(Expr.Call(expr.Name, b.Left), Expr.Call(expr.Name, b.Right));
            }
            if (b.Operator.Equals(MathOperator.Divide))
            {
                return Expr.Subtract(Expr.Call(expr.Name, b.Left), Expr.Call(expr.Name, b.Right));
            }
            if (b.Operator.Equals(MathOperator.Power))
            {
                return Expr.Multiply(b.Right, Expr.Call(expr.Name, b.Left));
            }
        }

        return expr;
    }

    private Expression ExpandTrigonometric(Expression expr)
    {
        return expr switch
        {
            FunctionCallExpression f when f.Name == "sin" && f.Arguments.Count == 1 =>
                ExpandTrigArgument("sin", f.Arguments[0]),
            FunctionCallExpression f when f.Name == "cos" && f.Arguments.Count == 1 =>
                ExpandTrigArgument("cos", f.Arguments[0]),
            FunctionCallExpression f when f.Name == "tan" && f.Arguments.Count == 1 =>
                ExpandTrigArgument("tan", f.Arguments[0]),
            BinaryExpression b => new BinaryExpression(b.Operator, ExpandTrigonometric(b.Left), ExpandTrigonometric(b.Right)),
            UnaryExpression u => new UnaryExpression(u.Operator, ExpandTrigonometric(u.Operand)),
            _ => expr
        };
    }

    private Expression ExpandTrigArgument(string func, Expression arg)
    {
        if (arg is BinaryExpression b)
        {
            if (b.Operator.Equals(MathOperator.Add) || b.Operator.Equals(MathOperator.Subtract))
            {
                var A = b.Left;
                var B = b.Right;
                var isSub = b.Operator.Equals(MathOperator.Subtract);

                if (func == "sin")
                {
                    var sinA = Expr.Call("sin", A);
                    var cosA = Expr.Call("cos", A);
                    var sinB = Expr.Call("sin", B);
                    var cosB = Expr.Call("cos", B);

                    if (!isSub)
                        return Expr.Add(Expr.Multiply(sinA, cosB), Expr.Multiply(cosA, sinB));
                    else
                        return Expr.Subtract(Expr.Multiply(sinA, cosB), Expr.Multiply(cosA, sinB));
                }
                else if (func == "cos")
                {
                    var cosA = Expr.Call("cos", A);
                    var sinA = Expr.Call("sin", A);
                    var cosB = Expr.Call("cos", B);
                    var sinB = Expr.Call("sin", B);

                    if (!isSub)
                        return Expr.Subtract(Expr.Multiply(cosA, cosB), Expr.Multiply(sinA, sinB));
                    else
                        return Expr.Add(Expr.Multiply(cosA, cosB), Expr.Multiply(sinA, sinB));
                }
                else if (func == "tan")
                {
                    var tanA = Expr.Call("tan", A);
                    var tanB = Expr.Call("tan", B);

                    if (!isSub)
                        return Expr.Divide(Expr.Add(tanA, tanB), Expr.Subtract(Expr.Literal(1), Expr.Multiply(tanA, tanB)));
                    else
                        return Expr.Divide(Expr.Subtract(tanA, tanB), Expr.Add(Expr.Literal(1), Expr.Multiply(tanA, tanB)));
                }
            }
        }

        return Expr.Call(func, arg);
    }

    public void ClearCache() => _cache.Clear();
    public int CacheCount => _cache.Count;
}