namespace BuildingBlocks.Application.Abstractions.Messaging;

/// <summary>
/// Base contract for integration events to be published through the Outbox.
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// Globally unique identifier for this event.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// UTC timestamp of when the event occurred.
    /// </summary>
    DateTimeOffset OccurredOnUtc { get; }

    /// <summary>
    /// The TenantId associated with the event, if any.
    /// </summary>
    Guid? TenantId { get; }
}
