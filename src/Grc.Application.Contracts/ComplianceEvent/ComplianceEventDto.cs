using Volo.Abp.Application.Dtos;

namespace Grc.Application.Contracts.ComplianceEvent;

public class ComplianceEventDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public string Status { get; set; } = "Scheduled";
    public string EventType { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public Guid? RelatedFrameworkId { get; set; }
    public Guid? RelatedRegulatorId { get; set; }
    public string? Frequency { get; set; }
    public string? Priority { get; set; }
    public string? Notes { get; set; }
}

public class CreateComplianceEventDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DataClassification { get; set; }
    public string? Owner { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? RelatedFrameworkId { get; set; }
    public Guid? RelatedRegulatorId { get; set; }
    public string? Frequency { get; set; }
    public string? Priority { get; set; }
    public string? Notes { get; set; }
}

public class UpdateComplianceEventDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DataClassification { get; set; }
    public string? Owner { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? RelatedFrameworkId { get; set; }
    public Guid? RelatedRegulatorId { get; set; }
    public string? Frequency { get; set; }
    public string? Priority { get; set; }
    public string? Notes { get; set; }
}
