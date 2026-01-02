using Volo.Abp.Domain.Repositories;

namespace Grc.Domain.ComplianceEvent;

public interface IComplianceEventRepository : IRepository<ComplianceEvent, Guid>
{
}
