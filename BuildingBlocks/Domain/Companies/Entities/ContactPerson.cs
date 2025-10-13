namespace BuildingBlocks.Domain.Companies.Entities;

/// <summary>
/// Represents a contact person associated with a company.
/// </summary>
public class ContactPerson
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// The role of the contact person (e.g., "Billing", "Technical", "Primary").
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Foreign key to the parent Company entity.
    /// </summary>
    public Guid CompanyId { get; set; }
}

