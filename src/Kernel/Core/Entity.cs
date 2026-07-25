namespace MathVerse.Core;

/// <summary>
/// Represents a domain entity with an identity.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>Initializes a new entity with the specified identifier.</summary>
    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>Gets the entity's unique identifier.</summary>
    public TId Id { get; }

    /// <summary>Gets the entity's version for optimistic concurrency.</summary>
    public long Version { get; protected set; }

    /// <summary>Raises the entity's version.</summary>
    protected void IncrementVersion() => Version++;

    /// <inheritdoc/>
    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && GetType() == other.GetType();
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        obj is Entity<TId> other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    /// <summary>Equality operator.</summary>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
        Equals(left, right);

    /// <summary>Inequality operator.</summary>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) =>
        !Equals(left, right);
}

/// <summary>
/// Represents a domain entity with a Guid identifier.
/// </summary>
public abstract class Entity : Entity<Guid>
{
    /// <summary>Initializes a new entity with a generated identifier.</summary>
    protected Entity() : base(Guid.NewGuid()) { }

    /// <summary>Initializes a new entity with the specified identifier.</summary>
    protected Entity(Guid id) : base(id) { }
}
