using Volo.Abp.Application.Services;

namespace Grc.Application.Contracts.Admin.UserManagement;

public interface IUserManagementAppService : ICrudAppService<UserDto, Guid, UserListInputDto, CreateUserDto, UpdateUserDto>
{
    Task ResetPasswordAsync(Guid id, string newPassword);
    Task<List<string>> GetRoleNamesAsync(Guid userId);
    Task SetRoleNamesAsync(Guid userId, List<string> roleNames);
}
