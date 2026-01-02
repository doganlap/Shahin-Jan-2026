using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Grc.Application.Contracts.RegulatoryFramework;

public interface IRegulatoryFrameworkAppService : IApplicationService
{
    Task<PagedResultDto<RegulatoryFrameworkDto>> GetListAsync(RegulatoryFrameworkListInputDto input);
    Task<RegulatoryFrameworkDto> GetAsync(Guid id);
    Task<RegulatoryFrameworkDto> CreateAsync(CreateRegulatoryFrameworkDto input);
    Task<RegulatoryFrameworkDto> UpdateAsync(Guid id, UpdateRegulatoryFrameworkDto input);
    Task DeleteAsync(Guid id);
}
