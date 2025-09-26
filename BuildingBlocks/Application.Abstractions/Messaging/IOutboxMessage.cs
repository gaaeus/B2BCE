namespace BuildingBlocks.Application.Abstractions.Messaging
{
    public interface IOutboxMessage
    {
        Guid Id { get; }
        string Type { get; }
        string Payload { get; }
        DateTimeOffset OccurredOnUtc { get; }
        DateTimeOffset? ProcessedOnUtc { get; }
        string? Error { get; }
        DateTimeOffset? LockedAtUtc { get; }
        string? LockedBy { get; }
    }
}
