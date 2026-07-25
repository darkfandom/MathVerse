namespace MathVerse.Math.Performance.Caching;

/// <summary>
/// Generic thread-safe LRU cache for evaluation results with optional TTL expiration.
/// </summary>
/// <typeparam name="TResult">The type of cached result values.</typeparam>
public sealed class EvaluationCache<TResult>
{
    private readonly int _capacity;
    private readonly ConcurrentDictionary<string, CacheEntry> _entries;
    private readonly LinkedList<string> _accessOrder;
    private readonly Lock _lock = new();

    private int _hits;
    private int _misses;
    private int _evictions;

    /// <summary>Initializes a new evaluation cache with the specified capacity.</summary>
    /// <param name="capacity">The maximum number of entries. Defaults to 1024.</param>
    public EvaluationCache(int capacity = 1024)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");

        _capacity = capacity;
        _entries = new ConcurrentDictionary<string, CacheEntry>(StringComparer.Ordinal);
        _accessOrder = new LinkedList<string>();
    }

    /// <summary>Stores a result in the cache under the specified key.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="result">The result value to cache.</param>
    /// <param name="ttl">Optional time-to-live. If null, the entry never expires.</param>
    public void Store(string key, TResult result, TimeSpan? ttl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var entry = new CacheEntry(result, ttl.HasValue ? Stopwatch.GetTimestamp() + ttl.Value.Ticks : null, ttl);

        lock (_lock)
        {
            if (_entries.TryAdd(key, entry))
            {
                _accessOrder.AddLast(key);
            }
            else
            {
                _entries[key] = entry;
                TouchKeyLocked(key);
            }

            EvictOldestIfNeededLocked();
        }
    }

    /// <summary>Attempts to retrieve a cached result for the specified key.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="result">The cached result if found and not expired.</param>
    /// <returns>True if a valid cached result was found; otherwise, false.</returns>
    public bool TryGet(string key, out TResult? result)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        lock (_lock)
        {
            if (_entries.TryGetValue(key, out var entry))
            {
                if (entry.ExpirationTimestamp.HasValue &&
                    Stopwatch.GetTimestamp() > entry.ExpirationTimestamp.Value)
                {
                    RemoveEntryLocked(key);
                    Interlocked.Increment(ref _misses);
                    result = default;
                    return false;
                }

                TouchKeyLocked(key);
                Interlocked.Increment(ref _hits);
                result = entry.Value;
                return true;
            }
        }

        Interlocked.Increment(ref _misses);
        result = default;
        return false;
    }

    /// <summary>Removes a specific entry from the cache.</summary>
    /// <param name="key">The cache key to invalidate.</param>
    public void Invalidate(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        lock (_lock)
        {
            RemoveEntryLocked(key);
        }
    }

    /// <summary>Removes all entries from the cache and resets statistics.</summary>
    public void Clear()
    {
        lock (_lock)
        {
            _entries.Clear();
            _accessOrder.Clear();
        }

        Interlocked.Exchange(ref _hits, 0);
        Interlocked.Exchange(ref _misses, 0);
        Interlocked.Exchange(ref _evictions, 0);
    }

    /// <summary>Gets the current cache statistics.</summary>
    public CacheStatistics Statistics
    {
        get
        {
            lock (_lock)
            {
                return new CacheStatistics
                {
                    Hits = Volatile.Read(ref _hits),
                    Misses = Volatile.Read(ref _misses),
                    Evictions = Volatile.Read(ref _evictions),
                    Count = _entries.Count,
                    Capacity = _capacity
                };
            }
        }
    }

    private void TouchKeyLocked(string key)
    {
        var node = _accessOrder.Find(key);
        if (node is not null)
        {
            _accessOrder.Remove(node);
            _accessOrder.AddLast(node);
        }
    }

    private void RemoveEntryLocked(string key)
    {
        if (_entries.TryRemove(key, out _))
        {
            var node = _accessOrder.Find(key);
            if (node is not null)
                _accessOrder.Remove(node);
        }
    }

    private void EvictOldestIfNeededLocked()
    {
        while (_entries.Count > _capacity)
        {
            var oldest = _accessOrder.First;
            if (oldest is null)
                break;

            RemoveEntryLocked(oldest.Value);
            Interlocked.Increment(ref _evictions);
        }
    }

    private readonly record struct CacheEntry(TResult Value, long? ExpirationTimestamp, TimeSpan? TimeToLive);
}
