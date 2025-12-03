using MediatR;

namespace Application.Companies;

// Returns Unit (no payload)
public sealed record RefreshCompanyRegistrationsCommand(Guid CompanyId) : IRequest<Unit>;
