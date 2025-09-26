using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Infrastructure.Persistence;

namespace API.Health;

/// <summary>
/// Checks the Outbox for failed or stale-locked messages.
/// Healthy  : no failed, no stale locked.
/// Degraded : stale locked messages exist (lock older than 10 min).
/// Unhealthy: failed messages exist.
/// </summary>
public sealed class OutboxHealthCheck : IHealthCheck
{
    private readonly AppDbContext _db;

    public OutboxHealthCheck(AppDbContext db) => _db = db;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        var staleThreshold = DateTimeOffset.UtcNow - TimeSpan.FromMinutes(10);

        var failed = await _db.OutboxMessages
            .CountAsync(x => x.Error != null && x.ProcessedOnUtc == null, ct);

        var staleLocked = await _db.OutboxMessages
            .CountAsync(x => x.ProcessedOnUtc == null && x.LockedAtUtc != null && x.LockedAtUtc < staleThreshold, ct);

        if (failed > 0)
            return HealthCheckResult.Unhealthy($"Outbox has {failed} failed messages.");

        if (staleLocked > 0)
            return HealthCheckResult.Degraded($"Outbox has {staleLocked} stale locked messages.");

        return HealthCheckResult.Healthy("Outbox OK.");
    }
}
