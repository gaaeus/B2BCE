namespace API.Contracts.Companies;

public sealed record CompanyResponse(Guid Id, string LegalName, string TaxId, string? Email);
