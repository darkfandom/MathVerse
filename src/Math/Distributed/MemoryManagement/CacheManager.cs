namespace MathVerse.Math.Distributed.MemoryManagement
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// In-memory cache with LRU eviction using ConcurrentDictionary and LinkedList.
    /// </summary>
    public sealed class CacheManager
    {
        private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
        private readonly LinkedList<string> _accessOrder = new();
        private readonly object _evictionLock = new();
        private readonly int _maxSize;

        /// <summary>
        /// Initializes a new instance of CacheManager.
        /// </summary>
        /// <param name="maxSize">Maximum number of items in cache.</param>
        public CacheManager(int maxSize = 10000)
        {
            _maxSize = maxSize > 0 ? maxSize : 10000;
        }

        /// <summary>
        /// Gets a cached value by key.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <returns>The cached value or default if not found.</returns>
        public T? Get<T>(string key)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.Expiry.HasValue && DateTime.UtcNow > entry.Expiry.Value)
                {
                    Remove(key);
                    return default;
                }
                UpdateAccessOrder(key);
                return (T)entry.Value!;
            }
            return default;
        }

        /// <summary>
        /// Sets a value in the cache.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <param name="value">Value to cache.</param>
        /// <param name="expiry">Optional expiration timespan.</param>
        public void Set<T>(string key, T value, TimeSpan? expiry = null)
        {
            DateTime? expiryTime = expiry.HasValue ? DateTime.UtcNow + expiry.Value : null;
            var entry = new CacheEntry(value!, expiryTime);
            _cache[key] = entry;
            UpdateAccessOrder(key);
            EvictIfNeeded();
        }

        /// <summary>
        /// Removes a cached value.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <returns>True if removed, false if not found.</returns>
        public bool Remove(string key)
        {
            bool removed = _cache.TryRemove(key, out _);
            lock (_evictionLock)
            {
                _accessOrder.Remove(key);
            }
            return removed;
        }

        /// <summary>
        /// Clears all cached items.
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
            lock (_evictionLock)
            {
                _accessOrder.Clear();
            }
        }

        /// <summary>
        /// Gets the current number of items in cache.
        /// </summary>
        public int Count => _cache.Count;

        private void UpdateAccessOrder(string key)
        {
            lock (_evictionLock)
            {
                _accessOrder.Remove(key);
                _accessOrder.AddLast(key);
            }
        }

        private void EvictIfNeeded()
        {
            while (_cache.Count > _maxSize)
            {
                string? oldest = null;
                lock (_evictionLock)
                {
                    if (_accessOrder.Count > 0)
                    {
                        oldest = _accessOrder.First!.Value;
                        _accessOrder.RemoveFirst();
                    }
                }
                if (oldest != null)
                    _cache.TryRemove(oldest, out _);
                else
                    break;
            }
        }

        private sealed class CacheEntry
        {
            public object Value { get; }
            public DateTime? Expiry { get; }

            public CacheEntry(object value, DateTime? expiry)
            {
                Value = value;
                Expiry = expiry;
            }
        }
    }
}
