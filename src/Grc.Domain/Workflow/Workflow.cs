using Volo.Abp.Domain.Entities.Auditing;
using Grc.Domain.Shared;

namespace Grc.Domain.Workflow;

public class Workflow : FullAuditedAggregateRoot<Guid>, IGovernedResource
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public string Status { get; set; } = "Draft";
    public string WorkflowType { get; set; } = string.Empty; // Approval, Review, Assessment, etc.
    public string? Definition { get; set; } // JSON or XML workflow definition
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public string? TriggerEvent { get; set; }
    public string? Conditions { get; set; } // JSON conditions
    public string? Steps { get; set; } // JSON workflow steps
    public DateTime? LastExecutedAt { get; set; }

    public string ResourceType => "Workflow";

    protected Workflow() { }

    public Workflow(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}
