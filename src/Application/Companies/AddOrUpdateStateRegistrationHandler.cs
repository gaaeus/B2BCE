using Application.Abstractions;
using BuildingBlocks.Domain.Companies;
using MediatR;

namespace Application.Companies;

public sealed class AddOrUpdateStateRegistrationHandler : IRequestHandler<AddOrUpdateStateRegistrationCommand, Unit>
{
    private readonly ICompanyRepository _repo;
    private readonly IUnitOfWork _uow;

    public AddOrUpdateStateRegistrationHandler(ICompanyRepository repo, IUnitOfWork uow)
    { _repo = repo; _uow = uow; }

    public async Task<Unit> Handle(AddOrUpdateStateRegistrationCommand request, CancellationToken cancellationToken)
    {
        var company = await _repo.GetByIdAsync(request.CompanyId, cancellationToken)
                      ?? throw new KeyNotFoundException("Company not found.");

        company.AddOrUpdateStateRegistration(request.Uf, request.Ie, request.Status, request.RegimeTributario, request.LastCheckedAt);
        await _repo.UpdateAsync(company, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
