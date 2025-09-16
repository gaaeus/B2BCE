using System.Text.RegularExpressions;
using BuildingBlocks.Domain.Base;

namespace BuildingBlocks.Domain.Companies.ValueObjects;

/// <summary>
/// Immutable email value object with basic RFC-ish validation.
/// </summary>
public sealed class EmailAddress : ValueObject
{
    private static readonly Regex Pattern =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private EmailAddress(string value) => Value = value;

    public static EmailAddress Create(string email)
    {
        Guard.AgainstNullOrWhiteSpace(email, nameof(email));
        if (!Pattern.IsMatch(email)) throw new ArgumentException("Invalid email format.", nameof(email));
        return new EmailAddress(email.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }

    public override string ToString() => Value;
}
