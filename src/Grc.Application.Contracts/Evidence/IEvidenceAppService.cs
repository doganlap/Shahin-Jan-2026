using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Grc.Application.Contracts.Evidence;

public interface IEvidenceAppService : IApplicationService
{
    Task<PagedResultDto<EvidenceDto>> GetListAsync(EvidenceListInputDto input);
    Task<EvidenceDto> GetAsync(Guid id);
    Task<EvidenceDto> CreateAsync(CreateEvidenceDto input);
    Task<EvidenceDto> UpdateAsync(Guid id, UpdateEvidenceDto input);
    Task<EvidenceDto> ApproveAsync(Guid id);
    Task DeleteAsync(Guid id);
}
