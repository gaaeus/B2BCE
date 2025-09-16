using BuildingBlocks.Domain.Base;
using BuildingBlocks.Domain.Companies;
using BuildingBlocks.Domain.Companies.ValueObjects;
using MediatR;

namespace Application.Companies;

/// <summary>
/// Handles company registration.
/// </summary>
public sealed class RegisterCompanyHandler : IRequestHandler<RegisterCompanyCommand, Guid>
{
    private readonly ICompanyRepository _repo;
    private readonly IUnitOfWork _uow;

    public RegisterCompanyHandler(ICompanyRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Guid> Handle(RegisterCompanyCommand request, CancellationToken cancellationToken)
    {
        if (await _repo.GetByTaxIdAsync(request.TaxId, cancellationToken) is not null)
            throw new InvalidOperationException("A company with this TaxId already exists.");

        var taxId = TaxId.Create(request.TaxId);
        var email = string.IsNullOrWhiteSpace(request.Email) ? null : EmailAddress.Create(request.Email);

        var company = Company.Register(request.LegalName, taxId, email);

        await _repo.AddAsync(company, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return company.Id;
    }
}
