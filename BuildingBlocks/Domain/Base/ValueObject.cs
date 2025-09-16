namespace BuildingBlocks.Domain.Base;

/// <summary>
/// Base for immutable value objects.
/// </summary>
public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj) =>
        obj is ValueObject other && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    public override int GetHashCode() =>
        GetEqualityComponents()
            .Aggregate(1, (current, obj) => HashCode.Combine(current, obj?.GetHashCode() ?? 0));
}
