using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Grc.Application.Contracts.Notification;

public interface INotificationAppService : IApplicationService
{
    Task<PagedResultDto<NotificationDto>> GetListAsync(NotificationListInputDto input);
    Task<NotificationDto> GetAsync(Guid id);
    Task<NotificationDto> CreateAsync(CreateNotificationDto input);
    Task<NotificationDto> UpdateAsync(Guid id, UpdateNotificationDto input);
    Task DeleteAsync(Guid id);
    Task MarkAsReadAsync(Guid id);
    Task<int> GetUnreadCountAsync();
    Task MarkAllAsReadAsync();
}
