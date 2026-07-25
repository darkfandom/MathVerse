namespace MathVerse.Math.Compiler.Memory;

using System;
using System.Collections.Generic;
using System.Text;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Plans memory allocation for compiled kernels. Assigns buffer locations (offsets) to minimize
/// total allocation size and maximize reuse of memory regions.
/// </summary>
public sealed class MemoryPlanner
{
    private readonly LifetimeAnalyzer _lifetimeAnalyzer = new();
    private readonly BufferReuse _bufferReuse = new();

    /// <summary>
    /// Plans memory allocation for a list of functions. Produces an optimized memory plan
    /// with assigned offsets and reuse regions.
    /// </summary>
    /// <param name="functions">The functions to plan memory for.</param>
    /// <returns>An optimized memory plan.</returns>
    public MemoryPlan Plan(IReadOnlyList<IRFunction> functions)
    {
        ArgumentNullException.ThrowIfNull(functions);
        if (functions.Count == 0)
            return new MemoryPlan(new Dictionary<IRValue, BufferAllocation>(), 0, 0, 0);

        var allocations = new Dictionary<IRValue, BufferAllocation>();
        var currentOffset = 0;
        var peakOffset = 0;
        var regionCount = 0;

        for (var f = 0; f < functions.Count; f++)
        {
            var function = functions[f];
            var lifetimes = _lifetimeAnalyzer.Analyze(function);
            var reuseGroups = _bufferReuse.FindReuseGroups(function);

            // Build reuse map: value → group index
            var valueToGroup = new Dictionary<int, int>();
            for (var g = 0; g < reuseGroups.Count; g++)
            {
                for (var i = 0; i < reuseGroups[g].Buffers.Count; i++)
                    valueToGroup[reuseGroups[g].Buffers[i].Id] = g;
            }

            // Track active allocations for peak computation
            var activeAllocations = new List<(int Offset, int Size)>();

            foreach (var kvp in lifetimes)
            {
                var value = kvp.Key;
                var lifetime = kvp.Value;

                if (value.IsConstant) continue;

                var size = IRTypeHelper.SizeInBytes(value.Type);
                if (size <= 0) continue;

                // Check if this value is in a reuse group
                if (valueToGroup.TryGetValue(value.Id, out var groupIdx))
                {
                    var group = reuseGroups[groupIdx];

                    // Only allocate once per group (for the first buffer in the group)
                    if (group.Buffers[0].Id == value.Id)
                    {
                        // Free overlapping allocations to find space
                        var offset = FindFreeSlot(activeAllocations, group.RequiredSize, currentOffset);

                        for (var i = 0; i < group.Buffers.Count; i++)
                        {
                            var buf = group.Buffers[i];
                            var bufSize = IRTypeHelper.SizeInBytes(buf.Type);
                            allocations[buf] = new BufferAllocation(
                                buf, offset, bufSize, lifetimes.TryGetValue(buf, out var lt) ? lt : lifetime, true);
                        }

                        regionCount++;
                        activeAllocations.Add((offset, group.RequiredSize));

                        var endOffset = offset + group.RequiredSize;
                        if (endOffset > peakOffset)
                            peakOffset = endOffset;
                    }
                }
                else
                {
                    // Free expired allocations
                    FreeExpiredAllocations(activeAllocations, lifetime.FirstUse);

                    var offset = FindFreeSlot(activeAllocations, size, currentOffset);
                    allocations[value] = new BufferAllocation(
                        value, offset, size, lifetime, true);

                    activeAllocations.Add((offset, size));
                    regionCount++;

                    var endOffset = offset + size;
                    if (endOffset > peakOffset)
                        peakOffset = endOffset;
                }
            }
        }

        var totalMemory = ComputeTotalMemory(allocations);
        return new MemoryPlan(allocations, totalMemory, peakOffset, regionCount);
    }

    /// <summary>
    /// Plans memory for a single function.
    /// </summary>
    /// <param name="function">The function to plan memory for.</param>
    /// <returns>An optimized memory plan.</returns>
    public MemoryPlan PlanSingle(IRFunction function)
    {
        return Plan(new[] { function });
    }

    private static int FindFreeSlot(List<(int Offset, int Size)> activeAllocations, int requiredSize, int fallbackOffset)
    {
        // Sort by offset to find gaps
        activeAllocations.Sort((a, b) => a.Offset.CompareTo(b.Offset));

        var searchEnd = fallbackOffset + 1024;

        // Try to find a gap between existing allocations
        var candidateOffset = 0;
        for (var i = 0; i <= activeAllocations.Count; i++)
        {
            var gapStart = candidateOffset;
            var gapEnd = i < activeAllocations.Count ? activeAllocations[i].Offset : searchEnd;

            if (gapEnd - gapStart >= requiredSize)
                return gapStart;

            if (i < activeAllocations.Count)
                candidateOffset = activeAllocations[i].Offset + activeAllocations[i].Size;
        }

        return candidateOffset;
    }

    private static void FreeExpiredAllocations(List<(int Offset, int Size)> activeAllocations, int currentInstructionIndex)
    {
        // In a real implementation, we'd track when allocations expire.
        // For now, we keep it simple and just remove old entries when the list gets too large.
        if (activeAllocations.Count > 256)
        {
            activeAllocations.RemoveRange(0, activeAllocations.Count / 2);
        }
    }

    private static int ComputeTotalMemory(Dictionary<IRValue, BufferAllocation> allocations)
    {
        var maxEnd = 0;
        foreach (var kvp in allocations)
        {
            var end = kvp.Value.Offset + kvp.Value.Size;
            if (end > maxEnd)
                maxEnd = end;
        }
        return maxEnd;
    }
}
