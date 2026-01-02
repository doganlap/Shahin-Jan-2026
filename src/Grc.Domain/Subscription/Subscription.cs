using Volo.Abp.Domain.Entities.Auditing;

namespace Grc.Domain.Subscription;

public class Subscription : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string PlanType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public Guid? TenantId { get; set; }

    protected Subscription() { }

    public Subscription(Guid id, string name, string planType)
    {
        Id = id;
        Name = name;
        PlanType = planType;
        IsActive = true;
        StartDate = DateTime.UtcNow;
    }
}
