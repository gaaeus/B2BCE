using MediatR;

namespace Application.Companies;

/// <summary>Add or update a company's state registration (UF + IE).</summary>
public sealed record AddOrUpdateStateRegistrationCommand(
    Guid CompanyId, string Uf, string Ie, string? Status, string? RegimeTributario, DateTimeOffset? LastCheckedAt
) : IRequest<Unit>;
