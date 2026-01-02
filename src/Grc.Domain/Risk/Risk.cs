using Volo.Abp.Domain.Entities.Auditing;
using Grc.Domain.Shared;

namespace Grc.Domain.Risk;

public class Risk : FullAuditedAggregateRoot<Guid>, IGovernedResource
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public string Status { get; set; } = "Open";
    
    // Risk-specific properties
    public string? Category { get; set; }
    public string? Severity { get; set; }
    public string? Likelihood { get; set; }
    public string? Impact { get; set; }
    public int? RiskScore { get; set; }
    public string? MitigationStrategy { get; set; }
    public string? TreatmentPlan { get; set; }
    public DateTime? IdentifiedDate { get; set; }
    public DateTime? ReviewDate { get; set; }
    public bool IsAccepted { get; set; }
    public string? AcceptedBy { get; set; }
    public DateTime? AcceptedDate { get; set; }

    public string ResourceType => "Risk";

    protected Risk() { }

    public Risk(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}
