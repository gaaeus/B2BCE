using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BuildingBlocks.Infrastructure.Persistence;
using API.Contracts.Admin;

namespace API.Controllers.Admin;

[ApiController]
[Route("api/admin/outbox")]
[Authorize(Policy = "Admin")]
public sealed class OutboxAdminController : ControllerBase
{
    private readonly AppDbContext _db;

    public OutboxAdminController(AppDbContext db) => _db = db;

    /// <summary>Get pending (unprocessed) outbox messages.</summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending([FromQuery] int limit = 50)
    {
        var items = await _db.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.OccurredOnUtc)
            .Take(limit)
            .Select(x => new OutboxMessageDto(
                x.Id, x.Type, x.OccurredOnUtc, x.ProcessedOnUtc, x.LockedAtUtc, x.LockedBy, x.Error))
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>Get failed outbox messages (error populated).</summary>
    [HttpGet("failed")]
    public async Task<IActionResult> GetFailed([FromQuery] int limit = 50)
    {
        var items = await _db.OutboxMessages
            .Where(x => x.Error != null && x.ProcessedOnUtc == null)
            .OrderByDescending(x => x.OccurredOnUtc)
            .Take(limit)
            .Select(x => new OutboxMessageDto(
                x.Id, x.Type, x.OccurredOnUtc, x.ProcessedOnUtc, x.LockedAtUtc, x.LockedBy, x.Error))
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>Retry a message: clears lock/error so dispatcher can pick it up again.</summary>
    [HttpPost("{id:guid}/retry")]
    public async Task<IActionResult> Retry(Guid id)
    {
        var msg = await _db.OutboxMessages.FirstOrDefaultAsync(x => x.Id == id);
        if (msg is null) return NotFound();

        msg.MarkProcessing(null); // will set LockedAtUtc = now and LockedBy = null (we'll immediately release)
        msg.MarkFailed(null); // clear error / processed - we'll reset fields manually below
        // release lock and clear error so dispatcher can pick it again
        // directly set properties via reflection-friendly methods on entity if available
        // Here we use EF property access since OutboxMessage methods encapsulate state transitions.

        // Ensure `msg` is not null before passing to `_db.Entry` to satisfy the nullability constraint.
        if (msg != null)
        {
            _db.Entry(msg).Property("LockedAtUtc").CurrentValue = null;
            _db.Entry(msg).Property("LockedBy").CurrentValue = null;
            _db.Entry(msg).Property("Error").CurrentValue = null;
        }

        await _db.SaveChangesAsync();

        return NoContent();
    }
}
