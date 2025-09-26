namespace API.Contracts.Companies;

public sealed class RefreshSefazRequest
{
    /// <summary>
    /// Two-letter UF (e.g., "SP").
    /// </summary>
    public string Uf { get; set; } = default!;
}
