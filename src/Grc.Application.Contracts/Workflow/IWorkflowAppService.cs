using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Grc.Application.Contracts.Workflow;

public interface IWorkflowAppService : IApplicationService
{
    Task<PagedResultDto<WorkflowDto>> GetListAsync(WorkflowListInputDto input);
    Task<WorkflowDto> GetAsync(Guid id);
    Task<WorkflowDto> CreateAsync(CreateWorkflowDto input);
    Task<WorkflowDto> UpdateAsync(Guid id, UpdateWorkflowDto input);
    Task DeleteAsync(Guid id);
    Task<WorkflowExecutionResultDto> ExecuteAsync(Guid id, WorkflowExecutionDto input);
    Task<WorkflowStatusDto> GetStatusAsync(Guid id);
}
