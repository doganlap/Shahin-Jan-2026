using Volo.Abp.Domain.Entities.Auditing;
using Grc.Domain.Shared;

namespace Grc.Domain.Audit;

public class Audit : FullAuditedAggregateRoot<Guid>, IGovernedResource
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public string Status { get; set; } = "Planned";
    
    // Audit-specific properties
    public string? AuditType { get; set; }
    public string? Scope { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? LeadAuditor { get; set; }
    public string? AuditTeam { get; set; }
    public string? Findings { get; set; }
    public string? Recommendations { get; set; }
    public string? AuditConclusion { get; set; }
    public Guid? FrameworkId { get; set; }

    public string ResourceType => "Audit";

    protected Audit() { }

    public Audit(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}
