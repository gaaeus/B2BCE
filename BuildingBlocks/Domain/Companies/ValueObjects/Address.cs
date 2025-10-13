using System;

namespace BuildingBlocks.Domain.Companies.ValueObjects
{
    /// <summary>
    /// Represents a structured postal address (Value Object).
    /// </summary>
    public sealed class Address : IEquatable<Address>
    {
        public string Street { get; private set; } = string.Empty;
        public string Number { get; private set; } = string.Empty;
        public string? Complement { get; private set; }
        public string District { get; private set; } = string.Empty;
        public string City { get; private set; } = string.Empty;
        public string State { get; private set; } = string.Empty; // UF
        public string ZipCode { get; private set; } = string.Empty; // CEP
        public string Country { get; private set; } = "BR";

        private Address() { } // EF Core constructor

        public Address(string street, string number, string district, string city, string state, string zipCode, string? complement = null)
        {
            Street = street;
            Number = number;
            Complement = complement;
            District = district;
            City = city;
            State = state.ToUpperInvariant();
            ZipCode = zipCode;
        }

        public bool Equals(Address? other)
        {
            if (other is null) return false;
            return Street == other.Street &&
                   Number == other.Number &&
                   Complement == other.Complement &&
                   District == other.District &&
                   City == other.City &&
                   State == other.State &&
                   ZipCode == other.ZipCode &&
                   Country == other.Country;
        }

        public override bool Equals(object? obj) => Equals(obj as Address);
        public override int GetHashCode() => HashCode.Combine(Street, Number, District, City, State, ZipCode, Country);
    }
}
