using Microsoft.AspNetCore.Authorization;

namespace API.Security;

public sealed class AdminKeyRequirement : IAuthorizationRequirement
{
}


#warning TODO
/*
 
 7) Notes, safety & follow-ups

The admin auth is intentionally minimal. For production, replace this with:

JWT Bearer validation + role/claim checks, or

an internal auth gateway and network-level protections.

The Retry operation clears lock/error so the dispatcher will pick the message again. If you need more controlled retry semantics (increment retry count, avoid infinite loops), extend OutboxMessage with a RetryCount and a NextAttemptUtc.

Consider rate-limiting or RBAC for admin endpoints to avoid accidental mass replays.
 */