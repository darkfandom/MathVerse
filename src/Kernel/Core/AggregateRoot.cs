namespace MathVerse.Core;

/// <summary>
/// Represents an aggregate root that maintains consistency boundaries.
/// </summary>
/// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>Initializes a new aggregate with the specified identifier.</summary>
    protected AggregateRoot(TId id) : base(id) { }

    /// <summary>Gets the uncommitted domain events.</summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    /// <summary>Raises a domain event.</summary>
    protected void RaiseEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
        IncrementVersion();
    }

    /// <summary>Clears all uncommitted domain events.</summary>
    public void ClearEvents() => _domainEvents.Clear();
}

/// <summary>
/// Represents an aggregate root with a Guid identifier.
/// </summary>
public abstract class AggregateRoot : AggregateRoot<Guid>
{
    /// <summary>Initializes a new aggregate with a generated identifier.</summary>
    protected AggregateRoot() : base(Guid.NewGuid()) { }

    /// <summary>Initializes a new aggregate with the specified identifier.</summary>
    protected AggregateRoot(Guid id) : base(id) { }
}

/// <summary>
/// Represents a domain event.
/// </summary>
public interface IDomainEvent
{
    /// <summary>Gets when the event occurred.</summary>
    DateTimeOffset OccurredOn { get; }
}
