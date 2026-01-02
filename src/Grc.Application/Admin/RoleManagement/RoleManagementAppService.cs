using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.PermissionManagement;
using Grc.Application.Contracts.Admin.RoleManagement;
using Grc.Application.Contracts.Admin.UserManagement;
using Grc.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Grc.Application.Admin.RoleManagement;

[Authorize(GrcPermissions.Admin.Roles)]
public class RoleManagementAppService : CrudAppService<
    IdentityRole,
    RoleDto,
    Guid,
    RoleListInputDto,
    CreateRoleDto,
    UpdateRoleDto>, IRoleManagementAppService
{
    private readonly IdentityRoleManager _roleManager;
    private readonly IPermissionGrantRepository _permissionGrantRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IdentityUserManager _userManager;

    public RoleManagementAppService(
        IRepository<IdentityRole, Guid> repository,
        IdentityRoleManager roleManager,
        IPermissionGrantRepository permissionGrantRepository,
        IRepository<IdentityUser, Guid> userRepository,
        IdentityUserManager userManager)
        : base(repository)
    {
        _roleManager = roleManager;
        _permissionGrantRepository = permissionGrantRepository;
        _userRepository = userRepository;
        _userManager = userManager;
    }

    protected override async Task<IQueryable<IdentityRole>> CreateFilteredQueryAsync(RoleListInputDto input)
    {
        var query = await base.CreateFilteredQueryAsync(input);

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(r => r.Name!.Contains(input.Filter));
        }

        if (input.IsDefault.HasValue)
        {
            query = query.Where(r => r.IsDefault == input.IsDefault.Value);
        }

        if (input.IsPublic.HasValue)
        {
            query = query.Where(r => r.IsPublic == input.IsPublic.Value);
        }

        if (input.IsStatic.HasValue)
        {
            query = query.Where(r => r.IsStatic == input.IsStatic.Value);
        }

        return query;
    }

    public async Task<List<string>> GetPermissionsAsync(Guid roleId)
    {
        var role = await Repository.GetAsync(roleId);
        var permissions = await _permissionGrantRepository.GetListAsync(
            RolePermissionValueProvider.ProviderName,
            role.Name!
        );
        return permissions.Select(p => p.Name).ToList();
    }

    public async Task SetPermissionsAsync(Guid roleId, List<string> permissions)
    {
        var role = await Repository.GetAsync(roleId);
        var roleName = role.Name!;
        
        // Get existing permissions
        var existingGrants = await _permissionGrantRepository.GetListAsync(
            RolePermissionValueProvider.ProviderName,
            roleName
        );
        
        // Remove permissions not in the new list
        foreach (var grant in existingGrants)
        {
            if (!permissions.Contains(grant.Name))
            {
                await _permissionGrantRepository.DeleteAsync(grant);
            }
        }
        
        // Add new permissions
        var existingPermissionNames = existingGrants.Select(g => g.Name).ToHashSet();
        foreach (var permission in permissions)
        {
            if (!existingPermissionNames.Contains(permission))
            {
                var grant = new PermissionGrant(
                    GuidGenerator.Create(),
                    permission,
                    RolePermissionValueProvider.ProviderName,
                    roleName,
                    CurrentTenant.Id
                );
                await _permissionGrantRepository.InsertAsync(grant);
            }
        }
    }

    public async Task<List<UserDto>> GetUsersInRoleAsync(Guid roleId)
    {
        var role = await Repository.GetAsync(roleId);
        var users = await _userManager.GetUsersInRoleAsync(role.Name!);
        return ObjectMapper.Map<List<IdentityUser>, List<UserDto>>(users.ToList());
    }

    public override async Task<RoleDto> CreateAsync(CreateRoleDto input)
    {
        var role = new IdentityRole(GuidGenerator.Create(), input.Name, CurrentTenant.Id)
        {
            IsDefault = input.IsDefault,
            IsPublic = input.IsPublic
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            throw new Volo.Abp.BusinessException(
                code: "Grc:RoleCreationFailed",
                message: string.Join(", ", result.Errors.Select(e => e.Description))
            );
        }

        // Grant permissions if any
        if (input.Permissions != null && input.Permissions.Count > 0)
        {
            await SetPermissionsAsync(role.Id, input.Permissions);
        }

        return ObjectMapper.Map<IdentityRole, RoleDto>(role);
    }

    public override async Task<RoleDto> UpdateAsync(Guid id, UpdateRoleDto input)
    {
        var role = await Repository.GetAsync(id);
        
        role.IsDefault = input.IsDefault;
        role.IsPublic = input.IsPublic;

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            throw new Volo.Abp.BusinessException(
                code: "Grc:RoleUpdateFailed",
                message: string.Join(", ", result.Errors.Select(e => e.Description))
            );
        }

        // Update permissions if provided
        if (input.Permissions != null)
        {
            await SetPermissionsAsync(role.Id, input.Permissions);
        }

        return ObjectMapper.Map<IdentityRole, RoleDto>(role);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var role = await Repository.GetAsync(id);
        
        if (role.IsStatic)
        {
            throw new Volo.Abp.BusinessException(
                code: "Grc:CannotDeleteStaticRole",
                message: "Cannot delete static roles."
            );
        }

        // Delete permissions first
        var permissions = await _permissionGrantRepository.GetListAsync(
            RolePermissionValueProvider.ProviderName,
            role.Name!
        );
        
        foreach (var permission in permissions)
        {
            await _permissionGrantRepository.DeleteAsync(permission);
        }

        await _roleManager.DeleteAsync(role);
    }
}
