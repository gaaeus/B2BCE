using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Domain.Base;
using BuildingBlocks.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;


namespace BuildingBlocks.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Intercepts EF Core SaveChanges to collect domain events from aggregates
/// and store them as OutboxMessages (for eventual dispatch).
/// </summary>
public sealed class OutboxInterceptor : SaveChangesInterceptor
{
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<OutboxInterceptor> _logger;

    public OutboxInterceptor(ITenantProvider tenantProvider, ILogger<OutboxInterceptor> logger)
    {
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not DbContext dbContext)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        var tenantId = _tenantProvider.TenantId;

        // Collect domain events from aggregates
        var domainEntities = dbContext.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .ToList();

        foreach (var (entry, events) in from entry in domainEntities
                                        let events = entry.Entity.DomainEvents.ToList()
                                        select (entry, events))
        {
            foreach (var domainEvent in events)
            {
                var outboxMessage = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    OccurredOnUtc = DateTimeOffset.UtcNow,
                    Type = domainEvent.GetType().Name,
                    Payload = JsonSerializer.Serialize(domainEvent,
                        domainEvent.GetType(),
                        new JsonSerializerOptions { WriteIndented = false }),
                    TenantId = tenantId
                };

                dbContext.Set<OutboxMessage>().Add(outboxMessage);
            }

            entry.Entity.ClearDomainEvents();
        }

        _logger.LogDebug("OutboxInterceptor captured {Count} domain events.", domainEntities.Count);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
