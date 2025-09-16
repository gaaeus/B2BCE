namespace API.Contracts.Companies;

public sealed class RegisterCompanyRequest
{
    public string LegalName { get; set; } = default!;
    public string TaxId { get; set; } = default!;
    public string? Email { get; set; }
}
