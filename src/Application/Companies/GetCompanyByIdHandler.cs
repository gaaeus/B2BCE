using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Domain.Companies;
using MediatR;

namespace Application.Companies
{
    /// <summary>Reads a company with simple cache.</summary>
    public sealed class GetCompanyByIdHandler : IRequestHandler<GetCompanyByIdQuery, CompanyDto>
    {
        private readonly ICompanyRepository _repo;
        private readonly ICache _cache;

        public GetCompanyByIdHandler(ICompanyRepository repo, ICache cache)
        {
            _repo = repo;
            _cache = cache;
        }

        public async Task<CompanyDto> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"company:{request.CompanyId}";
            var cached = await _cache.GetAsync<CompanyDto>(cacheKey, cancellationToken);
            if (cached is not null) return cached;

            var entity = await _repo.GetByIdAsync(request.CompanyId, cancellationToken)
                         ?? throw new KeyNotFoundException("Company not found.");

            var dto = new CompanyDto(entity.Id, entity.LegalName, entity.TaxId.Value, entity.Email?.Value);
            await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5), cancellationToken);
            return dto;
        }
    }
}
