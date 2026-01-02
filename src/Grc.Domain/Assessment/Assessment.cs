using Volo.Abp.Domain.Entities.Auditing;
using Grc.Domain.Shared;

namespace Grc.Domain.Assessment;

public class Assessment : FullAuditedAggregateRoot<Guid>, IGovernedResource
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public string Status { get; set; } = "Draft";
    
    // Assessment-specific properties
    public Guid? FrameworkId { get; set; }
    public string? AssessmentType { get; set; }
    public string? Scope { get; set; }
    public DateTime? AssessmentDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? AssessedBy { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public int? ComplianceScore { get; set; }
    public string? Summary { get; set; }
    public string? Findings { get; set; }

    public string ResourceType => "Assessment";

    protected Assessment() { }

    public Assessment(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}
