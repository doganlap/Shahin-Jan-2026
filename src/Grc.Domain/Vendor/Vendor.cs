using Volo.Abp.Domain.Entities.Auditing;
using Grc.Domain.Shared;

namespace Grc.Domain.Vendor;

public class Vendor : FullAuditedAggregateRoot<Guid>, IGovernedResource
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public string Status { get; set; } = "Active";
    public string VendorType { get; set; } = string.Empty; // Supplier, ServiceProvider, Partner
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? Country { get; set; }
    public string? RiskRating { get; set; } // Low, Medium, High
    public DateTime? LastAssessmentDate { get; set; }
    public DateTime? NextAssessmentDate { get; set; }

    public string ResourceType => "Vendor";

    protected Vendor() { }

    public Vendor(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}
