namespace MathVerse.Math.Compiler.Memory;

using System;
using System.Collections.Generic;
using MathVerse.Math.Compiler.IR;

/// <summary>
/// Represents a single buffer allocation within a memory plan.
/// </summary>
public sealed class BufferAllocation
{
    /// <summary>The IR value associated with this allocation.</summary>
    public IRValue Buffer { get; }

    /// <summary>The byte offset within the allocated memory block.</summary>
    public int Offset { get; }

    /// <summary>The size in bytes of this allocation.</summary>
    public int Size { get; }

    /// <summary>The lifetime range of this buffer in the instruction stream.</summary>
    public LifetimeRange Lifetime { get; }

    /// <summary>Whether this allocation can be placed on the stack.</summary>
    public bool IsStackAllocatable { get; }

    /// <summary>
    /// Initializes a new buffer allocation record.
    /// </summary>
    public BufferAllocation(IRValue buffer, int offset, int size, LifetimeRange lifetime, bool isStackAllocatable)
    {
        Buffer = buffer;
        Offset = offset;
        Size = size;
        Lifetime = lifetime;
        IsStackAllocatable = isStackAllocatable;
    }
}

/// <summary>
/// Contains the result of memory planning: buffer allocations, total memory required, and peak usage.
/// </summary>
public sealed class MemoryPlan
{
    /// <summary>Map from IR values to their buffer allocations.</summary>
    public IReadOnlyDictionary<IRValue, BufferAllocation> Allocations { get; }

    /// <summary>Total bytes of memory required by this plan.</summary>
    public int TotalMemoryRequired { get; }

    /// <summary>Peak memory usage at any point during execution.</summary>
    public int PeakMemoryUsage { get; }

    /// <summary>The number of distinct memory regions (for pool sizing).</summary>
    public int RegionCount { get; }

    /// <summary>
    /// Initializes a new memory plan.
    /// </summary>
    public MemoryPlan(IReadOnlyDictionary<IRValue, BufferAllocation> allocations, int totalMemoryRequired, int peakMemoryUsage, int regionCount)
    {
        Allocations = allocations ?? new Dictionary<IRValue, BufferAllocation>();
        TotalMemoryRequired = totalMemoryRequired;
        PeakMemoryUsage = peakMemoryUsage;
        RegionCount = regionCount;
    }
}
