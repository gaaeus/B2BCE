using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace API.Security;

/// <summary>
/// Authorizes requests that include an X-Admin-Key header matching configuration "Admin:Key".
/// This is intentionally simple for dev/testing; replace with proper identity provider for prod.
/// </summary>
public sealed class AdminKeyHandler : AuthorizationHandler<AdminKeyRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string? _adminKey;

    public const string HeaderName = "X-Admin-Key";

    public AdminKeyHandler(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _adminKey = configuration["Admin:Key"] ?? System.Environment.GetEnvironmentVariable("ADMIN_KEY");
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminKeyRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null) return Task.CompletedTask;

        if (string.IsNullOrEmpty(_adminKey))
        {
            // If no admin key configured, deny for safety.
            return Task.CompletedTask;
        }

        if (httpContext.Request.Headers.TryGetValue(HeaderName, out var values))
        {
            var provided = values.ToString();
            if (!string.IsNullOrWhiteSpace(provided) && provided == _adminKey)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
