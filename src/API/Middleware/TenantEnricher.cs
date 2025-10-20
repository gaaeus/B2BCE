using BuildingBlocks.Application.Abstractions.Tenancy;
using Serilog.Core;
using Serilog.Events;

namespace API.Middleware;

/// <summary>
/// Enriches log events with a property representing the current tenant identifier.
/// </summary>
/// <remarks>This enricher adds a property named "TenantId" to log events. The value of the property is determined
/// by the <see cref="ITenantProvider"/> provided during construction. If no tenant provider is available or the tenant
/// identifier is null, the value "none" is used.</remarks>
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
