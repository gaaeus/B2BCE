using BuildingBlocks.Application.Abstractions.Tenancy;
using BuildingBlocks.Application.Abstractions.Tenancy.Models;
using BuildingBlocks.Domain.Tenants;
using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BuildingBlocks.Application.Tenancy;

/// <summary>
/// Application service responsible for registering and managing tenants.
/// Maps Domain entities (Tenant) to DTOs exposed through the application boundary.
/// </summary>
public sealed class TenantRegistryService : ITenantRegistryService
{
    private readonly AppDbContext _db;
    private readonly ITenantProvisioningService _provisioner;

    public TenantRegistryService(AppDbContext db, ITenantProvisioningService provisioner)
    {
        _db = db;
        _provisioner = provisioner;
    }

    public async Task<TenantDto> RegisterTenantAsync(
        string name,
        string? cnpj,
        string? contactEmail,
        string? sefazApiKey,
        string? sefazEnvironment,
        CancellationToken ct = default)
    {
        var tenant = Tenant.Create(name, cnpj, contactEmail, sefazApiKey, sefazEnvironment);

        await _db.Set<Tenant>().AddAsync(tenant, ct);
        await _db.SaveChangesAsync(ct);

        // Provision tenant-specific database schema
        await _provisioner.EnsureTenantDatabaseAsync(tenant.Id);

        return MapToDto(tenant);
    }

    public async Task<IEnumerable<TenantDto>> ListTenantsAsync(CancellationToken ct = default)
    {
        var tenants = await _db.Set<Tenant>().AsNoTracking().OrderBy(t => t.Name).ToListAsync(ct);
        return tenants.Select(MapToDto);
    }

    public async Task<TenantDto?> GetTenantAsync(Guid id, CancellationToken ct = default)
    {
        var tenant = await _db.Set<Tenant>().FindAsync([id], ct);
        return tenant is null ? null : MapToDto(tenant);
    }

    private static TenantDto MapToDto(Tenant t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Cnpj = t.Cnpj,
        ContactEmail = t.ContactEmail,
        SefazApiKey = t.SefazApiKey,
        SefazEnvironment = t.SefazEnvironment,
        CreatedAt = t.CreatedAt,
        IsActive = t.IsActive
    };
}
