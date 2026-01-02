using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;
using Grc.Domain.Shared.Roles;

namespace Grc.Domain.Seed;

/// <summary>
/// Seeds predefined GRC role profiles with their permissions and SLA definitions
/// </summary>
public class GrcRoleDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IPermissionGrantRepository _permissionGrantRepository;
    private readonly IPermissionDefinitionManager _permissionDefinitionManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly IdentityRoleManager _roleManager;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ILogger<GrcRoleDataSeedContributor> _logger;

    public GrcRoleDataSeedContributor(
        IIdentityRoleRepository roleRepository,
        IPermissionGrantRepository permissionGrantRepository,
        IPermissionDefinitionManager permissionDefinitionManager,
        ICurrentTenant currentTenant,
        IdentityRoleManager roleManager,
        IUnitOfWorkManager unitOfWorkManager,
        IGuidGenerator guidGenerator,
        ILogger<GrcRoleDataSeedContributor> logger)
    {
        _roleRepository = roleRepository;
        _permissionGrantRepository = permissionGrantRepository;
        _permissionDefinitionManager = permissionDefinitionManager;
        _currentTenant = currentTenant;
        _roleManager = roleManager;
        _unitOfWorkManager = unitOfWorkManager;
        _guidGenerator = guidGenerator;
        _logger = logger;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        using (_currentTenant.Change(context.TenantId))
        using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false))
        {
            try
            {
                _logger.LogInformation("Starting GRC role profiles seeding for tenant: {TenantId}", context.TenantId);
                
                var roles = GrcRoleDefinitions.GetAllRoles();
                var createdCount = 0;
                var updatedCount = 0;

                foreach (var roleDef in roles)
                {
                    try
                    {
                        var existingRole = await _roleRepository.FindByNormalizedNameAsync(
                            roleDef.Name.ToUpperInvariant()
                        );

                        if (existingRole == null)
                        {
                            // Create new role
                            var role = new IdentityRole(
                                _guidGenerator.Create(),
                                roleDef.Name,
                                context.TenantId
                            )
                            {
                                IsPublic = true,
                                IsDefault = roleDef.Name == GrcRoleDefinitions.Viewer.Name
                            };

                            var createResult = await _roleManager.CreateAsync(role);
                            if (!createResult.Succeeded)
                            {
                                _logger.LogWarning("Failed to create role {RoleName}: {Errors}", 
                                    roleDef.Name, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                                continue;
                            }

                            // Grant permissions
                            await GrantPermissionsToRoleAsync(roleDef.Name, roleDef.Permissions, context.TenantId);
                            createdCount++;
                            
                            _logger.LogInformation("Created role: {RoleName} ({DisplayName})", 
                                roleDef.Name, roleDef.DisplayName);
                        }
                        else
                        {
                            // Update existing role permissions
                            _logger.LogInformation("Updating permissions for existing role: {RoleName}", roleDef.Name);
                            
                            // Remove all existing GRC permissions first
                            await RemoveExistingGrcPermissionsAsync(roleDef.Name, context.TenantId);

                            // Grant new permissions
                            await GrantPermissionsToRoleAsync(roleDef.Name, roleDef.Permissions, context.TenantId);
                            updatedCount++;
                            
                            _logger.LogInformation("Updated permissions for role: {RoleName}", roleDef.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error seeding role {RoleName}", roleDef.Name);
                        // Continue with next role
                    }
                }

                await uow.CompleteAsync();
                
                _logger.LogInformation("GRC role seeding completed. Created: {Created}, Updated: {Updated}", 
                    createdCount, updatedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GRC role seeding");
                throw;
            }
        }
    }

    private async Task RemoveExistingGrcPermissionsAsync(string roleName, Guid? tenantId)
    {
        try
        {
            var existingGrants = await _permissionGrantRepository.GetListAsync(
                RolePermissionValueProvider.ProviderName,
                roleName
            );

            var grcGrants = existingGrants
                .Where(g => g.Name.StartsWith("Grc."))
                .ToList();

            foreach (var grant in grcGrants)
            {
                await _permissionGrantRepository.DeleteAsync(grant);
            }

            _logger.LogInformation("Removed {Count} existing GRC permissions for role {RoleName}", 
                grcGrants.Count, roleName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not remove existing GRC permissions for role {RoleName}", roleName);
            // Continue - permissions might not exist yet
        }
    }

    private async Task GrantPermissionsToRoleAsync(string roleName, string[] permissions, Guid? tenantId)
    {
        var grantedCount = 0;
        var failedPermissions = new List<string>();

        foreach (var permission in permissions)
        {
            try
            {
                if (permission == "Grc.*")
                {
                    // Grant all GRC permissions using the permission groups
                    var groups = await _permissionDefinitionManager.GetGroupsAsync();
                    var grcGroup = groups.FirstOrDefault(g => g.Name == "Grc");
                    
                    if (grcGroup != null)
                    {
                        foreach (var perm in grcGroup.GetPermissionsWithChildren())
                        {
                            await GrantPermissionAsync(roleName, perm.Name, tenantId);
                            grantedCount++;
                        }
                    }
                }
                else
                {
                    // Verify permission exists before granting
                    var permissionDef = await _permissionDefinitionManager.GetOrNullAsync(permission);
                    
                    if (permissionDef != null)
                    {
                        await GrantPermissionAsync(roleName, permission, tenantId);
                        grantedCount++;
                    }
                    else
                    {
                        _logger.LogWarning("Permission {Permission} does not exist, skipping for role {RoleName}", 
                            permission, roleName);
                        failedPermissions.Add(permission);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to grant permission {Permission} to role {RoleName}", 
                    permission, roleName);
                failedPermissions.Add(permission);
            }
        }

        _logger.LogInformation("Granted {Count} permissions to role {RoleName}. Failed: {FailedCount}", 
            grantedCount, roleName, failedPermissions.Count);
        
        if (failedPermissions.Any())
        {
            _logger.LogWarning("Failed permissions for role {RoleName}: {Permissions}", 
                roleName, string.Join(", ", failedPermissions));
        }
    }

    private async Task GrantPermissionAsync(string roleName, string permissionName, Guid? tenantId)
    {
        // Check if permission grant already exists
        var existing = await _permissionGrantRepository.FindAsync(
            permissionName,
            RolePermissionValueProvider.ProviderName,
            roleName
        );

        if (existing == null)
        {
            var grant = new PermissionGrant(
                _guidGenerator.Create(),
                permissionName,
                RolePermissionValueProvider.ProviderName,
                roleName,
                tenantId
            );

            await _permissionGrantRepository.InsertAsync(grant);
        }
    }
}
