using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Grc.Application.Contracts.Roles;

public interface IRoleProfileAppService : IApplicationService
{
    Task<List<RoleProfileDto>> GetAllProfilesAsync();
    Task<RoleProfileDto?> GetProfileAsync(string roleName);
    Task<List<RoleProfileDto>> GetAvailableProfilesAsync();
    Task<RoleProfileDto> CreateRoleFromProfileAsync(CreateRoleFromProfileDto input);
    Task<List<RoleProfileSummaryDto>> GetProfileSummariesAsync();
}
