namespace API.Contracts.Companies;

public sealed class AddOrUpdateStateRegistrationRequest
{
    public string Uf { get; set; } = default!;
    public string Ie { get; set; } = default!;
    public string? Status { get; set; }
    public string? RegimeTributario { get; set; }
    public DateTimeOffset? LastCheckedAt { get; set; }
}
