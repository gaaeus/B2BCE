using Microsoft.AspNetCore.Mvc;
using API.Tenancy;

namespace API.Controllers;

/// <summary>
/// Administrative endpoints for tenant management.
/// Used to create or update a tenant's dedicated database.
/// </summary>
[ApiController]
[Route("api/admin/tenants")]
public sealed class TenantAdminController : ControllerBase
{
    private readonly ITenantProvisioningService _provisioning;
    private readonly ILogger<TenantAdminController> _logger;

    public TenantAdminController(
        ITenantProvisioningService provisioning,
        ILogger<TenantAdminController> logger)
    {
        _provisioning = provisioning;
        _logger = logger;
    }

    /// <summary>
    /// Ensures that the specified tenant database exists and is migrated to the latest version.
    /// </summary>
    /// <param name="tenantId">The tenant's unique identifier (Guid).</param>
    /// <returns>NoContent if successful.</returns>
    /// <remarks>
    /// Example:
    /// POST /api/admin/tenants/11111111-2222-3333-4444-555555555555/provision
    /// </remarks>
    [HttpPost("{tenantId:guid}/provision")]
    public async Task<IActionResult> ProvisionAsync(Guid tenantId, CancellationToken ct)
    {
        _logger.LogInformation("Starting provisioning for tenant {TenantId}", tenantId);

        await _provisioning.EnsureTenantDatabaseAsync(tenantId);

        _logger.LogInformation("Tenant {TenantId} database ready.", tenantId);

        return NoContent();
    }
}
