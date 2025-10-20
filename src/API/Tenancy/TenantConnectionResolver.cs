using BuildingBlocks.Application.Abstractions.Tenancy;

namespace API.Tenancy;

/// <summary>
/// Simple resolver that:
/// - first tries to find Tenants:{tenantId}:ConnectionString in config
/// - then falls back to a per-tenant sqlite file in ./data/tenant-{tenantId}.db
/// - if tenantId is null or not found, returns default connection string (ConnectionStrings:Default)
/// </summary>
public sealed class TenantConnectionResolver : ITenantConnectionResolver
{
    private readonly IConfiguration _cfg;
    private readonly string _defaultConnectionString;

    public TenantConnectionResolver(IConfiguration cfg)
    {
        _cfg = cfg;
        _defaultConnectionString = _cfg.GetConnectionString("Default") ?? "Data Source=app.db";
    }

    public string GetConnectionString(Guid? tenantId)
    {
        if (tenantId == null) return _defaultConnectionString;

        // look for explicit connection string in config: "Tenants": { "<guid>": { "ConnectionString": "..." } }
        var explicitConn = _cfg.GetSection($"Tenants:{tenantId}:ConnectionString").Value;
        if (!string.IsNullOrWhiteSpace(explicitConn)) return explicitConn;

        // fallback: per-tenant sqlite file under ./data/
        var dataDir = _cfg.GetValue<string>("TenantDataFolder") ?? "data";
        Directory.CreateDirectory(dataDir);
        var file = Path.Combine(dataDir, $"tenant-{tenantId}.db");
        return $"Data Source={file}";
    }
}
