using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Domain.Tenants;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/admin/tenants")]
public sealed class TenantRegistryController : ControllerBase
{
    private readonly ITenantRegistryService _registry;

    public TenantRegistryController(ITenantRegistryService registry)
    {
        _registry = registry;
    }

    [HttpPost]
    public async Task<ActionResult<Tenant>> RegisterTenant([FromBody] RegisterTenantRequest request, CancellationToken ct)
    {
        var tenant = await _registry.RegisterTenantAsync(
            request.Name,
            request.Cnpj,
            request.ContactEmail,
            request.SefazApiKey,
            request.SefazEnvironment,
            ct);

        return CreatedAtAction(nameof(GetTenant), new { id = tenant.Id }, tenant);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tenant>>> ListTenants(CancellationToken ct)
    {
        var tenants = await _registry.ListTenantsAsync(ct);
        return Ok(tenants);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Tenant>> GetTenant(Guid id, CancellationToken ct)
    {
        var tenant = await _registry.GetTenantAsync(id, ct);
        return tenant is null ? NotFound() : Ok(tenant);
    }

    public sealed class RegisterTenantRequest
    {
        public string Name { get; set; } = default!;
        public string? Cnpj { get; set; }
        public string? ContactEmail { get; set; }
        public string? SefazApiKey { get; set; }
        public string? SefazEnvironment { get; set; }
    }
}
