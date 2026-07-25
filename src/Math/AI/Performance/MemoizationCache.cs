namespace MathVerse.Math.AI.Performance;

using System.Collections.Concurrent;

/// <summary>Thread-safe memoization cache for pure functions with LRU eviction based on access order.</summary>
public sealed class MemoizationCache
{
    private readonly ConcurrentDictionary<CacheKey, CacheEntry> _cache = new();
    private readonly int _maxSize;
    private int _hitCount;
    private int _missCount;
    private int _evictionCount;

    /// <summary>Initializes a new instance of the <see cref="MemoizationCache"/> class.</summary>
    /// <param name="maxSize">The maximum number of entries to retain.</param>
    public MemoizationCache(int maxSize = 256)
    {
        _maxSize = maxSize > 0 ? maxSize : 256;
    }

    /// <summary>Gets the number of cache hits since creation or last reset.</summary>
    public int HitCount => Volatile.Read(ref _hitCount);

    /// <summary>Gets the number of cache misses since creation or last reset.</summary>
    public int MissCount => Volatile.Read(ref _missCount);

    /// <summary>Gets the number of entries evicted since creation or last reset.</summary>
    public int EvictionCount => Volatile.Read(ref _evictionCount);

    /// <summary>Gets the current number of entries in the cache.</summary>
    public int Count => _cache.Count;

    /// <summary>Gets the cache hit ratio as a value between 0 and 1.</summary>
    public double HitRatio
    {
        get
        {
            int hits = Volatile.Read(ref _hitCount);
            int misses = Volatile.Read(ref _missCount);
            int total = hits + misses;
            return total > 0 ? (double)hits / total : 0.0;
        }
    }

    /// <summary>Creates a memoized wrapper around a single-argument pure function.</summary>
    /// <typeparam name="T">The input type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="func">The pure function to memoize.</param>
    /// <returns>A memoized function with the same signature.</returns>
    public Func<T, TResult> Memoize<T, TResult>(Func<T, TResult> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        return (T input) =>
        {
            var key = new CacheKey(typeof(T), input!);

            if (_cache.TryGetValue(key, out var entry) && entry.IsValueAlive)
            {
                entry.LastAccess = DateTime.UtcNow;
                Interlocked.Increment(ref _hitCount);
                return (TResult)entry.Value!;
            }

            Interlocked.Increment(ref _missCount);
            TResult result = func(input);
            StoreKeyed(key, result);
            return result;
        };
    }

    /// <summary>Creates a memoized wrapper around a two-argument pure function.</summary>
    /// <typeparam name="T1">The first input type.</typeparam>
    /// <typeparam name="T2">The second input type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="func">The pure function to memoize.</param>
    /// <returns>A memoized function with the same signature.</returns>
    public Func<T1, T2, TResult> Memoize<T1, T2, TResult>(Func<T1, T2, TResult> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        return (T1 input1, T2 input2) =>
        {
            var key = new CacheKey(typeof(T1), input1!, typeof(T2), input2!);

            if (_cache.TryGetValue(key, out var entry) && entry.IsValueAlive)
            {
                entry.LastAccess = DateTime.UtcNow;
                Interlocked.Increment(ref _hitCount);
                return (TResult)entry.Value!;
            }

            Interlocked.Increment(ref _missCount);
            TResult result = func(input1, input2);
            StoreKeyed(key, result);
            return result;
        };
    }

    /// <summary>Looks up a cached value by key, returning it if found.</summary>
    /// <typeparam name="TResult">The expected result type.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The cached value if found.</param>
    /// <returns>true if the value was found; otherwise, false.</returns>
    public bool TryGet<TResult>(object key, out TResult? value)
    {
        var cacheKey = new CacheKey(key.GetType()!, key);

        if (_cache.TryGetValue(cacheKey, out var entry) && entry.IsValueAlive)
        {
            entry.LastAccess = DateTime.UtcNow;
            Interlocked.Increment(ref _hitCount);
            value = (TResult)entry.Value!;
            return true;
        }

        Interlocked.Increment(ref _missCount);
        value = default;
        return false;
    }

    /// <summary>Stores a value in the cache under the given key.</summary>
    /// <typeparam name="TResult">The value type.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    public void Store<TResult>(object key, TResult value)
    {
        var cacheKey = new CacheKey(key.GetType()!, key);
        StoreKeyed(cacheKey, value);
    }

    /// <summary>Evicts the least recently accessed entry from the cache.</summary>
    /// <returns>true if an entry was evicted; false if the cache was empty.</returns>
    public bool EvictOldest()
    {
        CacheKey oldestKey = default;
        DateTime oldestTime = DateTime.MaxValue;
        bool found = false;

        foreach (var kvp in _cache)
        {
            if (kvp.Value.IsValueAlive && kvp.Value.LastAccess < oldestTime)
            {
                oldestTime = kvp.Value.LastAccess;
                oldestKey = kvp.Key;
                found = true;
            }
        }

        if (found && _cache.TryRemove(oldestKey, out _))
        {
            Interlocked.Increment(ref _evictionCount);
            return true;
        }

        return false;
    }

    /// <summary>Removes all entries from the cache and resets statistics.</summary>
    public void Clear()
    {
        _cache.Clear();
        Interlocked.Exchange(ref _hitCount, 0);
        Interlocked.Exchange(ref _missCount, 0);
        Interlocked.Exchange(ref _evictionCount, 0);
    }

    /// <summary>Stores a value using a pre-built cache key.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to store.</param>
    private void StoreKeyed(CacheKey key, object? value)
    {
        if (_cache.Count >= _maxSize)
        {
            EvictOldest();
        }

        _cache[key] = new CacheEntry
        {
            Value = value,
            LastAccess = DateTime.UtcNow,
            IsValueAlive = true
        };
    }

    /// <summary>Represents a composite cache key with a type discriminator and value(s).</summary>
    private readonly struct CacheKey : IEquatable<CacheKey>
    {
        private readonly Type _type1;
        private readonly object? _value1;
        private readonly Type? _type2;
        private readonly object? _value2;
        private readonly int _hashCode;

        /// <summary>Initializes a single-argument cache key.</summary>
        public CacheKey(Type type, object? value)
        {
            _type1 = type;
            _value1 = value;
            _type2 = null;
            _value2 = null;
            _hashCode = HashCode.Combine(type, value);
        }

        /// <summary>Initializes a two-argument cache key.</summary>
        public CacheKey(Type type1, object? value1, Type type2, object? value2)
        {
            _type1 = type1;
            _value1 = value1;
            _type2 = type2;
            _value2 = value2;
            _hashCode = HashCode.Combine(type1, value1, type2, value2);
        }

        /// <inheritdoc/>
        public bool Equals(CacheKey other)
        {
            return _type1 == other._type1
                && Equals(_value1, other._value1)
                && _type2 == other._type2
                && Equals(_value2, other._value2);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is CacheKey other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => _hashCode;
    }

    /// <summary>Represents a single cache entry with its value and access metadata.</summary>
    private sealed class CacheEntry
    {
        /// <summary>Gets or sets the cached value.</summary>
        public object? Value { get; set; }

        /// <summary>Gets or sets the last access timestamp for LRU tracking.</summary>
        public DateTime LastAccess { get; set; }

        /// <summary>Gets or sets whether the entry is still valid.</summary>
        public bool IsValueAlive { get; set; }
    }
}
