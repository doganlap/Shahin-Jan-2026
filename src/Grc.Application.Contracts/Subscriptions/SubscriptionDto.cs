using Volo.Abp.Application.Dtos;

namespace Grc.Application.Contracts.Subscriptions;

public class SubscriptionDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string PlanType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public Guid? TenantId { get; set; }
    public string? TenantName { get; set; }
}

public class CreateSubscriptionDto
{
    public string Name { get; set; } = string.Empty;
    public string PlanType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public Guid? TenantId { get; set; }
}

public class UpdateSubscriptionDto
{
    public string? Name { get; set; }
    public string? PlanType { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
}

public class SubscriptionListInputDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public bool? IsActive { get; set; }
    public Guid? TenantId { get; set; }
}
