namespace BuildingBlocks.Application.Abstractions.Messaging;

public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTimeOffset OccurredOnUtc { get; }
}
