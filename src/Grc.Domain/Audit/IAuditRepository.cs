using Volo.Abp.Domain.Repositories;

namespace Grc.Domain.Audit;

public interface IAuditRepository : IRepository<Audit, Guid>
{
}
