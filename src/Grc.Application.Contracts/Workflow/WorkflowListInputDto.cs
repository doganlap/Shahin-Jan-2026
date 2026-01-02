using Volo.Abp.Application.Dtos;

namespace Grc.Application.Contracts.Workflow;

public class WorkflowListInputDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public string? Status { get; set; }
    public string? Type { get; set; }
    public string? ResourceType { get; set; }
}
