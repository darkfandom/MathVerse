namespace MathVerse.Math.Performance.Incremental;

/// <summary>
/// Represents a node in a dependency graph, tracking which nodes depend on it
/// and which nodes it depends on.
/// </summary>
public sealed class DependencyNode
{
    private readonly List<int> _dependents = [];
    private readonly List<int> _dependencies = [];
    private volatile bool _isDirty;

    /// <summary>Initializes a new dependency node.</summary>
    /// <param name="id">The unique node identifier.</param>
    /// <param name="name">The human-readable node name.</param>
    public DependencyNode(int id, string name)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>Gets the unique identifier of this node.</summary>
    public int Id { get; }

    /// <summary>Gets the human-readable name of this node.</summary>
    public string Name { get; }

    /// <summary>Gets the identifiers of nodes that depend on this node.</summary>
    public IReadOnlyList<int> Dependents => _dependents;

    /// <summary>Gets the identifiers of nodes that this node depends on.</summary>
    public IReadOnlyList<int> Dependencies => _dependencies;

    /// <summary>Gets whether this node has been marked as needing recomputation.</summary>
    public bool IsDirty => _isDirty;

    /// <summary>Marks this node as dirty, indicating it needs recomputation.</summary>
    public void MarkDirty()
    {
        _isDirty = true;
    }

    /// <summary>Marks this node as clean after successful recomputation.</summary>
    public void MarkClean()
    {
        _isDirty = false;
    }

    /// <summary>Adds a dependent node identifier.</summary>
    /// <param name="dependentId">The identifier of the dependent node.</param>
    internal void AddDependent(int dependentId)
    {
        if (!_dependents.Contains(dependentId))
            _dependents.Add(dependentId);
    }

    /// <summary>Adds a dependency node identifier.</summary>
    /// <param name="dependencyId">The identifier of the dependency node.</param>
    internal void AddDependency(int dependencyId)
    {
        if (!_dependencies.Contains(dependencyId))
            _dependencies.Add(dependencyId);
    }

    /// <summary>Removes a dependent node identifier.</summary>
    /// <param name="dependentId">The identifier to remove.</param>
    internal void RemoveDependent(int dependentId)
    {
        _dependents.Remove(dependentId);
    }

    /// <summary>Removes a dependency node identifier.</summary>
    /// <param name="dependencyId">The identifier to remove.</param>
    internal void RemoveDependency(int dependencyId)
    {
        _dependencies.Remove(dependencyId);
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"DependencyNode(Id={Id}, Name={Name}, IsDirty={IsDirty}, Dependencies={_dependencies.Count}, Dependents={_dependents.Count})";
}
