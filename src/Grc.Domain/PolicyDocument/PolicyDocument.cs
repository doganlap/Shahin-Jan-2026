using Volo.Abp.Domain.Entities.Auditing;
using Grc.Domain.Shared;

namespace Grc.Domain.PolicyDocument;

public class PolicyDocument : FullAuditedAggregateRoot<Guid>, IGovernedResource
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public string Status { get; set; } = "Draft";
    
    // Policy-specific properties
    public string? Category { get; set; }
    public string? Version { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime? ReviewDate { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? Content { get; set; }
    public string? Summary { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedDate { get; set; }
    public bool ApprovedForProd { get; set; }

    public string ResourceType => "PolicyDocument";

    protected PolicyDocument() { }

    public PolicyDocument(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}
