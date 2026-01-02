using Volo.Abp.Application.Dtos;

namespace Grc.Application.Contracts.ComplianceEvent;

public class ComplianceEventListInputDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public string? Status { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public Guid? FrameworkId { get; set; }
}
