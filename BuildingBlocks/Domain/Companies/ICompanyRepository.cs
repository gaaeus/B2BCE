using BuildingBlocks.Domain.Base;

namespace BuildingBlocks.Domain.Companies;
public interface ICompanyRepository : IRepository<Company>
{
    Task<Company?> GetByTaxIdAsync(string taxId, CancellationToken ct = default);
    Task<Company?> GetWithStateRegistrationAsync(Guid id, string uf, CancellationToken ct = default);
}
