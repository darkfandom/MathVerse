namespace MathVerse.Math.Distributed.Caching;

using System.Collections.Concurrent;
using System.Security.Cryptography;

/// <summary>Caches computation results using hash-based keys derived from operation names and input data.</summary>
public sealed class ResultCache
{
    private readonly ConcurrentDictionary<string, double[]> _cache = new();
    private long _hits;
    private long _misses;
    private long _computations;

    /// <summary>Gets the number of entries in the cache.</summary>
    public int Count => _cache.Count;

    /// <summary>Gets the total number of cache hits.</summary>
    public long Hits => Interlocked.Read(ref _hits);

    /// <summary>Gets the total number of cache misses.</summary>
    public long Misses => Interlocked.Read(ref _misses);

    /// <summary>Gets the total number of computations performed.</summary>
    public long Computations => Interlocked.Read(ref _computations);

    /// <summary>Retrieves a cached result for the given operation and inputs, or computes and caches a new result.</summary>
    /// <param name="operation">The operation name.</param>
    /// <param name="inputs">The input data.</param>
    /// <param name="compute">The compute function invoked on cache miss.</param>
    /// <returns>The cached or freshly computed result.</returns>
    public double[] GetOrCompute(string operation, double[] inputs, Func<double[], double[]> compute)
    {
        string key = ComputeKey(operation, inputs);

        if (_cache.TryGetValue(key, out var cached))
        {
            Interlocked.Increment(ref _hits);
            return cached;
        }

        Interlocked.Increment(ref _misses);
        Interlocked.Increment(ref _computations);

        double[] result = compute(inputs);
        _cache[key] = result;
        return result;
    }

    /// <summary>Retrieves a cached result by key without computing.</summary>
    /// <param name="key">The precomputed cache key.</param>
    /// <param name="result">The cached result, if found.</param>
    /// <returns>True if the result was found in the cache.</returns>
    public bool TryGet(string key, out double[]? result)
    {
        if (_cache.TryGetValue(key, out var cached))
        {
            Interlocked.Increment(ref _hits);
            result = cached;
            return true;
        }

        Interlocked.Increment(ref _misses);
        result = null;
        return false;
    }

    /// <summary>Stores a result with a precomputed key.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="result">The result to cache.</param>
    public void Set(string key, double[] result)
    {
        _cache[key] = result;
    }

    /// <summary>Generates a cache key from an operation name and input data.</summary>
    /// <param name="operation">The operation name.</param>
    /// <param name="inputs">The input data.</param>
    /// <returns>A hex string cache key.</returns>
    public static string ComputeKey(string operation, double[] inputs)
    {
        int byteLength = operation.Length * 2 + inputs.Length * 8;
        byte[] buffer = new byte[byteLength];

        int offset = 0;
        for (int i = 0; i < operation.Length; i++)
        {
            buffer[offset++] = (byte)(operation[i] >> 8);
            buffer[offset++] = (byte)(operation[i] & 0xFF);
        }

        foreach (double value in inputs)
        {
            long bits = BitConverter.DoubleToInt64Bits(value);
            buffer[offset++] = (byte)(bits >> 56);
            buffer[offset++] = (byte)(bits >> 48);
            buffer[offset++] = (byte)(bits >> 40);
            buffer[offset++] = (byte)(bits >> 32);
            buffer[offset++] = (byte)(bits >> 24);
            buffer[offset++] = (byte)(bits >> 16);
            buffer[offset++] = (byte)(bits >> 8);
            buffer[offset++] = (byte)(bits);
        }

        byte[] hash = SHA256.HashData(buffer);
        return Convert.ToHexString(hash);
    }

    /// <summary>Invalidates the cache entry for the given operation and inputs.</summary>
    /// <param name="operation">The operation name.</param>
    /// <param name="inputs">The input data.</param>
    /// <returns>True if the entry existed and was removed.</returns>
    public bool Invalidate(string operation, double[] inputs)
    {
        string key = ComputeKey(operation, inputs);
        return _cache.TryRemove(key, out _);
    }

    /// <summary>Clears all cached results.</summary>
    public void Clear()
    {
        _cache.Clear();
        Interlocked.Exchange(ref _hits, 0);
        Interlocked.Exchange(ref _misses, 0);
    }

    /// <summary>Returns the cache hit rate as a value between 0 and 1.</summary>
    /// <returns>The hit rate, or 0 if no requests have been made.</returns>
    public double GetHitRate()
    {
        long total = Interlocked.Read(ref _hits) + Interlocked.Read(ref _misses);
        if (total == 0)
        {
            return 0.0;
        }
        return (double)Interlocked.Read(ref _hits) / (double)total;
    }
}
