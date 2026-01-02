namespace Grc.Application.Contracts.Roles;

/// <summary>
/// Service for integrating role profiles with other GRC services
/// </summary>
public interface IRoleProfileIntegrationService
{
    /// <summary>
    /// Validates if a role profile has all required permissions for a specific module
    /// </summary>
    Task<bool> ValidateRoleForModuleAsync(string roleName, string moduleName);
    
    /// <summary>
    /// Gets recommended role profiles for a specific module
    /// </summary>
    Task<List<RoleProfileDto>> GetRecommendedProfilesForModuleAsync(string moduleName);
    
    /// <summary>
    /// Checks if a role profile can perform a specific action
    /// </summary>
    Task<bool> CanPerformActionAsync(string roleName, string permission);
}
