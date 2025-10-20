namespace BuildingBlocks.Application.Abstractions.Tenancy;

public interface ITenantConnectionResolver
{
    /// <summary>
    /// Returns the connection string to use for the provided tenantId.
    /// If tenantId is null, return the default connection string for the app.
    /// Implementation should never return null.
    /// </summary>
    string GetConnectionString(Guid? tenantId);
}
