using MediatR;

namespace Application.Companies;

/// <summary>
/// Command to register a new company.
/// </summary>
public sealed record RegisterCompanyCommand(string LegalName, string TaxId, string? Email)
    : IRequest<Guid>;
