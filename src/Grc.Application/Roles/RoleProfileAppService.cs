using System.Linq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;
using Grc.Application.Contracts.Roles;
using Grc.Domain.Shared.Roles;
using Grc.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace Grc.Application.Roles;

/// <summary>
/// Application service for managing GRC role profiles
/// </summary>
[Authorize(GrcPermissions.Admin.Roles)]
public class RoleProfileAppService : ApplicationService, IRoleProfileAppService
{
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IPermissionGrantRepository _permissionGrantRepository;
    private readonly IdentityUserManager _userManager;
    private readonly IdentityRoleManager _roleManager;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ILogger<RoleProfileAppService> _logger;

    public RoleProfileAppService(
        IIdentityRoleRepository roleRepository,
        IPermissionGrantRepository permissionGrantRepository,
        IdentityUserManager userManager,
        IdentityRoleManager roleManager,
        IRepository<IdentityUser, Guid> userRepository,
        IGuidGenerator guidGenerator,
        ILogger<RoleProfileAppService> logger)
    {
        _roleRepository = roleRepository;
        _permissionGrantRepository = permissionGrantRepository;
        _userManager = userManager;
        _roleManager = roleManager;
        _userRepository = userRepository;
        _guidGenerator = guidGenerator;
        _logger = logger;
    }

    public async Task<List<RoleProfileDto>> GetAllProfilesAsync()
    {
        _logger.LogInformation("Retrieving all role profiles");
        
        var roles = GrcRoleDefinitions.GetAllRoles();
        var result = new List<RoleProfileDto>();

        foreach (var roleDef in roles)
        {
            var existingRole = await _roleRepository.FindByNormalizedNameAsync(roleDef.Name.ToUpperInvariant());
            var userCount = existingRole != null ? await GetUserCountForRoleAsync(existingRole.Id) : 0;
            var permissions = existingRole != null 
                ? await GetRolePermissionsAsync(roleDef.Name) 
                : roleDef.Permissions.ToList();

            result.Add(new RoleProfileDto
            {
                Id = existingRole?.Id ?? Guid.Empty,
                Name = roleDef.Name,
                DisplayName = roleDef.DisplayName,
                Description = roleDef.Description,
                Permissions = permissions,
                SLA = roleDef.SLA,
                UserCount = userCount,
                IsActive = existingRole != null,
                IsPublic = existingRole?.IsPublic ?? true,
                IsDefault = existingRole?.IsDefault ?? false
            });
        }

        return result;
    }

    public async Task<RoleProfileDto?> GetProfileAsync(string roleName)
    {
        var roleDef = GrcRoleDefinitions.GetAllRoles()
            .FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));

        if (roleDef == null)
        {
            return null;
        }

        var existingRole = await _roleRepository.FindByNormalizedNameAsync(roleName.ToUpperInvariant());
        var userCount = existingRole != null ? await GetUserCountForRoleAsync(existingRole.Id) : 0;
        var permissions = existingRole != null 
            ? await GetRolePermissionsAsync(roleName) 
            : roleDef.Permissions.ToList();

        return new RoleProfileDto
        {
            Id = existingRole?.Id ?? Guid.Empty,
            Name = roleDef.Name,
            DisplayName = roleDef.DisplayName,
            Description = roleDef.Description,
            Permissions = permissions,
            SLA = roleDef.SLA,
            UserCount = userCount,
            IsActive = existingRole != null,
            IsPublic = existingRole?.IsPublic ?? true,
            IsDefault = existingRole?.IsDefault ?? false
        };
    }

    public async Task<List<RoleProfileDto>> GetAvailableProfilesAsync()
    {
        return await GetAllProfilesAsync();
    }

    public async Task<int> GetUserCountAsync(string roleName)
    {
        var role = await _roleRepository.FindByNormalizedNameAsync(roleName.ToUpperInvariant());
        if (role == null) return 0;
        return await GetUserCountForRoleAsync(role.Id);
    }

    public async Task<List<string>> GetRolePermissionsAsync(string roleName)
    {
        try
        {
            var permissions = await _permissionGrantRepository.GetListAsync(
                RolePermissionValueProvider.ProviderName,
                roleName
            );
            
            return permissions
                .Where(p => p.Name.StartsWith("Grc."))
                .Select(p => p.Name)
                .OrderBy(p => p)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting permissions for role {RoleName}", roleName);
            var roleDef = GrcRoleDefinitions.GetAllRoles()
                .FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
            return roleDef?.Permissions.ToList() ?? new List<string>();
        }
    }

    public async Task<List<RoleProfileSummaryDto>> GetProfileSummariesAsync()
    {
        var profiles = await GetAllProfilesAsync();
        return profiles.Select(p => new RoleProfileSummaryDto
        {
            Id = p.Id,
            Name = p.Name,
            DisplayName = p.DisplayName,
            Description = p.Description,
            PermissionCount = p.Permissions.Count,
            UserCount = p.UserCount,
            IsPublic = p.IsPublic,
            IsDefault = p.IsDefault
        }).ToList();
    }

    public async Task<RoleProfileDto> CreateRoleFromProfileAsync(CreateRoleFromProfileDto input)
    {
        _logger.LogInformation("Creating role from profile: {ProfileName}", input.ProfileName);

        var roleDef = GrcRoleDefinitions.GetAllRoles()
            .FirstOrDefault(r => r.Name.Equals(input.ProfileName, StringComparison.OrdinalIgnoreCase));

        if (roleDef == null)
        {
            throw new Volo.Abp.BusinessException(
                code: "Grc:ProfileNotFound",
                message: $"Role profile '{input.ProfileName}' not found"
            );
        }

        var existingRole = await _roleRepository.FindByNormalizedNameAsync(roleDef.Name.ToUpperInvariant());
        if (existingRole != null)
        {
            throw new Volo.Abp.BusinessException(
                code: "Grc:RoleAlreadyExists",
                message: $"Role '{roleDef.Name}' already exists"
            );
        }

        var role = new IdentityRole(_guidGenerator.Create(), roleDef.Name, CurrentTenant.Id)
        {
            IsPublic = true,
            IsDefault = roleDef.Name == GrcRoleDefinitions.Viewer.Name
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            throw new Volo.Abp.BusinessException(
                code: "Grc:RoleCreationFailed",
                message: string.Join(", ", result.Errors.Select(e => e.Description))
            );
        }

        // Grant permissions
        foreach (var permission in roleDef.Permissions.Where(p => p != "Grc.*"))
        {
            var grant = new PermissionGrant(
                _guidGenerator.Create(),
                permission,
                RolePermissionValueProvider.ProviderName,
                roleDef.Name,
                CurrentTenant.Id
            );
            await _permissionGrantRepository.InsertAsync(grant);
        }

        return await GetProfileAsync(roleDef.Name) ?? throw new InvalidOperationException("Role creation failed");
    }

    private async Task<int> GetUserCountForRoleAsync(Guid roleId)
    {
        try
        {
            var role = await _roleRepository.GetAsync(roleId);
            var users = await _userManager.GetUsersInRoleAsync(role.Name!);
            return users.Count;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting user count for role {RoleId}", roleId);
            return 0;
        }
    }
}
