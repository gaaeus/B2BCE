namespace BuildingBlocks.Application.Abstractions.Messaging
{
    public interface IOutboxService
    {
        Task EnqueueAsync(IIntegrationEvent @event, CancellationToken ct = default);
        Task EnqueueAsync(IEnumerable<IIntegrationEvent> events, CancellationToken ct = default);
    }
}
