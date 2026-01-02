using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;
using Volo.Abp.Domain.Repositories;
using Grc.Application.Contracts.Admin.UserManagement;
using Grc.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Grc.Application.Admin.UserManagement;

[Authorize(GrcPermissions.Admin.Users)]
public class UserManagementAppService : CrudAppService<
    IdentityUser,
    UserDto,
    Guid,
    UserListInputDto,
    CreateUserDto,
    UpdateUserDto>, IUserManagementAppService
{
    private readonly IdentityUserManager _userManager;
    private readonly IRepository<IdentityRole, Guid> _roleRepository;

    public UserManagementAppService(
        IRepository<IdentityUser, Guid> repository,
        IdentityUserManager userManager,
        IRepository<IdentityRole, Guid> roleRepository)
        : base(repository)
    {
        _userManager = userManager;
        _roleRepository = roleRepository;
        GetPolicyName = GrcPermissions.Admin.Users;
        GetListPolicyName = GrcPermissions.Admin.Users;
        CreatePolicyName = GrcPermissions.Admin.Users;
        UpdatePolicyName = GrcPermissions.Admin.Users;
        DeletePolicyName = GrcPermissions.Admin.Users;
    }

    protected override async Task<IQueryable<IdentityUser>> CreateFilteredQueryAsync(UserListInputDto input)
    {
        var query = await base.CreateFilteredQueryAsync(input);

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(u =>
                u.UserName!.Contains(input.Filter) ||
                u.Email!.Contains(input.Filter) ||
                u.Name!.Contains(input.Filter) ||
                u.Surname!.Contains(input.Filter));
        }

        if (input.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == input.IsActive.Value);
        }

        return query;
    }

    protected override UserDto MapToGetOutputDto(IdentityUser entity)
    {
        var dto = ObjectMapper.Map<IdentityUser, UserDto>(entity);
        dto.Roles = GetUserRolesAsync(entity.Id).Result;
        return dto;
    }

    protected override async Task<IdentityUser> MapToEntityAsync(CreateUserDto createInput)
    {
        var user = new IdentityUser(
            GuidGenerator.Create(),
            createInput.UserName,
            createInput.Email,
            CurrentTenant.Id
        )
        {
            Name = createInput.Name,
            Surname = createInput.Surname
        };

        user.SetPhoneNumber(createInput.PhoneNumber, false);
        user.SetIsActive(createInput.IsActive);

        return user;
    }

    protected override async Task MapToEntityAsync(UpdateUserDto updateInput, IdentityUser entity)
    {
        ObjectMapper.Map(updateInput, entity);
    }

    public override async Task<UserDto> CreateAsync(CreateUserDto input)
    {
        var user = await MapToEntityAsync(input);

        var result = await _userManager.CreateAsync(user, input.Password);
        result.CheckErrors();

        if (input.RoleNames != null && input.RoleNames.Any())
        {
            await _userManager.SetRolesAsync(user, input.RoleNames.ToArray());
        }

        return await GetAsync(user.Id);
    }

    public override async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto input)
    {
        var user = await _userManager.GetByIdAsync(id);

        await MapToEntityAsync(input, user);

        (await _userManager.UpdateAsync(user)).CheckErrors();

        if (input.RoleNames != null)
        {
            await _userManager.SetRolesAsync(user, input.RoleNames.ToArray());
        }

        return await GetAsync(id);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var user = await _userManager.GetByIdAsync(id);
        (await _userManager.DeleteAsync(user)).CheckErrors();
    }

    public async Task ResetPasswordAsync(Guid id, string newPassword)
    {
        var user = await _userManager.GetByIdAsync(id);
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        (await _userManager.ResetPasswordAsync(user, token, newPassword)).CheckErrors();
    }

    public async Task<List<string>> GetRoleNamesAsync(Guid userId)
    {
        var user = await _userManager.GetByIdAsync(userId);
        return (await _userManager.GetRolesAsync(user)).ToList();
    }

    public async Task SetRoleNamesAsync(Guid userId, List<string> roleNames)
    {
        var user = await _userManager.GetByIdAsync(userId);
        (await _userManager.SetRolesAsync(user, roleNames.ToArray())).CheckErrors();
    }

    private async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        var user = await _userManager.GetByIdAsync(userId);
        return (await _userManager.GetRolesAsync(user)).ToList();
    }
}
