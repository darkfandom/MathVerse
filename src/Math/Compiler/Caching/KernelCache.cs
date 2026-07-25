namespace MathVerse.Math.Compiler.Caching;

using System;
using System.Collections.Concurrent;

/// <summary>Thread-safe cache for generated kernel code strings.</summary>
public sealed class KernelCache
{
    private readonly ConcurrentDictionary<string, string> _cache = new();

    /// <summary>Gets an existing kernel code string or generates and caches a new one.</summary>
    /// <param name="kernelSignature">The unique signature of the kernel.</param>
    /// <param name="generator">Factory to generate the code string if not cached.</param>
    /// <returns>The kernel code string.</returns>
    public string GetOrGenerate(string kernelSignature, Func<string> generator)
    {
        if (kernelSignature is null) throw new ArgumentNullException(nameof(kernelSignature));
        if (generator is null) throw new ArgumentNullException(nameof(generator));

        return _cache.GetOrAdd(kernelSignature, _ => generator());
    }

    /// <summary>Checks whether a given signature is already cached.</summary>
    /// <param name="signature">The kernel signature to check.</param>
    /// <returns>True if the signature exists in the cache.</returns>
    public bool Contains(string signature)
    {
        if (signature is null) throw new ArgumentNullException(nameof(signature));
        return _cache.ContainsKey(signature);
    }

    /// <summary>Removes a specific signature from the cache.</summary>
    public void Remove(string signature)
    {
        if (signature is null) throw new ArgumentNullException(nameof(signature));
        _cache.TryRemove(signature, out _);
    }

    /// <summary>Clears all cached kernel strings.</summary>
    public void Clear()
    {
        _cache.Clear();
    }
}
