using Volo.Abp.Application.Dtos;

namespace Grc.Application.Contracts.RegulatoryFramework;

public class RegulatoryFrameworkDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public string Status { get; set; } = "Active";
    public string FrameworkType { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? Jurisdiction { get; set; }
    public string? Website { get; set; }
}

public class CreateRegulatoryFrameworkDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DataClassification { get; set; }
    public string? Owner { get; set; }
    public string FrameworkType { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? Jurisdiction { get; set; }
    public string? Website { get; set; }
}

public class UpdateRegulatoryFrameworkDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DataClassification { get; set; }
    public string? Owner { get; set; }
    public string FrameworkType { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? Jurisdiction { get; set; }
    public string? Website { get; set; }
}
