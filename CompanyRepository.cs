using BuildingBlocks.Domain.Companies;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BuildingBlocks.Infrastructure.Persistence.Repositories;

public sealed class CompanyRepository : ICompanyRepository
{
    private readonly AppDbContext _db;

    public CompanyRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Company aggregate, CancellationToken ct = default)
        => await _db.Companies.AddAsync(aggregate, ct);

    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Companies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task UpdateAsync(Company aggregate, CancellationToken ct = default)
    {
        _db.Companies.Update(aggregate);
        await Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<Company, bool>> predicate, CancellationToken ct = default)
        => _db.Companies.AnyAsync(predicate, ct);

    public Task<Company?> GetByTaxIdAsync(string taxId, CancellationToken ct = default)
        => _db.Companies.AsNoTracking().FirstOrDefaultAsync(x => x.TaxId.Value == taxId, ct);
}
