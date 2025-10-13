namespace API.Contracts.Admin;

public sealed record OutboxMessageDto(
    Guid Id,
    string Type,
    DateTimeOffset OccurredOnUtc,
    DateTimeOffset? ProcessedOnUtc,
    DateTimeOffset? LockedAtUtc,
    string? LockedBy,
    string? Error);
