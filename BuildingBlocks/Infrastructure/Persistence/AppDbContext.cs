using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Domain.Companies;
using Microsoft.EntityFrameworkCore;


namespace BuildingBlocks.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Outbox.OutboxMessage> OutboxMessages => Set<Outbox.OutboxMessage>();
    private readonly ITenantProvider _tenantProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider) : base(options)
    {
        _tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));

        // Example of using tenantProvider to set a property or filter data based on the current tenant.
        // This is just illustrative; actual multi-tenancy implementation may vary.
        var tenantId = tenantProvider.TenantId;
        if (tenantId == null)
        {
            throw new InvalidOperationException("TenantId is not set in TenantProvider.");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Apply tenant filter for Company (and optionally other tenant-scoped entities)
        modelBuilder.Entity<Company>()
            .HasQueryFilter(c => !_tenantProvider.TenantId.HasValue || c.TenantId == _tenantProvider.TenantId);

        // If you also store establishments/contacts in separate tables:
        modelBuilder.Entity<Domain.Companies.Entities.Establishment>()
            .HasQueryFilter(e => !_tenantProvider.TenantId.HasValue || e.CompanyId == _tenantProvider.TenantId
                || (e.Company != null && e.Company.TenantId == _tenantProvider.TenantId));
        // Adjust filters for your exact relationships as needed.
    }
}
