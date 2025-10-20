namespace BuildingBlocks.Application.Abstractions.Tenancy;

public interface ITenantProvider
{
    /// <summary>
    /// Current resolved tenant id (or null when unknown).
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>
    /// Sets the tenant id for the current scope. Called by middleware.
    /// </summary>
    void SetTenant(Guid tenantId);
}
