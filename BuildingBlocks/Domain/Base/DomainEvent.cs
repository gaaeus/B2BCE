namespace BuildingBlocks.Domain.Base;

public abstract class DomainEvent : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
