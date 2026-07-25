namespace MathVerse.Performance.Tests;

public sealed class PoolTests
{
    [Fact]
    public void ObjectPool_Rent_EmptyPool_CreatesNew()
    {
        var pool = new ObjectPool<List<int>>();
        var list = pool.Rent(() => new List<int>());

        list.Should().NotBeNull();
        list.Should().BeEmpty();
    }

    [Fact]
    public void ObjectPool_RentAndReturn_ReusesObject()
    {
        var pool = new ObjectPool<List<int>>();
        var list = pool.Rent(() => new List<int>());
        list.Add(42);

        pool.Return(list);
        var reused = pool.Rent(() => new List<int>());

        reused.Should().BeSameAs(list);
    }

    [Fact]
    public void ObjectPool_Count_TracksReturnedItems()
    {
        var pool = new ObjectPool<List<int>>();
        pool.Count.Should().Be(0);

        pool.Return(new List<int>());
        pool.Count.Should().Be(1);

        pool.Return(new List<int>());
        pool.Count.Should().Be(2);
    }

    [Fact]
    public void ObjectPool_Count_DecreasesOnRent()
    {
        var pool = new ObjectPool<List<int>>();
        pool.Return(new List<int>());
        pool.Return(new List<int>());
        pool.Count.Should().Be(2);

        pool.Rent(() => new List<int>());
        pool.Count.Should().Be(1);
    }

    [Fact]
    public void ObjectPool_Clear_EmptiesPool()
    {
        var pool = new ObjectPool<List<int>>();
        pool.Return(new List<int>());
        pool.Return(new List<int>());

        pool.Clear();

        pool.Count.Should().Be(0);
    }

