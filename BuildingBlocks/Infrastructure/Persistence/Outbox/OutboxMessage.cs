using BuildingBlocks.Application.Abstractions.Messaging;

namespace BuildingBlocks.Infrastructure.Persistence.Outbox;

/// <summary>EF entity that stores pending integration events (transactional outbox).</summary>
public sealed class OutboxMessage : IOutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public DateTimeOffset OccurredOnUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
    public DateTimeOffset? LockedAtUtc { get; set; }
    public string? LockedBy { get; set; }
    public Guid? TenantId { get; set; }

    public OutboxMessage() { }

    public static OutboxMessage Create(string type, string payload, DateTimeOffset occurredOn, Guid tenantId)
        => new() { Type = type, Payload = payload, OccurredOnUtc = occurredOn, TenantId =  tenantId };

    public void MarkProcessing(string? locker) { LockedBy = locker; LockedAtUtc = DateTimeOffset.UtcNow; }
    public void MarkProcessed() { ProcessedOnUtc = DateTimeOffset.UtcNow; LockedBy = null; LockedAtUtc = null; Error = null; }
    public void MarkFailed(string? error) { Error = error; LockedBy = null; LockedAtUtc = null; }
}
