namespace BuildingBlocks.Domain.Base;

/// <summary>
/// Domain event marker.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
