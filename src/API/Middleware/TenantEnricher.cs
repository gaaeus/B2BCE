using BuildingBlocks.Application.Abstractions.Tenancy;
using Serilog.Core;
using Serilog.Events;

namespace API.Middleware;

public sealed class TenantEnricher : ILogEventEnricher
{
    private readonly ITenantProvider? _tenantProvider;

    public TenantEnricher() { }

    public TenantEnricher(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var tenant = _tenantProvider?.TenantId?.ToString() ?? "none";
        var property = propertyFactory.CreateProperty("TenantId", tenant);
        logEvent.AddPropertyIfAbsent(property);
    }
}
