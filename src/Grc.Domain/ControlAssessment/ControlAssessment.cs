using Volo.Abp.Domain.Entities.Auditing;
using Grc.Domain.Shared;

namespace Grc.Domain.ControlAssessment;

public class ControlAssessment : FullAuditedAggregateRoot<Guid>, IGovernedResource
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public string Status { get; set; } = "Draft";
    public Guid? AssessmentId { get; set; } // Link to parent Assessment
    public string ControlId { get; set; } = string.Empty;
    public string ControlName { get; set; } = string.Empty;
    public string? Effectiveness { get; set; } // Effective, PartiallyEffective, Ineffective
    public string? Notes { get; set; }

    public string ResourceType => "ControlAssessment";

    protected ControlAssessment() { }

    public ControlAssessment(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}
