using Volo.Abp.Application.Dtos;

namespace Grc.Application.Contracts.ControlAssessment;

public class ControlAssessmentDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public string Status { get; set; } = "Draft";
    public Guid? AssessmentId { get; set; }
    public string ControlId { get; set; } = string.Empty;
    public string ControlName { get; set; } = string.Empty;
    public string? Effectiveness { get; set; }
    public string? Notes { get; set; }
}

public class CreateControlAssessmentDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DataClassification { get; set; }
    public string? Owner { get; set; }
    public Guid? AssessmentId { get; set; }
    public string ControlId { get; set; } = string.Empty;
    public string ControlName { get; set; } = string.Empty;
    public string? Effectiveness { get; set; }
    public string? Notes { get; set; }
}

public class UpdateControlAssessmentDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DataClassification { get; set; }
    public string? Owner { get; set; }
    public string? Effectiveness { get; set; }
    public string? Notes { get; set; }
}
