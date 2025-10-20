using BuildingBlocks.Application.Abstractions.Messaging;

namespace BuildingBlocks.Domain.Events;

public abstract class IntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOnUtc { get; } = DateTimeOffset.UtcNow;
    public Guid? TenantId { get; protected set; }

    protected IntegrationEvent(Guid? tenantId)
    {
        TenantId = tenantId;
    }
}
