using Volo.Abp.Application.Services;
using Grc.Application.Contracts.Admin.UserManagement;

namespace Grc.Application.Contracts.Admin.RoleManagement;

public interface IRoleManagementAppService : ICrudAppService<RoleDto, Guid, RoleListInputDto, CreateRoleDto, UpdateRoleDto>
{
    Task<List<string>> GetPermissionsAsync(Guid roleId);
    Task SetPermissionsAsync(Guid roleId, List<string> permissions);
    Task<List<UserDto>> GetUsersInRoleAsync(Guid roleId);
}
