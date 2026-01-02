using Volo.Abp.Domain.Entities.Auditing;
using Grc.Domain.Shared;

namespace Grc.Domain.ComplianceEvent;

public class ComplianceEvent : FullAuditedAggregateRoot<Guid>, IGovernedResource
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public string Status { get; set; } = "Scheduled";
    public string EventType { get; set; } = string.Empty; // Assessment, Audit, Review, Submission, Renewal
    public DateTime EventDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public Guid? RelatedFrameworkId { get; set; }
    public Guid? RelatedRegulatorId { get; set; }
    public string? Frequency { get; set; } // Monthly, Quarterly, Annually
    public string? Priority { get; set; } // Low, Medium, High, Critical
    public string? Notes { get; set; }

    public string ResourceType => "ComplianceEvent";

    protected ComplianceEvent() { }

    public ComplianceEvent(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}
