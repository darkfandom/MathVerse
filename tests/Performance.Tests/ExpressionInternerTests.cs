namespace MathVerse.Performance.Tests;

public sealed class ExpressionInternerTests
{
    [Fact]
    public void Intern_Literal_ReturnsSameInstanceOnSecondCall()
    {
        var interner = new ExpressionInterner();
        var expr = Expr.Literal(42.0);

        var first = interner.Intern(expr);
        var second = interner.Intern(Expr.Literal(42.0));

        first.Should().BeSameAs(second);
    }

    [Fact]
    public void Intern_DifferentLiterals_ReturnsDifferentInstances()
    {
        var interner = new ExpressionInterner();

        var a = interner.Intern(Expr.Literal(1.0));
        var b = interner.Intern(Expr.Literal(2.0));

        a.Should().NotBeSameAs(b);
    }

    [Fact]
    public void Intern_Variable_ReturnsCanonicalInstance()
    {
        var interner = new ExpressionInterner();
        var v1 = interner.Intern(Expr.Variable("x"));
        var v2 = interner.Intern(Expr.Variable("x"));

        v1.Should().BeSameAs(v2);
    }

    [Fact]
    public void Intern_DifferentVariableNames_AreDistinct()
    {
        var interner = new ExpressionInterner();

        var x = interner.Intern(Expr.Variable("x"));
        var y = interner.Intern(Expr.Variable("y"));

        x.Should().NotBeSameAs(y);
    }

    [Fact]
    public void Intern_ComplexExpression_StructurallyEqual()
    {
        var interner = new ExpressionInterner();
        var expr = Expr.Add(Expr.Variable("x"), Expr.Literal(1.0));

        var first = interner.Intern(expr);
        var second = interner.Intern(Expr.Add(Expr.Variable("x"), Expr.Literal(1.0)));

        first.Should().BeSameAs(second);
    }

