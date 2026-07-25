namespace MathVerse.Math.Compiler.Performance;

using System;
using System.Collections.Concurrent;

/// <summary>Caches SIMD-vectorized operation plans for reuse.</summary>
public sealed class SIMDExecutionCache
{
    private readonly ConcurrentDictionary<string, VectorPlan> _plans = new();

    /// <summary>Gets an existing <see cref="VectorPlan"/> or creates and caches a new one using the planner.</summary>
    /// <param name="operationKey">The unique key identifying the operation.</param>
    /// <param name="planner">Factory to produce a new plan if not cached.</param>
    /// <returns>The cached or newly created vector plan.</returns>
    public VectorPlan GetOrPlan(string operationKey, Func<VectorPlan> planner)
    {
        if (operationKey is null) throw new ArgumentNullException(nameof(operationKey));
        if (planner is null) throw new ArgumentNullException(nameof(planner));

        return _plans.GetOrAdd(operationKey, _ => planner());
    }

    /// <summary>Attempts to retrieve a cached plan for the given key.</summary>
    public bool TryGetPlan(string operationKey, out VectorPlan? plan)
    {
        if (operationKey is null) throw new ArgumentNullException(nameof(operationKey));
        return _plans.TryGetValue(operationKey, out plan);
    }

    /// <summary>Removes a specific plan from the cache.</summary>
    public void Remove(string operationKey)
    {
        if (operationKey is null) throw new ArgumentNullException(nameof(operationKey));
        _plans.TryRemove(operationKey, out _);
    }

    /// <summary>Clears all cached plans.</summary>
    public void Clear()
    {
        _plans.Clear();
    }

    /// <summary>Gets the number of cached plans.</summary>
    public int Count => _plans.Count;
}
