using Volo.Abp.Domain.Entities.Auditing;
using Grc.Domain.Shared;

namespace Grc.Domain.Notification;

public class Notification : FullAuditedAggregateRoot<Guid>, IGovernedResource
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public string Status { get; set; } = "Unread";
    public string NotificationType { get; set; } = string.Empty; // Alert, Reminder, Approval, Info
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? RecipientId { get; set; }
    public string? RecipientEmail { get; set; }
    public DateTime? SentDate { get; set; }
    public DateTime? ReadDate { get; set; }
    public string? Priority { get; set; } // Low, Medium, High, Urgent
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; } = false;
    public bool IsEmailSent { get; set; } = false;

    public string ResourceType => "Notification";

    protected Notification() { }

    public Notification(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}
