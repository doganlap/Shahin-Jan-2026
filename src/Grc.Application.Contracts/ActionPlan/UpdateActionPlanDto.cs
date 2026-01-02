namespace Grc.Application.Contracts.ActionPlan;

public class UpdateActionPlanDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? RiskId { get; set; }
    public Guid? AuditId { get; set; }
}
