using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Grc.Application.Contracts.Risk;

public interface IRiskAppService : IApplicationService
{
    Task<PagedResultDto<RiskDto>> GetListAsync(RiskListInputDto input);
    Task<RiskDto> GetAsync(Guid id);
    Task<RiskDto> CreateAsync(CreateRiskDto input);
    Task<RiskDto> UpdateAsync(Guid id, UpdateRiskDto input);
    Task<RiskDto> AcceptAsync(Guid id);
    Task DeleteAsync(Guid id);
}
