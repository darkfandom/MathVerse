namespace MathVerse.Math.Compiler.Memory;

using System;

/// <summary>
/// Represents the lifetime range of a value in IR — from its first use to its last use,
/// measured in instruction sequence numbers.
/// </summary>
public readonly record struct LifetimeRange
{
    /// <summary>The instruction index where this value is first defined or used.</summary>
    public int FirstUse { get; }

    /// <summary>The instruction index where this value is last used.</summary>
    public int LastUse { get; }

    /// <summary>The duration of this lifetime in instruction count.</summary>
    public int Duration => LastUse - FirstUse;

    /// <summary>
    /// Initializes a new lifetime range.
    /// </summary>
    /// <param name="firstUse">The first instruction index.</param>
    /// <param name="lastUse">The last instruction index.</param>
    public LifetimeRange(int firstUse, int lastUse)
    {
        FirstUse = firstUse;
        LastUse = lastUse;
    }

    /// <summary>
    /// Determines whether this lifetime overlaps with another.
    /// </summary>
    /// <param name="other">The other lifetime range.</param>
    /// <returns>True if the ranges overlap.</returns>
    public bool Overlaps(LifetimeRange other)
    {
        return FirstUse <= other.LastUse && other.FirstUse <= LastUse;
    }

    /// <summary>
    /// Determines whether this lifetime completely contains another.
    /// </summary>
    /// <param name="other">The other lifetime range.</param>
    /// <returns>True if this range contains the other.</returns>
    public bool Contains(LifetimeRange other)
    {
        return FirstUse <= other.FirstUse && LastUse >= other.LastUse;
    }

    /// <summary>
    /// Merges two overlapping or adjacent lifetime ranges.
    /// </summary>
    /// <param name="other">The other range to merge with.</param>
    /// <returns>The union of both ranges.</returns>
    public LifetimeRange Merge(LifetimeRange other)
    {
        return new LifetimeRange(
            Math.Min(FirstUse, other.FirstUse),
            Math.Max(LastUse, other.LastUse));
    }

    /// <inheritdoc/>
    public override string ToString() => $"[{FirstUse}..{LastUse}]";
}
