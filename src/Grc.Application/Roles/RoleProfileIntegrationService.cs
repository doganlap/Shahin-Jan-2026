using System.Linq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;
using Grc.Application.Contracts.Roles;
using Grc.Domain.Shared.Roles;
using Grc.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace Grc.Application.Roles;

/// <summary>
/// Integration service for role profiles with other GRC modules
/// </summary>
[Authorize]
public class RoleProfileIntegrationService : ApplicationService, IRoleProfileIntegrationService
{
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IPermissionGrantRepository _permissionGrantRepository;
    private readonly ILogger<RoleProfileIntegrationService> _logger;

    public RoleProfileIntegrationService(
        IIdentityRoleRepository roleRepository,
        IPermissionGrantRepository permissionGrantRepository,
        ILogger<RoleProfileIntegrationService> logger)
    {
        _roleRepository = roleRepository;
        _permissionGrantRepository = permissionGrantRepository;
        _logger = logger;
    }

    public async Task<bool> ValidateRoleForModuleAsync(string roleName, string moduleName)
    {
        try
        {
            var permissions = await _permissionGrantRepository.GetListAsync(
                RolePermissionValueProvider.ProviderName,
                roleName
            );

            var modulePrefix = $"Grc.{moduleName}";
            var hasViewPermission = permissions.Any(p => p.Name.StartsWith(modulePrefix));
            
            return hasViewPermission;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error validating role {RoleName} for module {ModuleName}", roleName, moduleName);
            return false;
        }
    }

    public async Task<List<RoleProfileDto>> GetRecommendedProfilesForModuleAsync(string moduleName)
    {
        var modulePrefix = $"Grc.{moduleName}";
        var allRoles = GrcRoleDefinitions.GetAllRoles();
        var result = new List<RoleProfileDto>();

        foreach (var roleDef in allRoles)
        {
            // Check if this role has permissions for the module
            var hasModulePermissions = roleDef.Permissions.Any(p => 
                p.StartsWith(modulePrefix) || p == "Grc.*");

            if (hasModulePermissions)
            {
                var existingRole = await _roleRepository.FindByNormalizedNameAsync(roleDef.Name.ToUpperInvariant());
                
                result.Add(new RoleProfileDto
                {
                    Id = existingRole?.Id ?? Guid.Empty,
                    Name = roleDef.Name,
                    DisplayName = roleDef.DisplayName,
                    Description = roleDef.Description,
                    Permissions = roleDef.Permissions.Where(p => p.StartsWith(modulePrefix) || p == "Grc.*").ToList(),
                    SLA = roleDef.SLA,
                    UserCount = 0,
                    IsActive = existingRole != null,
                    IsPublic = existingRole?.IsPublic ?? true,
                    IsDefault = existingRole?.IsDefault ?? false
                });
            }
        }

        return result;
    }

    public async Task<bool> CanPerformActionAsync(string roleName, string permission)
    {
        try
        {
            var permissions = await _permissionGrantRepository.GetListAsync(
                RolePermissionValueProvider.ProviderName,
                roleName
            );

            return permissions.Any(p => p.Name == permission);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking if role {RoleName} can perform action {Permission}", roleName, permission);
            return false;
        }
    }
}
