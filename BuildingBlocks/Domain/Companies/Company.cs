using BuildingBlocks.Domain.Base;
using BuildingBlocks.Domain.Companies.Entities;
using BuildingBlocks.Domain.Companies.Events;
using BuildingBlocks.Domain.Companies.ValueObjects;
using System.Net.Mail;

namespace BuildingBlocks.Domain.Companies;

public sealed class Company : AggregateRoot
{
    private Company() { } // EF

    public string LegalName { get; private set; } = default!;
    public TaxId TaxId { get; private set; } = default!;
    public EmailAddress? Email { get; private set; }

    private readonly List<StateRegistration> _stateRegistrations = new();
    public IReadOnlyCollection<StateRegistration> StateRegistrations => _stateRegistrations.AsReadOnly();

    public static Company Register(string legalName, TaxId taxId, EmailAddress? email)
    {
        Guard.AgainstNullOrWhiteSpace(legalName, nameof(legalName));
        var company = new Company { LegalName = legalName.Trim(), TaxId = taxId, Email = email };
        company.Raise(new CompanyRegistered(company.Id, company.LegalName, company.TaxId.Value));
        return company;
    }

    public void UpdateContact(EmailAddress? email) => Email = email;

    public StateRegistration AddOrUpdateStateRegistration(string uf, string ie, string? status = null, string? regime = null, DateTimeOffset? lastCheckedAt = null)
    {
        var existing = _stateRegistrations.FirstOrDefault(r => r.Uf == uf.ToUpperInvariant());
        if (existing is null)
        {
            var reg = StateRegistration.Create(uf, ie);
            reg.Update(status: status, regime: regime, lastCheckedAt: lastCheckedAt);
            _stateRegistrations.Add(reg);
            return reg;
        }
        existing.Update(ie: ie, status: status, regime: regime, lastCheckedAt: lastCheckedAt);
        return existing;
    }

    public StateRegistration? GetStateRegistration(string uf) =>
        _stateRegistrations.FirstOrDefault(r => r.Uf == uf.ToUpperInvariant());
}
