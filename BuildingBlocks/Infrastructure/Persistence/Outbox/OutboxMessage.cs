using BuildingBlocks.Application.Abstractions.Messaging;

namespace BuildingBlocks.Infrastructure.Persistence.Outbox
{
    /// <summary>EF entity that stores pending integration events (transactional outbox).</summary>
    public sealed class OutboxMessage : IOutboxMessage
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Type { get; private set; } = default!;
        public string Payload { get; private set; } = default!;
        public DateTimeOffset OccurredOnUtc { get; private set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? ProcessedOnUtc { get; private set; }
        public string? Error { get; private set; }
        public DateTimeOffset? LockedAtUtc { get; private set; }
        public string? LockedBy { get; private set; }

        private OutboxMessage() { }

        public static OutboxMessage Create(string type, string payload, DateTimeOffset occurredOn)
            => new() { Type = type, Payload = payload, OccurredOnUtc = occurredOn };

        public void MarkProcessing(string locker) { LockedBy = locker; LockedAtUtc = DateTimeOffset.UtcNow; }
        public void MarkProcessed() { ProcessedOnUtc = DateTimeOffset.UtcNow; LockedBy = null; LockedAtUtc = null; Error = null; }
        public void MarkFailed(string error) { Error = error; LockedBy = null; LockedAtUtc = null; }
    }
}
