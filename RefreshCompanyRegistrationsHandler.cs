using BuildingBlocks.Application.Abstractions.Sefaz;
using BuildingBlocks.Domain.Base;
using BuildingBlocks.Domain.Companies;
using BuildingBlocks.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Companies
{
    public sealed class RefreshCompanyRegistrationsHandler : IRequestHandler<RefreshCompanyRegistrationsCommand, Unit>
    {
        private readonly ICompanyRepository _repo;
        private readonly ISefazClient _sefaz;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<RefreshCompanyRegistrationsHandler> _logger;

        public RefreshCompanyRegistrationsHandler(
            ICompanyRepository repo,
            ISefazClient sefaz,
            IUnitOfWork uow,
            ILogger<RefreshCompanyRegistrationsHandler> logger)
        {
            _repo = repo;
            _sefaz = sefaz;
            _uow = uow;
            _logger = logger;
        }

        public async Task<Unit> Handle(RefreshCompanyRegistrationsCommand request, CancellationToken ct)
        {
            var company = await _repo.GetByIdAsync(request.CompanyId, ct)
                          ?? throw new KeyNotFoundException($"Company {request.CompanyId} not found.");

            // iterate registrations (use copy to avoid modification while iterating)
            var regs = company.StateRegistrations.ToList();

            foreach (var reg in regs)
            {
                try
                {
                    var result = await _sefaz.QueryAsync(company.TaxId.Value, reg.Uf, reg.Ie, ct);

                    // update via domain method (keeps invariants)
                    company.AddOrUpdateStateRegistration(
                        uf: reg.Uf,
                        ie: reg.Ie,
                        status: result.Status,
                        regime: result.RegimeTributario,
                        lastCheckedAt: result.CheckedAtUtc
                    );

                    // mark canonical info (source/version); use checkedAt as version token
                    company.SetCanonicalInfo($"sefaz-{reg.Uf.ToLowerInvariant()}", result.CheckedAtUtc.ToString("O"));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error querying SEFAZ for company {CompanyId} UF={Uf}", company.Id, reg.Uf);
                    // do not stop whole loop — continue with others
                }
            }

            await _uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
