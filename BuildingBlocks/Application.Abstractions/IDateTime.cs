namespace BuildingBlocks.Application.Abstractions;

public interface IDateTime
{
    DateTime UtcNow { get; }
}
