using Volo.Abp.Domain.Repositories;

namespace Grc.Domain.Subscription;

public interface ISubscriptionRepository : IRepository<Subscription, Guid>
{
}
