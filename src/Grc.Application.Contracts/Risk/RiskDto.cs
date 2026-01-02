using Volo.Abp.Application.Dtos;

namespace Grc.Application.Contracts.Risk;

public class RiskDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public string Status { get; set; } = "Draft";
    public string? Category { get; set; }
    public string? Severity { get; set; }
    public string? Likelihood { get; set; }
    public string? Impact { get; set; }
    public int? RiskScore { get; set; }
    public string? MitigationPlan { get; set; }
    public string? ResidualRisk { get; set; }
    public DateTime? ReviewDate { get; set; }
}

public class CreateRiskDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DataClassification { get; set; }
    public string? Owner { get; set; }
    public string? Category { get; set; }
    public string? Severity { get; set; }
    public string? Likelihood { get; set; }
    public string? Impact { get; set; }
    public string? MitigationPlan { get; set; }
}

public class UpdateRiskDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DataClassification { get; set; }
    public string? Owner { get; set; }
    public string? Category { get; set; }
    public string? Severity { get; set; }
    public string? Likelihood { get; set; }
    public string? Impact { get; set; }
    public string? MitigationPlan { get; set; }
    public string Status { get; set; } = "Draft";
}

public class RiskListInputDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public string? Status { get; set; }
    public string? Severity { get; set; }
    public string? Category { get; set; }
    public string? Owner { get; set; }
}
