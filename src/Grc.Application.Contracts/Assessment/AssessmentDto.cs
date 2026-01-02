using Volo.Abp.Application.Dtos;

namespace Grc.Application.Contracts.Assessment;

public class AssessmentDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public string Status { get; set; } = "Draft";
}

public class CreateAssessmentDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DataClassification { get; set; }
    public string? Owner { get; set; }
}

public class UpdateAssessmentDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DataClassification { get; set; }
    public string? Owner { get; set; }
}
