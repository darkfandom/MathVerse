namespace MathVerse.Math.Compiler.Graph;

/// <summary>Represents a directed edge in a computation graph connecting two nodes via ports.</summary>
/// <param name="From">Source node ID.</param>
/// <param name="To">Destination node ID.</param>
/// <param name="FromPort">Output port index on the source node.</param>
/// <param name="ToPort">Input port index on the destination node.</param>
/// <param name="Weight">Optional weight for the edge (used in weighted graphs).</param>
public sealed record GraphEdge
(
    int From,
    int To,
    int FromPort = 0,
    int ToPort = 0,
    double? Weight = null
)
{
    /// <summary>Whether this edge has an associated weight.</summary>
    public bool HasWeight => Weight.HasValue;

    /// <summary>Whether this edge is a self-loop.</summary>
    public bool IsSelfLoop => From == To;

    /// <inheritdoc />
    public override string ToString() =>
        Weight.HasValue
            ? $"Edge({From}:{FromPort} -> {To}:{ToPort}, w={Weight.Value:G})"
            : $"Edge({From}:{FromPort} -> {To}:{ToPort})";
}
