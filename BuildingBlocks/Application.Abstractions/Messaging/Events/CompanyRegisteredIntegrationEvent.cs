// NOTES TO SELF:
/*
Domain → Application usage (how to emit events)

Anywhere in your Application handlers (e.g., when company registers, SEFAZ refresh, etc.), inject IOutboxService and call EnqueueAsync(new CompanyRegisteredIntegrationEvent(...)).

Create one sample integration event so we have something concrete:
 Create one sample integration event so we have something concrete
 You can enqueue it in RegisterCompanyHandler after SaveChangesAsync (or before—either is fine since the outbox row is persisted in the same transaction as the aggregate).
*/

namespace BuildingBlocks.Application.Abstractions.Messaging.Events
{
    public sealed class CompanyRegisteredIntegrationEvent : IIntegrationEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTimeOffset OccurredOnUtc { get; } = DateTimeOffset.UtcNow;

        public Guid CompanyId { get; init; }
        public string LegalName { get; init; } = default!;
        public string TaxId { get; init; } = default!;
    }
}
