using Volo.Abp.Domain.Entities.Auditing;
using Grc.Domain.Shared;

namespace Grc.Domain.ActionPlan;

public class ActionPlan : FullAuditedAggregateRoot<Guid>, IGovernedResource
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public string Status { get; set; } = "Open";
    
    // Related entities
    public Guid? RiskId { get; set; }
    public Guid? AuditId { get; set; }
    public Guid? AssessmentId { get; set; }
    
    // Action plan specific
    public string ActionType { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string? AssignedTo { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int? Progress { get; set; }
    public string? Notes { get; set; }

    public string ResourceType => "ActionPlan";

    protected ActionPlan() { }

    public ActionPlan(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}
