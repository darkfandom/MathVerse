namespace MathVerse.Math.Performance.Incremental;

/// <summary>
/// Immutable record representing a set of changes in a dependency graph.
/// </summary>
public sealed class ChangeSet
{
    /// <summary>An empty change set with no changes.</summary>
    public static readonly ChangeSet Empty = new(new HashSet<int>(), new HashSet<int>());

    private readonly HashSet<int> _changedNodes;
    private readonly HashSet<int> _affectedNodes;

    /// <summary>Initializes a new change set.</summary>
    /// <param name="changedNodes">The set of directly changed node identifiers.</param>
    /// <param name="affectedNodes">The set of all transitively affected node identifiers.</param>
    public ChangeSet(IReadOnlySet<int> changedNodes, IReadOnlySet<int> affectedNodes)
    {
        _changedNodes = new HashSet<int>(changedNodes);
        _affectedNodes = new HashSet<int>(affectedNodes);
    }

    /// <summary>Gets the set of directly changed node identifiers.</summary>
    public IReadOnlySet<int> ChangedNodes => _changedNodes;

    /// <summary>Gets the set of all transitively affected node identifiers.</summary>
    public IReadOnlySet<int> AffectedNodes => _affectedNodes;

    /// <summary>Gets whether this change set contains any changes.</summary>
    public bool HasChanges => _changedNodes.Count > 0;

    /// <summary>Merges this change set with another, producing a combined change set.</summary>
    /// <param name="other">The other change set to merge with.</param>
    /// <returns>A new <see cref="ChangeSet"/> containing the union of both change sets.</returns>
    public ChangeSet Merge(ChangeSet other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var mergedChanged = new HashSet<int>(_changedNodes);
        mergedChanged.UnionWith(other._changedNodes);

        var mergedAffected = new HashSet<int>(_affectedNodes);
        mergedAffected.UnionWith(other._affectedNodes);

        return new ChangeSet(mergedChanged, mergedAffected);
    }
}
