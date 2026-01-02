using Volo.Abp.Domain.Entities.Auditing;
using Grc.Domain.Shared;

namespace Grc.Domain.RegulatoryFramework;

public class RegulatoryFramework : FullAuditedAggregateRoot<Guid>, IGovernedResource
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public string Status { get; set; } = "Active";
    
    // Framework-specific properties
    public Guid? RegulatorId { get; set; }
    public string? FrameworkType { get; set; }
    public string? Version { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? Category { get; set; }
    public string? Jurisdiction { get; set; }
    public string? Website { get; set; }
    public string? ControlCount { get; set; }
    public bool IsImported { get; set; }
    public DateTime? ImportedDate { get; set; }

    public string ResourceType => "RegulatoryFramework";

    protected RegulatoryFramework() { }

    public RegulatoryFramework(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}
