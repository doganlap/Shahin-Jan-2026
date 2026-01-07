using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace GrcMvc.Authorization;

/// <summary>
/// Dynamic authorization policy provider that creates policies on-demand for permission-based authorization.
/// Handles [Authorize("Grc.*")] attributes without requiring manual policy registration for each permission.
/// </summary>
public sealed class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    private readonly ConcurrentDictionary<string, AuthorizationPolicy> _cache = new();

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Only handle permission-style policies (Grc.* pattern); otherwise fall back to default
        if (!policyName.StartsWith("Grc.", StringComparison.OrdinalIgnoreCase))
        {
            return await base.GetPolicyAsync(policyName);
        }

        // Create and cache the policy for this permission
        var policy = _cache.GetOrAdd(policyName, name =>
            new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(name))
                .Build());

        return policy;
    }
}
