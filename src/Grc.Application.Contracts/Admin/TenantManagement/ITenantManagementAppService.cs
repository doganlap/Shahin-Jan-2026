using Volo.Abp.Application.Services;

namespace Grc.Application.Contracts.Admin.TenantManagement;

public interface ITenantManagementAppService : IReadOnlyAppService<TenantDto, Guid, TenantListInputDto>
{
    Task<TenantDto> GetDetailsAsync(Guid id);
}
