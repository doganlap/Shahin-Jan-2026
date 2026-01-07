using Microsoft.AspNetCore.Authorization;

namespace GrcMvc.Authorization;

/// <summary>
/// Authorization handler that checks if user has the required permission claim.
/// Supports multiple claim types: "permission", "permissions", and role-based fallback.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            _logger.LogDebug("Permission check failed: user not authenticated. Permission={Permission}", requirement.Permission);
            return Task.CompletedTask;
        }

        // Check for permission claim (single or multiple claim types)
        var hasPermission = context.User.Claims.Any(c =>
            (c.Type == "permission" || c.Type == "permissions" || c.Type == "scope") &&
            c.Value.Equals(requirement.Permission, StringComparison.OrdinalIgnoreCase));

        // Fallback: Admin role has all permissions
        if (!hasPermission && context.User.IsInRole("Admin"))
        {
            hasPermission = true;
            _logger.LogDebug("Permission granted via Admin role. Permission={Permission}", requirement.Permission);
        }

        // Fallback: Owner role has all permissions
        if (!hasPermission && context.User.IsInRole("Owner"))
        {
            hasPermission = true;
            _logger.LogDebug("Permission granted via Owner role. Permission={Permission}", requirement.Permission);
        }

        if (hasPermission)
        {
            context.Succeed(requirement);
            _logger.LogDebug("Permission check passed. Permission={Permission}, User={User}",
                requirement.Permission, context.User.Identity.Name);
        }
        else
        {
            _logger.LogDebug("Permission check failed. Permission={Permission}, User={User}",
                requirement.Permission, context.User.Identity.Name);
        }

        return Task.CompletedTask;
    }
}
