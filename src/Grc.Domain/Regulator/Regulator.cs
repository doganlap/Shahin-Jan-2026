using Volo.Abp.Domain.Entities.Auditing;
using Grc.Domain.Shared;

namespace Grc.Domain.Regulator;

public class Regulator : FullAuditedAggregateRoot<Guid>, IGovernedResource
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public string Status { get; set; } = "Active";
    public string RegulatorType { get; set; } = string.Empty; // Government, Industry, International
    public string? Country { get; set; }
    public string? Region { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }

    public string ResourceType => "Regulator";

    protected Regulator() { }

    public Regulator(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}
