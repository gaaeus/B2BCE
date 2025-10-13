namespace BuildingBlocks.Application.Abstractions.Sefaz;

public interface ISefazClient
{
    Task<SefazResult> QueryAsync(string cnpj, string uf, string ie, CancellationToken ct = default);
}

public sealed record SefazResult(
    string Status,
    string RegimeTributario,
    DateTimeOffset CheckedAtUtc
);
