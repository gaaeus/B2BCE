using MediatR;

namespace Application.Companies;

/// <summary>Refreshes a company's state registration (UF) against SEFAZ and updates the aggregate.</summary>
public sealed record RefreshStateRegistrationFromSefazCommand(Guid CompanyId, string Uf) : IRequest<Unit>;
