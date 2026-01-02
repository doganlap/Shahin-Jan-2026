using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Grc.Application.Contracts.Assessment;

public interface IAssessmentAppService : IApplicationService
{
    Task<PagedResultDto<AssessmentDto>> GetListAsync(AssessmentListInputDto input);
    Task<AssessmentDto> GetAsync(Guid id);
    Task<AssessmentDto> CreateAsync(CreateAssessmentDto input);
    Task<AssessmentDto> UpdateAsync(Guid id, UpdateAssessmentDto input);
    Task<AssessmentDto> SubmitAsync(Guid id);
    Task<AssessmentDto> ApproveAsync(Guid id);
    Task DeleteAsync(Guid id);
}
