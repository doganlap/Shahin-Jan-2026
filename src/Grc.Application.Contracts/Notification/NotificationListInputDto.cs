using Volo.Abp.Application.Dtos;

namespace Grc.Application.Contracts.Notification;

public class NotificationListInputDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public string? Type { get; set; }
    public bool? IsRead { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
}
