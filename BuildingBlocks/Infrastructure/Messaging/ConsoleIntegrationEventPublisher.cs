using BuildingBlocks.Application.Abstractions.Messaging;

namespace BuildingBlocks.Infrastructure.Messaging
{
    public sealed class ConsoleIntegrationEventPublisher : IIntegrationEventPublisher
    {
        public Task PublishAsync(IIntegrationEvent @event, CancellationToken ct = default)
        {
            Console.WriteLine($"[PUBLISH] {@event.GetType().Name} at {@event.OccurredOnUtc:O}");
            return Task.CompletedTask;
        }

        public async Task PublishAsync(IEnumerable<IIntegrationEvent> events, CancellationToken ct = default)
        {
            foreach (var e in events) await PublishAsync(e, ct);
        }
    }
}
