using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Application.Abstractions.Tenancy;

namespace API.Tenancy;

/// <summary>
/// Service to provision tenant databases (apply migrations)
/// </summary>
public interface ITenantProvisioningService
{
    Task EnsureTenantDatabaseAsync(Guid tenantId);
}

public sealed class TenantProvisioningService : ITenantProvisioningService
{
    private readonly IServiceProvider _sp;
    private readonly ITenantConnectionResolver _resolver;

    public TenantProvisioningService(IServiceProvider sp, ITenantConnectionResolver resolver)
    {
        _sp = sp;
        _resolver = resolver;
    }

    public async Task EnsureTenantDatabaseAsync(Guid tenantId)
    {
        // create a scope and set the TenantId on the scoped TenantProvider so DbContext uses correct connection
        using var scope = _sp.CreateScope();

        // set tenant on provider (requires an implementation that exposes SetTenant method)
        var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>() as TenantProvider;
        if (tenantProvider == null)
            throw new InvalidOperationException("TenantProvider implementation not found or incompatible.");

        tenantProvider.SetTenant(tenantId);

        // Resolve the dbcontext from the scope, it will be configured with the tenant connection
        var db = scope.ServiceProvider.GetRequiredService<BuildingBlocks.Infrastructure.Persistence.AppDbContext>();

        // Apply migrations
        await db.Database.MigrateAsync();
    }
}
