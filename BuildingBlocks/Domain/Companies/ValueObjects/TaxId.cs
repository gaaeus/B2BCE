using BuildingBlocks.Domain.Base;
using System.Text.RegularExpressions;

namespace BuildingBlocks.Domain.Companies.ValueObjects;

/// <summary>
/// Generic TaxId VO. For Brazil (CNPJ), we currently validate length-only to stay light.
/// </summary>
public sealed class TaxId : ValueObject
{
    public string Value { get; }

    private TaxId(string value) => Value = value;

    public static TaxId Create(string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        var cleaned = new string(value.Where(char.IsDigit).ToArray());
        if (!IsValidCnpj(cleaned) ||!IsValidCnpjRegex(cleaned))
            throw new ArgumentException("TaxId must be valid: CPF(11) or CNPJ(14) digits.", nameof(value));
        return new TaxId(cleaned);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    /// <summary>Validates a CNPJ using default rules</summary>
    /// <param>CNPJ string to validate</param>
    private static bool IsValidCnpj(string cnpj)
    {
        // Remove non-numeric characters
        cnpj = new string(cnpj.Where(char.IsDigit).ToArray());

        // Check length
        if (cnpj.Length is not (11 or 14)) // CPF or CNPJ lengths
            return false;

        // Check for repeated sequences (e.g., "11111111111111")
        if (cnpj.Distinct().Count() == 1)
            return false;

        // Calculate first verification digit
        int[] multiplier1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int sum = 0;
        for (int i = 0; i < 12; i++)
            sum += (cnpj[i] - '0') * multiplier1[i];

        int remainder = sum % 11;
        int firstDigit = remainder < 2 ? 0 : 11 - remainder;

        // Check first verification digit
        if (cnpj[12] - '0' != firstDigit)
            return false;

        // Calculate second verification digit
        int[] multiplier2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        sum = 0;
        for (int i = 0; i < 13; i++)
            sum += (cnpj[i] - '0') * multiplier2[i];

        remainder = sum % 11;
        int secondDigit = remainder < 2 ? 0 : 11 - remainder;

        // Check second verification digit
        return cnpj[13] - '0' == secondDigit;
    }

    /// <summary>Validates a CNPJ using regex</summary>
    /// <param>CNPJ string to validate</param>
    private static bool IsValidCnpjRegex(string cnpj)
    {
        return (Regex.Match(cnpj, @"[0-9]{2}\.?[0-9]{3}\.?[0-9]{3}\/?[0-9]{4}\-?[0-9]{2}").Success);
    }
}
