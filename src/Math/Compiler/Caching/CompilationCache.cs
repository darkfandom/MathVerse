namespace MathVerse.Math.Compiler.Caching;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public sealed class CompilationCache
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly int _maxSize;

    public CompilationCache(int maxSize = 1024)
    {
        _maxSize = maxSize;
    }

    public int Count => _cache.Count;

    public CacheEntry GetOrAdd(string key, Func<CacheEntry> factory)
    {
        return _cache.GetOrAdd(key, _ => factory());
    }

    public bool TryGet(string key, out CacheEntry? entry)
    {
        if (_cache.TryGetValue(key, out var e))
        {
            e.LastAccess = DateTime.UtcNow;
            e.AccessCount++;
            entry = e;
            return true;
        }
        entry = null;
        return false;
    }

    public void Store(string key, CacheEntry entry)
    {
        if (_cache.Count >= _maxSize)
            EvictOldest();
        _cache[key] = entry;
    }

    public bool Invalidate(string key) => _cache.TryRemove(key, out _);

    public void Clear() => _cache.Clear();

    public IReadOnlyDictionary<string, CacheEntry> GetAll()
        => new Dictionary<string, CacheEntry>(_cache);

    private void EvictOldest()
    {
        string? oldestKey = null;
        DateTime oldestTime = DateTime.MaxValue;
        foreach (var kvp in _cache)
        {
            if (kvp.Value.LastAccess < oldestTime)
            {
                oldestTime = kvp.Value.LastAccess;
                oldestKey = kvp.Key;
            }
        }
        if (oldestKey != null)
            _cache.TryRemove(oldestKey, out _);
    }
}

public sealed class CacheEntry
{
    public object? Value { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime LastAccess { get; set; } = DateTime.UtcNow;
    public long AccessCount { get; set; }
    public TimeSpan? TimeToLive { get; init; }
    public bool IsExpired => TimeToLive.HasValue && (DateTime.UtcNow - CreatedAt) > TimeToLive.Value;
}
