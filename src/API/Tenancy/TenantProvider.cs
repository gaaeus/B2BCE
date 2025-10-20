using BuildingBlocks.Application.Abstractions.Tenancy;

namespace API.Tenancy;

public sealed class TenantProvider : ITenantProvider
{
    private Guid? _tenantId;
    public Guid? TenantId => _tenantId;

    public void SetTenant(Guid tenantId) => _tenantId = tenantId;
}
