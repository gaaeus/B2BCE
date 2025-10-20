using BuildingBlocks.Domain.Events;

namespace BuildingBlocks.Domain.Companies.Events;

/// <summary>
/// Represents an integration event that occurs when a company is registered.
/// </summary>
/// <remarks>This event contains information about the registered company, including its unique
/// identifier, legal name, and tax ID. It is typically used to notify other systems or services about the
/// registration of a new company.</remarks>
public sealed class CompanyRegisteredIntegrationEvent : IntegrationEvent
{
    public Guid CompanyId { get; }
    public string LegalName { get; }
    public string TaxId { get; }

    public CompanyRegisteredIntegrationEvent(Guid? tenantId, Guid companyId, string legalName, string taxId)
        : base(tenantId)
    {
        CompanyId = companyId;
        LegalName = legalName;
        TaxId = taxId;
    }
}
