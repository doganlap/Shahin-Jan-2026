using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Grc.Application.Contracts.ControlAssessment;

public interface IControlAssessmentAppService : IApplicationService
{
    Task<PagedResultDto<ControlAssessmentDto>> GetListAsync(ControlAssessmentListInputDto input);
    Task<ControlAssessmentDto> GetAsync(Guid id);
    Task<ControlAssessmentDto> CreateAsync(CreateControlAssessmentDto input);
    Task<ControlAssessmentDto> UpdateAsync(Guid id, UpdateControlAssessmentDto input);
    Task DeleteAsync(Guid id);
}
