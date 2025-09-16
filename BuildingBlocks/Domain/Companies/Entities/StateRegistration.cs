using BuildingBlocks.Domain.Base;

namespace BuildingBlocks.Domain.Companies.Entities;

/// <summary>
/// State (UF) registration used for SEFAZ queries.
/// </summary>
public sealed class StateRegistration : Entity
{
    private StateRegistration() { } // EF

    public string Uf { get; private set; } = default!;   // e.g., "SP"
    public string Ie { get; private set; } = default!;   // state-level inscription
    public string? Status { get; private set; }
    public string? RegimeTributario { get; private set; }
    public DateTimeOffset? LastCheckedAt { get; private set; }

    private static string NormalizeUf(string uf)
    {
        Guard.AgainstNullOrWhiteSpace(uf, nameof(uf));
        uf = uf.Trim().ToUpperInvariant();
        if (uf.Length != 2) throw new ArgumentException("UF must be two letters.", nameof(uf));
        return uf;
    }

    public static StateRegistration Create(string uf, string ie)
    {
        Guard.AgainstNullOrWhiteSpace(ie, nameof(ie));
        return new StateRegistration { Uf = NormalizeUf(uf), Ie = ie.Trim() };
    }

    public void Update(string? ie = null, string? status = null, string? regime = null, DateTimeOffset? lastCheckedAt = null)
    {
        if (!string.IsNullOrWhiteSpace(ie)) Ie = ie.Trim();
        if (status != null) Status = status;
        if (regime != null) RegimeTributario = regime;
        if (lastCheckedAt != null) LastCheckedAt = lastCheckedAt;
    }
}
