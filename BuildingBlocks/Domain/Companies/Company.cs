using BuildingBlocks.Domain.Base;
using BuildingBlocks.Domain.Companies.Entities;
using BuildingBlocks.Domain.Companies.Events;
using BuildingBlocks.Domain.Companies.ValueObjects;

using BuildingBlocks.Domain.Enums;

namespace BuildingBlocks.Domain.Companies;

/// <summary>
/// Root aggregate representing a company (CNPJ raiz).
/// Includes state-level registrations, establishments, contacts,
/// and provenance metadata (SEFAZ, Receita Federal, manual, etc.).
/// </summary>
public sealed class Company : AggregateRoot
{
    private readonly List<StateRegistration> _stateRegistrations = new();
    private readonly List<ContactPerson> _contactPeople = new();
    private readonly List<Establishment> _establishments = new();

    private Company() { } // EF Core constructor

    public Guid TenantId { get; private set; }

    /// <summary>Brazilian CNPJ (digits only, 14 chars).</summary>
    public TaxId TaxId { get; private set; } = default!;

    /// <summary>Legal registered name (Razão Social).</summary>
    public string LegalName { get; private set; } = string.Empty;

    /// <summary>Optional trade name (Nome Fantasia).</summary>
    public string? TradeName { get; private set; }

    /// <summary>Optional sector taxonomy code (CNAE principal, for example).</summary>
    public string? SectorCode { get; private set; }

    /// <summary>Company incorporation date (from Receita Federal or SEFAZ).</summary>
    public DateTimeOffset? IncorporationDate { get; private set; }

    /// <summary>Current registration status (Active, Suspended, etc.).</summary>
    public RegistrationStatus Status { get; private set; } = RegistrationStatus.Unknown;

    /// <summary>Primary email contact.</summary>
    public EmailAddress? Email { get; private set; }

    /// <summary>Main headquarters or fiscal address.</summary>
    public Address? Address { get; private set; }

    /// <summary>
    /// Original authoritative source of this record (provenance metadata)
    /// (e.g., 'sefaz-ce', 'company-registry', 'manual-entry').
    /// </summary>
    public string? CanonicalSource { get; private set; }

    /// <summary>
    /// Version or checksum of the canonical source record.
    /// Useful for sync/audit verification.
    /// </summary>
    public string? CanonicalVersion { get; private set; }

    /// <summary>Audit timestamps.</summary>
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }

    /// <summary>State-level registrations (Inscrições Estaduais).</summary>
    public IReadOnlyCollection<StateRegistration> StateRegistrations => _stateRegistrations.AsReadOnly();

    /// <summary>Establishments (branches, headquarters, warehouses, etc.).</summary>
    public IReadOnlyCollection<Establishment> Establishments => _establishments.AsReadOnly();

    /// <summary>Contact persons linked to the company.</summary>
    public IReadOnlyCollection<ContactPerson> ContactPeople => _contactPeople.AsReadOnly();

    // -------------------------
    // Factory / Domain Behavior
    // -------------------------
    public static Company Register(string legalName, TaxId taxId, EmailAddress? email, Guid tenantId)
    {
        Guard.AgainstNullOrWhiteSpace(legalName, nameof(legalName));
        var company = new Company
        {
            LegalName = legalName.Trim(),
            TaxId = taxId,
            Email = email,
            TenantId = tenantId,
            CreatedAt = DateTimeOffset.UtcNow
        };
        company.Raise(new CompanyRegistered(company.Id, company.LegalName, company.TaxId.Value));
        return company;
    }

    public void UpdateContact(EmailAddress? email)
    {
        Email = email;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateTradeName(string? tradeName)
    {
        TradeName = tradeName?.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateStatus(RegistrationStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateAddress(Address address)
    {
        Address = address;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateIncorporationDate(DateTimeOffset? date)
    {
        IncorporationDate = date;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateSectorCode(string? sectorCode)
    {
        SectorCode = sectorCode?.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AddEstablishment(Establishment establishment)
    {
        Guard.AgainstNull(establishment, nameof(establishment));
        _establishments.Add(establishment);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AddContactPerson(ContactPerson contact)
    {
        Guard.AgainstNull(contact, nameof(contact));
        _contactPeople.Add(contact);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveContactPerson(Guid contactId)
    {
        var contact = _contactPeople.FirstOrDefault(c => c.Id == contactId);
        if (contact is not null)
        {
            _contactPeople.Remove(contact);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }


    public void SetCanonicalInfo(string source, string version)
    {
        CanonicalSource = source;
        CanonicalVersion = version;
    }

    public StateRegistration AddOrUpdateStateRegistration(
        string uf, string ie, string? status = null, string? regime = null, DateTimeOffset? lastCheckedAt = null)
    {
        var existing = _stateRegistrations.FirstOrDefault(r => r.Uf == uf.ToUpperInvariant());
        if (existing is null)
        {
            var reg = StateRegistration.Create(uf, ie);
            reg.Update(ie, status, regime, lastCheckedAt);
            _stateRegistrations.Add(reg);
            return reg;
        }

        existing.Update(ie, status, regime, lastCheckedAt);
        return existing;
    }

    public StateRegistration? GetStateRegistration(string uf) =>
        _stateRegistrations.FirstOrDefault(r => r.Uf == uf.ToUpperInvariant());
}
