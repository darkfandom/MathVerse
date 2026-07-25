namespace MathVerse.Math.Compiler.Performance;

using System;
using System.Collections.Concurrent;

/// <summary>Object pool for compiled kernel instances. Thread-safe.</summary>
/// <typeparam name="T">The type of compiled kernel.</typeparam>
public sealed class CompiledKernelPool<T> where T : class
{
    private readonly ConcurrentBag<T> _pool = new();
    private readonly Func<T> _factory;
    private int _count;

    /// <summary>Initializes a new instance of the <see cref="CompiledKernelPool{T}"/> class.</summary>
    /// <param name="factory">Factory to create new instances when the pool is empty.</param>
    public CompiledKernelPool(Func<T> factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>Retrieves a kernel instance from the pool or creates a new one.</summary>
    public T Get()
    {
        if (_pool.TryTake(out var instance))
        {
            System.Threading.Interlocked.Decrement(ref _count);
            return instance;
        }
        return _factory();
    }

    /// <summary>Returns a kernel instance to the pool for reuse.</summary>
    public void Return(T instance)
    {
        if (instance is null) throw new ArgumentNullException(nameof(instance));
        _pool.Add(instance);
        System.Threading.Interlocked.Increment(ref _count);
    }

    /// <summary>Gets the number of instances currently in the pool.</summary>
    public int Count => System.Threading.Interlocked.CompareExchange(ref _count, 0, 0);
}
