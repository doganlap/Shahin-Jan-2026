using Volo.Abp.Domain.Repositories;

namespace Grc.Domain.Workflow;

public interface IWorkflowRepository : IRepository<Workflow, Guid>
{
}
