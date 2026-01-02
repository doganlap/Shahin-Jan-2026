using Volo.Abp.Domain.Repositories;

namespace Grc.Domain.ActionPlan;

public interface IActionPlanRepository : IRepository<ActionPlan, Guid>
{
}
