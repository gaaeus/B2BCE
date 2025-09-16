using BuildingBlocks.Domain.Base;

namespace BuildingBlocks.Domain.Companies.Events;

/// <summary>
/// Raised when a new company is registered.
/// </summary>
public sealed class CompanyRegistered : DomainEvent
{
    public Guid CompanyId { get; }
    public string LegalName { get; }
    public string TaxId { get; }

    public CompanyRegistered(Guid companyId, string legalName, string taxId)
    {
        CompanyId = companyId;
        LegalName = legalName;
        TaxId = taxId;
    }
}
