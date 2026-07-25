namespace MathVerse.Math.Compiler.Memory;

using System;
using System.Collections.Generic;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Represents a group of buffers that can share the same memory due to non-overlapping lifetimes.
/// </summary>
public sealed class ReuseGroup
{
    /// <summary>The buffers in this reuse group (they share the same memory).</summary>
    public IReadOnlyList<IRValue> Buffers { get; }

    /// <summary>The size needed for this group (max of all buffer sizes).</summary>
    public int RequiredSize { get; }

    /// <summary>The computed lifetime range covering all buffers in this group.</summary>
    public LifetimeRange GroupLifetime { get; }

    /// <summary>
    /// Initializes a new reuse group.
    /// </summary>
    public ReuseGroup(IReadOnlyList<IRValue> buffers, int requiredSize, LifetimeRange groupLifetime)
    {
        Buffers = buffers;
        RequiredSize = requiredSize;
        GroupLifetime = groupLifetime;
    }
}

/// <summary>
/// Identifies buffers with non-overlapping lifetimes that can share the same memory region.
/// Uses interval analysis on buffer lifetimes to find optimal reuse opportunities.
/// </summary>
public sealed class BufferReuse
{
    private readonly LifetimeAnalyzer _lifetimeAnalyzer = new();

    /// <summary>
    /// Analyzes a function and identifies groups of buffers that can share memory.
    /// </summary>
    /// <param name="function">The IR function to analyze.</param>
    /// <returns>A list of reuse groups, each representing buffers that share memory.</returns>
    public IReadOnlyList<ReuseGroup> FindReuseGroups(IRFunction function)
    {
        ArgumentNullException.ThrowIfNull(function);

        var lifetimes = _lifetimeAnalyzer.Analyze(function);
        return ComputeReuseGroups(lifetimes);
    }

    /// <summary>
    /// Analyzes a module and identifies buffer reuse opportunities across all functions.
    /// </summary>
    /// <param name="module">The IR module to analyze.</param>
    /// <returns>A dictionary mapping function names to their reuse groups.</returns>
    public IReadOnlyDictionary<string, IReadOnlyList<ReuseGroup>> FindReuseGroupsModule(IRModule module)
    {
        ArgumentNullException.ThrowIfNull(module);

        var results = new Dictionary<string, IReadOnlyList<ReuseGroup>>(module.Functions.Count);
        for (var i = 0; i < module.Functions.Count; i++)
        {
            var func = module.Functions[i];
            results[func.Name] = FindReuseGroups(func);
        }
        return results;
    }

    /// <summary>
    /// Estimates the total memory savings from applying buffer reuse to a function.
    /// </summary>
    /// <param name="function">The function to analyze.</param>
    /// <returns>The estimated number of bytes saved.</returns>
    public int EstimateSavings(IRFunction function)
    {
        ArgumentNullException.ThrowIfNull(function);

        var lifetimes = _lifetimeAnalyzer.Analyze(function);
        var groups = ComputeReuseGroups(lifetimes);

        var originalSize = 0;
        var reusedSize = 0;

        foreach (var kvp in lifetimes)
        {
            originalSize += IRTypeHelper.SizeInBytes(kvp.Key.Type);
        }

        for (var i = 0; i < groups.Count; i++)
        {
            reusedSize += groups[i].RequiredSize;
        }

        return Math.Max(0, originalSize - reusedSize);
    }

    private static List<ReuseGroup> ComputeReuseGroups(IReadOnlyDictionary<IRValue, LifetimeRange> lifetimes)
    {
        var entries = new List<(IRValue Value, LifetimeRange Range, int Size)>();
        foreach (var kvp in lifetimes)
        {
            if (kvp.Key.IsConstant) continue;
            var size = IRTypeHelper.SizeInBytes(kvp.Key.Type);
            if (size <= 0) continue;
            entries.Add((kvp.Key, kvp.Value, size));
        }

        // Sort by first use for greedy interval scheduling
        entries.Sort((a, b) => a.Range.FirstUse.CompareTo(b.Range.FirstUse));

        var reuseGroups = new List<ReuseGroup>();
        var activeGroups = new List<ReuseGroup>();

        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var placed = false;

            // Try to add to an existing group where the buffer fits
            for (var g = 0; g < activeGroups.Count; g++)
            {
                var group = activeGroups[g];
                if (!group.GroupLifetime.Overlaps(entry.Range))
                {
                    // Buffer's lifetime doesn't overlap with this group — can reuse
                    var newBuffers = new List<IRValue>(group.Buffers) { entry.Value };
                    var newSize = Math.Max(group.RequiredSize, entry.Size);
                    var newLifetime = group.GroupLifetime.Merge(entry.Range);
                    var newGroup = new ReuseGroup(newBuffers, newSize, newLifetime);
                    reuseGroups[g] = newGroup;
                    activeGroups[g] = newGroup;
                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                var newGroup = new ReuseGroup(
                    new[] { entry.Value },
                    entry.Size,
                    entry.Range);
                reuseGroups.Add(newGroup);
                activeGroups.Add(newGroup);
            }
        }

        // Filter out single-buffer groups (no reuse benefit)
        var result = new List<ReuseGroup>();
        for (var i = 0; i < reuseGroups.Count; i++)
        {
            if (reuseGroups[i].Buffers.Count > 1)
                result.Add(reuseGroups[i]);
        }

        return result;
    }
}
