using Volo.Abp.Application.Dtos;

namespace Grc.Application.Contracts.Assessment;

public class AssessmentListInputDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public string? Status { get; set; }
    public string? Owner { get; set; }
    public Guid? FrameworkId { get; set; }
}