    [Fact]
    public void Intern_NullExpression_Throws()
    {
        var interner = new ExpressionInterner();
        Action act = () => interner.Intern(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Count_IncreasesWithNewExpressions()
    {
        var interner = new ExpressionInterner();

        interner.Intern(Expr.Literal(1.0));
        interner.Intern(Expr.Literal(2.0));
        interner.Intern(Expr.Literal(3.0));

        interner.Count.Should().Be(3);
    }

    [Fact]
    public void Count_DoesNotIncreaseForDuplicates()
    {
        var interner = new ExpressionInterner();

        interner.Intern(Expr.Literal(1.0));
        interner.Intern(Expr.Literal(1.0));
        interner.Intern(Expr.Literal(1.0));

        interner.Count.Should().Be(1);
    }

    [Fact]
    public void Clear_ResetsCount()
    {
        var interner = new ExpressionInterner();
        interner.Intern(Expr.Literal(1.0));
        interner.Intern(Expr.Literal(2.0));

        interner.Clear();

        interner.Count.Should().Be(0);
    }

    [Fact]
    public void Clear_ResetStatistics()
    {
        var interner = new ExpressionInterner();
        interner.Intern(Expr.Literal(1.0));
        interner.Intern(Expr.Literal(1.0));

        interner.Clear();
        var stats = interner.Statistics;

        stats.Hits.Should().Be(0);
        stats.Misses.Should().Be(0);
        stats.UniqueCount.Should().Be(0);
    }

    [Fact]
    public void Statistics_TracksHitsAndMisses()
    {
        var interner = new ExpressionInterner();
        var expr = Expr.Literal(42.0);

        interner.Intern(expr);
        interner.Intern(Expr.Literal(42.0));
        interner.Intern(Expr.Literal(42.0));

        var stats = interner.Statistics;
        stats.Hits.Should().Be(2);
        stats.Misses.Should().Be(1);
        stats.TotalLookups.Should().Be(3);
    }

    [Fact]
    public void Statistics_HitRatio_Correct()
    {
        var interner = new ExpressionInterner();
        var expr = Expr.Literal(5.0);

        interner.Intern(expr);
        interner.Intern(Expr.Literal(5.0));
        interner.Intern(Expr.Literal(5.0));
        interner.Intern(Expr.Literal(5.0));

        var stats = interner.Statistics;
        stats.HitRatio.Should().BeApproximately(3.0 / 4.0, 0.001);
    }

    [Fact]
    public void Statistics_ZeroLookups_HitRatioIsZero()
    {
        var interner = new ExpressionInterner();

        interner.Statistics.HitRatio.Should().Be(0.0);
    }

    [Fact]
    public void Intern_BinaryExpression_ReturnsCanonical()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Multiply(Expr.Variable("x"), Expr.Literal(2.0));
        var b = Expr.Multiply(Expr.Variable("x"), Expr.Literal(2.0));

        var first = interner.Intern(a);
        var second = interner.Intern(b);

        first.Should().BeSameAs(second);
    }

    [Fact]
    public void Intern_FunctionCall_ReturnsCanonical()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Sin(Expr.Variable("x"));
        var b = Expr.Sin(Expr.Variable("x"));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_DifferentFunctions_AreDistinct()
    {
        var interner = new ExpressionInterner();

        var sin = interner.Intern(Expr.Sin(Expr.Variable("x")));
        var cos = interner.Intern(Expr.Cos(Expr.Variable("x")));

        sin.Should().NotBeSameAs(cos);
    }

    [Fact]
    public void Intern_VectorExpression_StructurallyEqual()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Vector(Expr.Literal(1.0), Expr.Literal(2.0));
        var b = Expr.Vector(Expr.Literal(1.0), Expr.Literal(2.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_DifferentVectors_AreDistinct()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Vector(Expr.Literal(1.0), Expr.Literal(2.0));
        var b = Expr.Vector(Expr.Literal(1.0), Expr.Literal(3.0));

        interner.Intern(a).Should().NotBeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_NestedExpression_Canonical()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Add(Expr.Multiply(Expr.Variable("x"), Expr.Literal(2.0)), Expr.Literal(3.0));
        var b = Expr.Add(Expr.Multiply(Expr.Variable("x"), Expr.Literal(2.0)), Expr.Literal(3.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_MultipleDistinctExpressions()
    {
        var interner = new ExpressionInterner();
        var expressions = new Expression[] { Expr.Literal(1.0),
            Expr.Literal(2.0),
            Expr.Variable("x"),
            Expr.Variable("y"),
            Expr.Add(Expr.Literal(1.0), Expr.Literal(2.0)),
            Expr.Sin(Expr.Literal(0.0)),
        };

        foreach (var expr in expressions)
            interner.Intern(expr);

        interner.Count.Should().Be(6);
    }

    [Fact]
    public void Intern_ConstantExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Constant("pi", System.Math.PI);
        var b = Expr.Constant("pi", System.Math.PI);

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_DifferentConstants_AreDistinct()
    {
        var interner = new ExpressionInterner();
        var pi = interner.Intern(Expr.Constant("pi", System.Math.PI));
        var e = interner.Intern(Expr.Constant("e", System.Math.E));

        pi.Should().NotBeSameAs(e);
    }

    [Fact]
    public void Intern_BooleanExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Boolean(true);
        var b = Expr.Boolean(true);

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_DifferentBooleans_AreDistinct()
    {
        var interner = new ExpressionInterner();
        var t = interner.Intern(Expr.Boolean(true));
        var f = interner.Intern(Expr.Boolean(false));

        t.Should().NotBeSameAs(f);
    }

    [Fact]
    public void Intern_EquationExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Equation(Expr.Variable("x"), Expr.Literal(5.0));
        var b = Expr.Equation(Expr.Variable("x"), Expr.Literal(5.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_RelationalExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.LessThan(Expr.Variable("x"), Expr.Literal(10.0));
        var b = Expr.LessThan(Expr.Variable("x"), Expr.Literal(10.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_UnaryExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Negate(Expr.Variable("x"));
        var b = Expr.Negate(Expr.Variable("x"));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_DifferentUnaryOps_AreDistinct()
    {
        var interner = new ExpressionInterner();
        var neg = interner.Intern(Expr.Negate(Expr.Variable("x")));
        var abs = interner.Intern(Expr.Abs(Expr.Variable("x")));

        neg.Should().NotBeSameAs(abs);
    }

    [Fact]
    public void Intern_ParameterExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Parameter("t");
        var b = Expr.Parameter("t");

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_NullExpression()
    {
        var interner = new ExpressionInterner();
        var a = interner.Intern(Expr.Null);
        var b = interner.Intern(Expr.Null);

        a.Should().BeSameAs(b);
    }

    [Fact]
    public void Intern_IdentityExpression()
    {
        var interner = new ExpressionInterner();
        var a = interner.Intern(Expr.Identity("add_identity"));
        var b = interner.Intern(Expr.Identity("add_identity"));

        a.Should().BeSameAs(b);
    }

    [Fact]
    public void Intern_DifferentIdentities_AreDistinct()
    {
        var interner = new ExpressionInterner();
        var a = interner.Intern(Expr.Identity("add_identity"));
        var b = interner.Intern(Expr.Identity("mul_identity"));

        a.Should().NotBeSameAs(b);
    }

    [Fact]
    public void Clear_AllowsReInterning()
    {
        var interner = new ExpressionInterner();
        var a = interner.Intern(Expr.Literal(1.0));

        interner.Clear();

        var b = interner.Intern(Expr.Literal(1.0));
        a.Should().NotBeSameAs(b);
        interner.Count.Should().Be(1);
    }

    [Fact]
    public void Intern_ExpressionWithDifferentValues_AreDistinct()
    {
        var interner = new ExpressionInterner();
        var a = interner.Intern(Expr.Add(Expr.Literal(1.0), Expr.Literal(2.0)));
        var b = interner.Intern(Expr.Add(Expr.Literal(1.0), Expr.Literal(3.0)));

        a.Should().NotBeSameAs(b);
    }

    [Fact]
    public void Intern_SwappedOperands_AreDistinct()
    {
        var interner = new ExpressionInterner();
        var a = interner.Intern(Expr.Add(Expr.Literal(1.0), Expr.Literal(2.0)));
        var b = interner.Intern(Expr.Add(Expr.Literal(2.0), Expr.Literal(1.0)));

        a.Should().NotBeSameAs(b);
    }

    [Fact]
    public void Intern_MultiplicationVsAddition_AreDistinct()
    {
        var interner = new ExpressionInterner();
        var add = interner.Intern(Expr.Add(Expr.Literal(1.0), Expr.Literal(2.0)));
        var mul = interner.Intern(Expr.Multiply(Expr.Literal(1.0), Expr.Literal(2.0)));

        add.Should().NotBeSameAs(mul);
    }

    [Fact]
    public void Intern_DivisionExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Divide(Expr.Variable("x"), Expr.Literal(2.0));
        var b = Expr.Divide(Expr.Variable("x"), Expr.Literal(2.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_PowerExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Pow(Expr.Variable("x"), Expr.Literal(2.0));
        var b = Expr.Pow(Expr.Variable("x"), Expr.Literal(2.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_ModuloExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Modulo(Expr.Variable("x"), Expr.Literal(3.0));
        var b = Expr.Modulo(Expr.Variable("x"), Expr.Literal(3.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_LogicalExpressions()
    {
        var interner = new ExpressionInterner();
        var a = Expr.And(Expr.Boolean(true), Expr.Boolean(false));
        var b = Expr.And(Expr.Boolean(true), Expr.Boolean(false));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_OrVsAnd_AreDistinct()
    {
        var interner = new ExpressionInterner();
        var and = interner.Intern(Expr.And(Expr.Boolean(true), Expr.Boolean(false)));
        var or = interner.Intern(Expr.Or(Expr.Boolean(true), Expr.Boolean(false)));

        and.Should().NotBeSameAs(or);
    }

    [Fact]
    public void Intern_NotExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Not(Expr.Boolean(true));
        var b = Expr.Not(Expr.Boolean(true));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_MultiArgFunction()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Log(Expr.Literal(10.0), Expr.Literal(10.0));
        var b = Expr.Log(Expr.Literal(10.0), Expr.Literal(10.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_LambdaExpression()
    {
        var interner = new ExpressionInterner();
        var p = Expr.Parameter("x");
        var body = Expr.Add(p, Expr.Literal(1.0));
        var a = Expr.Lambda(p, body);
        var b = Expr.Lambda(Expr.Parameter("x"), Expr.Add(Expr.Parameter("x"), Expr.Literal(1.0)));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public async Task ThreadSafety_ConcurrentInterns()
    {
        var interner = new ExpressionInterner();
        var tasks = Enumerable.Range(0, 100)
            .Select(i => Task.Run(() => interner.Intern(Expr.Literal(i % 10))))
            .ToArray();

        await Task.WhenAll(tasks);

        interner.Count.Should().Be(10);
    }

    [Fact]
    public async Task ThreadSafety_ConcurrentMixedOperations()
    {
        var interner = new ExpressionInterner();
        var tasks = new List<Task>();

        for (int i = 0; i < 50; i++)
        {
            var val = i;
            tasks.Add(Task.Run(() => interner.Intern(Expr.Literal(val))));
            tasks.Add(Task.Run(() => interner.Intern(Expr.Variable("x"))));
        }

        await Task.WhenAll(tasks);

        interner.Count.Should().Be(51);
    }

    [Fact]
    public async Task ThreadSafety_ConcurrentClearAndIntern()
    {
        var interner = new ExpressionInterner();
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        var internTask = Task.Run(() =>
        {
            int i = 0;
            while (!cts.Token.IsCancellationRequested)
            {
                interner.Intern(Expr.Literal(i++));
            }
        });

        var clearTask = Task.Run(() =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                interner.Clear();
                Thread.Sleep(1);
            }
        });

        await Task.WhenAll(internTask, clearTask);
        interner.Count.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Intern_VariousExpressionKinds_AllDistinct()
    {
        var interner = new ExpressionInterner();
        var expressions = new Expression[]
        {
            Expr.Literal(1.0),
            Expr.Variable("x"),
            Expr.Constant("pi", System.Math.PI),
            Expr.Add(Expr.Literal(1.0), Expr.Literal(2.0)),
            Expr.Multiply(Expr.Literal(1.0), Expr.Literal(2.0)),
            Expr.Sin(Expr.Literal(0.0)),
            Expr.Negate(Expr.Literal(1.0)),
            Expr.Boolean(true),
            Expr.Null,
        };

        foreach (var expr in expressions)
            interner.Intern(expr);

        interner.Count.Should().Be(9);
    }

    [Fact]
    public void Statistics_AfterMultipleRoundsOfInternAndClear()
    {
        var interner = new ExpressionInterner();

        interner.Intern(Expr.Literal(1.0));
        interner.Intern(Expr.Literal(1.0));
        interner.Clear();

        interner.Intern(Expr.Literal(2.0));
        interner.Intern(Expr.Literal(2.0));

        var stats = interner.Statistics;
        stats.Hits.Should().Be(1);
        stats.Misses.Should().Be(1);
        stats.UniqueCount.Should().Be(1);
    }

    [Fact]
    public void ExpressionCache_BasicOperations()
    {
        var cache = new ExpressionCache();
        var expr = Expr.Literal(42.0);

        cache.TryGet(expr, out _).Should().BeFalse();

        cache.Add(expr);
        cache.TryGet(expr, out var found).Should().BeTrue();
        found.Should().Be(expr);
    }

    [Fact]
    public void ExpressionCache_StructuralEquality()
    {
        var cache = new ExpressionCache();
        var a = Expr.Add(Expr.Variable("x"), Expr.Literal(1.0));
        cache.Add(a);

        var b = Expr.Add(Expr.Variable("x"), Expr.Literal(1.0));
        cache.TryGet(b, out var found).Should().BeTrue();
        found.Should().BeSameAs(a);
    }

    [Fact]
    public void ExpressionCache_Clear()
    {
        var cache = new ExpressionCache();
        cache.Add(Expr.Literal(1.0));
        cache.Add(Expr.Literal(2.0));

        cache.Clear();

        cache.Count.Should().Be(0);
    }

    [Fact]
    public void ExpressionCache_StatisticsTracking()
    {
        var cache = new ExpressionCache();
        var expr = Expr.Literal(5.0);

        cache.TryGet(expr, out _);
        cache.Add(expr);
        cache.TryGet(expr, out _);
        cache.TryGet(expr, out _);

        var stats = cache.Statistics;
        stats.Hits.Should().Be(2);
        stats.Misses.Should().Be(1);
        stats.UniqueCount.Should().Be(1);
    }

    [Fact]
    public void ExpressionCache_NullExpression_Throws()
    {
        var cache = new ExpressionCache();
        Action act = () => cache.TryGet(null!, out _);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ExpressionCache_AddNull_Throws()
    {
        var cache = new ExpressionCache();
        Action act = () => cache.Add(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ExpressionCache_StatisticsResetOnClear()
    {
        var cache = new ExpressionCache();
        cache.Add(Expr.Literal(1.0));
        cache.TryGet(Expr.Literal(1.0), out _);

        cache.Clear();
        var stats = cache.Statistics;

        stats.Hits.Should().Be(0);
        stats.Misses.Should().Be(0);
    }

    [Fact]
    public void ExpressionIdentity_ReferenceEquality_SameObject()
    {
        var expr = Expr.Literal(42.0);
        ExpressionIdentity.Instance.Equals(expr, expr).Should().BeTrue();
    }

    [Fact]
    public void ExpressionIdentity_ReferenceEquality_DifferentObjects()
    {
        var a = Expr.Literal(42.0);
        var b = Expr.Literal(42.0);
        ExpressionIdentity.Instance.Equals(a, b).Should().BeFalse();
    }

    [Fact]
    public void ExpressionIdentity_NullEquality()
    {
        ExpressionIdentity.Instance.Equals(null, null).Should().BeTrue();
    }

    [Fact]
    public void ExpressionIdentity_OneNull()
    {
        var expr = Expr.Literal(1.0);
        ExpressionIdentity.Instance.Equals(expr, null).Should().BeFalse();
        ExpressionIdentity.Instance.Equals(null, expr).Should().BeFalse();
    }

    [Fact]
    public void ExpressionIdentity_GetHashCode_IsIdentityHashCode()
    {
        var expr = Expr.Literal(42.0);
        var expected = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(expr);

        ExpressionIdentity.Instance.GetHashCode(expr).Should().Be(expected);
    }

    [Fact]
    public void ExpressionIdentity_Singleton()
    {
        ExpressionIdentity.Instance.Should().BeSameAs(ExpressionIdentity.Instance);
    }

    [Fact]
    public void ExpressionKey_Equals_StructurallyEqual()
    {
        var a = new ExpressionKey(Expr.Literal(42.0));
        var b = new ExpressionKey(Expr.Literal(42.0));

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void ExpressionKey_Equals_DifferentExpressions()
    {
        var a = new ExpressionKey(Expr.Literal(1.0));
        var b = new ExpressionKey(Expr.Literal(2.0));

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void ExpressionKey_Equals_Null()
    {
        var key = new ExpressionKey(Expr.Literal(1.0));
        key.Equals((ExpressionKey?)null).Should().BeFalse();
    }

    [Fact]
    public void ExpressionKey_Equals_Object()
    {
        var a = new ExpressionKey(Expr.Literal(1.0));
        var b = new ExpressionKey(Expr.Literal(1.0));

        a.Equals((object)b).Should().BeTrue();
    }

    [Fact]
    public void ExpressionKey_Equals_WrongType()
    {
        var key = new ExpressionKey(Expr.Literal(1.0));
        key.Equals("not a key").Should().BeFalse();
    }

    [Fact]
    public void ExpressionKey_GetHashCode_EqualForEqualExpressions()
    {
        var a = new ExpressionKey(Expr.Literal(42.0));
        var b = new ExpressionKey(Expr.Literal(42.0));

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void ExpressionKey_NullExpression_Throws()
    {
        Action act = () => new ExpressionKey(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ExpressionKey_StoresExpression()
    {
        var expr = Expr.Variable("x");
        var key = new ExpressionKey(expr);

        key.Expression.Should().BeSameAs(expr);
    }

    [Fact]
    public void ExpressionKey_HashCodeMatchesExpression()
    {
        var expr = Expr.Add(Expr.Literal(1.0), Expr.Literal(2.0));
        var key = new ExpressionKey(expr);

        key.HashCode.Should().Be(expr.GetHashCode());
    }

    [Fact]
    public async Task ExpressionCache_ThreadSafety()
    {
        var cache = new ExpressionCache();
        var tasks = Enumerable.Range(0, 100)
            .Select(i => Task.Run(() =>
            {
                var expr = Expr.Literal(i % 5);
                cache.Add(expr);
                cache.TryGet(expr, out _);
            }))
            .ToArray();

        await Task.WhenAll(tasks);
        cache.Count.Should().Be(5);
    }

    [Fact]
    public void InternStatistics_RecordStructEquality()
    {
        var a = new InternStatistics { Hits = 1, Misses = 2, TotalLookups = 3, UniqueCount = 1 };
        var b = new InternStatistics { Hits = 1, Misses = 2, TotalLookups = 3, UniqueCount = 1 };

        a.Should().Be(b);
    }

    [Fact]
    public void InternStatistics_ToString()
    {
        var stats = new InternStatistics { Hits = 5, Misses = 3, TotalLookups = 8, UniqueCount = 4 };

        var str = stats.ToString();

        str.Should().Contain("Hits=5");
        str.Should().Contain("Misses=3");
        str.Should().Contain("Unique=4");
    }

    [Fact]
    public void ExpressionCache_DoesNotReturnStaleReference()
    {
        var cache = new ExpressionCache();
        var a = Expr.Literal(1.0);
        cache.Add(a);
        cache.TryGet(Expr.Literal(1.0), out var found).Should().BeTrue();
        found.Should().BeSameAs(a);
    }

    [Fact]
    public void Intern_DuplicateComplexTree()
    {
        var interner = new ExpressionInterner();
        var tree = Expr.Add(
            Expr.Multiply(Expr.Variable("x"), Expr.Literal(2.0)),
            Expr.Pow(Expr.Variable("y"), Expr.Literal(3.0)));

        var a = interner.Intern(tree);
        var b = interner.Intern(Expr.Add(
            Expr.Multiply(Expr.Variable("x"), Expr.Literal(2.0)),
            Expr.Pow(Expr.Variable("y"), Expr.Literal(3.0))));

        a.Should().BeSameAs(b);
        interner.Count.Should().Be(1);
    }

    [Fact]
    public void Intern_NearlyIdenticalTrees_AreDistinct()
    {
        var interner = new ExpressionInterner();
        var a = interner.Intern(Expr.Add(Expr.Variable("x"), Expr.Literal(2.0)));
        var b = interner.Intern(Expr.Add(Expr.Variable("x"), Expr.Literal(3.0)));

        a.Should().NotBeSameAs(b);
        interner.Count.Should().Be(2);
    }

    [Fact]
    public void Intern_MultiplyByNegativeOne()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Multiply(Expr.Literal(-1.0), Expr.Variable("x"));
        var b = Expr.Multiply(Expr.Literal(-1.0), Expr.Variable("x"));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_DivideExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Divide(Expr.Variable("x"), Expr.Literal(2.0));
        var b = Expr.Divide(Expr.Variable("x"), Expr.Literal(2.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_DivideDifferentDenominator()
    {
        var interner = new ExpressionInterner();
        var a = interner.Intern(Expr.Divide(Expr.Variable("x"), Expr.Literal(2.0)));
        var b = interner.Intern(Expr.Divide(Expr.Variable("x"), Expr.Literal(3.0)));

        a.Should().NotBeSameAs(b);
    }

    [Fact]
    public void Intern_LessThanOrEqual()
    {
        var interner = new ExpressionInterner();
        var a = Expr.LessThanOrEqual(Expr.Variable("x"), Expr.Literal(5.0));
        var b = Expr.LessThanOrEqual(Expr.Variable("x"), Expr.Literal(5.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_GreaterThan()
    {
        var interner = new ExpressionInterner();
        var a = Expr.GreaterThan(Expr.Variable("x"), Expr.Literal(5.0));
        var b = Expr.GreaterThan(Expr.Variable("x"), Expr.Literal(5.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_GreaterThanOrEqual()
    {
        var interner = new ExpressionInterner();
        var a = Expr.GreaterThanOrEqual(Expr.Variable("x"), Expr.Literal(5.0));
        var b = Expr.GreaterThanOrEqual(Expr.Variable("x"), Expr.Literal(5.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_NotEqual()
    {
        var interner = new ExpressionInterner();
        var a = Expr.NotEqual(Expr.Variable("x"), Expr.Literal(5.0));
        var b = Expr.NotEqual(Expr.Variable("x"), Expr.Literal(5.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_EqualExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Equal(Expr.Variable("x"), Expr.Literal(5.0));
        var b = Expr.Equal(Expr.Variable("x"), Expr.Literal(5.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_NegateExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Negate(Expr.Literal(5.0));
        var b = Expr.Negate(Expr.Literal(5.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_AbsExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Abs(Expr.Literal(5.0));
        var b = Expr.Abs(Expr.Literal(5.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_SqrtExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Sqrt(Expr.Literal(4.0));
        var b = Expr.Sqrt(Expr.Literal(4.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_ExpExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Exp(Expr.Literal(1.0));
        var b = Expr.Exp(Expr.Literal(1.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_LnExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Ln(Expr.Literal(1.0));
        var b = Expr.Ln(Expr.Literal(1.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_Log10Expression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Log10(Expr.Literal(100.0));
        var b = Expr.Log10(Expr.Literal(100.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_TanExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Tan(Expr.Literal(0.0));
        var b = Expr.Tan(Expr.Literal(0.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_AsinExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Asin(Expr.Literal(0.5));
        var b = Expr.Asin(Expr.Literal(0.5));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_AcosExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Acos(Expr.Literal(0.5));
        var b = Expr.Acos(Expr.Literal(0.5));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_AtanExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Atan(Expr.Literal(1.0));
        var b = Expr.Atan(Expr.Literal(1.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_SinhExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Sinh(Expr.Literal(1.0));
        var b = Expr.Sinh(Expr.Literal(1.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_CoshExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Cosh(Expr.Literal(1.0));
        var b = Expr.Cosh(Expr.Literal(1.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_TanhExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Tanh(Expr.Literal(1.0));
        var b = Expr.Tanh(Expr.Literal(1.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_CbrtExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Cbrt(Expr.Literal(8.0));
        var b = Expr.Cbrt(Expr.Literal(8.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_SquareExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Square(Expr.Variable("x"));
        var b = Expr.Square(Expr.Variable("x"));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_CubeExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Cube(Expr.Variable("x"));
        var b = Expr.Cube(Expr.Variable("x"));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_ConditionalExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Conditional(Expr.Boolean(true), Expr.Literal(1.0), Expr.Literal(0.0));
        var b = Expr.Conditional(Expr.Boolean(true), Expr.Literal(1.0), Expr.Literal(0.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_DifferentConditional_AreDistinct()
    {
        var interner = new ExpressionInterner();
        var a = interner.Intern(Expr.Conditional(Expr.Boolean(true), Expr.Literal(1.0), Expr.Literal(0.0)));
        var b = interner.Intern(Expr.Conditional(Expr.Boolean(false), Expr.Literal(1.0), Expr.Literal(0.0)));

        a.Should().NotBeSameAs(b);
    }

    [Fact]
    public void Intern_TupleExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Tuple(Expr.Literal(1.0), Expr.Literal(2.0));
        var b = Expr.Tuple(Expr.Literal(1.0), Expr.Literal(2.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_IntervalExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Interval(Expr.Literal(0.0), Expr.Literal(1.0));
        var b = Expr.Interval(Expr.Literal(0.0), Expr.Literal(1.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_DifferentInterval_AreDistinct()
    {
        var interner = new ExpressionInterner();
        var a = interner.Intern(Expr.Interval(Expr.Literal(0.0), Expr.Literal(1.0)));
        var b = interner.Intern(Expr.Interval(Expr.Literal(0.0), Expr.Literal(2.0)));

        a.Should().NotBeSameAs(b);
    }

    [Fact]
    public void Intern_SetExpression()
    {
        var interner = new ExpressionInterner();
        var a = Expr.Set(Expr.Literal(1.0), Expr.Literal(2.0));
        var b = Expr.Set(Expr.Literal(1.0), Expr.Literal(2.0));

        interner.Intern(a).Should().BeSameAs(interner.Intern(b));
    }

    [Fact]
    public void Intern_ComplexNestedTree()
    {
        var interner = new ExpressionInterner();
        var tree = Expr.Add(
            Expr.Multiply(Expr.Sin(Expr.Variable("x")), Expr.Cos(Expr.Variable("y"))),
            Expr.Pow(Expr.Literal(2.0), Expr.Variable("z")));

        var a = interner.Intern(tree);
        var b = interner.Intern(Expr.Add(
            Expr.Multiply(Expr.Sin(Expr.Variable("x")), Expr.Cos(Expr.Variable("y"))),
            Expr.Pow(Expr.Literal(2.0), Expr.Variable("z"))));

        a.Should().BeSameAs(b);
    }

    [Fact]
    public void ExpressionCache_MultipleAddsAndRetrieves()
    {
        var cache = new ExpressionCache();
        var exprs = Enumerable.Range(0, 20)
            .Select(i => Expr.Literal((double)i))
            .ToArray();

        foreach (var e in exprs)
            cache.Add(e);

        foreach (var e in exprs)
            cache.TryGet(e, out _).Should().BeTrue();

        cache.Count.Should().Be(20);
    }

    [Fact]
    public void ExpressionCache_TryGetAfterClear()
    {
        var cache = new ExpressionCache();
        cache.Add(Expr.Literal(1.0));
        cache.Clear();

        cache.TryGet(Expr.Literal(1.0), out _).Should().BeFalse();
    }

    [Fact]
    public void ExpressionIdentity_MultipleDifferentExpressions()
    {
        var a = Expr.Literal(1.0);
        var b = Expr.Literal(2.0);
        var c = Expr.Variable("x");

        ExpressionIdentity.Instance.Equals(a, b).Should().BeFalse();
        ExpressionIdentity.Instance.Equals(a, c).Should().BeFalse();
        ExpressionIdentity.Instance.Equals(b, c).Should().BeFalse();
    }

    [Fact]
    public void ExpressionKey_DifferentHashCodes()
    {
        var a = new ExpressionKey(Expr.Literal(1.0));
        var b = new ExpressionKey(Expr.Literal(2.0));

        a.GetHashCode().Should().NotBe(b.GetHashCode());
    }

    [Fact]
    public void InternStatistics_HitRatio_AllHits()
    {
        var stats = new InternStatistics { Hits = 10, Misses = 0, TotalLookups = 10 };
        stats.HitRatio.Should().Be(1.0);
    }

    [Fact]
    public void InternStatistics_HitRatio_AllMisses()
    {
        var stats = new InternStatistics { Hits = 0, Misses = 10, TotalLookups = 10 };
        stats.HitRatio.Should().Be(0.0);
    }

    [Fact]
    public void ExpressionCache_StructuralEquality_FunctionCall()
    {
        var cache = new ExpressionCache();
        var a = Expr.Sin(Expr.Variable("x"));
        cache.Add(a);

        var b = Expr.Sin(Expr.Variable("x"));
        cache.TryGet(b, out var found).Should().BeTrue();
        found.Should().BeSameAs(a);
    }

    [Fact]
    public void ExpressionCache_DifferentFunction_NotFound()
    {
        var cache = new ExpressionCache();
        cache.Add(Expr.Sin(Expr.Variable("x")));

        cache.TryGet(Expr.Cos(Expr.Variable("x")), out _).Should().BeFalse();
    }

    [Fact]
    public void ExpressionCache_VectorExpression()
    {
        var cache = new ExpressionCache();
        var a = Expr.Vector(Expr.Literal(1.0), Expr.Literal(2.0));
        cache.Add(a);

        var b = Expr.Vector(Expr.Literal(1.0), Expr.Literal(2.0));
        cache.TryGet(b, out var found).Should().BeTrue();
        found.Should().BeSameAs(a);
    }

    [Fact]
    public void ExpressionKey_NullExpression_Throws2()
    {
        Action act = () => new ExpressionKey(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Intern_LargeExpressionTree()
    {
        var interner = new ExpressionInterner();
        Expression expr = Expr.Literal(1.0);
        for (int i = 0; i < 20; i++)
            expr = Expr.Add(expr, Expr.Literal((double)i));

        var a = interner.Intern(expr);

        expr = Expr.Literal(1.0);
        for (int i = 0; i < 20; i++)
            expr = Expr.Add(expr, Expr.Literal((double)i));

        var b = interner.Intern(expr);

        a.Should().BeSameAs(b);
    }

    [Fact]
    public void ExpressionCache_OverwriteSameKey()
    {
        var cache = new ExpressionCache();
        var expr = Expr.Literal(1.0);
        cache.Add(expr);

        var expr2 = Expr.Literal(1.0);
        cache.Add(expr2);

        cache.Count.Should().Be(1);
    }

    [Fact]
    public void ExpressionCache_ClearMultipleTimes()
    {
        var cache = new ExpressionCache();
        cache.Add(Expr.Literal(1.0));
        cache.Clear();
        cache.Clear();

        cache.Count.Should().Be(0);
    }

    [Fact]
    public void ExpressionKey_DifferentExpressionSameValue()
    {
        var a = new ExpressionKey(Expr.Add(Expr.Literal(1.0), Expr.Literal(2.0)));
        var b = new ExpressionKey(Expr.Add(Expr.Literal(1.0), Expr.Literal(2.0)));

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void ExpressionIdentity_SameReferenceDifferentAccess()
    {
        var expr = Expr.Literal(42.0);
        Expression a = expr;
        Expression b = expr;

        ExpressionIdentity.Instance.Equals(a, b).Should().BeTrue();
    }
}
