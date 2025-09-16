using MediatR;

namespace Application.Companies;

/// <summary>
/// Query for a single company by Id.
/// </summary>
public sealed record GetCompanyByIdQuery(Guid CompanyId) : IRequest<CompanyDto>;

public sealed record CompanyDto(Guid Id, string LegalName, string TaxId, string? Email);
