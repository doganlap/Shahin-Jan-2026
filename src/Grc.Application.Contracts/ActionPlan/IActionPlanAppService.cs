using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Grc.Application.Contracts.ActionPlan;

public interface IActionPlanAppService : IApplicationService
{
    Task<PagedResultDto<ActionPlanDto>> GetListAsync(ActionPlanListInputDto input);
    Task<ActionPlanDto> GetAsync(Guid id);
    Task<ActionPlanDto> CreateAsync(CreateActionPlanDto input);
    Task<ActionPlanDto> UpdateAsync(Guid id, UpdateActionPlanDto input);
    Task<ActionPlanDto> AssignAsync(Guid id, string assignee);
    Task<ActionPlanDto> CloseAsync(Guid id);
    Task DeleteAsync(Guid id);
}
