using BenchmarkDotNet.Attributes;
using MathVerse.Math.Expressions;
using MathVerse.Math.Operators;
using MathVerse.Math.Visitors;

namespace MathVerse.Performance.Tests.Benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[BenchmarkCategory("Creation")]
public class ExpressionBenchmarks
{
    [Params(1, 5, 10, 50)]
    public int Size { get; set; }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Creation")]
    public Expression CreateLiteral() => Expr.Literal(42.0);

    [Benchmark]
    [BenchmarkCategory("Creation")]
    public Expression CreateVariable() => Expr.Variable("x");

    [Benchmark]
    [BenchmarkCategory("Creation")]
    public Expression CreateBinaryAdd() => Expr.Add(Expr.Literal(1.0), Expr.Literal(2.0));

    [Benchmark]
    [BenchmarkCategory("Creation")]
    public Expression CreateChainedAdditions()
    {
        Expression result = Expr.Literal(0.0);
        for (var i = 1; i <= Size; i++)
            result = Expr.Add(result, Expr.Literal(i));
        return result;
    }

    [Benchmark]
    [BenchmarkCategory("Creation")]
    public Expression CreateNestedTree()
    {
        Expression x = Expr.Variable("x");
        Expression expr = Expr.Literal(1.0);
        for (var i = 0; i < Size; i++)
            expr = Expr.Add(Expr.Multiply(x, expr), Expr.Literal(i));
        return expr;
    }

    [Benchmark]
    [BenchmarkCategory("Creation")]
    public Expression CreateFunctionCall() => Expr.Sin(Expr.Add(Expr.Variable("x"), Expr.Literal(1.0)));

    [Benchmark]
    [BenchmarkCategory("Creation")]
    public Expression CreateDeeplyNestedFunctions()
    {
        Expression expr = Expr.Variable("x");
        for (var i = 0; i < Size; i++)
            expr = Expr.Sin(expr);
        return expr;
    }

    [Benchmark]
    [BenchmarkCategory("Creation")]
    public Expression CreatePowerChain()
    {
        Expression expr = Expr.Variable("x");
        for (var i = 0; i < Size; i++)
            expr = Expr.Pow(expr, Expr.Literal(2.0));
        return expr;
    }

    [Benchmark]
    [BenchmarkCategory("Creation")]
    public Expression CreateMatrixExpression()
    {
        var rows = new Expression[Size];
        for (var i = 0; i < Size; i++)
        {
            var cols = new Expression[Size];
            for (var j = 0; j < Size; j++)
                cols[j] = Expr.Literal(i * Size + j);
            rows[i] = Expr.Vector(cols);
        }
        return Expr.Matrix(rows);
    }

    [Benchmark]
    [BenchmarkCategory("Creation")]
    public Expression CreateLambdaExpression()
    {
        var parameters = new ParameterExpression[Size];
        for (var i = 0; i < Size; i++)
            parameters[i] = Expr.Parameter($"x{i}");
        Expression body = Expr.Literal(0.0);
        for (var i = 0; i < Size; i++)
            body = Expr.Add(body, Expr.Multiply(parameters[i], Expr.Literal(i + 1)));
        return Expr.Lambda(parameters, body);
    }

    private Expression[] _equalityLeft = null!;
    private Expression[] _equalityRight = null!;
    private Expression[] _differentPairs = null!;

    [GlobalSetup]
    public void SetupEquality()
    {
        var count = 100;
        _equalityLeft = new Expression[count];
        _equalityRight = new Expression[count];
        _differentPairs = new Expression[count];
        for (var i = 0; i < count; i++)
        {
            var expr = Expr.Add(Expr.Multiply(Expr.Variable("x"), Expr.Literal(i)), Expr.Sin(Expr.Variable("y")));
            _equalityLeft[i] = expr;
            _equalityRight[i] = Expr.Add(Expr.Multiply(Expr.Variable("x"), Expr.Literal(i)), Expr.Sin(Expr.Variable("y")));
            _differentPairs[i] = Expr.Add(Expr.Multiply(Expr.Variable("x"), Expr.Literal(i + 1000)), Expr.Cos(Expr.Variable("y")));
        }
    }

    [Benchmark]
    [BenchmarkCategory("Creation")]
    public Expression CreateVectorExpression()
    {
        var components = new Expression[Size];
        for (var i = 0; i < Size; i++)
            components[i] = Expr.Add(Expr.Variable($"x{i}"), Expr.Literal(i));
        return Expr.Vector(components);
    }

    [Benchmark]
    [BenchmarkCategory("Equality")]
    public bool StructuralEquality_Equal()
    {
        var result = true;
        for (var i = 0; i < _equalityLeft.Length; i++)
            result &= _equalityLeft[i].Equals(_equalityRight[i]);
        return result;
    }

    [Benchmark]
    [BenchmarkCategory("Equality")]
    public bool StructuralEquality_Different()
    {
        var result = true;
        for (var i = 0; i < _differentPairs.Length; i++)
            result &= _equalityLeft[i].Equals(_differentPairs[i]);
        return result;
    }

    [Benchmark]
    [BenchmarkCategory("Equality")]
    public int GetHashCode_SmallExpression() => _equalityLeft[0].GetHashCode();

    [Benchmark]
    [BenchmarkCategory("Equality")]
    public int GetHashCode_LargeExpression()
    {
        var expr = Expr.Add(Expr.Multiply(Expr.Variable("x"), Expr.Pow(Expr.Variable("y"), Expr.Literal(3.0))), Expr.Sin(Expr.Variable("z")));
        return expr.GetHashCode();
    }

    [Benchmark]
    [BenchmarkCategory("Traversal")]
    public int NodeCount_Small()
    {
        var expr = Expr.Add(Expr.Literal(1.0), Expr.Literal(2.0));
        return expr.NodeCount;
    }

    [Benchmark]
    [BenchmarkCategory("Traversal")]
    public int NodeCount_Medium()
    {
        var expr = Expr.Add(Expr.Multiply(Expr.Variable("x"), Expr.Pow(Expr.Variable("y"), Expr.Literal(3.0))), Expr.Sin(Expr.Variable("z")));
        return expr.NodeCount;
    }

    [Benchmark]
    [BenchmarkCategory("Traversal")]
    public int ChildrenAccess_Small()
    {
        var expr = Expr.Add(Expr.Literal(1.0), Expr.Literal(2.0));
        return expr.Children.Count;
    }

    [Benchmark]
    [BenchmarkCategory("Traversal")]
    public string ToString_Small() => Expr.Add(Expr.Literal(1.0), Expr.Literal(2.0)).ToString();

    [Benchmark]
    [BenchmarkCategory("Traversal")]
    public string ToString_Medium()
    {
        var expr = Expr.Add(Expr.Multiply(Expr.Variable("x"), Expr.Pow(Expr.Variable("y"), Expr.Literal(3.0))), Expr.Sin(Expr.Variable("z")));
        return expr.ToString();
    }
}
