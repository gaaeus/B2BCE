using MediatR;
using System;

namespace Application.Companies
{
    // Returns Unit (no payload)
    public sealed record RefreshCompanyRegistrationsCommand(Guid CompanyId) : IRequest;
}
