using Volo.Abp.Application.Services;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace Grc.Application.Policy;

public abstract class BasePolicyAppService : ApplicationService
{
    protected readonly IPolicyEnforcer PolicyEnforcer;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentTenant _currentTenant;
    protected readonly IEnvironmentProvider? EnvironmentProvider;
    protected readonly IRoleResolver? RoleResolver;

    protected BasePolicyAppService(
        IPolicyEnforcer policyEnforcer,
        ICurrentUser currentUser,
        ICurrentTenant currentTenant,
        IEnvironmentProvider? environmentProvider = null,
        IRoleResolver? roleResolver = null)
    {
        PolicyEnforcer = policyEnforcer;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
        EnvironmentProvider = environmentProvider;
        RoleResolver = roleResolver;
    }

    protected async Task EnforceAsync(string action, string resourceType, object resource, string? environment = null)
    {
        var context = new PolicyContext
        {
            Action = action,
            ResourceType = resourceType,
            Resource = resource,
            Environment = environment ?? GetEnvironment(),
            TenantId = _currentTenant.Id,
            PrincipalId = _currentUser.Id?.ToString(),
            PrincipalRoles = await GetCurrentRolesAsync()
        };

        await PolicyEnforcer.EnforceAsync(context);
    }

    protected virtual string GetEnvironment()
    {
        // Try to get from injected service, fallback to environment variable
        return EnvironmentProvider?.GetEnvironment() 
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLowerInvariant() 
            ?? "dev";
    }

    protected virtual Task<IReadOnlyList<string>> GetCurrentRolesAsync()
    {
        return RoleResolver?.GetCurrentRolesAsync() 
            ?? Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
    }
}
