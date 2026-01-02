using Volo.Abp.Domain.Entities.Auditing;
using Grc.Domain.Shared;

namespace Grc.Domain.Evidence;

public class Evidence : FullAuditedAggregateRoot<Guid>, IGovernedResource
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public bool ApprovedForProd { get; set; }
    public string Status { get; set; } = "Draft";

    public string ResourceType => "Evidence";

    protected Evidence() { }

    public Evidence(Guid id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }
}
