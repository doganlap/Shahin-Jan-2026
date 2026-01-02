using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;
using Volo.Abp.TenantManagement;
using Volo.Abp.Domain.Repositories;
using Grc.Application.Contracts.Admin.TenantManagement;
using Grc.Permissions;
using Microsoft.AspNetCore.Authorization;
using TenantDtoContract = Grc.Application.Contracts.Admin.TenantManagement.TenantDto;

namespace Grc.Application.Admin.TenantManagement;

[Authorize(GrcPermissions.Admin.Tenants)]
public class TenantManagementAppService : ApplicationService, ITenantManagementAppService
{
    private readonly ITenantRepository _tenantRepository;

    public TenantManagementAppService(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<PagedResultDto<TenantDtoContract>> GetListAsync(TenantListInputDto input)
    {
        var totalCount = await _tenantRepository.GetCountAsync(input.Filter);
        var tenants = await _tenantRepository.GetListAsync(
            input.Sorting ?? "Name",
            input.MaxResultCount,
            input.SkipCount,
            input.Filter
        );

        return new PagedResultDto<TenantDtoContract>(
            totalCount,
            ObjectMapper.Map<List<Tenant>, List<TenantDtoContract>>(tenants)
        );
    }

    public async Task<TenantDtoContract> GetAsync(Guid id)
    {
        var tenant = await _tenantRepository.GetAsync(id);
        return ObjectMapper.Map<Tenant, TenantDtoContract>(tenant);
    }

    public async Task<TenantDtoContract> GetDetailsAsync(Guid id)
    {
        var tenant = await _tenantRepository.GetAsync(id);
        var dto = ObjectMapper.Map<Tenant, TenantDtoContract>(tenant);
        
        // User count would require additional service, leaving as 0 for now
        dto.UserCount = 0;
        
        return dto;
    }
}
