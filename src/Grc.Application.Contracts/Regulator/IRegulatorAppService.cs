using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Grc.Application.Contracts.Regulator;

public interface IRegulatorAppService : IApplicationService
{
    Task<PagedResultDto<RegulatorDto>> GetListAsync(RegulatorListInputDto input);
    Task<RegulatorDto> GetAsync(Guid id);
    Task<RegulatorDto> CreateAsync(CreateRegulatorDto input);
    Task<RegulatorDto> UpdateAsync(Guid id, UpdateRegulatorDto input);
    Task DeleteAsync(Guid id);
}