    [Fact]
    public void ObjectPool_NullFactory_Throws()
    {
        var pool = new ObjectPool<List<int>>();
        Func<List<int>>? factory = null;

        Action act = () => pool.Rent(factory!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ObjectPool_NullReturn_Throws()
    {
        var pool = new ObjectPool<List<int>>();
        Action act = () => pool.Return(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ObjectPool_MultipleItems()
    {
        var pool = new ObjectPool<List<int>>();
        var items = Enumerable.Range(0, 10)
            .Select(_ => pool.Rent(() => new List<int>()))
            .ToList();

        foreach (var item in items)
            pool.Return(item);

        pool.Count.Should().Be(10);
    }

    [Fact]
    public async Task ObjectPool_ThreadSafety()
    {
        var pool = new ObjectPool<List<int>>();
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => Task.Run(() =>
            {
                var list = pool.Rent(() => new List<int>());
                list.Add(1);
                pool.Return(list);
            }))
            .ToArray();

        await Task.WhenAll(tasks);
        pool.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void PooledList_AddAndCount()
    {
        var pool = new ObjectPool<List<int>>();
        using var pooled = new PooledList<int>(pool);

        pooled.Add(1);
        pooled.Add(2);
        pooled.Add(3);

        pooled.Count.Should().Be(3);
    }

    [Fact]
    public void PooledList_Dispose_ReturnsToListPool()
    {
        var pool = new ObjectPool<List<int>>();
        var list = new List<int>();

        pool.Return(list);
        pool.Count.Should().Be(1);

        using (var pooled = new PooledList<int>(pool))
        {
            pooled.Add(42);
            pooled.Count.Should().Be(1);
        }

        pool.Count.Should().Be(1);
    }

    [Fact]
    public void PooledList_Dispose_ClearsList()
    {
        var pool = new ObjectPool<List<int>>();
        List<int> capturedList;

        using (var pooled = new PooledList<int>(pool))
        {
            pooled.Add(1);
            pooled.Add(2);
            capturedList = pooled.List;
        }

        capturedList.Should().BeEmpty();
    }

    [Fact]
    public void PooledList_UnderlyingListAccessible()
    {
        var pool = new ObjectPool<List<int>>();
        using var pooled = new PooledList<int>(pool);

        pooled.List.Should().NotBeNull();
        pooled.List.Should().BeSameAs(pooled.List);
    }

    [Fact]
    public void PooledList_NullPool_Throws()
    {
        Action act = () => new PooledList<int>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PooledList_DoubleDispose()
    {
        var pool = new ObjectPool<List<int>>();
        var pooled = new PooledList<int>(pool);
        pooled.Add(1);

        pooled.Dispose();
        Action act = () => pooled.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public void PooledList_AddAfterDispose_Throws()
    {
        var pool = new ObjectPool<List<int>>();
        var pooled = new PooledList<int>(pool);
        pooled.Dispose();

        Action act = () => pooled.Add(1);

        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void PooledList_ReusesAcrossInstances()
    {
        var pool = new ObjectPool<List<int>>();
        List<int> captured;

        using (var first = new PooledList<int>(pool))
        {
            first.Add(42);
            captured = first.List;
        }

        using (var second = new PooledList<int>(pool))
        {
            second.List.Should().BeSameAs(captured);
            second.Count.Should().Be(0);
        }
    }

    [Fact]
    public void PooledDictionary_AddAndCount()
    {
        var pool = new ObjectPool<Dictionary<string, int>>();
        using var pooled = new PooledDictionary<string, int>(pool);

        pooled.Add("one", 1);
        pooled.Add("two", 2);

        pooled.Count.Should().Be(2);
    }

    [Fact]
    public void PooledDictionary_Dispose_ClearsAndReturns()
    {
        var pool = new ObjectPool<Dictionary<string, int>>();

        using (var pooled = new PooledDictionary<string, int>(pool))
        {
            pooled.Add("key", 42);
        }

        pool.Count.Should().Be(1);
    }

    [Fact]
    public void PooledDictionary_UnderlyingDictionaryAccessible()
    {
        var pool = new ObjectPool<Dictionary<string, int>>();
        using var pooled = new PooledDictionary<string, int>(pool);

        pooled.Dictionary.Should().NotBeNull();
    }

    [Fact]
    public void PooledDictionary_NullPool_Throws()
    {
        Action act = () => new PooledDictionary<string, int>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PooledDictionary_UpdateExisting()
    {
        var pool = new ObjectPool<Dictionary<string, int>>();
        using var pooled = new PooledDictionary<string, int>(pool);

        pooled.Add("key", 1);
        pooled.Add("key", 2);

        pooled.Count.Should().Be(1);
        pooled.Dictionary["key"].Should().Be(2);
    }

    [Fact]
    public void PooledDictionary_AddAfterDispose_Throws()
    {
        var pool = new ObjectPool<Dictionary<string, int>>();
        var pooled = new PooledDictionary<string, int>(pool);
        pooled.Dispose();

        Action act = () => pooled.Add("key", 1);

        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void PooledDictionary_DoubleDispose()
    {
        var pool = new ObjectPool<Dictionary<string, int>>();
        var pooled = new PooledDictionary<string, int>(pool);

        pooled.Dispose();
        Action act = () => pooled.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public void PooledDictionary_ReusesAcrossInstances()
    {
        var pool = new ObjectPool<Dictionary<string, int>>();
        Dictionary<string, int> captured;

        using (var first = new PooledDictionary<string, int>(pool))
        {
            first.Add("key", 1);
            captured = first.Dictionary;
        }

        using (var second = new PooledDictionary<string, int>(pool))
        {
            second.Dictionary.Should().BeSameAs(captured);
            second.Count.Should().Be(0);
        }
    }

    [Fact]
    public void ArrayPoolAdapter_Rent_ReturnsSizedArray()
    {
        var adapter = new ArrayPoolAdapter();
        var array = adapter.Rent(128);

        array.Should().NotBeNull();
        array.Length.Should().BeGreaterThanOrEqualTo(128);

        adapter.Return(array);
    }

    [Fact]
    public void ArrayPoolAdapter_Return_ClearsArray()
    {
        var adapter = new ArrayPoolAdapter();
        var array = adapter.Rent(10);
        array[0] = 42;

        adapter.Return(array);

        array[0].Should().Be(0);
    }

    [Fact]
    public void ArrayPoolAdapter_ZeroLength_Throws()
    {
        var adapter = new ArrayPoolAdapter();
        Action act = () => adapter.Rent(0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ArrayPoolAdapter_NegativeLength_Throws()
    {
        var adapter = new ArrayPoolAdapter();
        Action act = () => adapter.Rent(-1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ArrayPoolAdapter_ReturnNull_Throws()
    {
        var adapter = new ArrayPoolAdapter();
        Action act = () => adapter.Return(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ArrayPoolAdapter_NullPool_Throws()
    {
        Action act = () => new ArrayPoolAdapter(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ArrayPoolAdapter_RentMultiple()
    {
        var adapter = new ArrayPoolAdapter();
        var a = adapter.Rent(64);
        var b = adapter.Rent(64);

        a.Should().NotBeSameAs(b);

        adapter.Return(a);
        adapter.Return(b);
    }

    [Fact]
    public void ArrayPoolAdapter_RentAfterReturn_Reuses()
    {
        var adapter = new ArrayPoolAdapter();
        var a = adapter.Rent(64);
        adapter.Return(a);
        var b = adapter.Rent(64);

        b.Should().BeSameAs(a);
    }

    [Fact]
    public async Task ArrayPoolAdapter_ThreadSafety()
    {
        var adapter = new ArrayPoolAdapter();
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => Task.Run(() =>
            {
                var array = adapter.Rent(64);
                array[0] = 1;
                adapter.Return(array);
            }))
            .ToArray();

        await Task.WhenAll(tasks);
    }

    [Fact]
    public void ExpressionPool_Current_GetSet()
    {
        var pool = new ExpressionPool();
        var expr = Expr.Literal(42.0);

        pool.Current = expr;
        pool.Current.Should().BeSameAs(expr);
    }

    [Fact]
    public void ExpressionPool_Reset_ClearsCurrent()
    {
        var pool = new ExpressionPool();
        pool.Current = Expr.Literal(42.0);

        pool.Reset();

        pool.Current.Should().BeNull();
    }

    [Fact]
    public void ExpressionPool_InitiallyNull()
    {
        var pool = new ExpressionPool();
        pool.Current.Should().BeNull();
    }

    [Fact]
    public void ExpressionPool_MultipleResets()
    {
        var pool = new ExpressionPool();
        pool.Current = Expr.Literal(1.0);
        pool.Reset();
        pool.Current = Expr.Literal(2.0);
        pool.Reset();

        pool.Current.Should().BeNull();
    }

    [Fact]
    public void ExpressionPool_WithObjectPool()
    {
        var objPool = new ObjectPool<ExpressionPool>();

        var entry = objPool.Rent(() => new ExpressionPool());
        entry.Current = Expr.Literal(42.0);

        objPool.Return(entry);
        var reused = objPool.Rent(() => new ExpressionPool());

        reused.Should().BeSameAs(entry);
        reused.Current.Should().NotBeNull();
    }

    [Fact]
    public void BuilderPool_Rent_CreatesNew()
    {
        var pool = new BuilderPool<List<int>>();
        var builder = pool.Rent();

        builder.Should().NotBeNull();
        builder.Should().BeEmpty();
    }

    [Fact]
    public void BuilderPool_RentAndReturn_Reuses()
    {
        var pool = new BuilderPool<List<int>>();
        var first = pool.Rent();
        first.Add(42);
        pool.Return(first);

        var second = pool.Rent();
        second.Should().BeSameAs(first);
    }

    [Fact]
    public void BuilderPool_NullReturn_Throws()
    {
        var pool = new BuilderPool<List<int>>();
        Action act = () => pool.Return(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void BuilderPool_MultipleRents()
    {
        var pool = new BuilderPool<List<int>>();
        var items = Enumerable.Range(0, 5)
            .Select(_ => pool.Rent())
            .ToList();

        foreach (var item in items)
            pool.Return(item);

        var reused = pool.Rent();
        reused.Should().NotBeNull();
    }

    [Fact]
    public async Task BuilderPool_ThreadSafety()
    {
        var pool = new BuilderPool<List<int>>();
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => Task.Run(() =>
            {
                var b = pool.Rent();
                b.Add(1);
                pool.Return(b);
            }))
            .ToArray();

        await Task.WhenAll(tasks);
    }

    [Fact]
    public void PoolPolicy_DefaultFactory_Throws()
    {
        var policy = new PoolPolicy<List<int>>();
        Action act = () => policy.Factory();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PoolPolicy_CustomFactory()
    {
        var policy = new PoolPolicy<List<int>>
        {
            Factory = () => new List<int> { 1, 2, 3 }
        };

        var list = policy.Factory();
        list.Should().HaveCount(3);
    }

    [Fact]
    public void PoolPolicy_ResetAction()
    {
        var policy = new PoolPolicy<List<int>>
        {
            Factory = () => new List<int>(),
            ResetAction = list => list.Clear()
        };

        var list = policy.Factory();
        list.Add(42);
        policy.ResetAction(list);

        list.Should().BeEmpty();
    }

    [Fact]
    public void PoolPolicy_DefaultResetAction_DoesNothing()
    {
        var policy = new PoolPolicy<List<int>>
        {
            Factory = () => new List<int>()
        };

        var list = policy.Factory();
        list.Add(42);
        policy.ResetAction(list);

        list.Should().HaveCount(1);
    }

    [Fact]
    public void ObjectPool_RentAlwaysCreatesWhenEmpty()
    {
        var pool = new ObjectPool<List<int>>();
        var a = pool.Rent(() => new List<int>());
        pool.Return(a);
        var b = pool.Rent(() => new List<int>());

        b.Should().BeSameAs(a);
    }

    [Fact]
    public async Task ObjectPool_ConcurrentRentAndReturn()
    {
        var pool = new ObjectPool<List<int>>();
        for (int i = 0; i < 50; i++)
            pool.Return(new List<int>());

        var tasks = Enumerable.Range(0, 50)
            .Select(_ => Task.Run(() =>
            {
                var item = pool.Rent(() => new List<int>());
                Thread.SpinWait(10);
                pool.Return(item);
            }))
            .ToArray();

        await Task.WhenAll(tasks);
        pool.Count.Should().Be(50);
    }

    [Fact]
    public async Task PooledList_ConcurrentAdd()
    {
        var pool = new ObjectPool<List<int>>();
        var pooled = new PooledList<int>(pool);
        var lockObj = new object();

        var tasks = Enumerable.Range(0, 100)
            .Select(i => Task.Run(() => { lock (lockObj) { pooled.Add(i); } }))
            .ToArray();

        await Task.WhenAll(tasks);
        pooled.Count.Should().Be(100);
        pooled.Dispose();
    }

    [Fact]
    public async Task PooledDictionary_ConcurrentAdd()
    {
        var pool = new ObjectPool<Dictionary<int, int>>();
        var pooled = new PooledDictionary<int, int>(pool);
        var lockObj = new object();

        var tasks = Enumerable.Range(0, 100)
            .Select(i => Task.Run(() => { lock (lockObj) { pooled.Add(i, i * 10); } }))
            .ToArray();

        await Task.WhenAll(tasks);
        pooled.Count.Should().Be(100);
        pooled.Dispose();
    }

    [Fact]
    public void ArrayPoolAdapter_LargeRent()
    {
        var adapter = new ArrayPoolAdapter();
        var array = adapter.Rent(1024 * 1024);

        array.Length.Should().BeGreaterThanOrEqualTo(1024 * 1024);
        adapter.Return(array);
    }
}
