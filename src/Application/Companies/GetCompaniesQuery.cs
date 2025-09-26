using MediatR;
using BuildingBlocks.Application.Abstractions.Paging;

namespace Application.Companies;

public sealed record GetCompaniesQuery(int Page = 1, int PageSize = 20)
    : IRequest<PageResult<CompanyDto>>;
