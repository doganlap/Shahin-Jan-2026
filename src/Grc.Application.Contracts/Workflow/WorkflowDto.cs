using Volo.Abp.Application.Dtos;

namespace Grc.Application.Contracts.Workflow;

public class WorkflowDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? DataClassification { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public string Status { get; set; } = "Draft";
    public string WorkflowType { get; set; } = string.Empty;
    public string? Definition { get; set; }
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public string? TriggerEvent { get; set; }
    public string? Conditions { get; set; }
    public string? Steps { get; set; }
}

public class CreateWorkflowDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DataClassification { get; set; }
    public string? Owner { get; set; }
    public string WorkflowType { get; set; } = string.Empty;
    public string? Definition { get; set; }
    public string? TriggerEvent { get; set; }
    public string? Conditions { get; set; }
    public string? Steps { get; set; }
}

public class UpdateWorkflowDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DataClassification { get; set; }
    public string? Owner { get; set; }
    public string WorkflowType { get; set; } = string.Empty;
    public string? Definition { get; set; }
    public string? TriggerEvent { get; set; }
    public string? Conditions { get; set; }
    public string? Steps { get; set; }
    public bool IsActive { get; set; } = true;
}
