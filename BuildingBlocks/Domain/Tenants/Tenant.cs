using BuildingBlocks.Domain.Base;

namespace BuildingBlocks.Domain.Tenants;

/// <summary>
/// Represents an onboarded tenant within the B2B PPP platform.
/// </summary>
public sealed class Tenant : AggregateRoot
{
    private Tenant() { }

    public string Name { get; private set; } = default!;
    public string? Cnpj { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? ConnectionString { get; private set; }
    public string? SefazApiKey { get; private set; }
    public string? SefazEnvironment { get; private set; } // e.g., "production", "homologation"
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static Tenant Create(
        string name,
        string? cnpj = null,
        string? contactEmail = null,
        string? sefazApiKey = null,
        string? sefazEnvironment = "production")
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        var tenant = new Tenant
        {
            Name = name.Trim(),
            Cnpj = cnpj?.Trim(),
            ContactEmail = contactEmail?.Trim(),
            SefazApiKey = sefazApiKey,
            SefazEnvironment = sefazEnvironment
        };
        return tenant;
    }

    public void UpdateConnectionString(string connectionString)
    {
        Guard.AgainstNullOrWhiteSpace(connectionString, nameof(connectionString));
        ConnectionString = connectionString;
    }

    public void Deactivate() => IsActive = false;
}
