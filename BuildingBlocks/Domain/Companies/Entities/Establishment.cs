using BuildingBlocks.Domain.Companies.ValueObjects;

namespace BuildingBlocks.Domain.Companies.Entities;

/// <summary>
/// Represents a physical or legal establishment (branch, headquarters, or warehouse)
/// registered under a Company (CNPJ raiz).
/// </summary>
public class Establishment
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Establishment name or trade name (nome fantasia).
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Full CNPJ of the establishment (14 digits).
    /// </summary>
    public string Cnpj { get; private set; } = string.Empty;

    /// <summary>
    /// State registration (Inscrição Estadual).
    /// </summary>
    public string? StateRegistration { get; private set; }

    /// <summary>
    /// Municipal registration (Inscrição Municipal), used for service providers.
    /// </summary>
    public string? MunicipalRegistration { get; private set; }

    /// <summary>
    /// Establishment address.
    /// </summary>
    public Address? Address { get; private set; }

    /// <summary>
    /// Reference to the parent Company aggregate.
    /// </summary>
    public Guid CompanyId { get;  }

    // EF Core navigation
    public Company? Company { get; }

    private Establishment() { } // EF constructor

    public Establishment(
        string name,
        string cnpj,
        string? stateRegistration,
        string? municipalRegistration,
        Address? address)
    {
        Id = Guid.NewGuid();
        Name = name;
        Cnpj = cnpj;
        StateRegistration = stateRegistration;
        MunicipalRegistration = municipalRegistration;
        Address = address;
    }

    public void UpdateAddress(Address newAddress)
    {
        Address = newAddress;
    }

    public void UpdateTradeName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Trade name cannot be empty.", nameof(newName));

        Name = newName;
    }
}

