using MediatR;
using BuildingBlocks.Domain.Companies;
using BuildingBlocks.Domain.Base;
using BuildingBlocks.Application.Abstractions.Sefaz;

namespace Application.Companies;

public sealed class RefreshStateRegistrationFromSefazHandler : IRequestHandler<RefreshStateRegistrationFromSefazCommand, Unit>
{
    private readonly ICompanyRepository _repo;
    private readonly ISefazClient _sefaz;
    private readonly IUnitOfWork _uow;

    public RefreshStateRegistrationFromSefazHandler(
        ICompanyRepository repo,
        ISefazClient sefaz,
        IUnitOfWork uow)
    {
        _repo = repo;
        _sefaz = sefaz;
        _uow = uow;
    }

    public async Task<Unit> Handle(RefreshStateRegistrationFromSefazCommand request, CancellationToken cancellationToken)
    {
        var company = await _repo.GetByIdAsync(request.CompanyId, cancellationToken)
                      ?? throw new KeyNotFoundException("Company not found.");

        var reg = company.GetStateRegistration(request.Uf)
                  ?? throw new InvalidOperationException("State registration for UF is missing.");

        var result = await _sefaz.QueryAsync(company.TaxId.Value, reg.Uf, reg.Ie, cancellationToken);

        company.AddOrUpdateStateRegistration(reg.Uf, reg.Ie, result.Status, result.RegimeTributario, result.CheckedAtUtc);
        await _repo.UpdateAsync(company, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
