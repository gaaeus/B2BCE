// useful for testability
namespace Application.Abstractions;

public interface IDateTime
{
    DateTime UtcNow { get; }
}
