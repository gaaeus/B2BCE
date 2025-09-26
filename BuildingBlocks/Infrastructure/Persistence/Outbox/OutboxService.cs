using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Infrastructure.Serialization;

namespace BuildingBlocks.Infrastructure.Persistence.Outbox;

public sealed class OutboxService : IOutboxService
{
    private readonly AppDbContext _db;
    public OutboxService(AppDbContext db) => _db = db;

    public Task EnqueueAsync(IIntegrationEvent @event, CancellationToken ct = default)
        => EnqueueAsync(new[] { @event }, ct);

    public async Task EnqueueAsync(IEnumerable<IIntegrationEvent> events, CancellationToken ct = default)
    {
        foreach (var e in events)
        {
            var typeName = e.GetType().AssemblyQualifiedName ?? e.GetType().FullName!;
            var payload = SystemTextJsonSerializer.Serialize(e);
            var row = OutboxMessage.Create(typeName, payload, e.OccurredOnUtc);
            await _db.OutboxMessages.AddAsync(row, ct);
        }
    }
}
