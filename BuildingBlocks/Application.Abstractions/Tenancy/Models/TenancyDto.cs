namespace BuildingBlocks.Application.Abstractions.Tenancy.Models;

public sealed class TenantDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string? Cnpj { get; init; }
    public string? ContactEmail { get; init; }
    public string? SefazApiKey { get; init; }
    public string? SefazEnvironment { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
