using BuildingBlocks.Application.Abstractions.Tenancy.Models;

namespace BuildingBlocks.Application.Abstractions.Tenancy;

public interface ITenantRegistryService
{
    Task<TenantDto> RegisterTenantAsync(
        string name,
        string? cnpj,
        string? contactEmail,
        string? sefazApiKey,
        string? sefazEnvironment,
        CancellationToken ct = default);

    Task<IEnumerable<TenantDto>> ListTenantsAsync(CancellationToken ct = default);
    Task<TenantDto?> GetTenantAsync(Guid id, CancellationToken ct = default);
}
