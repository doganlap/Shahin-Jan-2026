using Volo.Abp.Domain.Repositories;

namespace Grc.Domain.Notification;

public interface INotificationRepository : IRepository<Notification, Guid>
{
}
