using API.Tenancy;
using BuildingBlocks.Application.Abstractions.Tenancy;

namespace API.Middleware;

/// <summary>
/// Simple tenant resolution middleware.
/// Reads tenant id from configured header (default "X-Tenant-Id") and sets it on ITenantProvider.
/// </summary>
public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;
    public const string DefaultHeader = "X-Tenant-Id";

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider, TenantRegistry registry)
    {
        if (context.Request.Headers.TryGetValue(DefaultHeader, out var values))
        {
            var raw = values.ToString();
            if (Guid.TryParse(raw, out var tenantId))
            {
                if (registry.IsValid(tenantId))
                {
                    tenantProvider.SetTenant(tenantId);
                    _logger.LogDebug("Tenant resolved and validated: {TenantId}", tenantId);
                }
                else
                {
                    _logger.LogWarning("Invalid tenant id {TenantId}", tenantId);
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Invalid tenant id.");
                    return;
                }
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Invalid X-Tenant-Id format.");
                return;
            }
        }
        else
        {
            _logger.LogWarning("Missing tenant header {Header}", DefaultHeader);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Missing X-Tenant-Id header.");
            return;
        }

        await _next(context);
    }

}
