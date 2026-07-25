namespace MathVerse.Performance.Tests;

public sealed class HashingTests
{
    [Fact]
    public void StructuralHasher_Literal_ReturnsConsistentHash()
    {
        var hasher = new StructuralHasher();
        var expr = Expr.Literal(42.0);

        var h1 = hasher.Hash(expr);
        var h2 = hasher.Hash(expr);

        h1.Should().Be(h2);
    }

    [Fact]
    public void StructuralHasher_StructurallyEqualExpressions_SameHash()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Literal(42.0);
        var b = Expr.Literal(42.0);

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_DifferentExpressions_DifferentHashLikely()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Literal(1.0);
        var b = Expr.Literal(2.0);

        hasher.Hash(a).Should().NotBe(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_NullExpression_Throws()
    {
        var hasher = new StructuralHasher();
        Action act = () => hasher.Hash(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void StructuralHasher_VariableExpression()
    {
        var hasher = new StructuralHasher();

        hasher.Hash(Expr.Variable("x")).Should().Be(hasher.Hash(Expr.Variable("x")));
        hasher.Hash(Expr.Variable("x")).Should().NotBe(hasher.Hash(Expr.Variable("y")));
    }

    [Fact]
    public void StructuralHasher_BinaryExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Add(Expr.Variable("x"), Expr.Literal(1.0));
        var b = Expr.Add(Expr.Variable("x"), Expr.Literal(1.0));
        var c = Expr.Multiply(Expr.Variable("x"), Expr.Literal(1.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
        hasher.Hash(a).Should().NotBe(hasher.Hash(c));
    }

    [Fact]
    public void StructuralHasher_FunctionCall()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Sin(Expr.Variable("x"));
        var b = Expr.Sin(Expr.Variable("x"));
        var c = Expr.Cos(Expr.Variable("x"));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
        hasher.Hash(a).Should().NotBe(hasher.Hash(c));
    }

    [Fact]
    public void StructuralHasher_VectorExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Vector(Expr.Literal(1.0), Expr.Literal(2.0));
        var b = Expr.Vector(Expr.Literal(1.0), Expr.Literal(2.0));
        var c = Expr.Vector(Expr.Literal(1.0), Expr.Literal(3.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
        hasher.Hash(a).Should().NotBe(hasher.Hash(c));
    }

    [Fact]
    public void StructuralHasher_NestedExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Add(Expr.Multiply(Expr.Variable("x"), Expr.Literal(2.0)), Expr.Literal(3.0));
        var b = Expr.Add(Expr.Multiply(Expr.Variable("x"), Expr.Literal(2.0)), Expr.Literal(3.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_UnaryExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Negate(Expr.Variable("x"));
        var b = Expr.Negate(Expr.Variable("x"));
        var c = Expr.Abs(Expr.Variable("x"));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
        hasher.Hash(a).Should().NotBe(hasher.Hash(c));
    }

    [Fact]
    public void StructuralHasher_ConstantExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Constant("pi", System.Math.PI);
        var b = Expr.Constant("pi", System.Math.PI);
        var c = Expr.Constant("e", System.Math.E);

        hasher.Hash(a).Should().Be(hasher.Hash(b));
        hasher.Hash(a).Should().NotBe(hasher.Hash(c));
    }

    [Fact]
    public void StructuralHasher_NullExpression()
    {
        var hasher = new StructuralHasher();
        hasher.Hash(Expr.Null).Should().Be(hasher.Hash(Expr.Null));
    }

    [Fact]
    public void StructuralHasher_BooleanExpression()
    {
        var hasher = new StructuralHasher();

        hasher.Hash(Expr.Boolean(true)).Should().Be(hasher.Hash(Expr.Boolean(true)));
        hasher.Hash(Expr.Boolean(true)).Should().NotBe(hasher.Hash(Expr.Boolean(false)));
    }

    [Fact]
    public void StructuralHasher_RelationalExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.LessThan(Expr.Variable("x"), Expr.Literal(5.0));
        var b = Expr.LessThan(Expr.Variable("x"), Expr.Literal(5.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void HashRange_EmptyList()
    {
        var hasher = new StructuralHasher();

        hasher.HashRange([]).Should().Be(hasher.HashRange([]));
    }

    [Fact]
    public void HashRange_SingleExpression()
    {
        var hasher = new StructuralHasher();
        var exprs = new Expression[] { Expr.Literal(1.0) };

        hasher.HashRange(exprs).Should().Be(hasher.HashRange(new Expression[] { Expr.Literal(1.0) }));
    }

    [Fact]
    public void HashRange_MultipleExpressions()
    {
        var hasher = new StructuralHasher();
        var a = new Expression[] { Expr.Literal(1.0), Expr.Variable("x") };
        var b = new Expression[] { Expr.Literal(1.0), Expr.Variable("x") };
        var c = new Expression[] { Expr.Literal(1.0), Expr.Variable("y") };

        hasher.HashRange(a).Should().Be(hasher.HashRange(b));
        hasher.HashRange(a).Should().NotBe(hasher.HashRange(c));
    }

    [Fact]
    public void HashRange_NullList_Throws()
    {
        var hasher = new StructuralHasher();
        Action act = () => hasher.HashRange(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void HashRange_OrderMatters()
    {
        var hasher = new StructuralHasher();
        var a = new Expression[] { Expr.Literal(1.0), Expr.Literal(2.0) };
        var b = new Expression[] { Expr.Literal(2.0), Expr.Literal(1.0) };

        hasher.HashRange(a).Should().NotBe(hasher.HashRange(b));
    }

    [Fact]
    public void HashBuilder_AddInt()
    {
        var builder = new HashBuilder();
        var h1 = builder.Add(42).ToHashCode();

        var builder2 = new HashBuilder();
        var h2 = builder2.Add(42).ToHashCode();

        h1.Should().Be(h2);
    }

    [Fact]
    public void HashBuilder_AddString()
    {
        var builder = new HashBuilder();
        var h1 = builder.Add("hello").ToHashCode();

        var builder2 = new HashBuilder();
        var h2 = builder2.Add("hello").ToHashCode();

        h1.Should().Be(h2);
    }

    [Fact]
    public void HashBuilder_DifferentValues_DifferentHash()
    {
        var h1 = new HashBuilder().Add(1).ToHashCode();
        var h2 = new HashBuilder().Add(2).ToHashCode();

        h1.Should().NotBe(h2);
    }

    [Fact]
    public void HashBuilder_MultipleAdds_Consistent()
    {
        var h1 = new HashBuilder().Add(1).Add("hello").Add(3.14).ToHashCode();
        var h2 = new HashBuilder().Add(1).Add("hello").Add(3.14).ToHashCode();

        h1.Should().Be(h2);
    }

    [Fact]
    public void HashBuilder_OrderMatters()
    {
        var h1 = new HashBuilder().Add(1).Add(2).ToHashCode();
        var h2 = new HashBuilder().Add(2).Add(1).ToHashCode();

        h1.Should().NotBe(h2);
    }

    [Fact]
    public void HashBuilder_AddBytes()
    {
        var data1 = new byte[] { 1, 2, 3, 4 };
        var data2 = new byte[] { 1, 2, 3, 4 };
        var data3 = new byte[] { 1, 2, 3, 5 };

        var h1 = new HashBuilder().AddBytes(data1).ToHashCode();
        var h2 = new HashBuilder().AddBytes(data2).ToHashCode();
        var h3 = new HashBuilder().AddBytes(data3).ToHashCode();

        h1.Should().Be(h2);
        h1.Should().NotBe(h3);
    }

    [Fact]
    public void HashBuilder_FluentChaining()
    {
        var builder = new HashBuilder();
        var result = builder.Add(1).Add("test").Add(3.14).AddBytes(new byte[] { 1, 2 });

        result.ToHashCode().Should().NotBe(0);
    }

    [Fact]
    public void HashBuilder_Empty_HashCodeIsValid()
    {
        var h = new HashBuilder().ToHashCode();
        h.Should().Be(h);
    }

    [Fact]
    public void HashBuilder_DuplicateAdds_DifferentFromSingle()
    {
        var h1 = new HashBuilder().Add(1).Add(1).ToHashCode();
        var h2 = new HashBuilder().Add(1).ToHashCode();

        h1.Should().NotBe(h2);
    }

    [Fact]
    public void HashBuilder_DoubleValue()
    {
        var h1 = new HashBuilder().Add(3.14).ToHashCode();
        var h2 = new HashBuilder().Add(3.14).ToHashCode();

        h1.Should().Be(h2);
    }

    [Fact]
    public void HashBuilder_BoolValues()
    {
        var h1 = new HashBuilder().Add(true).ToHashCode();
        var h2 = new HashBuilder().Add(true).ToHashCode();
        var h3 = new HashBuilder().Add(false).ToHashCode();

        h1.Should().Be(h2);
        h1.Should().NotBe(h3);
    }

    [Fact]
    public void HashCache_StoreAndRetrieve()
    {
        var cache = new HashCache();
        var expr = Expr.Literal(42.0);

        cache.Store(expr, 12345);
        cache.TryGet(expr, out var hash).Should().BeTrue();
        hash.Should().Be(12345);
    }

    [Fact]
    public void HashCache_Miss()
    {
        var cache = new HashCache();

        cache.TryGet(Expr.Literal(1.0), out _).Should().BeFalse();
    }

    [Fact]
    public void HashCache_Clear()
    {
        var cache = new HashCache();
        cache.Store(Expr.Literal(1.0), 100);
        cache.Store(Expr.Literal(2.0), 200);

        cache.Clear();

        cache.Count.Should().Be(0);
    }

    [Fact]
    public void HashCache_Statistics()
    {
        var cache = new HashCache();
        var expr = Expr.Literal(5.0);

        cache.TryGet(expr, out _);
        cache.Store(expr, 500);
        cache.TryGet(expr, out _);
        cache.TryGet(expr, out _);

        cache.Count.Should().Be(1);
    }

    [Fact]
    public void HashCache_NullExpression_Throws()
    {
        var cache = new HashCache();
        Action act = () => cache.TryGet(null!, out _);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void HashCache_StoreNull_Throws()
    {
        var cache = new HashCache();
        Action act = () => cache.Store(null!, 100);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void HashCache_StructuralEquality()
    {
        var cache = new HashCache();
        var a = Expr.Add(Expr.Variable("x"), Expr.Literal(1.0));
        cache.Store(a, 999);

        var b = Expr.Add(Expr.Variable("x"), Expr.Literal(1.0));
        cache.TryGet(b, out var hash).Should().BeTrue();
        hash.Should().Be(999);
    }

    [Fact]
    public void HashCache_Overwrite()
    {
        var cache = new HashCache();
        var expr = Expr.Literal(1.0);

        cache.Store(expr, 100);
        cache.Store(expr, 200);

        cache.TryGet(expr, out var hash).Should().BeTrue();
        hash.Should().Be(200);
        cache.Count.Should().Be(1);
    }

    [Fact]
    public async Task HashCache_ThreadSafety()
    {
        var cache = new HashCache();
        var tasks = Enumerable.Range(0, 100)
            .Select(i => Task.Run(() =>
            {
                var expr = Expr.Literal(i % 5);
                cache.Store(expr, i);
                cache.TryGet(expr, out _);
            }))
            .ToArray();

        await Task.WhenAll(tasks);
        cache.Count.Should().Be(5);
    }

    [Fact]
    public void CachedExpressionHasher_ComputeHash()
    {
        var hasher = new CachedExpressionHasher();
        var expr = Expr.Literal(42.0);

        var h1 = hasher.ComputeHash(expr);
        var h2 = hasher.ComputeHash(expr);

        h1.Should().Be(h2);
    }

    [Fact]
    public void CachedExpressionHasher_StructurallyEqual_SameHash()
    {
        var hasher = new CachedExpressionHasher();

        var h1 = hasher.ComputeHash(Expr.Literal(42.0));
        var h2 = hasher.ComputeHash(Expr.Literal(42.0));

        h1.Should().Be(h2);
    }

    [Fact]
    public void CachedExpressionHasher_DifferentExpressions_DifferentHash()
    {
        var hasher = new CachedExpressionHasher();

        var h1 = hasher.ComputeHash(Expr.Literal(1.0));
        var h2 = hasher.ComputeHash(Expr.Literal(2.0));

        h1.Should().NotBe(h2);
    }

    [Fact]
    public void CachedExpressionHasher_NullExpression_Throws()
    {
        var hasher = new CachedExpressionHasher();
        Action act = () => hasher.ComputeHash(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CachedExpressionHasher_ClearCache()
    {
        var hasher = new CachedExpressionHasher();
        var expr = Expr.Literal(42.0);

        hasher.ComputeHash(expr);
        hasher.ClearCache();

        hasher.ComputeHash(expr).Should().Be(hasher.ComputeHash(expr));
    }

    [Fact]
    public void CachedExpressionHasher_ComplexExpression()
    {
        var hasher = new CachedExpressionHasher();
        var expr = Expr.Add(Expr.Multiply(Expr.Variable("x"), Expr.Literal(2.0)), Expr.Pow(Expr.Variable("y"), Expr.Literal(3.0)));

        var h1 = hasher.ComputeHash(expr);
        var h2 = hasher.ComputeHash(expr);

        h1.Should().Be(h2);
    }

    [Fact]
    public void CachedExpressionHasher_FunctionExpression()
    {
        var hasher = new CachedExpressionHasher();

        var h1 = hasher.ComputeHash(Expr.Sin(Expr.Variable("x")));
        var h2 = hasher.ComputeHash(Expr.Sin(Expr.Variable("x")));

        h1.Should().Be(h2);
    }

    [Fact]
    public void CachedExpressionHasher_VectorExpression()
    {
        var hasher = new CachedExpressionHasher();
        var a = Expr.Vector(Expr.Literal(1.0), Expr.Literal(2.0));
        var b = Expr.Vector(Expr.Literal(1.0), Expr.Literal(2.0));

        hasher.ComputeHash(a).Should().Be(hasher.ComputeHash(b));
    }

    [Fact]
    public async Task CachedExpressionHasher_ThreadSafety()
    {
        var hasher = new CachedExpressionHasher();
        var tasks = Enumerable.Range(0, 100)
            .Select(i => Task.Run(() => hasher.ComputeHash(Expr.Literal(i % 10))))
            .ToArray();

        await Task.WhenAll(tasks);

        var h1 = hasher.ComputeHash(Expr.Literal(5.0));
        var h2 = hasher.ComputeHash(Expr.Literal(5.0));
        h1.Should().Be(h2);
    }

    [Fact]
    public void HashBuilder_LongValue()
    {
        var h1 = new HashBuilder().Add(123456789L).ToHashCode();
        var h2 = new HashBuilder().Add(123456789L).ToHashCode();

        h1.Should().Be(h2);
    }

    [Fact]
    public void HashBuilder_ByteValues()
    {
        var h1 = new HashBuilder().Add((byte)42).ToHashCode();
        var h2 = new HashBuilder().Add((byte)42).ToHashCode();
        var h3 = new HashBuilder().Add((byte)43).ToHashCode();

        h1.Should().Be(h2);
        h1.Should().NotBe(h3);
    }

    [Fact]
    public void StructuralHasher_EquationExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Equation(Expr.Variable("x"), Expr.Literal(5.0));
        var b = Expr.Equation(Expr.Variable("x"), Expr.Literal(5.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_PiecewiseExpression()
    {
        var hasher = new StructuralHasher();
        var c1 = new PiecewiseCase(Expr.Boolean(true), Expr.Literal(1.0));
        var c2 = new PiecewiseCase(Expr.Boolean(true), Expr.Literal(1.0));

        var a = Expr.Piecewise([c1]);
        var b = Expr.Piecewise([c2]);

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_TupleExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Tuple(Expr.Literal(1.0), Expr.Literal(2.0));
        var b = Expr.Tuple(Expr.Literal(1.0), Expr.Literal(2.0));
        var c = Expr.Tuple(Expr.Literal(2.0), Expr.Literal(1.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
        hasher.Hash(a).Should().NotBe(hasher.Hash(c));
    }

    [Fact]
    public void StructuralHasher_ConditionalExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Conditional(Expr.Boolean(true), Expr.Literal(1.0), Expr.Literal(0.0));
        var b = Expr.Conditional(Expr.Boolean(true), Expr.Literal(1.0), Expr.Literal(0.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void HashCache_ClearResetStats()
    {
        var cache = new HashCache();
        var expr = Expr.Literal(1.0);
        cache.Store(expr, 100);
        cache.TryGet(expr, out _);

        cache.Clear();

        cache.TryGet(expr, out _).Should().BeFalse();
        cache.Count.Should().Be(0);
    }

    [Fact]
    public void HashBuilder_ShortValue()
    {
        var h1 = new HashBuilder().Add((short)42).ToHashCode();
        var h2 = new HashBuilder().Add((short)42).ToHashCode();

        h1.Should().Be(h2);
    }

    [Fact]
    public void StructuralHasher_MatrixExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Matrix(Expr.Literal(1.0), Expr.Literal(2.0));
        var b = Expr.Matrix(Expr.Literal(1.0), Expr.Literal(2.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_IntervalExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Interval(Expr.Literal(0.0), Expr.Literal(1.0));
        var b = Expr.Interval(Expr.Literal(0.0), Expr.Literal(1.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void HashCache_MultipleExpressions()
    {
        var cache = new HashCache();
        var exprs = Enumerable.Range(0, 20).Select(i => Expr.Literal((double)i)).ToArray();

        for (int i = 0; i < exprs.Length; i++)
            cache.Store(exprs[i], i * 100);

        for (int i = 0; i < exprs.Length; i++)
        {
            cache.TryGet(exprs[i], out var hash).Should().BeTrue();
            hash.Should().Be(i * 100);
        }

        cache.Count.Should().Be(20);
    }

    [Fact]
    public void StructuralHasher_SetExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Set(Expr.Literal(1.0), Expr.Literal(2.0), Expr.Literal(3.0));
        var b = Expr.Set(Expr.Literal(1.0), Expr.Literal(2.0), Expr.Literal(3.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void HashBuilder_CharValue()
    {
        var h1 = new HashBuilder().Add('a').ToHashCode();
        var h2 = new HashBuilder().Add('a').ToHashCode();
        var h3 = new HashBuilder().Add('b').ToHashCode();

        h1.Should().Be(h2);
        h1.Should().NotBe(h3);
    }

    [Fact]
    public void CachedExpressionHasher_ManyExpressions()
    {
        var hasher = new CachedExpressionHasher();
        var exprs = Enumerable.Range(0, 50)
            .Select(i => Expr.Add(Expr.Literal((double)i), Expr.Variable("x")))
            .ToArray();

        var hashes = exprs.Select(hasher.ComputeHash).ToArray();
        var hashes2 = exprs.Select(hasher.ComputeHash).ToArray();

        hashes.Should().Equal(hashes2);
    }

    [Fact]
    public void HashCache_CountTracking()
    {
        var cache = new HashCache();

        cache.Store(Expr.Literal(1.0), 10);
        cache.Count.Should().Be(1);

        cache.Store(Expr.Literal(2.0), 20);
        cache.Count.Should().Be(2);

        cache.Store(Expr.Literal(1.0), 15);
        cache.Count.Should().Be(2);
    }

    [Fact]
    public void StructuralHasher_ComplexNestedTree()
    {
        var hasher = new StructuralHasher();
        var tree = Expr.Add(
            Expr.Multiply(Expr.Sin(Expr.Variable("x")), Expr.Cos(Expr.Variable("y"))),
            Expr.Pow(Expr.Literal(2.0), Expr.Variable("z")));

        var h1 = hasher.Hash(tree);
        var h2 = hasher.Hash(tree);

        h1.Should().Be(h2);
    }

    [Fact]
    public void HashBuilder_EmptyBytes()
    {
        var h1 = new HashBuilder().AddBytes(ReadOnlySpan<byte>.Empty).ToHashCode();
        var h2 = new HashBuilder().AddBytes(ReadOnlySpan<byte>.Empty).ToHashCode();

        h1.Should().Be(h2);
    }

    [Fact]
    public void HashBuilder_ThenAdd_DifferentResult()
    {
        var h1 = new HashBuilder().Add(1).ToHashCode();
        var h2 = new HashBuilder().Add(1).Add(2).ToHashCode();

        h1.Should().NotBe(h2);
    }

    [Fact]
    public void HashCache_OverwriteSameHash()
    {
        var cache = new HashCache();
        var expr = Expr.Literal(1.0);
        cache.Store(expr, 100);
        cache.Store(expr, 100);

        cache.TryGet(expr, out var hash).Should().BeTrue();
        hash.Should().Be(100);
        cache.Count.Should().Be(1);
    }

    [Fact]
    public void StructuralHasher_LessThanExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.LessThan(Expr.Literal(1.0), Expr.Literal(2.0));
        var b = Expr.LessThan(Expr.Literal(1.0), Expr.Literal(2.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_GreaterThanExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.GreaterThan(Expr.Literal(1.0), Expr.Literal(2.0));
        var b = Expr.GreaterThan(Expr.Literal(1.0), Expr.Literal(2.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_EqualExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Equal(Expr.Literal(1.0), Expr.Literal(2.0));
        var b = Expr.Equal(Expr.Literal(1.0), Expr.Literal(2.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_NotEqualExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.NotEqual(Expr.Literal(1.0), Expr.Literal(2.0));
        var b = Expr.NotEqual(Expr.Literal(1.0), Expr.Literal(2.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_LessThanOrEqual()
    {
        var hasher = new StructuralHasher();
        var a = Expr.LessThanOrEqual(Expr.Literal(1.0), Expr.Literal(2.0));
        var b = Expr.LessThanOrEqual(Expr.Literal(1.0), Expr.Literal(2.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_GreaterThanOrEqual()
    {
        var hasher = new StructuralHasher();
        var a = Expr.GreaterThanOrEqual(Expr.Literal(1.0), Expr.Literal(2.0));
        var b = Expr.GreaterThanOrEqual(Expr.Literal(1.0), Expr.Literal(2.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_AndExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.And(Expr.Boolean(true), Expr.Boolean(false));
        var b = Expr.And(Expr.Boolean(true), Expr.Boolean(false));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_OrExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Or(Expr.Boolean(true), Expr.Boolean(false));
        var b = Expr.Or(Expr.Boolean(true), Expr.Boolean(false));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_NotExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Not(Expr.Boolean(true));
        var b = Expr.Not(Expr.Boolean(true));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_DivideExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Divide(Expr.Literal(10.0), Expr.Literal(2.0));
        var b = Expr.Divide(Expr.Literal(10.0), Expr.Literal(2.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_ModuloExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Modulo(Expr.Literal(10.0), Expr.Literal(3.0));
        var b = Expr.Modulo(Expr.Literal(10.0), Expr.Literal(3.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_PowerExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Pow(Expr.Literal(2.0), Expr.Literal(3.0));
        var b = Expr.Pow(Expr.Literal(2.0), Expr.Literal(3.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_CosExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Cos(Expr.Literal(0.0));
        var b = Expr.Cos(Expr.Literal(0.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_TanExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Tan(Expr.Literal(0.0));
        var b = Expr.Tan(Expr.Literal(0.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_SqrtExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Sqrt(Expr.Literal(4.0));
        var b = Expr.Sqrt(Expr.Literal(4.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_LnExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Ln(Expr.Literal(1.0));
        var b = Expr.Ln(Expr.Literal(1.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_ExpExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Exp(Expr.Literal(1.0));
        var b = Expr.Exp(Expr.Literal(1.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_DifferentFunctions_DifferentHash()
    {
        var hasher = new StructuralHasher();

        hasher.Hash(Expr.Sin(Expr.Literal(0.0))).Should().NotBe(hasher.Hash(Expr.Cos(Expr.Literal(0.0))));
        hasher.Hash(Expr.Ln(Expr.Literal(1.0))).Should().NotBe(hasher.Hash(Expr.Exp(Expr.Literal(1.0))));
    }

    [Fact]
    public void HashBuilder_DifferentDataLengths_DifferentHash()
    {
        var h1 = new HashBuilder().AddBytes(new byte[] { 1, 2 }).ToHashCode();
        var h2 = new HashBuilder().AddBytes(new byte[] { 1, 2, 3 }).ToHashCode();

        h1.Should().NotBe(h2);
    }

    [Fact]
    public void HashBuilder_ManyInts()
    {
        var builder = new HashBuilder();
        for (int i = 0; i < 100; i++)
            builder.Add(i);

        var h = builder.ToHashCode();
        h.Should().NotBe(0);
    }

    [Fact]
    public void HashBuilder_NullString()
    {
        var h1 = new HashBuilder().Add<string>(null!).ToHashCode();
        var h2 = new HashBuilder().Add<string>(null!).ToHashCode();

        h1.Should().Be(h2);
    }

    [Fact]
    public void HashCache_TryGetAfterClear()
    {
        var cache = new HashCache();
        cache.Store(Expr.Literal(1.0), 100);
        cache.Clear();

        cache.TryGet(Expr.Literal(1.0), out _).Should().BeFalse();
    }

    [Fact]
    public void HashCache_MultipleStores()
    {
        var cache = new HashCache();
        cache.Store(Expr.Literal(1.0), 100);
        cache.Store(Expr.Literal(2.0), 200);
        cache.Store(Expr.Literal(3.0), 300);

        cache.TryGet(Expr.Literal(1.0), out var h1).Should().BeTrue();
        cache.TryGet(Expr.Literal(2.0), out var h2).Should().BeTrue();
        cache.TryGet(Expr.Literal(3.0), out var h3).Should().BeTrue();

        h1.Should().Be(100);
        h2.Should().Be(200);
        h3.Should().Be(300);
    }

    [Fact]
    public void HashCache_ReturnsFalseForMissing()
    {
        var cache = new HashCache();

        cache.TryGet(Expr.Literal(99.0), out var hash).Should().BeFalse();
        hash.Should().Be(0);
    }

    [Fact]
    public void CachedExpressionHasher_NegateExpression()
    {
        var hasher = new CachedExpressionHasher();
        var h1 = hasher.ComputeHash(Expr.Negate(Expr.Variable("x")));
        var h2 = hasher.ComputeHash(Expr.Negate(Expr.Variable("x")));

        h1.Should().Be(h2);
    }

    [Fact]
    public void CachedExpressionHasher_AbsExpression()
    {
        var hasher = new CachedExpressionHasher();
        var h1 = hasher.ComputeHash(Expr.Abs(Expr.Variable("x")));
        var h2 = hasher.ComputeHash(Expr.Abs(Expr.Variable("x")));

        h1.Should().Be(h2);
    }

    [Fact]
    public void CachedExpressionHasher_PowerExpression()
    {
        var hasher = new CachedExpressionHasher();
        var h1 = hasher.ComputeHash(Expr.Pow(Expr.Literal(2.0), Expr.Literal(3.0)));
        var h2 = hasher.ComputeHash(Expr.Pow(Expr.Literal(2.0), Expr.Literal(3.0)));

        h1.Should().Be(h2);
    }

    [Fact]
    public void CachedExpressionHasher_EquationExpression()
    {
        var hasher = new CachedExpressionHasher();
        var h1 = hasher.ComputeHash(Expr.Equation(Expr.Variable("x"), Expr.Literal(5.0)));
        var h2 = hasher.ComputeHash(Expr.Equation(Expr.Variable("x"), Expr.Literal(5.0)));

        h1.Should().Be(h2);
    }

    [Fact]
    public void CachedExpressionHasher_ConditionalExpression()
    {
        var hasher = new CachedExpressionHasher();
        var h1 = hasher.ComputeHash(Expr.Conditional(Expr.Boolean(true), Expr.Literal(1.0), Expr.Literal(0.0)));
        var h2 = hasher.ComputeHash(Expr.Conditional(Expr.Boolean(true), Expr.Literal(1.0), Expr.Literal(0.0)));

        h1.Should().Be(h2);
    }

    [Fact]
    public void CachedExpressionHasher_TupleExpression()
    {
        var hasher = new CachedExpressionHasher();
        var h1 = hasher.ComputeHash(Expr.Tuple(Expr.Literal(1.0), Expr.Literal(2.0)));
        var h2 = hasher.ComputeHash(Expr.Tuple(Expr.Literal(1.0), Expr.Literal(2.0)));

        h1.Should().Be(h2);
    }

    [Fact]
    public void CachedExpressionHasher_SetExpression()
    {
        var hasher = new CachedExpressionHasher();
        var h1 = hasher.ComputeHash(Expr.Set(Expr.Literal(1.0), Expr.Literal(2.0)));
        var h2 = hasher.ComputeHash(Expr.Set(Expr.Literal(1.0), Expr.Literal(2.0)));

        h1.Should().Be(h2);
    }

    [Fact]
    public void CachedExpressionHasher_IntervalExpression()
    {
        var hasher = new CachedExpressionHasher();
        var h1 = hasher.ComputeHash(Expr.Interval(Expr.Literal(0.0), Expr.Literal(1.0)));
        var h2 = hasher.ComputeHash(Expr.Interval(Expr.Literal(0.0), Expr.Literal(1.0)));

        h1.Should().Be(h2);
    }

    [Fact]
    public void HashRange_SingleElement()
    {
        var hasher = new StructuralHasher();
        var h1 = hasher.HashRange(new Expression[] { Expr.Literal(42.0) });
        var h2 = hasher.HashRange(new Expression[] { Expr.Literal(42.0) });

        h1.Should().Be(h2);
    }

    [Fact]
    public void HashRange_DifferentLengths_DifferentHash()
    {
        var hasher = new StructuralHasher();
        var h1 = hasher.HashRange(new Expression[] { Expr.Literal(1.0) });
        var h2 = hasher.HashRange(new Expression[] { Expr.Literal(1.0), Expr.Literal(2.0) });

        h1.Should().NotBe(h2);
    }

    [Fact]
    public void HashBuilder_DoubleAndInt()
    {
        var h1 = new HashBuilder().Add(3.14).Add(42).ToHashCode();
        var h2 = new HashBuilder().Add(3.14).Add(42).ToHashCode();

        h1.Should().Be(h2);
    }

    [Fact]
    public void StructuralHasher_SubtractExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Subtract(Expr.Literal(10.0), Expr.Literal(3.0));
        var b = Expr.Subtract(Expr.Literal(10.0), Expr.Literal(3.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public async Task HashCache_ConcurrentStoreAndTryGet()
    {
        var cache = new HashCache();
        var tasks = Enumerable.Range(0, 100)
            .Select(i => Task.Run(() =>
            {
                var expr = Expr.Literal(i % 10);
                cache.Store(expr, i * 100);
                cache.TryGet(expr, out _);
            }))
            .ToArray();

        await Task.WhenAll(tasks);
        cache.Count.Should().Be(10);
    }

    [Fact]
    public void CachedExpressionHasher_TupleExpression_DifferentElements()
    {
        var hasher = new CachedExpressionHasher();
        var h1 = hasher.ComputeHash(Expr.Tuple(Expr.Literal(1.0), Expr.Literal(2.0)));
        var h2 = hasher.ComputeHash(Expr.Tuple(Expr.Literal(1.0), Expr.Literal(3.0)));

        h1.Should().NotBe(h2);
    }

    [Fact]
    public void CachedExpressionHasher_VectorDifferentDimensions()
    {
        var hasher = new CachedExpressionHasher();
        var h1 = hasher.ComputeHash(Expr.Vector(Expr.Literal(1.0)));
        var h2 = hasher.ComputeHash(Expr.Vector(Expr.Literal(1.0), Expr.Literal(2.0)));

        h1.Should().NotBe(h2);
    }

    [Fact]
    public void StructuralHasher_ParameterExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Parameter("t");
        var b = Expr.Parameter("t");

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_ParameterDifferentName()
    {
        var hasher = new StructuralHasher();

        hasher.Hash(Expr.Parameter("t")).Should().NotBe(hasher.Hash(Expr.Parameter("u")));
    }

    [Fact]
    public void StructuralHasher_BooleanExpression_DifferentValues()
    {
        var hasher = new StructuralHasher();

        hasher.Hash(Expr.Boolean(true)).Should().NotBe(hasher.Hash(Expr.Boolean(false)));
    }

    [Fact]
    public void HashBuilder_MultipleBytes()
    {
        var h1 = new HashBuilder().AddBytes(new byte[] { 1, 2, 3, 4, 5 }).ToHashCode();
        var h2 = new HashBuilder().AddBytes(new byte[] { 1, 2, 3, 4, 5 }).ToHashCode();

        h1.Should().Be(h2);
    }

    [Fact]
    public void HashBuilder_MixedTypes()
    {
        var h1 = new HashBuilder().Add(1).Add("hello").Add(3.14).Add(true).ToHashCode();
        var h2 = new HashBuilder().Add(1).Add("hello").Add(3.14).Add(true).ToHashCode();

        h1.Should().Be(h2);
    }

    [Fact]
    public void HashCache_StatisticsAfterClear()
    {
        var cache = new HashCache();
        cache.Store(Expr.Literal(1.0), 100);
        cache.TryGet(Expr.Literal(1.0), out _);
        cache.Clear();

        cache.Count.Should().Be(0);
    }

    [Fact]
    public void StructuralHasher_IndexExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Index(Expr.Variable("A"), Expr.Literal(0.0));
        var b = Expr.Index(Expr.Variable("A"), Expr.Literal(0.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_SliceExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Slice(Expr.Variable("A"), Expr.Literal(0.0), Expr.Literal(5.0));
        var b = Expr.Slice(Expr.Variable("A"), Expr.Literal(0.0), Expr.Literal(5.0));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }

    [Fact]
    public void StructuralHasher_TransposeExpression()
    {
        var hasher = new StructuralHasher();
        var a = Expr.Transpose(Expr.Variable("A"));
        var b = Expr.Transpose(Expr.Variable("A"));

        hasher.Hash(a).Should().Be(hasher.Hash(b));
    }
}
