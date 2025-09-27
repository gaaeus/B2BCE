using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Infrastructure.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Persistence.Outbox;

public sealed class OutboxDispatcher : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<OutboxDispatcher> _logger;
    private readonly string _instanceId = Environment.MachineName + "-" + Guid.NewGuid().ToString("N")[..6];

    private const int BatchSize = 20;
    private static readonly TimeSpan LockTimeout = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    public OutboxDispatcher(IServiceProvider sp, ILogger<OutboxDispatcher> logger)
    { _sp = sp; _logger = logger; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox dispatcher starting (instance {Instance})", _instanceId);
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ProcessBatchAsync(stoppingToken); }
            catch (Exception ex) { _logger.LogError(ex, "Outbox dispatcher error"); }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IIntegrationEventPublisher>();

        // pick a batch not processed and not locked (or lock expired)
        var now = DateTimeOffset.UtcNow;
        var threshold = now - LockTimeout;

        var query = db.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .Where(x => !x.LockedAtUtc.HasValue || x.LockedAtUtc.Value < threshold)
            .OrderBy(x => x.OccurredOnUtc)
            .Take(BatchSize);

        var candidates = await query.ToListAsync(ct);

        if (candidates.Count == 0) return;

        // lock them
        foreach (var msg in candidates) msg.MarkProcessing(_instanceId);
        await db.SaveChangesAsync(ct);

        foreach (var msg in candidates)
        {
            try
            {
                IIntegrationEvent? obj = SystemTextJsonSerializer.Deserialize(msg.Payload, msg.Type) as IIntegrationEvent ?? throw new InvalidOperationException($"Cannot deserialize event type '{msg.Type}'.");
                await publisher.PublishAsync(obj, ct);
                msg.MarkProcessed();
            }
            catch (Exception ex)
            {
                msg.MarkFailed(ex.Message);
                _logger.LogError(ex, "Failed to publish outbox message {MessageId}", msg.Id);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
