using System.Linq.Expressions;

namespace BuildingBlocks.Domain.Base;

/// <summary>
/// Repository contract for aggregate roots.
/// </summary>
public interface IRepository<TAggregate> where TAggregate : AggregateRoot
{
    Task<TAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(TAggregate aggregate, CancellationToken ct = default);
    Task UpdateAsync(TAggregate aggregate, CancellationToken ct = default);
    Task<bool> ExistsAsync(Expression<Func<TAggregate, bool>> predicate, CancellationToken ct = default);
}
