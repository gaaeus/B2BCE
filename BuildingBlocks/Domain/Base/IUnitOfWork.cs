namespace BuildingBlocks.Domain.Base;

/// <summary>
/// Commit boundary for the persistence context.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
