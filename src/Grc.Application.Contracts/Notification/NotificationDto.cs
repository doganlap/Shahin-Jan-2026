using Volo.Abp.Application.Dtos;

namespace Grc.Application.Contracts.Notification;

public class NotificationDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public string Status { get; set; } = "Unread";
    public string NotificationType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? RecipientId { get; set; }
    public string? RecipientEmail { get; set; }
    public DateTime? SentDate { get; set; }
    public DateTime? ReadDate { get; set; }
    public string? Priority { get; set; }
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; } = false;
    public bool IsEmailSent { get; set; } = false;
}

public class CreateNotificationDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DataClassification { get; set; }
    public string? Owner { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? RecipientId { get; set; }
    public string? RecipientEmail { get; set; }
    public string? Priority { get; set; }
    public string? ActionUrl { get; set; }
}

public class UpdateNotificationDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DataClassification { get; set; }
    public string? Owner { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Priority { get; set; }
    public string? ActionUrl { get; set; }
}
