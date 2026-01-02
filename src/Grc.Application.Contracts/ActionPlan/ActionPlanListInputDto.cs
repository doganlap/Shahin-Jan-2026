using Volo.Abp.Application.Dtos;

namespace Grc.Application.Contracts.ActionPlan;

public class ActionPlanListInputDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public string? Status { get; set; }
    public string? Owner { get; set; }
    public Guid? RiskId { get; set; }
    public Guid? AuditId { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
}
