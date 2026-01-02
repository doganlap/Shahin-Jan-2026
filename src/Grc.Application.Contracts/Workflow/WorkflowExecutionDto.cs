namespace Grc.Application.Contracts.Workflow;

public class WorkflowExecutionDto
{
    public Dictionary<string, object>? Parameters { get; set; }
    public string? TriggeredBy { get; set; }
}

public class WorkflowExecutionResultDto
{
    public Guid WorkflowId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public string? Result { get; set; }
    public Dictionary<string, object>? Output { get; set; }
}

public class WorkflowStatusDto
{
    public Guid WorkflowId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? LastExecutedAt { get; set; }
    public bool IsActive { get; set; }
}
