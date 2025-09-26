namespace BuildingBlocks.Application.Abstractions.Messaging;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(IIntegrationEvent @event, CancellationToken ct = default);
    Task PublishAsync(IEnumerable<IIntegrationEvent> events, CancellationToken ct = default);
}
