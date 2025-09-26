using BuildingBlocks.Application.Abstractions.Paging;
using BuildingBlocks.Domain.Companies;
using BuildingBlocks.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Companies;

public sealed class GetCompaniesHandler : IRequestHandler<GetCompaniesQuery, PageResult<CompanyDto>>
{
    private readonly ICompanyRepository _repo;
    private readonly AppDbContext _db; // direct read context

    public GetCompaniesHandler(ICompanyRepository repo, AppDbContext db)
    {
        _repo = repo;
        _db = db;
    }

    public async Task<PageResult<CompanyDto>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
    {
        var page = new PageRequest(request.Page, request.PageSize);

        var total = await _db.Companies.CountAsync(cancellationToken);
        var items = await _db.Companies
            .AsNoTracking()
            .OrderBy(c => c.LegalName)
            .Skip(page.Skip)
            .Take(page.Take)
            .Select(c => new CompanyDto(c.Id, c.LegalName, c.TaxId.Value, c.Email!.Value))
            .ToListAsync(cancellationToken);

        return new PageResult<CompanyDto>(items, request.Page, request.PageSize, total);
    }
}
