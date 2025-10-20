using System;
using System.Collections.Generic;

namespace API.Tenancy;

/// <summary>
/// Validates tenants and stores known tenant ids.
/// In production, this would be backed by a database or API.
/// </summary>
public sealed class TenantRegistry
{
    private readonly HashSet<Guid> _knownTenants = new();

    public TenantRegistry()
    {
        // Example tenants for now
        _knownTenants.Add(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        _knownTenants.Add(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));
    }

    public bool IsValid(Guid tenantId) => _knownTenants.Contains(tenantId);
}
